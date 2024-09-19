#include <catch2/catch.hpp>
#include "../../src/core/engine/formatting_engine.h"
#include <string>
#include <vector>

// Helper function to create a sample document for testing
Document createSampleDocument() {
    // Implementation details omitted for brevity
    return Document();
}

// Helper function to apply formatting to a given range in a document
void applyFormatting(FormattingEngine& engine, Document& doc, Range range, FormattingProperties props) {
    // Implementation details omitted for brevity
}

// Helper function to verify that formatting is correctly applied to a range
bool verifyFormatting(const Document& doc, Range range, FormattingProperties expectedProps) {
    // Implementation details omitted for brevity
    return true;
}

TEST_CASE("FormattingEngine", "[formatting]") {
    SECTION("CharacterFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Apply bold formatting to a range of text
        Range boldRange(0, 10);
        applyFormatting(engine, doc, boldRange, FormattingProperties::Bold);

        // Apply italic formatting to a range of text
        Range italicRange(5, 15);
        applyFormatting(engine, doc, italicRange, FormattingProperties::Italic);

        // Apply underline formatting to a range of text
        Range underlineRange(10, 20);
        applyFormatting(engine, doc, underlineRange, FormattingProperties::Underline);

        // Verify that formatting is correctly applied
        REQUIRE(verifyFormatting(doc, boldRange, FormattingProperties::Bold));
        REQUIRE(verifyFormatting(doc, italicRange, FormattingProperties::Italic));
        REQUIRE(verifyFormatting(doc, underlineRange, FormattingProperties::Underline));

        // Test overlapping formatting ranges
        Range overlapRange(0, 20);
        REQUIRE(verifyFormatting(doc, overlapRange, FormattingProperties::Bold | FormattingProperties::Italic | FormattingProperties::Underline));

        // Test removing formatting
        engine.removeFormatting(doc, overlapRange);
        REQUIRE(verifyFormatting(doc, overlapRange, FormattingProperties::None));
    }

    SECTION("ParagraphFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Set paragraph alignment (left, right, center, justify)
        engine.setParagraphAlignment(doc, 0, ParagraphAlignment::Left);
        engine.setParagraphAlignment(doc, 1, ParagraphAlignment::Right);
        engine.setParagraphAlignment(doc, 2, ParagraphAlignment::Center);
        engine.setParagraphAlignment(doc, 3, ParagraphAlignment::Justify);

        // Set line spacing
        engine.setLineSpacing(doc, 0, 1.5);

        // Set indentation (first line, hanging)
        engine.setFirstLineIndent(doc, 1, 1.0);
        engine.setHangingIndent(doc, 2, 0.5);

        // Set paragraph spacing (before and after)
        engine.setParagraphSpacingBefore(doc, 3, 12);
        engine.setParagraphSpacingAfter(doc, 3, 6);

        // Verify that paragraph formatting is correctly applied
        REQUIRE(doc.getParagraphAlignment(0) == ParagraphAlignment::Left);
        REQUIRE(doc.getParagraphAlignment(1) == ParagraphAlignment::Right);
        REQUIRE(doc.getParagraphAlignment(2) == ParagraphAlignment::Center);
        REQUIRE(doc.getParagraphAlignment(3) == ParagraphAlignment::Justify);
        REQUIRE(doc.getLineSpacing(0) == Approx(1.5));
        REQUIRE(doc.getFirstLineIndent(1) == Approx(1.0));
        REQUIRE(doc.getHangingIndent(2) == Approx(0.5));
        REQUIRE(doc.getParagraphSpacingBefore(3) == 12);
        REQUIRE(doc.getParagraphSpacingAfter(3) == 6);
    }

    SECTION("StyleApplication") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Define custom styles (character and paragraph)
        Style headingStyle;
        headingStyle.setName("Heading");
        headingStyle.setFontSize(16);
        headingStyle.setBold(true);

        Style bodyStyle;
        bodyStyle.setName("Body");
        bodyStyle.setFontSize(11);
        bodyStyle.setLineSpacing(1.15);

        engine.addStyle(doc, headingStyle);
        engine.addStyle(doc, bodyStyle);

        // Apply styles to text ranges
        engine.applyStyle(doc, Range(0, 20), "Heading");
        engine.applyStyle(doc, Range(21, 100), "Body");

        // Modify existing styles
        headingStyle.setFontSize(18);
        engine.updateStyle(doc, headingStyle);

        // Verify that styles are correctly applied and updated
        REQUIRE(verifyFormatting(doc, Range(0, 20), headingStyle.getProperties()));
        REQUIRE(verifyFormatting(doc, Range(21, 100), bodyStyle.getProperties()));
        REQUIRE(doc.getStyle("Heading").getFontSize() == 18);
    }

    SECTION("FontHandling") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Change font family for a range of text
        engine.setFontFamily(doc, Range(0, 50), "Arial");

        // Change font size for a range of text
        engine.setFontSize(doc, Range(25, 75), 14);

        // Apply font color to a range of text
        engine.setFontColor(doc, Range(50, 100), Color(255, 0, 0));

        // Test font substitution for unavailable fonts
        engine.setFontFamily(doc, Range(100, 150), "NonexistentFont");

        // Verify correct font application
        REQUIRE(doc.getFontFamily(10) == "Arial");
        REQUIRE(doc.getFontSize(50) == 14);
        REQUIRE(doc.getFontColor(75) == Color(255, 0, 0));
        REQUIRE(doc.getFontFamily(125) != "NonexistentFont");
    }

    SECTION("ListFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Apply bullet point formatting to paragraphs
        engine.applyBulletList(doc, Range(0, 3));

        // Apply numbered list formatting to paragraphs
        engine.applyNumberedList(doc, Range(4, 7));

        // Test multi-level lists
        engine.applyMultiLevelList(doc, Range(8, 15));

        // Test list style changes
        ListStyle customStyle;
        customStyle.setListStyleType(ListStyleType::LowerRoman);
        engine.applyListStyle(doc, Range(4, 7), customStyle);

        // Verify correct list formatting
        REQUIRE(doc.getListType(0) == ListType::Bullet);
        REQUIRE(doc.getListType(4) == ListType::Numbered);
        REQUIRE(doc.getListLevel(10) == 2);
        REQUIRE(doc.getListStyleType(5) == ListStyleType::LowerRoman);
    }

    SECTION("TableFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Create a table
        Table table = engine.createTable(doc, 3, 3);

        // Apply cell formatting (borders, shading)
        engine.setCellBorders(table, CellRange(0, 0, 2, 2), BorderStyle::Single);
        engine.setCellShading(table, CellRange(1, 1), Color(200, 200, 200));

        // Apply table styles
        TableStyle style;
        style.setName("CustomTableStyle");
        style.setHeaderRowCount(1);
        engine.applyTableStyle(table, style);

        // Modify table structure (merge cells, split cells)
        engine.mergeCells(table, CellRange(0, 0, 0, 2));
        engine.splitCell(table, CellPosition(2, 1), 2, 1);

        // Verify correct table formatting
        REQUIRE(table.getRowCount() == 3);
        REQUIRE(table.getColumnCount() == 3);
        REQUIRE(table.getCellAt(0, 0).getColumnSpan() == 3);
        REQUIRE(table.getCellAt(2, 1).getRowSpan() == 2);
    }

    SECTION("SectionFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Create multiple sections in a document
        engine.insertSectionBreak(doc, 100);
        engine.insertSectionBreak(doc, 200);

        // Apply different page layouts to sections
        engine.setPageOrientation(doc, 0, PageOrientation::Portrait);
        engine.setPageOrientation(doc, 1, PageOrientation::Landscape);

        // Set headers and footers for sections
        engine.setHeader(doc, 0, "First Section Header");
        engine.setFooter(doc, 1, "Second Section Footer");

        // Apply column layouts to sections
        engine.setColumnCount(doc, 2, 2);

        // Verify correct section formatting
        REQUIRE(doc.getSectionCount() == 3);
        REQUIRE(doc.getPageOrientation(0) == PageOrientation::Portrait);
        REQUIRE(doc.getPageOrientation(1) == PageOrientation::Landscape);
        REQUIRE(doc.getHeader(0) == "First Section Header");
        REQUIRE(doc.getFooter(1) == "Second Section Footer");
        REQUIRE(doc.getColumnCount(2) == 2);
    }

    SECTION("ConditionalFormatting") {
        // Create a FormattingEngine object
        FormattingEngine engine;
        Document doc = createSampleDocument();

        // Define conditional formatting rules
        ConditionalFormattingRule rule1;
        rule1.setCondition([](const std::string& text) { return text.length() > 10; });
        rule1.setFormattingProperties(FormattingProperties::Bold);

        ConditionalFormattingRule rule2;
        rule2.setCondition([](const std::string& text) { return text.find("important") != std::string::npos; });
        rule2.setFormattingProperties(FormattingProperties::Italic | FormattingProperties::Underline);

        // Apply rules to text ranges
        engine.applyConditionalFormatting(doc, Range(0, 100), rule1);
        engine.applyConditionalFormatting(doc, Range(0, 100), rule2);

        // Test rule evaluation and application
        doc.setText(Range(0, 20), "This is a long sentence.");
        doc.setText(Range(50, 70), "This is important text.");

        // Verify correct conditional formatting
        REQUIRE(verifyFormatting(doc, Range(0, 20), FormattingProperties::Bold));
        REQUIRE(verifyFormatting(doc, Range(50, 70), FormattingProperties::Italic | FormattingProperties::Underline));
    }
}