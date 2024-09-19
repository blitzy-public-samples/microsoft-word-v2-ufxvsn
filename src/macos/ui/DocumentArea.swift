import Cocoa
import MicrosoftWordCore

class DocumentArea: NSView {
    // MARK: - Properties
    var documentViewModel: DocumentViewModel
    var textView: NSTextView
    var scrollView: NSScrollView
    var pageLayoutView: NSView
    var currentZoom: CGFloat
    
    // MARK: - Initialization
    init(documentViewModel: DocumentViewModel) {
        self.documentViewModel = documentViewModel
        self.currentZoom = 1.0
        self.textView = NSTextView()
        self.scrollView = NSScrollView()
        self.pageLayoutView = NSView()
        
        super.init(frame: .zero)
        
        setupUI()
        setupBindings()
    }
    
    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
    
    // MARK: - UI Setup
    private func setupUI() {
        // Create and configure scrollView
        scrollView = NSScrollView(frame: bounds)
        scrollView.hasVerticalScroller = true
        scrollView.hasHorizontalScroller = true
        scrollView.autoresizingMask = [.width, .height]
        
        // Create and configure pageLayoutView
        pageLayoutView = NSView(frame: scrollView.bounds)
        pageLayoutView.wantsLayer = true
        pageLayoutView.layer?.backgroundColor = NSColor.white.cgColor
        
        // Create and configure textView
        textView = NSTextView(frame: pageLayoutView.bounds)
        textView.autoresizingMask = [.width, .height]
        textView.isEditable = true
        textView.isSelectable = true
        textView.allowsUndo = true
        textView.delegate = self
        
        // Add textView to pageLayoutView
        pageLayoutView.addSubview(textView)
        
        // Add pageLayoutView to scrollView
        scrollView.documentView = pageLayoutView
        
        // Add scrollView to self
        addSubview(scrollView)
        
        // Set up Auto Layout constraints
        scrollView.translatesAutoresizingMaskIntoConstraints = false
        NSLayoutConstraint.activate([
            scrollView.topAnchor.constraint(equalTo: topAnchor),
            scrollView.leadingAnchor.constraint(equalTo: leadingAnchor),
            scrollView.trailingAnchor.constraint(equalTo: trailingAnchor),
            scrollView.bottomAnchor.constraint(equalTo: bottomAnchor)
        ])
    }
    
    // MARK: - Bindings
    private func setupBindings() {
        // Bind textView.string to documentViewModel.content
        documentViewModel.addObserver(self, forKeyPath: "content", options: [.new], context: nil)
        
        // Set up notification observers for text changes
        NotificationCenter.default.addObserver(self, selector: #selector(textDidChange(_:)), name: NSText.didChangeNotification, object: textView)
        
        // Bind currentZoom to documentViewModel.zoomLevel
        documentViewModel.addObserver(self, forKeyPath: "zoomLevel", options: [.new], context: nil)
        
        // Set up bindings for page layout properties
        documentViewModel.addObserver(self, forKeyPath: "pageSize", options: [.new], context: nil)
        documentViewModel.addObserver(self, forKeyPath: "margins", options: [.new], context: nil)
    }
    
    // MARK: - Text Change Handling
    @objc private func textDidChange(_ notification: Notification) {
        // Update documentViewModel.content with new text
        documentViewModel.content = textView.string
        
        // Trigger word count update
        documentViewModel.updateWordCount()
        
        // Update undo manager state
        undoManager?.registerUndo(withTarget: self) { target in
            target.textView.string = self.documentViewModel.content
        }
    }
    
    // MARK: - Zoom Handling
    func updateZoom(zoomFactor: CGFloat) {
        // Set currentZoom to zoomFactor
        currentZoom = zoomFactor
        
        // Update textView and pageLayoutView scales
        textView.scaleUnitSquare(to: NSSize(width: currentZoom, height: currentZoom))
        pageLayoutView.scaleUnitSquare(to: NSSize(width: currentZoom, height: currentZoom))
        
        // Adjust scroll view content insets
        scrollView.contentInsets = NSEdgeInsets(top: 20 * currentZoom, left: 20 * currentZoom, bottom: 20 * currentZoom, right: 20 * currentZoom)
        
        // Refresh document layout
        updatePageLayout()
    }
    
    // MARK: - Style Application
    func applyStyle(_ style: TextStyle) {
        // Get selected range from textView
        guard let selectedRange = textView.selectedRanges.first as? NSRange else { return }
        
        // Apply style attributes to the selected range
        textView.textStorage?.addAttributes(style.attributes, range: selectedRange)
        
        // Update documentViewModel with style changes
        documentViewModel.applyStyle(style, range: selectedRange)
    }
    
    // MARK: - Table Insertion
    func insertTable(rows: Int, columns: Int) {
        // Create NSTextTable with specified rows and columns
        let table = NSTextTable(numberOfColumns: columns, options: [])
        table.numberOfRows = rows
        
        // Insert table at current cursor position in textView
        if let selectedRange = textView.selectedRanges.first as? NSRange {
            let attachment = NSTextTableBlock(table: table, startingRow: 0, rowSpan: rows, startingColumn: 0, columnSpan: columns)
            textView.textStorage?.insert(attachment, at: selectedRange.location)
        }
        
        // Update documentViewModel with table insertion
        documentViewModel.insertTable(rows: rows, columns: columns)
    }
    
    // MARK: - Image Insertion
    func insertImage(_ image: NSImage) {
        // Create NSTextAttachment with the provided image
        let attachment = NSTextAttachment()
        attachment.image = image
        
        // Insert attachment at current cursor position in textView
        if let selectedRange = textView.selectedRanges.first as? NSRange {
            textView.textStorage?.insert(NSAttributedString(attachment: attachment), at: selectedRange.location)
        }
        
        // Update documentViewModel with image insertion
        documentViewModel.insertImage(image)
    }
    
    // MARK: - Page Layout
    func updatePageLayout() {
        // Get page size and margins from documentViewModel
        let pageSize = documentViewModel.pageSize
        let margins = documentViewModel.margins
        
        // Update pageLayoutView size and constraints
        pageLayoutView.frame.size = pageSize
        
        // Adjust textView frame within pageLayoutView
        textView.frame = NSRect(x: margins.left, y: margins.top, width: pageSize.width - margins.left - margins.right, height: pageSize.height - margins.top - margins.bottom)
        
        // Refresh document content layout
        textView.layoutManager?.ensureLayout(for: textView.textContainer!)
    }
}

// MARK: - NSTextViewDelegate
extension DocumentArea: NSTextViewDelegate {
    func textView(_ textView: NSTextView, didChangeTypingAttributes: Void) {
        // Update documentViewModel with new typing attributes
        documentViewModel.updateTypingAttributes(textView.typingAttributes)
        
        // Refresh UI to reflect changes in current style
        // (Implement this method to update any UI elements showing current text style)
    }
}

// MARK: - Human Tasks
/*
 TODO: Implement the following human tasks:
 1. Design and implement a custom NSTextView subclass to handle advanced text layout features.
 2. Create a mechanism for handling real-time collaboration and conflict resolution.
 3. Implement a system for tracking and displaying document changes and revision history.
 4. Design and implement a custom rendering engine for complex document layouts.
 5. Create a performance optimization strategy for handling large documents with many elements.
*/