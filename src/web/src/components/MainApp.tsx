import React, { useState, useEffect } from 'react';
import { RibbonInterface } from './RibbonInterface';
import { DocumentArea } from './DocumentArea';
import { SidebarPanels } from './SidebarPanels';
import { StatusBar } from './StatusBar';
import { DocumentViewModel } from '../viewmodels/DocumentViewModel';
import { useDocumentState } from '../hooks/useDocumentState';
import { api } from '../utils/api';

// Define the props interface for the MainApp component
interface MainAppProps {
  initialDocumentId: string | null;
}

// Main component for the Microsoft Word web application
export const MainApp: React.FC<MainAppProps> = ({ initialDocumentId }) => {
  // State for the document view model and loading status
  const [documentViewModel, setDocumentViewModel] = useState<DocumentViewModel | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Custom hook for managing document state
  const [documentState, updateDocumentState] = useDocumentState(documentViewModel);

  // Effect for initializing the document
  useEffect(() => {
    const initializeDocument = async () => {
      setIsLoading(true);
      try {
        let document;
        if (initialDocumentId) {
          // Load existing document if initialDocumentId is provided
          document = await api.loadDocument(initialDocumentId);
        } else {
          // Create a new document if no initialDocumentId is provided
          document = await api.createNewDocument();
        }
        setDocumentViewModel(new DocumentViewModel(document));
      } catch (error) {
        console.error('Error initializing document:', error);
        // Handle error (e.g., show error message to user)
      } finally {
        setIsLoading(false);
      }
    };

    initializeDocument();
  }, [initialDocumentId]);

  // Function to handle saving the document
  const handleSave = async (): Promise<void> => {
    if (!documentViewModel) return;

    try {
      const content = documentViewModel.getContent();
      await api.saveDocument(content);
      updateDocumentState({ isSaved: true, lastSaved: new Date() });
    } catch (error) {
      console.error('Error saving document:', error);
      // Handle error (e.g., show error message to user)
    }
  };

  // Function to handle changes in zoom level
  const handleZoomChange = (zoomLevel: number): void => {
    if (!documentViewModel) return;

    documentViewModel.setZoomLevel(zoomLevel);
    updateDocumentState({ zoomLevel });
  };

  // Render loading state if document is not yet initialized
  if (isLoading || !documentViewModel) {
    return <div>Loading...</div>;
  }

  // Render the main application layout
  return (
    <div className="main-app-container">
      <RibbonInterface documentViewModel={documentViewModel} onSave={handleSave} />
      <div className="content-container">
        <SidebarPanels documentViewModel={documentViewModel} />
        <DocumentArea documentViewModel={documentViewModel} onContentChange={updateDocumentState} />
      </div>
      <StatusBar documentState={documentState} onZoomChange={handleZoomChange} />
    </div>
  );
};