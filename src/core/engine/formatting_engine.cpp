#include <string>
#include <vector>
#include <memory>
#include <unordered_map>
#include "formatting_engine.h"
#include "style_management.h"
#include "layout_engine.h"

// Global constants
const int DEFAULT_FONT_SIZE = 12;
const std::string DEFAULT_FONT_FAMILY = "Arial";

// Main class for handling document and text formatting operations
FormattingEngine::FormattingEngine() {
    // Initialize m_styleManager with a new StyleManagement object
    m_styleManager = std::make_shared<StyleManagement>();

    // Initialize m_layoutEngine with a new LayoutEngine object
    m_layoutEngine = std::make_shared<LayoutEngine>();

    // Initialize m_fontCache with default system fonts
    // This is a placeholder implementation and should be replaced with actual system font loading
    m_fontCache["Arial"] = FontProperties{/* ... */};
    m_fontCache["Times New Roman"] = FontProperties{/* ... */};
    m_fontCache["Calibri"] = FontProperties{/* ... */};
}

bool FormattingEngine::applyTextFormatting(TextRange range, TextFormatProperties properties) {
    // Validate the input TextRange and TextFormatProperties
    if (!isValidTextRange(range) || !isValidTextFormatProperties(properties)) {
        return false;
    }

    // Apply the formatting properties to the text range using m_styleManager
    bool styleApplied = m_styleManager->applyStyle(range, properties);
    if (!styleApplied) {
        return false;
    }

    // Update the document layout using m_layoutEngine
    m_layoutEngine->updateLayout();

    // Return the result of the operation
    return true;
}

bool FormattingEngine::applyParagraphFormatting(ParagraphRange range, ParagraphFormatProperties properties) {
    // Validate the input ParagraphRange and ParagraphFormatProperties
    if (!isValidParagraphRange(range) || !isValidParagraphFormatProperties(properties)) {
        return false;
    }

    // Apply the formatting properties to the paragraph range using m_styleManager
    bool styleApplied = m_styleManager->applyParagraphStyle(range, properties);
    if (!styleApplied) {
        return false;
    }

    // Update the document layout using m_layoutEngine
    m_layoutEngine->updateLayout();

    // Return the result of the operation
    return true;
}

bool FormattingEngine::applyStyle(DocumentRange range, StyleID styleId) {
    // Retrieve the style properties using m_styleManager
    StyleProperties styleProperties = m_styleManager->getStyle(styleId);
    if (styleProperties.isEmpty()) {
        return false;
    }

    // Determine if the style is for text or paragraph formatting
    if (styleProperties.isTextStyle()) {
        // Call applyTextFormatting for text styles
        return applyTextFormatting(range.toTextRange(), styleProperties.getTextProperties());
    } else if (styleProperties.isParagraphStyle()) {
        // Call applyParagraphFormatting for paragraph styles
        return applyParagraphFormatting(range.toParagraphRange(), styleProperties.getParagraphProperties());
    }

    // Return false if the style type is unknown
    return false;
}

StyleID FormattingEngine::createCustomStyle(StyleProperties properties, std::string styleName) {
    // Validate the input StyleProperties and styleName
    if (!isValidStyleProperties(properties) || styleName.empty()) {
        return InvalidStyleID;
    }

    // Create a new style using m_styleManager
    StyleID newStyleId = m_styleManager->createStyle(properties, styleName);

    // Return the ID of the newly created style
    return newStyleId;
}

void FormattingEngine::updateDocumentLayout() {
    // Call m_layoutEngine to update the entire document layout
    m_layoutEngine->updateLayout();

    // Refresh the document view to reflect changes
    refreshDocumentView();
}

FontProperties FormattingEngine::getFontProperties(const std::string& fontName) {
    // Check if the font properties are in m_fontCache
    auto it = m_fontCache.find(fontName);
    if (it != m_fontCache.end()) {
        // If found, return the cached properties
        return it->second;
    }

    // If not found, load the font properties from the system
    FontProperties properties = loadFontPropertiesFromSystem(fontName);

    // Cache the loaded properties in m_fontCache
    m_fontCache[fontName] = properties;

    // Return the font properties
    return properties;
}

// Helper functions (not part of the class interface)

bool isValidTextRange(const TextRange& range) {
    // Implement validation logic for TextRange
    return true; // Placeholder
}

bool isValidTextFormatProperties(const TextFormatProperties& properties) {
    // Implement validation logic for TextFormatProperties
    return true; // Placeholder
}

bool isValidParagraphRange(const ParagraphRange& range) {
    // Implement validation logic for ParagraphRange
    return true; // Placeholder
}

bool isValidParagraphFormatProperties(const ParagraphFormatProperties& properties) {
    // Implement validation logic for ParagraphFormatProperties
    return true; // Placeholder
}

bool isValidStyleProperties(const StyleProperties& properties) {
    // Implement validation logic for StyleProperties
    return true; // Placeholder
}

FontProperties loadFontPropertiesFromSystem(const std::string& fontName) {
    // Implement logic to load font properties from the system
    return FontProperties{}; // Placeholder
}

void refreshDocumentView() {
    // Implement logic to refresh the document view
}