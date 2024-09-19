import Cocoa
import MicrosoftWordCore

class MainWindow: NSWindow {
    // MARK: - Properties
    var documentViewModel: DocumentViewModel
    var ribbonInterface: RibbonInterface
    var documentArea: DocumentArea
    var sidebarPanels: SidebarPanels
    var statusBar: StatusBar

    // MARK: - Initialization
    override init(contentRect: NSRect, styleMask style: NSWindow.StyleMask, backing backingStoreType: NSWindow.BackingStoreType, defer flag: Bool) {
        // Initialize properties
        self.documentViewModel = DocumentViewModel()
        self.ribbonInterface = RibbonInterface()
        self.documentArea = DocumentArea()
        self.sidebarPanels = SidebarPanels()
        self.statusBar = StatusBar()

        // Call super.init()
        super.init(contentRect: contentRect, styleMask: style, backing: backingStoreType, defer: flag)

        // Set up window properties
        self.title = "Microsoft Word"
        self.setFrameAutosaveName("MainWindow")
        self.minSize = NSSize(width: 800, height: 600)

        // Set up UI components
        setupUI()

        // Set up bindings
        setupBindings()

        // Set the window delegate
        self.delegate = self
    }

    // MARK: - UI Setup
    private func setupUI() {
        // Create and configure UI components
        ribbonInterface.configure()
        documentArea.configure()
        sidebarPanels.configure()
        statusBar.configure()

        // Add components to the window's content view
        guard let contentView = self.contentView else { return }
        contentView.addSubview(ribbonInterface)
        contentView.addSubview(documentArea)
        contentView.addSubview(sidebarPanels)
        contentView.addSubview(statusBar)

        // Set up Auto Layout constraints
        NSLayoutConstraint.activate([
            ribbonInterface.topAnchor.constraint(equalTo: contentView.topAnchor),
            ribbonInterface.leadingAnchor.constraint(equalTo: contentView.leadingAnchor),
            ribbonInterface.trailingAnchor.constraint(equalTo: contentView.trailingAnchor),

            documentArea.topAnchor.constraint(equalTo: ribbonInterface.bottomAnchor),
            documentArea.leadingAnchor.constraint(equalTo: contentView.leadingAnchor),
            documentArea.trailingAnchor.constraint(equalTo: sidebarPanels.leadingAnchor),
            documentArea.bottomAnchor.constraint(equalTo: statusBar.topAnchor),

            sidebarPanels.topAnchor.constraint(equalTo: ribbonInterface.bottomAnchor),
            sidebarPanels.trailingAnchor.constraint(equalTo: contentView.trailingAnchor),
            sidebarPanels.bottomAnchor.constraint(equalTo: statusBar.topAnchor),
            sidebarPanels.widthAnchor.constraint(equalToConstant: 250),

            statusBar.leadingAnchor.constraint(equalTo: contentView.leadingAnchor),
            statusBar.trailingAnchor.constraint(equalTo: contentView.trailingAnchor),
            statusBar.bottomAnchor.constraint(equalTo: contentView.bottomAnchor),
            statusBar.heightAnchor.constraint(equalToConstant: 22)
        ])
    }

    // MARK: - Bindings Setup
    private func setupBindings() {
        // Bind documentArea to documentViewModel.content
        documentArea.bind(.content, to: documentViewModel, withKeyPath: "content", options: nil)

        // Bind statusBar information to documentViewModel properties
        statusBar.bind(.wordCount, to: documentViewModel, withKeyPath: "wordCount", options: nil)
        statusBar.bind(.pageCount, to: documentViewModel, withKeyPath: "pageCount", options: nil)

        // Bind sidebarPanels data to documentViewModel
        sidebarPanels.bind(.outline, to: documentViewModel, withKeyPath: "outline", options: nil)
        sidebarPanels.bind(.comments, to: documentViewModel, withKeyPath: "comments", options: nil)

        // Set up bindings for ribbonInterface actions
        ribbonInterface.newDocumentAction = { [weak self] in
            self?.newDocument()
        }
        ribbonInterface.openDocumentAction = { [weak self] in
            self?.openDocument()
        }
        ribbonInterface.saveDocumentAction = { [weak self] in
            self?.saveDocument()
        }
    }

    // MARK: - Document Actions
    func newDocument() {
        // Call documentViewModel to create a new document
        documentViewModel.createNewDocument()

        // Update UI to reflect the new document state
        documentArea.resetContent()
        statusBar.resetStatus()
        sidebarPanels.resetPanels()
    }

    func openDocument() {
        // Show open file dialog
        let openPanel = NSOpenPanel()
        openPanel.allowedFileTypes = ["docx", "doc"]
        openPanel.allowsMultipleSelection = false
        openPanel.canChooseDirectories = false
        openPanel.canCreateDirectories = false
        openPanel.canChooseFiles = true

        openPanel.beginSheetModal(for: self) { [weak self] response in
            if response == .OK, let url = openPanel.url {
                // If file selected, call documentViewModel to open the document
                self?.documentViewModel.openDocument(at: url)

                // Update UI to reflect the opened document
                self?.documentArea.refreshContent()
                self?.statusBar.updateStatus()
                self?.sidebarPanels.refreshPanels()
            }
        }
    }

    func saveDocument() {
        // If document has no file path, show save file dialog
        if documentViewModel.documentURL == nil {
            let savePanel = NSSavePanel()
            savePanel.allowedFileTypes = ["docx"]
            savePanel.canCreateDirectories = true

            savePanel.beginSheetModal(for: self) { [weak self] response in
                if response == .OK, let url = savePanel.url {
                    // Call documentViewModel to save the document
                    self?.documentViewModel.saveDocument(to: url)

                    // Update UI to reflect saved state
                    self?.statusBar.updateStatus()
                }
            }
        } else {
            // Call documentViewModel to save the document
            documentViewModel.saveDocument()

            // Update UI to reflect saved state
            statusBar.updateStatus()
        }
    }

    // MARK: - Window Delegate Methods
    func windowWillClose(_ notification: Notification) {
        // Check if there are unsaved changes
        if documentViewModel.hasUnsavedChanges {
            let alert = NSAlert()
            alert.messageText = "Do you want to save changes to your document?"
            alert.informativeText = "Your changes will be lost if you don't save them."
            alert.addButton(withTitle: "Save")
            alert.addButton(withTitle: "Don't Save")
            alert.addButton(withTitle: "Cancel")

            alert.beginSheetModal(for: self) { [weak self] response in
                switch response {
                case .alertFirstButtonReturn:
                    // User chose to save
                    self?.saveDocument()
                case .alertSecondButtonReturn:
                    // User chose not to save, do nothing
                    break
                case .alertThirdButtonReturn:
                    // User cancelled, prevent window from closing
                    self?.makeKeyAndOrderFront(nil)
                    return
                default:
                    break
                }

                // Perform any necessary cleanup
                self?.documentViewModel.cleanup()
            }
        } else {
            // Perform any necessary cleanup
            documentViewModel.cleanup()
        }
    }
}

// MARK: - NSWindowDelegate Extension
extension MainWindow: NSWindowDelegate {
    func windowDidResize(_ notification: Notification) {
        // Update layout of UI components if necessary
        documentArea.updateLayout()
        sidebarPanels.updateLayout()

        // Notify documentArea of size change for possible content reflowing
        documentArea.handleWindowResize()
    }

    func windowDidBecomeKey(_ notification: Notification) {
        // Update UI state for active window
        ribbonInterface.updateForActiveWindow()

        // Refresh document content if necessary
        documentArea.refreshContent()
    }
}