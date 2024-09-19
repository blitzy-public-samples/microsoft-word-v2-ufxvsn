import XCTest
@testable import MicrosoftWord

class MainWindowTests: XCTestCase {
    var sut: MainWindow!
    var mockDocumentViewModel: MockDocumentViewModel!

    override func setUpWithError() throws {
        try super.setUpWithError()
        // Initialize mockDocumentViewModel
        mockDocumentViewModel = MockDocumentViewModel()
        // Initialize sut (MainWindow) with mockDocumentViewModel
        sut = MainWindow(documentViewModel: mockDocumentViewModel)
    }

    override func tearDownWithError() throws {
        // Set sut to nil
        sut = nil
        // Set mockDocumentViewModel to nil
        mockDocumentViewModel = nil
        try super.tearDownWithError()
    }

    func testMainWindowInitialization() {
        XCTAssertNotNil(sut.documentViewModel)
        XCTAssertNotNil(sut.ribbonInterface)
        XCTAssertNotNil(sut.documentArea)
        XCTAssertNotNil(sut.sidebarPanels)
        XCTAssertNotNil(sut.statusBar)
        XCTAssertEqual(sut.title, "Microsoft Word")
    }

    func testNewDocumentAction() {
        sut.newDocument(nil)
        XCTAssertTrue(mockDocumentViewModel.createNewDocumentCalled)
        XCTAssertEqual(sut.title, "Untitled - Microsoft Word")
    }

    func testOpenDocumentAction() {
        // Mock NSOpenPanel to return a file URL
        let mockedFileURL = URL(fileURLWithPath: "/path/to/MockDocument.docx")
        let mockOpenPanel = MockNSOpenPanel(url: mockedFileURL)
        sut.openDocument(nil, openPanel: mockOpenPanel)
        XCTAssertTrue(mockDocumentViewModel.openDocumentCalled)
        XCTAssertEqual(mockDocumentViewModel.lastOpenedURL, mockedFileURL)
        XCTAssertEqual(sut.title, "MockDocument.docx - Microsoft Word")
    }

    func testSaveDocumentAction() {
        sut.saveDocument(nil)
        XCTAssertTrue(mockDocumentViewModel.saveDocumentCalled)
        // Verify that the UI is updated to reflect the saved state
        // This might involve checking the window title or a status indicator
    }

    func testWindowWillClose() {
        mockDocumentViewModel.hasUnsavedChanges = true
        // Mock NSAlert to simulate user choosing to save
        let mockAlert = MockNSAlert(returnCode: .alertFirstButtonReturn)
        sut.windowWillClose(Notification(name: NSWindow.willCloseNotification), alert: mockAlert)
        XCTAssertTrue(mockDocumentViewModel.saveDocumentCalled)
        // Verify that the window closes after saving
        // This might involve checking a property on sut or using a completion handler
    }

    func testRibbonInterfaceInteraction() {
        // Simulate user interaction with various Ribbon controls
        sut.ribbonInterface.boldButton.performClick(nil)
        XCTAssertTrue(mockDocumentViewModel.applyBoldFormatCalled)
        
        sut.ribbonInterface.italicButton.performClick(nil)
        XCTAssertTrue(mockDocumentViewModel.applyItalicFormatCalled)
        
        // Verify that the UI is updated accordingly
        // This might involve checking the state of buttons or other UI elements
    }

    func testDocumentAreaInteraction() {
        // Simulate text input in the DocumentArea
        let testText = "Hello, World!"
        sut.documentArea.insertText(testText)
        XCTAssertEqual(mockDocumentViewModel.lastInsertedText, testText)
        
        // Verify that the UI reflects the changes
        XCTAssertEqual(sut.documentArea.string, testText)
    }

    func testSidebarPanelsInteraction() {
        // Simulate user interaction with different sidebar panels
        sut.sidebarPanels.stylePanel.selectStyle(name: "Heading 1")
        XCTAssertEqual(mockDocumentViewModel.lastAppliedStyle, "Heading 1")
        
        sut.sidebarPanels.outlinePanel.addItem(title: "New Section")
        XCTAssertEqual(mockDocumentViewModel.lastAddedOutlineItem, "New Section")
        
        // Verify that the UI is updated to reflect the sidebar interactions
        XCTAssertTrue(sut.sidebarPanels.stylePanel.isStyleSelected("Heading 1"))
        XCTAssertTrue(sut.sidebarPanels.outlinePanel.containsItem(title: "New Section"))
    }

    func testStatusBarUpdates() {
        // Simulate changes in document state
        mockDocumentViewModel.wordCount = 100
        mockDocumentViewModel.pageCount = 2
        
        // Trigger status bar update
        sut.updateStatusBar()
        
        // Verify that the StatusBar is updated with the correct information
        XCTAssertEqual(sut.statusBar.wordCountLabel.stringValue, "Words: 100")
        XCTAssertEqual(sut.statusBar.pageCountLabel.stringValue, "Pages: 2")
    }
}

// MARK: - Helper Classes

class MockDocumentViewModel {
    var createNewDocumentCalled = false
    var openDocumentCalled = false
    var saveDocumentCalled = false
    var lastOpenedURL: URL?
    var hasUnsavedChanges = false
    
    var applyBoldFormatCalled = false
    var applyItalicFormatCalled = false
    var lastInsertedText: String?
    var lastAppliedStyle: String?
    var lastAddedOutlineItem: String?
    var wordCount = 0
    var pageCount = 0

    func createNewDocument() {
        createNewDocumentCalled = true
    }

    func openDocument(_ url: URL) {
        openDocumentCalled = true
        lastOpenedURL = url
    }

    func saveDocument() {
        saveDocumentCalled = true
    }
}

class MockNSOpenPanel {
    let url: URL?
    
    init(url: URL?) {
        self.url = url
    }
    
    func runModal() -> NSApplication.ModalResponse {
        return .OK
    }
}

class MockNSAlert {
    let returnCode: NSApplication.ModalResponse
    
    init(returnCode: NSApplication.ModalResponse) {
        self.returnCode = returnCode
    }
    
    func runModal() -> NSApplication.ModalResponse {
        return returnCode
    }
}