using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicrosoftWord.Core;
using MicrosoftWord.Windows.ViewModels;

namespace MicrosoftWord.Windows.UI
{
    public partial class SidebarPanels : UserControl
    {
        // ViewModel property for data binding
        public DocumentViewModel ViewModel { get; private set; }

        public SidebarPanels()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize ViewModel
            ViewModel = new DocumentViewModel();

            // Set up event handlers for panel interactions
            navigationTreeView.SelectedItemChanged += OnNavigationItemSelected;
            stylesListBox.SelectionChanged += OnStyleSelected;
            trackChangesToggle.Click += OnToggleTrackChanges;
            reviewItemsListBox.SelectionChanged += OnReviewItemSelected;
            insertCitationButton.Click += OnInsertCitation;
            manageSourcesButton.Click += OnManageSources;
        }

        private void OnNavigationItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Get the selected item from the TreeView
            var selectedItem = (DocumentSection)navigationTreeView.SelectedItem;

            // Call ViewModel.NavigateToDocumentSection with the selected item
            ViewModel.NavigateToDocumentSection(selectedItem);

            // Update the document view to show the selected section
            // (This might be handled by the ViewModel through data binding)
        }

        private void OnStyleSelected(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected style from the ListBox
            var selectedStyle = (Style)stylesListBox.SelectedItem;

            // Call ViewModel.ApplyStyle with the selected style
            ViewModel.ApplyStyle(selectedStyle);

            // Update the document view to reflect the applied style
            // (This might be handled by the ViewModel through data binding)
        }

        private void OnToggleTrackChanges(object sender, RoutedEventArgs e)
        {
            // Call ViewModel.ToggleTrackChanges()
            bool isTrackChangesEnabled = ViewModel.ToggleTrackChanges();

            // Update the UI to reflect the current track changes state
            trackChangesToggle.Content = isTrackChangesEnabled ? "Turn Off Track Changes" : "Turn On Track Changes";
        }

        private void OnReviewItemSelected(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected review item from the ListBox
            var selectedReviewItem = (ReviewItem)reviewItemsListBox.SelectedItem;

            // Call ViewModel.NavigateToReviewItem with the selected item
            ViewModel.NavigateToReviewItem(selectedReviewItem);

            // Update the document view to show the selected review item
            // (This might be handled by the ViewModel through data binding)
        }

        private void OnInsertCitation(object sender, RoutedEventArgs e)
        {
            // Open the Insert Citation dialog
            var insertCitationDialog = new InsertCitationDialog();
            if (insertCitationDialog.ShowDialog() == true)
            {
                // If a citation is selected, call ViewModel.InsertCitation with the citation data
                var citationData = insertCitationDialog.SelectedCitation;
                ViewModel.InsertCitation(citationData);

                // Update the document view to show the inserted citation
                // (This might be handled by the ViewModel through data binding)
            }
        }

        private void OnManageSources(object sender, RoutedEventArgs e)
        {
            // Open the Manage Sources dialog
            var manageSourcesDialog = new ManageSourcesDialog(ViewModel.GetSources());
            if (manageSourcesDialog.ShowDialog() == true)
            {
                // If changes are made, call ViewModel.UpdateSources with the updated source data
                var updatedSources = manageSourcesDialog.UpdatedSources;
                ViewModel.UpdateSources(updatedSources);

                // Refresh the references list in the UI
                RefreshReferencesList();
            }
        }

        public void UpdateDocumentStructure()
        {
            // Get the updated document structure from ViewModel.GetDocumentStructure()
            var documentStructure = ViewModel.GetDocumentStructure();

            // Update the TreeView in the navigation panel with the new structure
            navigationTreeView.ItemsSource = documentStructure;
        }

        public void RefreshReviewItems()
        {
            // Get the updated review items from ViewModel.GetReviewItems()
            var reviewItems = ViewModel.GetReviewItems();

            // Update the ListBox in the review panel with the new items
            reviewItemsListBox.ItemsSource = reviewItems;
        }

        private void RefreshReferencesList()
        {
            // Get the updated references list from ViewModel
            var references = ViewModel.GetReferences();

            // Update the references list in the UI
            referencesListBox.ItemsSource = references;
        }
    }
}