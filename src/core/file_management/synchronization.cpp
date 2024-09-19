#include <string>
#include <vector>
#include <memory>
#include <chrono>
#include "synchronization.h"
#include "document.h"
#include "cloud_storage.h"
#include "conflict_resolution.h"
#include "delta_sync.h"
#include "error_handler.h"

const int SYNC_INTERVAL_SECONDS = 300; // Default sync interval of 5 minutes

Synchronization::Synchronization()
    : m_cloudStorage(std::make_shared<CloudStorage>()),
      m_conflictResolver(std::make_shared<ConflictResolution>()),
      m_deltaSync(std::make_shared<DeltaSync>()),
      m_errorHandler(std::make_shared<ErrorHandler>()),
      m_lastSyncTime(std::chrono::system_clock::now()) {
    // Constructor implementation
}

bool Synchronization::syncDocument(std::shared_ptr<Document>& localDocument) {
    // Check if synchronization is needed based on m_lastSyncTime
    if (calculateTimeSinceLastSync(m_lastSyncTime) < SYNC_INTERVAL_SECONDS) {
        return true; // No need to sync yet
    }

    try {
        // Retrieve cloud document metadata
        auto cloudMetadata = m_cloudStorage->getDocumentMetadata(localDocument->getId());

        // Compare local and cloud document versions
        if (cloudMetadata.version != localDocument->getVersion()) {
            // Generate delta between local and cloud versions
            auto delta = m_deltaSync->generateDelta(localDocument, cloudMetadata);

            // Apply delta to local document
            localDocument->applyDelta(delta);

            // If conflicts occur, use m_conflictResolver to resolve them
            if (delta.hasConflicts()) {
                auto cloudDocument = m_cloudStorage->getDocument(localDocument->getId());
                auto resolvedDocument = handleConflict(localDocument, cloudDocument);
                localDocument = resolvedDocument;
            }

            // Upload local changes to cloud if necessary
            m_cloudStorage->uploadDocument(localDocument);
        }

        // Update m_lastSyncTime
        m_lastSyncTime = std::chrono::system_clock::now();

        return true;
    } catch (const std::exception& e) {
        m_errorHandler->handleError("Synchronization failed: " + std::string(e.what()));
        return false;
    }
}

bool Synchronization::forceSyncDocument(std::shared_ptr<Document>& localDocument) {
    // Reset m_lastSyncTime to trigger immediate sync
    m_lastSyncTime = std::chrono::system_clock::now() - std::chrono::hours(24);

    // Call syncDocument to perform the synchronization
    return syncDocument(localDocument);
}

std::shared_ptr<Document> Synchronization::handleConflict(std::shared_ptr<Document>& localDocument, std::shared_ptr<Document>& cloudDocument) {
    // Use m_conflictResolver to resolve conflicts between local and cloud documents
    auto resolvedChanges = m_conflictResolver->resolveConflicts(localDocument, cloudDocument);

    // Apply resolved changes to the local document
    localDocument->applyChanges(resolvedChanges);

    return localDocument;
}

void Synchronization::setSyncInterval(int intervalSeconds) {
    // Validate the input interval
    if (intervalSeconds > 0) {
        // Update SYNC_INTERVAL_SECONDS with the new value
        const_cast<int&>(SYNC_INTERVAL_SECONDS) = intervalSeconds;
    } else {
        m_errorHandler->handleError("Invalid sync interval provided");
    }
}

int calculateTimeSinceLastSync(const std::chrono::time_point<std::chrono::system_clock>& lastSyncTime) {
    // Get the current time
    auto currentTime = std::chrono::system_clock::now();

    // Calculate the duration between current time and lastSyncTime
    auto duration = std::chrono::duration_cast<std::chrono::seconds>(currentTime - lastSyncTime);

    // Return the elapsed time in seconds
    return static_cast<int>(duration.count());
}

// Human tasks:
// TODO: Implement robust error handling for network issues during sync
// TODO: Add user notification system for sync status and conflicts
// TODO: Implement user interface for manual conflict resolution when necessary