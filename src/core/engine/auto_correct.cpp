#include <string>
#include <vector>
#include <unordered_map>
#include <memory>
#include "auto_correct.h"
#include "language_manager.h"
#include "user_preferences.h"
#include "text_processing.h"

const int MAX_AUTOCORRECT_LENGTH = 50;

AutoCorrect::AutoCorrect() {
    // Initialize components
    m_languageManager = std::make_shared<LanguageManager>();
    m_userPreferences = std::make_shared<UserPreferences>();
    m_textProcessor = std::make_shared<TextProcessing>();

    // Load default auto-correct rules
    // TODO: Implement loading of default rules from a configuration file

    // Load user-defined auto-correct rules from preferences
    auto userRules = m_userPreferences->getAutoCorrectRules();
    m_autoCorrectRules.insert(userRules.begin(), userRules.end());
}

void AutoCorrect::applyAutoCorrect(std::string& text) {
    // Tokenize the input text
    std::vector<std::string> tokens = m_textProcessor->tokenizeText(text);

    // Apply auto-correct rules to each token
    for (auto& token : tokens) {
        auto it = m_autoCorrectRules.find(token);
        if (it != m_autoCorrectRules.end()) {
            token = it->second;
        }
    }

    // Reconstruct the text from modified tokens
    text = "";
    for (const auto& token : tokens) {
        text += token + " ";
    }
    // Remove the trailing space
    if (!text.empty()) {
        text.pop_back();
    }
}

bool AutoCorrect::addAutoCorrectRule(const std::string& incorrect, const std::string& correct) {
    // Validate input strings
    if (incorrect.empty() || correct.empty() || 
        incorrect.length() > MAX_AUTOCORRECT_LENGTH || 
        correct.length() > MAX_AUTOCORRECT_LENGTH) {
        return false;
    }

    // Add the rule to m_autoCorrectRules
    m_autoCorrectRules[incorrect] = correct;

    // Update user preferences with the new rule
    m_userPreferences->addAutoCorrectRule(incorrect, correct);

    return true;
}

bool AutoCorrect::removeAutoCorrectRule(const std::string& incorrect) {
    // Check if the rule exists
    auto it = m_autoCorrectRules.find(incorrect);
    if (it == m_autoCorrectRules.end()) {
        return false;
    }

    // Remove the rule from m_autoCorrectRules
    m_autoCorrectRules.erase(it);

    // Update user preferences to remove the rule
    m_userPreferences->removeAutoCorrectRule(incorrect);

    return true;
}

void AutoCorrect::loadLanguageSpecificRules(const std::string& languageCode) {
    // Get language-specific auto-correct rules
    auto languageRules = m_languageManager->getAutoCorrectRules(languageCode);

    // Merge language-specific rules with m_autoCorrectRules
    for (const auto& rule : languageRules) {
        // Prioritize user-defined rules over language-specific rules
        if (m_autoCorrectRules.find(rule.first) == m_autoCorrectRules.end()) {
            m_autoCorrectRules[rule.first] = rule.second;
        }
    }
}

std::unordered_map<std::string, std::string> AutoCorrect::getAutoCorrectRules() const {
    return m_autoCorrectRules;
}

std::string sanitizeAutoCorrectInput(const std::string& input) {
    std::string sanitized = input;

    // Trim leading and trailing whitespace
    sanitized.erase(0, sanitized.find_first_not_of(" \t\n\r\f\v"));
    sanitized.erase(sanitized.find_last_not_of(" \t\n\r\f\v") + 1);

    // Convert to lowercase
    std::transform(sanitized.begin(), sanitized.end(), sanitized.begin(),
                   [](unsigned char c){ return std::tolower(c); });

    // Remove any non-alphanumeric characters
    sanitized.erase(std::remove_if(sanitized.begin(), sanitized.end(),
                                   [](unsigned char c){ return !std::isalnum(c); }),
                    sanitized.end());

    // Truncate to MAX_AUTOCORRECT_LENGTH if necessary
    if (sanitized.length() > MAX_AUTOCORRECT_LENGTH) {
        sanitized = sanitized.substr(0, MAX_AUTOCORRECT_LENGTH);
    }

    return sanitized;
}