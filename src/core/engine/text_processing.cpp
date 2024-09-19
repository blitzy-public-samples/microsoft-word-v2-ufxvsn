#include <string>
#include <vector>
#include <unordered_map>
#include <memory>
#include "text_processing.h"
#include "spell_check.h"
#include "grammar_check.h"
#include "auto_correct.h"

const int MAX_PARAGRAPH_LENGTH = 1000; // Maximum number of characters in a paragraph

TextProcessing::TextProcessing() {
    // Initialize spell checker
    m_spellChecker = std::make_shared<SpellCheck>();

    // Initialize grammar checker
    m_grammarChecker = std::make_shared<GrammarCheck>();

    // Initialize auto-correct
    m_autoCorrect = std::make_shared<AutoCorrect>();

    // Load custom dictionary from file or database
    // TODO: Implement loading of custom dictionary
}

std::string TextProcessing::processText(std::string text) {
    // Apply auto-correct to the input text
    applyAutoCorrect(text);

    // Check spelling of the text
    auto spellingErrors = checkSpelling(text);

    // Check grammar of the text
    auto grammarErrors = checkGrammar(text);

    // Combine and apply suggestions from spell check and grammar check
    // TODO: Implement logic to combine and apply suggestions

    // Return the processed text
    return text;
}

std::vector<SpellingSuggestion> TextProcessing::checkSpelling(const std::string& text) {
    std::vector<SpellingSuggestion> suggestions;
    auto tokens = tokenizeText(text);

    for (const auto& word : tokens) {
        // Check if the word is in the custom dictionary
        if (m_customDictionary.find(word) != m_customDictionary.end()) {
            continue;
        }

        // Use spell checker to check the word
        auto wordSuggestions = m_spellChecker->checkWord(word);
        if (!wordSuggestions.empty()) {
            suggestions.insert(suggestions.end(), wordSuggestions.begin(), wordSuggestions.end());
        }
    }

    return suggestions;
}

std::vector<GrammarSuggestion> TextProcessing::checkGrammar(const std::string& text) {
    std::vector<GrammarSuggestion> suggestions;
    
    // Split the input text into sentences
    // TODO: Implement sentence splitting logic

    for (const auto& sentence : sentences) {
        // Use grammar checker to check the sentence
        auto sentenceSuggestions = m_grammarChecker->checkSentence(sentence);
        suggestions.insert(suggestions.end(), sentenceSuggestions.begin(), sentenceSuggestions.end());
    }

    return suggestions;
}

void TextProcessing::applyAutoCorrect(std::string& text) {
    auto tokens = tokenizeText(text);

    for (auto& word : tokens) {
        // Apply auto-correct rules using m_autoCorrect
        std::string correctedWord = m_autoCorrect->correct(word);
        if (correctedWord != word) {
            // Replace the original word with the corrected version in the text
            // TODO: Implement word replacement logic
        }
    }
}

bool TextProcessing::addToCustomDictionary(const std::string& word) {
    // Check if the word already exists in m_customDictionary
    if (m_customDictionary.find(word) != m_customDictionary.end()) {
        return false;
    }

    // Add the word to m_customDictionary
    m_customDictionary[word] = word;

    // Save the updated custom dictionary to file or database
    // TODO: Implement saving of custom dictionary

    return true;
}

std::vector<std::string> tokenizeText(const std::string& text) {
    std::vector<std::string> tokens;
    std::string currentToken;

    for (char c : text) {
        if (std::isalnum(c) || c == '\'') {
            // Add character to current token if it's alphanumeric or apostrophe
            currentToken += c;
        } else {
            // Add current token to tokens if it's not empty
            if (!currentToken.empty()) {
                tokens.push_back(currentToken);
                currentToken.clear();
            }
            // Add punctuation or whitespace as a separate token
            if (!std::isspace(c)) {
                tokens.push_back(std::string(1, c));
            }
        }
    }

    // Add the last token if it's not empty
    if (!currentToken.empty()) {
        tokens.push_back(currentToken);
    }

    return tokens;
}

// Human tasks:
// 1. Implement loading of custom dictionary in the constructor
// 2. Implement logic to combine and apply suggestions in processText function
// 3. Implement sentence splitting logic in checkGrammar function
// 4. Implement word replacement logic in applyAutoCorrect function
// 5. Implement saving of custom dictionary in addToCustomDictionary function