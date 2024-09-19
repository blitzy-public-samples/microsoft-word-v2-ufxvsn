using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MicrosoftWord.Core;
using MicrosoftWord.Windows.ViewModels;

namespace MicrosoftWord.Windows.UI
{
    public partial class DocumentArea : UserControl
    {
        public DocumentViewModel ViewModel { get; private set; }

        public DocumentArea()
        {
            // Initialize the component
            InitializeComponent();

            // Set the DataContext to this instance
            DataContext = this;

            // Initialize the ViewModel
            ViewModel = new DocumentViewModel();

            // Set up event handlers for text changes and selection changes
            richTextBox.TextChanged += OnTextChanged;
            richTextBox.SelectionChanged += OnSelectionChanged;

            // Set up command bindings for document editing commands
            SetupCommandBindings();
        }

        private void SetupCommandBindings()
        {
            // TODO: Implement command bindings for various editing operations
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Update ViewModel with new document content
            ViewModel.UpdateContent(new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text);

            // Trigger word count update
            ViewModel.UpdateWordCount();

            // Mark document as modified
            ViewModel.SetModified(true);
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // Update ViewModel with current selection
            ViewModel.UpdateSelection(richTextBox.Selection);

            // Update UI to reflect current text formatting at cursor position
            UpdateFormattingUI();
        }

        private void UpdateFormattingUI()
        {
            // TODO: Implement UI updates for text formatting (bold, italic, underline, etc.)
        }

        public void ApplyFontStyle(TextStyleType styleType)
        {
            // Get current selection from RichTextBox
            TextSelection selection = richTextBox.Selection;

            // Apply specified style to the selected text
            switch (styleType)
            {
                case TextStyleType.Bold:
                    selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    break;
                case TextStyleType.Italic:
                    selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                    break;
                // TODO: Add more style types as needed
            }

            // Update ViewModel to reflect changes
            ViewModel.UpdateContent(new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text);
        }

        public void InsertTable(int rows, int columns)
        {
            // Create a new Table object
            Table table = new Table();
            TableRowGroup rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            for (int i = 0; i < rows; i++)
            {
                TableRow row = new TableRow();
                for (int j = 0; j < columns; j++)
                {
                    row.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                }
                rowGroup.Rows.Add(row);
            }

            // Insert the table at the current cursor position in the RichTextBox
            richTextBox.CaretPosition.InsertTable(table);

            // Update ViewModel to reflect changes
            ViewModel.UpdateContent(new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text);
        }

        public void InsertImage(string imagePath)
        {
            // Create a new Image object from the specified path
            Image image = new Image();
            image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath));

            // Insert the image at the current cursor position in the RichTextBox
            InlineUIContainer container = new InlineUIContainer(image, richTextBox.CaretPosition);

            // Update ViewModel to reflect changes
            ViewModel.UpdateContent(new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text);
        }

        public void UpdateZoom(double zoomFactor)
        {
            // Apply zoom factor to the RichTextBox content
            richTextBox.LayoutTransform = new ScaleTransform(zoomFactor, zoomFactor);

            // Update ViewModel with new zoom level
            ViewModel.SetZoomLevel(zoomFactor);

            // Refresh the document view
            richTextBox.UpdateLayout();
        }

        public bool SaveDocument()
        {
            // Get document content from RichTextBox
            string content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;

            // Call ViewModel.SaveDocument() with the content
            return ViewModel.SaveDocument(content);
        }

        public void LoadDocument(string content)
        {
            // Clear existing content in the RichTextBox
            richTextBox.Document.Blocks.Clear();

            // Load the provided content into the RichTextBox
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(content)));

            // Update ViewModel with the loaded content
            ViewModel.UpdateContent(content);

            // Reset modification flags
            ViewModel.SetModified(false);
        }
    }
}