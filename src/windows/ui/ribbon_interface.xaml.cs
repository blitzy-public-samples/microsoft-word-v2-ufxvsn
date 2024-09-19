using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using MicrosoftWord.Core;
using MicrosoftWord.Windows.ViewModels;

namespace MicrosoftWord.Windows.UI
{
    public partial class RibbonInterface : RibbonWindow
    {
        // ViewModel property for data binding
        public DocumentViewModel ViewModel { get; set; }

        public RibbonInterface()
        {
            InitializeComponent();

            // Initialize ViewModel
            ViewModel = new DocumentViewModel();
            DataContext = ViewModel;

            // Set up command bindings for ribbon buttons
            SetupCommandBindings();

            // Initialize any necessary event handlers
            InitializeEventHandlers();
        }

        private void SetupCommandBindings()
        {
            // Add command bindings for various ribbon actions
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintDocument_Executed));
            // Add more command bindings for font styles, paragraph styles, etc.
        }

        private void InitializeEventHandlers()
        {
            // Initialize any necessary event handlers
        }

        private void NewDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Call ViewModel to create a new document
            ViewModel.CreateNewDocument();

            // Update UI to reflect the new document state
            UpdateUIForNewDocument();
        }

        private void OpenDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Show OpenFileDialog
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Word Documents (*.docx)|*.docx|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // If file selected, call ViewModel to open the document
                ViewModel.OpenDocument(openFileDialog.FileName);

                // Update UI to reflect the opened document
                UpdateUIForOpenedDocument();
            }
        }

        private void SaveDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string filePath = ViewModel.CurrentDocumentPath;

            // If document has no file path, show SaveFileDialog
            if (string.IsNullOrEmpty(filePath))
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Word Documents (*.docx)|*.docx|All files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    filePath = saveFileDialog.FileName;
                }
                else
                {
                    return; // User cancelled the save operation
                }
            }

            // Call ViewModel to save the document
            ViewModel.SaveDocument(filePath);

            // Update UI to reflect saved state
            UpdateUIForSavedDocument();
        }

        private void PrintDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Call ViewModel to prepare the document for printing
            ViewModel.PrintDocument();

            // Show print dialog and handle printing process
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Implement printing logic here
            }
        }

        private void ApplyFontStyle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Determine which style was toggled (bold, italic, or underline)
            string style = e.Parameter as string;

            switch (style)
            {
                case "Bold":
                    ViewModel.ToggleBold();
                    break;
                case "Italic":
                    ViewModel.ToggleItalic();
                    break;
                case "Underline":
                    ViewModel.ToggleUnderline();
                    break;
            }

            // Update UI to reflect the applied style
            UpdateUIForFontStyle(style);
        }

        private void ApplyParagraphStyle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Determine which paragraph style was selected
            string style = e.Parameter as string;

            switch (style)
            {
                case "BulletList":
                    ViewModel.ApplyBulletList();
                    break;
                case "Numbering":
                    ViewModel.ApplyNumbering();
                    break;
                case "IncreaseIndentation":
                    ViewModel.IncreaseIndentation();
                    break;
                case "DecreaseIndentation":
                    ViewModel.DecreaseIndentation();
                    break;
            }

            // Update UI to reflect the applied style
            UpdateUIForParagraphStyle(style);
        }

        private void InsertTable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Show dialog to get table dimensions
            var tableDialog = new InsertTableDialog();
            if (tableDialog.ShowDialog() == true)
            {
                // Call ViewModel to insert the table
                ViewModel.InsertTable(tableDialog.Rows, tableDialog.Columns);

                // Update UI to show the inserted table
                UpdateUIForInsertedTable();
            }
        }

        private void InsertPicture_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Show OpenFileDialog to select an image file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // If file selected, call ViewModel to insert the picture
                ViewModel.InsertPicture(openFileDialog.FileName);

                // Update UI to show the inserted picture
                UpdateUIForInsertedPicture();
            }
        }

        private void SpellingGrammarCheck_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Call ViewModel to perform spelling and grammar check
            var errors = ViewModel.PerformSpellingGrammarCheck();

            // Show spelling and grammar dialog
            var spellCheckDialog = new SpellCheckDialog(errors);
            spellCheckDialog.ShowDialog();

            // Apply corrections as user interacts with the dialog
            ApplySpellCheckCorrections(spellCheckDialog.Corrections);
        }

        // Helper methods to update UI (implement these as needed)
        private void UpdateUIForNewDocument() { }
        private void UpdateUIForOpenedDocument() { }
        private void UpdateUIForSavedDocument() { }
        private void UpdateUIForFontStyle(string style) { }
        private void UpdateUIForParagraphStyle(string style) { }
        private void UpdateUIForInsertedTable() { }
        private void UpdateUIForInsertedPicture() { }
        private void ApplySpellCheckCorrections(List<Correction> corrections) { }
    }
}