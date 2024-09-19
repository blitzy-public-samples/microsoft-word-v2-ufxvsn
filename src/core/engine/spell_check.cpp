#include <string>
#include <vector>
#include <unordered_map>
#include <memory>
#include <hunspell/hunspell.hxx>
#include "spell_check.h"
#include "language_manager.h"
#include "custom_dictionary.h"

// Maximum number of spelling suggestions to return
const int MAX_SUGGESTIONS = 5;

SpellCheck::SpellCheck() {
    // Initialize language manager
    m_languageManager = std::make_shared<LanguageManager>();

    // Initialize custom dictionary
    m_customDictionary = std::make_shared<CustomDictionary>();

    // Set default language
    m_currentLanguage = m_languageManager->getDefaultLanguage();

    // Initialize Hunspell with default language dictionary
    std::string dictionaryPath = m_languageManager->getDictionaryPath(m_currentLanguage);
    m_hunspell = std::make_shared<Hunspell>(dictionaryPath + ".aff", dictionaryPath + ".dic");
}

bool SpellCheck::checkWord(const std::string& word) {
    // Check if the word is in the custom dictionary
    if (m_customDictionary->contains(word)) {
        return true;
    }

    // Use Hunspell to check the word
    return m_hunspell->spell(word);
}

std::vector<std::string> SpellCheck::getSuggestions(const std::string& word) {
    // Use Hunspell to generate suggestions for the word
    std::vector<std::string> suggestions = m_hunspell->suggest(word);

    // Limit the number of suggestions to MAX_SUGGESTIONS
    if (suggestions.size() > MAX_SUGGESTIONS) {
        suggestions.resize(MAX_SUGGESTIONS);
    }

    return suggestions;
}

bool SpellCheck::setLanguage(const std::string& languageCode) {
    // Get the dictionary path for the specified language
    std::string dictionaryPath = m_languageManager->getDictionaryPath(languageCode);

    // Create a new Hunspell object with the dictionary files
    std::shared_ptr<Hunspell> newHunspell = std::make_shared<Hunspell>(dictionaryPath + ".aff", dictionaryPath + ".dic");

    // If successful, update m_currentLanguage and m_hunspell
    if (newHunspell) {
        m_currentLanguage = languageCode;
        m_hunspell = newHunspell;
        return true;
    }

    return false;
}

bool SpellCheck::addToCustomDictionary(const std::string& word) {
    // Call m_customDictionary to add the word
    return m_customDictionary->addWord(word);
}

std::string normalizeWord(const std::string& word) {
    std::string normalized = word;

    // Convert the word to lowercase
    std::transform(normalized.begin(), normalized.end(), normalized.begin(), ::tolower);

    // Remove any leading or trailing whitespace
    normalized.erase(0, normalized.find_first_not_of(" \t\n\r\f\v"));
    normalized.erase(normalized.find_last_not_of(" \t\n\r\f\v") + 1);

    // Remove any non-alphabetic characters
    normalized.erase(std::remove_if(normalized.begin(), normalized.end(), [](char c) { return !std::isalpha(c); }), normalized.end());

    return normalized;
}