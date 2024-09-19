#include <catch2/catch.hpp>
#include "../../src/core/engine/text_processing.h"
#include <string>
#include <vector>

// Helper function to create sample text for testing
std::string createSampleText() {
    return "The quick brown fox jumps over the lazy dog.";
}

// Helper function to apply formatting to a given text range
void applyFormatting(TextProcessing& textProcessor, int start, int end, FormattingType type) {
    textProcessor.applyFormatting(start, end, type);
}

TEST_CASE("TextProcessing", "[text_processing]") {
    SECTION("BasicTextOperations") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Test inserting text
        textProcessor.insertText(0, "Hello, World!");
        REQUIRE(textProcessor.getText() == "Hello, World!");

        // Test deleting text
        textProcessor.deleteText(5, 7);
        REQUIRE(textProcessor.getText() == "Hello World!");

        // Test replacing text
        textProcessor.replaceText(6, 11, "Universe");
        REQUIRE(textProcessor.getText() == "Hello Universe!");

        // Verify the final text content
        REQUIRE(textProcessor.getText() == "Hello Universe!");
    }

    SECTION("SpellChecking") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Insert text with intentional misspellings
        textProcessor.insertText(0, "The quik brown fox jumpps over the lasy dog.");

        // Run spell check
        auto misspelledWords = textProcessor.spellCheck();

        // Verify that misspelled words are identified
        REQUIRE(misspelledWords.size() == 3);
        REQUIRE(std::find(misspelledWords.begin(), misspelledWords.end(), "quik") != misspelledWords.end());
        REQUIRE(std::find(misspelledWords.begin(), misspelledWords.end(), "jumpps") != misspelledWords.end());
        REQUIRE(std::find(misspelledWords.begin(), misspelledWords.end(), "lasy") != misspelledWords.end());

        // Verify that correct words are not flagged
        REQUIRE(std::find(misspelledWords.begin(), misspelledWords.end(), "brown") == misspelledWords.end());
        REQUIRE(std::find(misspelledWords.begin(), misspelledWords.end(), "fox") == misspelledWords.end());
    }

    SECTION("GrammarChecking") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Insert text with intentional grammar errors
        textProcessor.insertText(0, "The quick brown fox jump over the lazy dog. He don't like it.");

        // Run grammar check
        auto grammarErrors = textProcessor.grammarCheck();

        // Verify that grammar errors are identified
        REQUIRE(grammarErrors.size() == 2);
        REQUIRE(std::find(grammarErrors.begin(), grammarErrors.end(), "Subject-verb agreement: 'fox jump'") != grammarErrors.end());
        REQUIRE(std::find(grammarErrors.begin(), grammarErrors.end(), "Incorrect verb form: 'don't' should be 'doesn't'") != grammarErrors.end());

        // Verify that correct sentences are not flagged
        REQUIRE(grammarErrors.size() == 2);
    }

    SECTION("AutoCorrect") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Set up auto-correct rules
        textProcessor.addAutoCorrectRule("teh", "the");
        textProcessor.addAutoCorrectRule("dont", "don't");

        // Insert text with words that should be auto-corrected
        textProcessor.insertText(0, "Teh quick brown fox dont jump over teh lazy dog.");

        // Apply auto-correct
        textProcessor.applyAutoCorrect();

        // Verify that words are correctly auto-corrected
        REQUIRE(textProcessor.getText() == "The quick brown fox don't jump over the lazy dog.");
    }

    SECTION("Formatting") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Insert plain text
        textProcessor.insertText(0, createSampleText());

        // Apply bold formatting to a section
        applyFormatting(textProcessor, 4, 9, FormattingType::Bold);

        // Apply italic formatting to a section
        applyFormatting(textProcessor, 10, 15, FormattingType::Italic);

        // Apply underline formatting to a section
        applyFormatting(textProcessor, 16, 19, FormattingType::Underline);

        // Verify that formatting is correctly applied
        auto formattedRanges = textProcessor.getFormattedRanges();
        REQUIRE(formattedRanges.size() == 3);
        REQUIRE(formattedRanges[0] == std::make_tuple(4, 9, FormattingType::Bold));
        REQUIRE(formattedRanges[1] == std::make_tuple(10, 15, FormattingType::Italic));
        REQUIRE(formattedRanges[2] == std::make_tuple(16, 19, FormattingType::Underline));
    }

    SECTION("Undo_Redo") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Perform a series of text operations
        textProcessor.insertText(0, "Hello");
        textProcessor.insertText(5, " World");
        textProcessor.insertText(11, "!");

        // Undo several operations
        textProcessor.undo();
        textProcessor.undo();

        // Verify text state after undo
        REQUIRE(textProcessor.getText() == "Hello");

        // Redo operations
        textProcessor.redo();
        textProcessor.redo();

        // Verify text state after redo
        REQUIRE(textProcessor.getText() == "Hello World!");
    }

    SECTION("LargeTextHandling") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Insert a very large amount of text (e.g., 1 million characters)
        std::string largeText(1000000, 'a');
        textProcessor.insertText(0, largeText);

        // Perform various operations on the large text
        textProcessor.insertText(500000, "middle");
        textProcessor.deleteText(250000, 250010);
        textProcessor.replaceText(750000, 750010, "replaced");

        // Verify performance and correctness of operations
        REQUIRE(textProcessor.getText().length() == 1000001);
        REQUIRE(textProcessor.getText().substr(500000, 6) == "middle");
        REQUIRE(textProcessor.getText().substr(750000, 8) == "replaced");
    }

    SECTION("UnicodeSupport") {
        // Create a TextProcessing object
        TextProcessing textProcessor;

        // Insert text with various Unicode characters
        textProcessor.insertText(0, "Hello, 世界! 😊 Здравствуй, мир!");

        // Perform operations on Unicode text
        textProcessor.deleteText(7, 9);
        textProcessor.insertText(7, "🌍");

        // Verify correct handling of Unicode characters
        REQUIRE(textProcessor.getText() == "Hello, 🌍! 😊 Здравствуй, мир!");
        REQUIRE(textProcessor.getText().length() == 25);
    }
}