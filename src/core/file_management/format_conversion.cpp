#include <string>
#include <vector>
#include <memory>
#include <unordered_map>
#include "format_conversion.h"
#include "document.h"
#include "error_handler.h"
#include <openxml/openxml.h>
#include <pdf/pdflib.h>

// Define file extensions map
const std::unordered_map<std::string, FormatType> FILE_EXTENSIONS = {
    {"docx", FormatType::DOCX},
    {"pdf", FormatType::PDF},
    // Add more file extensions as needed
};

// Helper function to extract file extension
std::string getFileExtension(const std::string& filePath) {
    size_t dotPos = filePath.find_last_of('.');
    if (dotPos == std::string::npos) {
        return "";
    }
    std::string extension = filePath.substr(dotPos + 1);
    std::transform(extension.begin(), extension.end(), extension.begin(), ::tolower);
    return extension;
}

FormatConversion::FormatConversion() : m_errorHandler(std::make_shared<ErrorHandler>()) {
    // Initialize import filters for supported formats
    m_importFilters[FormatType::DOCX] = std::make_shared<DocxImportFilter>();
    m_importFilters[FormatType::PDF] = std::make_shared<PdfImportFilter>();

    // Initialize export filters for supported formats
    m_exportFilters[FormatType::DOCX] = std::make_shared<DocxExportFilter>();
    m_exportFilters[FormatType::PDF] = std::make_shared<PdfExportFilter>();
}

std::shared_ptr<Document> FormatConversion::importDocument(const std::string& filePath, FormatType format) {
    // Validate the file path and format
    if (filePath.empty() || format == FormatType::UNKNOWN) {
        m_errorHandler->logError("Invalid file path or format");
        return nullptr;
    }

    // Get the appropriate import filter for the format
    auto filterIt = m_importFilters.find(format);
    if (filterIt == m_importFilters.end()) {
        m_errorHandler->logError("Unsupported import format");
        return nullptr;
    }

    // Use the import filter to read the file and create a Document object
    try {
        return filterIt->second->import(filePath);
    } catch (const std::exception& e) {
        m_errorHandler->logError("Error importing document: " + std::string(e.what()));
        return nullptr;
    }
}

bool FormatConversion::exportDocument(const std::shared_ptr<Document>& document, const std::string& filePath, FormatType format) {
    // Validate the document, file path, and format
    if (!document || filePath.empty() || format == FormatType::UNKNOWN) {
        m_errorHandler->logError("Invalid document, file path, or format");
        return false;
    }

    // Get the appropriate export filter for the format
    auto filterIt = m_exportFilters.find(format);
    if (filterIt == m_exportFilters.end()) {
        m_errorHandler->logError("Unsupported export format");
        return false;
    }

    // Use the export filter to write the Document object to the file
    try {
        return filterIt->second->exportDoc(document, filePath);
    } catch (const std::exception& e) {
        m_errorHandler->logError("Error exporting document: " + std::string(e.what()));
        return false;
    }
}

FormatType FormatConversion::detectFormat(const std::string& filePath) {
    // Extract the file extension from the file path
    std::string extension = getFileExtension(filePath);

    // Look up the format type in the FILE_EXTENSIONS map
    auto it = FILE_EXTENSIONS.find(extension);
    if (it != FILE_EXTENSIONS.end()) {
        return it->second;
    }

    // If not found, attempt to detect format based on file content
    // This is a placeholder for more sophisticated format detection
    m_errorHandler->logWarning("Unable to detect format based on file extension");
    return FormatType::UNKNOWN;
}

void FormatConversion::registerImportFilter(FormatType format, std::shared_ptr<ImportFilter> filter) {
    // Validate the format and filter
    if (format == FormatType::UNKNOWN || !filter) {
        m_errorHandler->logError("Invalid format or filter for import registration");
        return;
    }

    // Add the filter to the m_importFilters map
    m_importFilters[format] = std::move(filter);
}

void FormatConversion::registerExportFilter(FormatType format, std::shared_ptr<ExportFilter> filter) {
    // Validate the format and filter
    if (format == FormatType::UNKNOWN || !filter) {
        m_errorHandler->logError("Invalid format or filter for export registration");
        return;
    }

    // Add the filter to the m_exportFilters map
    m_exportFilters[format] = std::move(filter);
}

// Human tasks:
// TODO: Implement robust error handling for unsupported formats
// TODO: Add support for additional document formats as needed
// TODO: Implement format-specific optimizations for large documents
// TODO: Add support for exporting specific document sections or pages