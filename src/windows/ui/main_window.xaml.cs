using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicrosoftWord.Core;
using MicrosoftWord.Windows.ViewModels;

namespace MicrosoftWord.Windows.UI
{
    /// <summary>
    /// Main window class for the Microsoft Word application
    /// </summary>
    public partial class MainWindow : Window
    {
        // ViewModel property for data binding
        public DocumentViewModel ViewModel { get; private set; }

        /// <summary>
        /// Initializes the MainWindow and sets up the ViewModel
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize ViewModel with a new DocumentViewModel instance
            ViewModel = new DocumentViewModel();

            // Set DataContext to this
            DataContext = this;

            // Subscribe to window events (Loaded, Closing)
            Loaded += OnLoaded;
            Closing += OnClosing;

            // Set up command bindings
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveDocument_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Exit_Executed));
        }

        /// <summary>
        /// Handler for the window's Loaded event
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Call ViewModel.LoadLastDocument()
            ViewModel.LoadLastDocument();

            // Set up any necessary UI state
            // TODO: Implement any additional UI setup logic
        }

        /// <summary>
        /// Handler for the window's Closing event
        /// </summary>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check if there are unsaved changes
            if (ViewModel.HasUnsavedChanges)
            {
                // If unsaved changes exist, prompt user to save
                MessageBoxResult result = MessageBox.Show("Do you want to save changes?", "Unsaved Changes", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    // Call ViewModel.SaveDocument() if user chooses to save
                    ViewModel.SaveDocument();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // Cancel closing if user chooses to cancel
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Command handler for creating a new document
        /// </summary>
        private void NewDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Call ViewModel.CreateNewDocument()
            ViewModel.CreateNewDocument();

            // Update UI to reflect the new document
            // TODO: Implement UI update logic for new document
        }

        /// <summary>
        /// Command handler for opening an existing document
        /// </summary>
        private void OpenDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Show OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Word Documents (*.docx)|*.docx|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // If file selected, call ViewModel.OpenDocument(filePath)
                ViewModel.OpenDocument(openFileDialog.FileName);

                // Update UI to reflect the opened document
                // TODO: Implement UI update logic for opened document
            }
        }

        /// <summary>
        /// Command handler for saving the current document
        /// </summary>
        private void SaveDocument_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string filePath = ViewModel.CurrentDocumentPath;

            // If document has no file path, show SaveFileDialog
            if (string.IsNullOrEmpty(filePath))
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
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

            // Call ViewModel.SaveDocument(filePath)
            ViewModel.SaveDocument(filePath);

            // Update UI to reflect saved state
            // TODO: Implement UI update logic for saved document
        }

        /// <summary>
        /// Command handler for exiting the application
        /// </summary>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Call Close() to initiate window closing process
            Close();
        }
    }
}

// Human tasks:
// TODO: Implement any additional UI setup logic in OnLoaded method
// TODO: Implement UI update logic for new document in NewDocument_Executed method
// TODO: Implement UI update logic for opened document in OpenDocument_Executed method
// TODO: Implement UI update logic for saved document in SaveDocument_Executed method