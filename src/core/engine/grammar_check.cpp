#include <string>
#include <vector>
#include <memory>
#include <languagetool/languagetool.h>
#include "grammar_check.h"
#include "language_manager.h"
#include "text_processing.h"

// Maximum number of suggestions to return for each grammar error
const int MAX_SUGGESTIONS = 5;

// Convert LanguageTool error to GrammarError object
GrammarError convertToGrammarError(const LanguageToolError& ltError) {
    // Extract error information from LanguageToolError
    std::string message = ltError.getMessage();
    int startPos = ltError.getStartPos();
    int endPos = ltError.getEndPos();
    std::string errorType = ltError.getErrorType();

    // Create and populate a new GrammarError object
    GrammarError grammarError;
    grammarError.message = message;
    grammarError.startPos = startPos;
    grammarError.endPos = endPos;
    grammarError.errorType = errorType;

    // Return the GrammarError object
    return grammarError;
}

GrammarCheck::GrammarCheck() {
    // Initialize m_languageManager with a new LanguageManager object
    m_languageManager = std::make_shared<LanguageManager>();

    // Initialize m_textProcessor with a new TextProcessing object
    m_textProcessor = std::make_shared<TextProcessing>();

    // Set m_currentLanguage to the default language
    m_currentLanguage = m_languageManager->getDefaultLanguage();

    // Initialize m_languageTool with the default language rules
    auto defaultRules = m_languageManager->getLanguageRules(m_currentLanguage);
    m_languageTool = std::make_shared<LanguageTool>(defaultRules);
}

std::vector<GrammarError> GrammarCheck::checkSentence(const std::string& sentence) {
    // Tokenize the input sentence using m_textProcessor
    std::vector<std::string> tokens = m_textProcessor->tokenizeSentence(sentence);

    // Use m_languageTool to check the tokenized sentence
    std::vector<LanguageToolError> ltErrors = m_languageTool->check(tokens);

    // Convert LanguageTool errors to GrammarError objects
    std::vector<GrammarError> grammarErrors;
    for (const auto& ltError : ltErrors) {
        grammarErrors.push_back(convertToGrammarError(ltError));
    }

    // Return the list of GrammarError objects
    return grammarErrors;
}

std::vector<std::string> GrammarCheck::getSuggestions(const GrammarError& error) {
    // Use m_languageTool to generate suggestions for the error
    std::vector<std::string> allSuggestions = m_languageTool->getSuggestions(error.message, error.startPos, error.endPos);

    // Limit the number of suggestions to MAX_SUGGESTIONS
    std::vector<std::string> limitedSuggestions;
    for (size_t i = 0; i < std::min(allSuggestions.size(), static_cast<size_t>(MAX_SUGGESTIONS)); ++i) {
        limitedSuggestions.push_back(allSuggestions[i]);
    }

    // Return the list of suggestions
    return limitedSuggestions;
}

bool GrammarCheck::setLanguage(const std::string& languageCode) {
    // Get the language rules for the specified language from m_languageManager
    auto languageRules = m_languageManager->getLanguageRules(languageCode);

    if (!languageRules.empty()) {
        // Create a new LanguageTool object with the language rules
        auto newLanguageTool = std::make_shared<LanguageTool>(languageRules);

        // If successful, update m_currentLanguage and m_languageTool
        m_currentLanguage = languageCode;
        m_languageTool = newLanguageTool;
        return true;
    }

    // Return the result of the operation
    return false;
}

std::vector<GrammarError> GrammarCheck::checkParagraph(const std::string& paragraph) {
    // Split the paragraph into sentences using m_textProcessor
    std::vector<std::string> sentences = m_textProcessor->splitIntoParagraphs(paragraph);

    // For each sentence, call checkSentence
    std::vector<GrammarError> allErrors;
    for (const auto& sentence : sentences) {
        auto sentenceErrors = checkSentence(sentence);
        
        // Aggregate all GrammarError objects from sentences
        allErrors.insert(allErrors.end(), sentenceErrors.begin(), sentenceErrors.end());
    }

    // Return the combined list of GrammarError objects
    return allErrors;
}