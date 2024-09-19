using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicrosoftWord.Core;
using MicrosoftWord.Windows.ViewModels;

namespace MicrosoftWord.Windows.UI
{
    public partial class StatusBar : UserControl
    {
        // ViewModel property for data binding
        public DocumentViewModel ViewModel { get; private set; }

        public StatusBar()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize ViewModel
            ViewModel = new DocumentViewModel();

            // Set up event handlers for status bar interactions
            zoomComboBox.SelectionChanged += OnZoomLevelChanged;
            viewModeToggleButton.Click += OnToggleViewMode;

            // Initial refresh of status bar information
            RefreshStatusBar();
        }

        private void OnZoomLevelChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected zoom level from the ComboBox
            if (zoomComboBox.SelectedItem is string zoomLevel)
            {
                // Call ViewModel.SetZoomLevel with the new zoom level
                ViewModel.SetZoomLevel(zoomLevel);

                // Update the document view to reflect the new zoom level
                // (This might be handled by data binding or require additional logic)
            }
        }

        private void OnToggleViewMode(object sender, RoutedEventArgs e)
        {
            // Call ViewModel.ToggleViewMode()
            ViewModel.ToggleViewMode();

            // Update the UI to reflect the new view mode
            // (This might be handled by data binding or require additional logic)
        }

        private void UpdatePageInfo()
        {
            // Get the current page number and total pages from ViewModel
            int currentPage = ViewModel.CurrentPageNumber;
            int totalPages = ViewModel.TotalPages;

            // Update the PageInfo property with the new information
            pageInfoTextBlock.Text = $"Page {currentPage} of {totalPages}";
        }

        private void UpdateWordCount()
        {
            // Get the current word count from ViewModel
            int wordCount = ViewModel.WordCount;

            // Update the WordCount property with the new count
            wordCountTextBlock.Text = $"Words: {wordCount}";
        }

        private void UpdateLanguage()
        {
            // Get the current language from ViewModel
            string currentLanguage = ViewModel.CurrentLanguage;

            // Update the CurrentLanguage property with the new language
            languageTextBlock.Text = currentLanguage;
        }

        private void UpdateDocumentState()
        {
            // Get the current document state from ViewModel
            string documentState = ViewModel.DocumentState;

            // Update the DocumentState property with the new state
            documentStateTextBlock.Text = documentState;
        }

        public void RefreshStatusBar()
        {
            // Call individual update methods
            UpdatePageInfo();
            UpdateWordCount();
            UpdateLanguage();
            UpdateDocumentState();

            // Update ZoomLevels and CurrentZoom from ViewModel
            zoomComboBox.ItemsSource = ViewModel.ZoomLevels;
            zoomComboBox.SelectedItem = ViewModel.CurrentZoom;
        }
    }
}