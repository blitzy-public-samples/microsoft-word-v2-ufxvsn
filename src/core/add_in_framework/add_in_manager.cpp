#include <string>
#include <vector>
#include <unordered_map>
#include <memory>
#include "add_in_manager.h"
#include "add_in.h"
#include "add_in_loader.h"
#include "security_manager.h"
#include "error_handler.h"
#include "platform_utils.h"

const std::string ADD_IN_DIRECTORY = "path/to/add_in_directory"; // Replace with actual path

AddInManager::AddInManager()
    : m_addInLoader(std::make_shared<AddInLoader>()),
      m_securityManager(std::make_shared<SecurityManager>()),
      m_errorHandler(std::make_shared<ErrorHandler>()),
      m_loadedAddIns() {
}

bool AddInManager::loadAddIns() {
    bool allLoaded = true;
    std::vector<std::string> addInFiles = PlatformUtils::getAddInFiles(ADD_IN_DIRECTORY);

    for (const auto& addInFile : addInFiles) {
        if (m_securityManager->validateAddIn(addInFile)) {
            std::shared_ptr<AddIn> addIn = m_addInLoader->loadAddIn(addInFile);
            if (addIn) {
                m_loadedAddIns[addIn->getId()] = addIn;
            } else {
                allLoaded = false;
                m_errorHandler->logError("Failed to load add-in: " + addInFile);
            }
        } else {
            allLoaded = false;
            m_errorHandler->logError("Add-in failed security validation: " + addInFile);
        }
    }

    return allLoaded;
}

bool AddInManager::unloadAddIn(const std::string& addInId) {
    auto it = m_loadedAddIns.find(addInId);
    if (it != m_loadedAddIns.end()) {
        if (it->second->unload()) {
            m_loadedAddIns.erase(it);
            return true;
        } else {
            m_errorHandler->logError("Failed to unload add-in: " + addInId);
        }
    }
    return false;
}

bool AddInManager::executeAddInCommand(const std::string& addInId, const std::string& commandId, const std::vector<std::string>& params) {
    auto it = m_loadedAddIns.find(addInId);
    if (it != m_loadedAddIns.end()) {
        return it->second->executeCommand(commandId, params);
    }
    m_errorHandler->logError("Add-in not found: " + addInId);
    return false;
}

std::vector<AddInInfo> AddInManager::getLoadedAddIns() {
    std::vector<AddInInfo> addInInfoList;
    for (const auto& pair : m_loadedAddIns) {
        addInInfoList.push_back(pair.second->getInfo());
    }
    return addInInfoList;
}

bool AddInManager::updateAddIn(const std::string& addInId, const std::string& newVersionPath) {
    auto it = m_loadedAddIns.find(addInId);
    if (it != m_loadedAddIns.end()) {
        if (m_securityManager->validateAddIn(newVersionPath)) {
            // Unload the current version
            if (it->second->unload()) {
                // Load the new version
                std::shared_ptr<AddIn> newAddIn = m_addInLoader->loadAddIn(newVersionPath);
                if (newAddIn) {
                    m_loadedAddIns[addInId] = newAddIn;
                    return true;
                } else {
                    m_errorHandler->logError("Failed to load new version of add-in: " + addInId);
                    // TODO: Implement rollback mechanism for failed updates
                }
            } else {
                m_errorHandler->logError("Failed to unload current version of add-in: " + addInId);
            }
        } else {
            m_errorHandler->logError("New version of add-in failed security validation: " + addInId);
        }
    } else {
        m_errorHandler->logError("Add-in not found for update: " + addInId);
    }
    return false;
}

bool validateAddInManifest(const std::string& manifestPath) {
    // Read the manifest file
    // TODO: Implement file reading logic

    // Check for required fields (name, version, compatibility)
    // TODO: Implement field checking logic

    // Validate the format and values of each field
    // TODO: Implement field validation logic

    // Return true if all checks pass, false otherwise
    return true; // Placeholder return value
}

// Human tasks:
// TODO: Implement a user interface for managing add-in permissions
// TODO: Add support for add-in dependencies and conflict resolution
// TODO: Implement rollback mechanism for failed updates
// TODO: Add support for automatic updates of add-ins