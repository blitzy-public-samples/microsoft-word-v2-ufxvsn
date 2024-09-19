#include <string>
#include <vector>
#include <memory>
#include <unordered_map>
#include "rendering_engine.h"
#include "display_composition.h"
#include "print_composition.h"
#include "formatting_engine.h"
#include "layout_engine.h"

// Global constants
const int DEFAULT_DPI = 96;
const float DEFAULT_ZOOM_LEVEL = 1.0f;

// Helper function to calculate scale factor
float calculateScaleFactor(float zoom, int dpi) {
    // Validate input zoom and dpi values
    if (zoom <= 0 || dpi <= 0) {
        throw std::invalid_argument("Zoom and DPI must be positive values");
    }

    // Calculate the scale factor using the formula: zoom * (dpi / DEFAULT_DPI)
    return zoom * (static_cast<float>(dpi) / DEFAULT_DPI);
}

RenderingEngine::RenderingEngine()
    : m_displayComposer(std::make_shared<DisplayComposition>()),
      m_printComposer(std::make_shared<PrintComposition>()),
      m_formattingEngine(std::make_shared<FormattingEngine>()),
      m_layoutEngine(std::make_shared<LayoutEngine>()),
      m_currentZoom(DEFAULT_ZOOM_LEVEL),
      m_currentDPI(DEFAULT_DPI) {
    // Constructor implementation
}

std::shared_ptr<RenderedPage> RenderingEngine::renderPage(int pageNumber, const RenderContext& context) {
    // Retrieve the page layout from m_layoutEngine
    auto pageLayout = m_layoutEngine->getPageLayout(pageNumber);

    // Apply current zoom level to the layout
    float scaleFactor = calculateScaleFactor(m_currentZoom, m_currentDPI);
    pageLayout->applyScale(scaleFactor);

    // Call m_displayComposer to compose the page
    auto renderedPage = m_displayComposer->composePage(pageLayout, context);

    // Apply any context-specific rendering options
    renderedPage->applyRenderingOptions(context.getRenderingOptions());

    // Return the rendered page object
    return renderedPage;
}

std::shared_ptr<PrintablePage> RenderingEngine::renderPrintPage(int pageNumber, const PrintContext& context) {
    // Retrieve the page layout from m_layoutEngine
    auto pageLayout = m_layoutEngine->getPageLayout(pageNumber);

    // Apply print-specific adjustments to the layout
    pageLayout->adjustForPrinting(context.getPrintSettings());

    // Call m_printComposer to compose the page for printing
    auto printablePage = m_printComposer->composePage(pageLayout, context);

    // Apply any context-specific print options
    printablePage->applyPrintOptions(context.getPrintOptions());

    // Return the printable page object
    return printablePage;
}

void RenderingEngine::updateZoom(float zoomLevel) {
    // Validate the input zoomLevel
    if (zoomLevel <= 0) {
        throw std::invalid_argument("Zoom level must be a positive value");
    }

    // Update m_currentZoom with the new zoom level
    m_currentZoom = zoomLevel;

    // Trigger a re-render of the current view
    triggerRerender();
}

void RenderingEngine::updateDPI(int dpi) {
    // Validate the input dpi
    if (dpi <= 0) {
        throw std::invalid_argument("DPI must be a positive value");
    }

    // Update m_currentDPI with the new DPI value
    m_currentDPI = dpi;

    // Trigger a re-render of the current view
    triggerRerender();
}

std::shared_ptr<Thumbnail> RenderingEngine::renderThumbnail(int pageNumber, const ThumbnailSize& size) {
    // Retrieve the page layout from m_layoutEngine
    auto pageLayout = m_layoutEngine->getPageLayout(pageNumber);

    // Scale the layout to the requested thumbnail size
    pageLayout->scaleToThumbnail(size);

    // Call m_displayComposer to compose the thumbnail
    auto thumbnail = m_displayComposer->composeThumbnail(pageLayout, size);

    // Return the generated thumbnail object
    return thumbnail;
}

// Helper function to trigger a re-render (not specified in the original JSON, but necessary for updateZoom and updateDPI)
void RenderingEngine::triggerRerender() {
    // Implementation to trigger a re-render of the current view
    // This could involve notifying observers or calling a specific update function
}