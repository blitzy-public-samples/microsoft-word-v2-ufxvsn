import Cocoa
import MicrosoftWordCore

class StatusBar: NSView {
    // MARK: - Properties
    var documentViewModel: DocumentViewModel
    var pageInfoLabel: NSTextField
    var wordCountLabel: NSTextField
    var languageLabel: NSTextField
    var documentStateLabel: NSTextField
    var zoomControl: NSPopUpButton
    var viewModeButton: NSButton

    // MARK: - Initialization
    init(documentViewModel: DocumentViewModel) {
        self.documentViewModel = documentViewModel
        
        // Initialize UI components
        self.pageInfoLabel = NSTextField()
        self.wordCountLabel = NSTextField()
        self.languageLabel = NSTextField()
        self.documentStateLabel = NSTextField()
        self.zoomControl = NSPopUpButton()
        self.viewModeButton = NSButton()
        
        super.init(frame: .zero)
        
        setupUI()
        setupBindings()
    }
    
    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }

    // MARK: - UI Setup
    private func setupUI() {
        // Configure pageInfoLabel
        pageInfoLabel.isEditable = false
        pageInfoLabel.isBordered = false
        pageInfoLabel.backgroundColor = .clear
        
        // Configure wordCountLabel
        wordCountLabel.isEditable = false
        wordCountLabel.isBordered = false
        wordCountLabel.backgroundColor = .clear
        
        // Configure languageLabel
        languageLabel.isEditable = false
        languageLabel.isBordered = false
        languageLabel.backgroundColor = .clear
        
        // Configure documentStateLabel
        documentStateLabel.isEditable = false
        documentStateLabel.isBordered = false
        documentStateLabel.backgroundColor = .clear
        
        // Configure zoomControl
        zoomControl.pullsDown = false
        zoomControl.target = self
        zoomControl.action = #selector(handleZoomChange(_:))
        
        // Configure viewModeButton
        viewModeButton.bezelStyle = .texturedRounded
        viewModeButton.target = self
        viewModeButton.action = #selector(toggleViewMode(_:))
        
        // Add components to the view
        addSubview(pageInfoLabel)
        addSubview(wordCountLabel)
        addSubview(languageLabel)
        addSubview(documentStateLabel)
        addSubview(zoomControl)
        addSubview(viewModeButton)
        
        // Set up Auto Layout constraints
        // (Constraints setup code would go here)
    }

    // MARK: - Bindings
    private func setupBindings() {
        // Bind pageInfoLabel
        documentViewModel.pageInfo.bind(to: pageInfoLabel, \.stringValue)
        
        // Bind wordCountLabel
        documentViewModel.wordCount.bind(to: wordCountLabel, \.stringValue)
        
        // Bind languageLabel
        documentViewModel.currentLanguage.bind(to: languageLabel, \.stringValue)
        
        // Bind documentStateLabel
        documentViewModel.documentState.bind(to: documentStateLabel, \.stringValue)
        
        // Bind zoomControl
        documentViewModel.currentZoom.bind(to: zoomControl, \.selectedItem)
        
        // Set up action for viewModeButton
        viewModeButton.target = self
        viewModeButton.action = #selector(toggleViewMode(_:))
    }

    // MARK: - Update Methods
    func updatePageInfo() {
        let currentPage = documentViewModel.currentPageNumber
        let totalPages = documentViewModel.totalPages
        pageInfoLabel.stringValue = "Page \(currentPage) of \(totalPages)"
    }

    func updateWordCount() {
        let wordCount = documentViewModel.currentWordCount
        wordCountLabel.stringValue = "Words: \(wordCount)"
    }

    func updateLanguage() {
        let currentLanguage = documentViewModel.currentLanguage
        languageLabel.stringValue = "Language: \(currentLanguage)"
    }

    func updateDocumentState() {
        let state = documentViewModel.documentState
        documentStateLabel.stringValue = state
    }

    // MARK: - Action Handlers
    @objc func handleZoomChange(_ sender: NSPopUpButton) {
        guard let zoomLevel = sender.selectedItem?.title else { return }
        documentViewModel.setZoomLevel(zoomLevel)
        // Update UI to reflect new zoom level
    }

    @objc func toggleViewMode(_ sender: NSButton) {
        let nextViewMode = documentViewModel.getNextViewMode()
        documentViewModel.setViewMode(nextViewMode)
        sender.title = nextViewMode
    }

    // MARK: - Refresh
    func refreshStatusBar() {
        updatePageInfo()
        updateWordCount()
        updateLanguage()
        updateDocumentState()
        zoomControl.selectItem(withTitle: documentViewModel.currentZoom)
        viewModeButton.title = documentViewModel.currentViewMode
    }
}

extension StatusBar: NSMenuDelegate {
    func menuNeedsUpdate(_ menu: NSMenu) {
        // Clear existing items
        menu.removeAllItems()
        
        // Get available zoom levels from documentViewModel
        let zoomLevels = documentViewModel.availableZoomLevels
        
        // Add menu items for each zoom level
        for zoomLevel in zoomLevels {
            let item = NSMenuItem(title: zoomLevel, action: #selector(handleZoomChange(_:)), keyEquivalent: "")
            item.target = self
            menu.addItem(item)
            
            // Mark current zoom level as selected
            if zoomLevel == documentViewModel.currentZoom {
                item.state = .on
            }
        }
    }
}