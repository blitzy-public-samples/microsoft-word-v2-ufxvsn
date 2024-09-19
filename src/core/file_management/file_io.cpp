#include <string>
#include <vector>
#include <fstream>
#include <memory>
#include "file_io.h"
#include "cloud_storage.h"
#include "document.h"
#include "error_handler.h"

const int MAX_FILE_SIZE = 1024 * 1024 * 100; // 100 MB

FileIO::FileIO() {
    // Initialize m_cloudStorage with a new CloudStorage object
    m_cloudStorage = std::make_shared<CloudStorage>();
    
    // Initialize m_errorHandler with a new ErrorHandler object
    m_errorHandler = std::make_shared<ErrorHandler>();
}

std::shared_ptr<Document> FileIO::openDocument(const std::string& filePath, bool isCloudStorage) {
    std::string fileContent;

    // Check if the file is in cloud storage or local
    if (isCloudStorage) {
        // If cloud storage, download the file using m_cloudStorage
        fileContent = m_cloudStorage->downloadFile(filePath);
    } else {
        // Open the file stream
        std::ifstream file(filePath, std::ios::binary);
        if (!file) {
            m_errorHandler->handleError("Failed to open file: " + filePath);
            return nullptr;
        }

        // Read the file contents
        fileContent = std::string((std::istreambuf_iterator<char>(file)),
                                   std::istreambuf_iterator<char>());
    }

    // Parse the file contents into a Document object
    auto document = std::make_shared<Document>();
    if (!document->deserialize(fileContent)) {
        m_errorHandler->handleError("Failed to parse document: " + filePath);
        return nullptr;
    }

    // Return the Document object
    return document;
}

bool FileIO::saveDocument(const std::shared_ptr<Document>& document, const std::string& filePath, bool isCloudStorage) {
    // Serialize the Document object to a string
    std::string serializedContent = document->serialize();

    if (isCloudStorage) {
        // If cloud storage, upload the file using m_cloudStorage
        return m_cloudStorage->uploadFile(filePath, serializedContent);
    } else {
        // Open the file stream for writing
        std::ofstream file(filePath, std::ios::binary);
        if (!file) {
            m_errorHandler->handleError("Failed to open file for writing: " + filePath);
            return false;
        }

        // Write the serialized content to the file
        file << serializedContent;

        // Return the result of the operation
        return file.good();
    }
}

std::shared_ptr<Document> FileIO::createNewDocument() {
    // Create a new Document object
    auto document = std::make_shared<Document>();

    // Initialize the document with default properties
    document->setTitle("Untitled Document");
    document->setAuthor("Unknown");
    document->setCreationDate(std::time(nullptr));

    // Return the new Document object
    return document;
}

bool FileIO::deleteDocument(const std::string& filePath, bool isCloudStorage) {
    // Check if the file is in cloud storage or local
    if (isCloudStorage) {
        // If cloud storage, delete the file using m_cloudStorage
        return m_cloudStorage->deleteFile(filePath);
    } else {
        // If local, delete the file from the file system
        if (std::remove(filePath.c_str()) != 0) {
            m_errorHandler->handleError("Failed to delete file: " + filePath);
            return false;
        }
        return true;
    }
}

bool validateFilePath(const std::string& filePath) {
    // Check if the file path is empty
    if (filePath.empty()) {
        return false;
    }

    // Check if the file path contains invalid characters
    const std::string invalidChars = "<>:\"|?*";
    if (filePath.find_first_of(invalidChars) != std::string::npos) {
        return false;
    }

    // Check if the file path exceeds the maximum allowed length
    const size_t MAX_PATH_LENGTH = 260;
    if (filePath.length() > MAX_PATH_LENGTH) {
        return false;
    }

    // Return the result of the validation
    return true;
}

// Human tasks:
// TODO: Implement robust error handling for file operations
// TODO: Add support for different file formats (e.g., .docx, .rtf)
// TODO: Implement version control for cloud-saved documents
// TODO: Add support for auto-save functionality
// TODO: Implement a recycle bin or soft delete feature for document recovery