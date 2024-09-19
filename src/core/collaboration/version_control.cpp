#include <string>
#include <vector>
#include <memory>
#include <chrono>
#include "version_control.h"
#include "document.h"
#include "user.h"
#include "diff_engine.h"
#include "storage_manager.h"
#include "error_handler.h"

const int MAX_VERSIONS_TO_KEEP = 100;

// Generate a unique identifier for a version
std::string generateVersionId() {
    // Get the current timestamp
    auto now = std::chrono::system_clock::now();
    auto timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()).count();

    // Generate a random component
    std::srand(static_cast<unsigned int>(std::time(nullptr)));
    int random = std::rand();

    // Combine timestamp and random component to create a unique ID
    return std::to_string(timestamp) + "_" + std::to_string(random);
}

VersionControl::VersionControl(std::shared_ptr<Document> document)
    : m_currentDocument(document),
      m_diffEngine(std::make_shared<DiffEngine>()),
      m_storageManager(std::make_shared<StorageManager>()),
      m_errorHandler(std::make_shared<ErrorHandler>()) {
    
    // Create an initial version of the document and add it to m_versionHistory
    auto initialVersion = document->clone();
    initialVersion->setVersionId(generateVersionId());
    m_versionHistory.push_back(initialVersion);
}

bool VersionControl::createVersion(const std::shared_ptr<User>& user, const std::string& commitMessage) {
    try {
        // Create a deep copy of m_currentDocument
        auto newVersion = m_currentDocument->clone();

        // Set version metadata (timestamp, user, commit message)
        newVersion->setVersionId(generateVersionId());
        newVersion->setTimestamp(std::chrono::system_clock::now());
        newVersion->setUser(user);
        newVersion->setCommitMessage(commitMessage);

        // Add the new version to m_versionHistory
        m_versionHistory.push_back(newVersion);

        // If m_versionHistory size exceeds MAX_VERSIONS_TO_KEEP, remove oldest version
        if (m_versionHistory.size() > MAX_VERSIONS_TO_KEEP) {
            m_versionHistory.erase(m_versionHistory.begin());
        }

        // Save the new version using m_storageManager
        m_storageManager->saveVersion(newVersion);

        return true;
    } catch (const std::exception& e) {
        m_errorHandler->handleError("Failed to create version: " + std::string(e.what()));
        return false;
    }
}

bool VersionControl::rollbackToVersion(int versionIndex) {
    try {
        // Validate the versionIndex
        if (versionIndex < 0 || versionIndex >= m_versionHistory.size()) {
            throw std::out_of_range("Invalid version index");
        }

        // Retrieve the specified version from m_versionHistory
        auto targetVersion = m_versionHistory[versionIndex];

        // Create a deep copy of the specified version
        m_currentDocument = targetVersion->clone();

        // Create a new version to represent the rollback action
        createVersion(nullptr, "Rolled back to version " + targetVersion->getVersionId());

        return true;
    } catch (const std::exception& e) {
        m_errorHandler->handleError("Failed to rollback to version: " + std::string(e.what()));
        return false;
    }
}

std::string VersionControl::compareVersions(int versionIndex1, int versionIndex2) {
    try {
        // Validate both versionIndex1 and versionIndex2
        if (versionIndex1 < 0 || versionIndex1 >= m_versionHistory.size() ||
            versionIndex2 < 0 || versionIndex2 >= m_versionHistory.size()) {
            throw std::out_of_range("Invalid version index");
        }

        // Retrieve the specified versions from m_versionHistory
        auto version1 = m_versionHistory[versionIndex1];
        auto version2 = m_versionHistory[versionIndex2];

        // Use m_diffEngine to generate a diff between the two versions
        return m_diffEngine->generateDiff(version1, version2);
    } catch (const std::exception& e) {
        m_errorHandler->handleError("Failed to compare versions: " + std::string(e.what()));
        return "";
    }
}

std::vector<VersionMetadata> VersionControl::getVersionHistory() {
    std::vector<VersionMetadata> history;

    // Iterate through m_versionHistory
    for (const auto& version : m_versionHistory) {
        // Extract metadata from each version and add to the vector
        VersionMetadata metadata;
        metadata.versionId = version->getVersionId();
        metadata.timestamp = version->getTimestamp();
        metadata.user = version->getUser();
        metadata.commitMessage = version->getCommitMessage();
        history.push_back(metadata);
    }

    return history;
}

bool VersionControl::mergeVersions(int sourceVersionIndex, int targetVersionIndex) {
    try {
        // Validate sourceVersionIndex and targetVersionIndex
        if (sourceVersionIndex < 0 || sourceVersionIndex >= m_versionHistory.size() ||
            targetVersionIndex < 0 || targetVersionIndex >= m_versionHistory.size()) {
            throw std::out_of_range("Invalid version index");
        }

        // Retrieve the specified versions from m_versionHistory
        auto sourceVersion = m_versionHistory[sourceVersionIndex];
        auto targetVersion = m_versionHistory[targetVersionIndex];

        // Generate a diff between the source and target versions
        std::string diff = m_diffEngine->generateDiff(sourceVersion, targetVersion);

        // Apply the diff to the target version using m_diffEngine
        auto mergedVersion = targetVersion->clone();
        m_diffEngine->applyPatch(mergedVersion, diff);

        // Create a new version representing the merged result
        m_currentDocument = mergedVersion;
        createVersion(nullptr, "Merged version " + sourceVersion->getVersionId() + " into " + targetVersion->getVersionId());

        return true;
    } catch (const std::exception& e) {
        m_errorHandler->handleError("Failed to merge versions: " + std::string(e.what()));
        return false;
    }
}

// Human tasks:
// TODO: Implement a more sophisticated version retention policy
// TODO: Add support for tagging specific versions
// TODO: Implement conflict resolution for complex merges
// TODO: Add support for visual diff and merge tools