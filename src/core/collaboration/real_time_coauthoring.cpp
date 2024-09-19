#include <string>
#include <vector>
#include <memory>
#include <unordered_map>
#include "real_time_coauthoring.h"
#include "document.h"
#include "user.h"
#include "operational_transform.h"
#include "collaboration_server.h"
#include "error_handler.h"

const int MAX_COLLABORATORS = 10; // Maximum number of simultaneous collaborators

RealTimeCoauthoring::RealTimeCoauthoring(std::shared_ptr<Document> document)
    : m_document(document),
      m_server(std::make_shared<CollaborationServer>()),
      m_operationalTransform(std::make_shared<OperationalTransform>()),
      m_errorHandler(std::make_shared<ErrorHandler>()) {
    // Initialize m_activeUsers as an empty map
    // Initialize m_operationQueue as an empty vector
}

bool RealTimeCoauthoring::joinSession(std::shared_ptr<User> user) {
    // Check if the number of active users is less than MAX_COLLABORATORS
    if (m_activeUsers.size() >= MAX_COLLABORATORS) {
        m_errorHandler->logError("Maximum number of collaborators reached");
        return false;
    }

    // Add the user to m_activeUsers
    m_activeUsers[user->getId()] = user;

    // Register the user with m_server
    m_server->registerUser(user);

    // Send the current document state to the new user
    m_server->sendDocumentState(user->getId(), m_document->getState());

    // Notify other users about the new participant
    m_server->broadcastUserJoined(user->getId());

    return true;
}

void RealTimeCoauthoring::leaveSession(const std::string& userId) {
    // Remove the user from m_activeUsers
    m_activeUsers.erase(userId);

    // Unregister the user from m_server
    m_server->unregisterUser(userId);

    // Notify other users about the participant leaving
    m_server->broadcastUserLeft(userId);
}

bool RealTimeCoauthoring::applyOperation(const Operation& operation, const std::string& userId) {
    // Validate the operation and user
    if (!validateOperation(operation, m_document) || m_activeUsers.find(userId) == m_activeUsers.end()) {
        m_errorHandler->logError("Invalid operation or user");
        return false;
    }

    // Apply operational transformation to the operation
    Operation transformedOperation = m_operationalTransform->transform(operation, m_operationQueue);

    // Apply the transformed operation to m_document
    if (!m_document->applyOperation(transformedOperation)) {
        m_errorHandler->logError("Failed to apply operation to document");
        return false;
    }

    // Add the operation to m_operationQueue
    m_operationQueue.push_back(transformedOperation);

    // Broadcast the operation to other users via m_server
    m_server->broadcastOperation(transformedOperation, userId);

    return true;
}

Operation RealTimeCoauthoring::resolveConflict(const Operation& localOperation, const Operation& remoteOperation) {
    // Use m_operationalTransform to resolve the conflict between operations
    return m_operationalTransform->resolveConflict(localOperation, remoteOperation);
}

bool RealTimeCoauthoring::synchronizeState(const std::string& userId) {
    // Get the current document state
    DocumentState currentState = m_document->getState();

    // Send the document state to the specified user via m_server
    return m_server->sendDocumentState(userId, currentState);
}

bool validateOperation(const Operation& operation, const std::shared_ptr<Document>& document) {
    // Check if the operation is within the document boundaries
    if (operation.getPosition() > document->getLength()) {
        return false;
    }

    // Verify that the operation type is supported
    if (!document->isSupportedOperationType(operation.getType())) {
        return false;
    }

    // Ensure the operation doesn't violate any document constraints
    if (!document->isValidOperation(operation)) {
        return false;
    }

    return true;
}

// Human tasks:
// TODO: Implement user authentication before joining the session
// TODO: Add support for user roles and permissions in co-authoring
// TODO: Implement undo/redo functionality for collaborative editing
// TODO: Optimize performance for handling a large number of concurrent operations