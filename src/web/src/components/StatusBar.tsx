import React, { useState, useEffect } from 'react';
import { DocumentViewModel } from '../viewmodels/DocumentViewModel';
import styles from '../styles/StatusBar.module.css';

// Define the props interface for the StatusBar component
interface StatusBarProps {
  documentViewModel: DocumentViewModel;
  onZoomChange: (zoomLevel: number) => void;
}

// StatusBar component for Microsoft Word web application
const StatusBar: React.FC<StatusBarProps> = ({ documentViewModel, onZoomChange }) => {
  // State variables for status bar information
  const [pageInfo, setPageInfo] = useState<string>('');
  const [wordCount, setWordCount] = useState<number>(0);
  const [currentLanguage, setCurrentLanguage] = useState<string>('');
  const [documentState, setDocumentState] = useState<string>('');
  const [currentZoom, setCurrentZoom] = useState<number>(1);

  // Effect for initializing and updating status bar information
  useEffect(() => {
    const updateListener = () => updateStatusInfo();

    // Set up listeners for document changes
    documentViewModel.addChangeListener(updateListener);

    // Initial update
    updateStatusInfo();

    // Cleanup function to remove listeners
    return () => {
      documentViewModel.removeChangeListener(updateListener);
    };
  }, [documentViewModel]);

  // Function to update all status bar information
  const updateStatusInfo = () => {
    setPageInfo(documentViewModel.getPageInfo());
    setWordCount(documentViewModel.getWordCount());
    setCurrentLanguage(documentViewModel.getCurrentLanguage());
    setDocumentState(documentViewModel.getDocumentState());
    setCurrentZoom(documentViewModel.getCurrentZoom());
  };

  // Function to handle changes in zoom level
  const handleZoomChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newZoomLevel = parseFloat(event.target.value);
    onZoomChange(newZoomLevel);
    setCurrentZoom(newZoomLevel);
  };

  // Function to toggle between different view modes
  const toggleViewMode = () => {
    const nextViewMode = documentViewModel.getNextViewMode();
    documentViewModel.setViewMode(nextViewMode);
    // Note: The UI update will be handled by the change listener
  };

  // Render the status bar
  return (
    <div className={styles.statusBarContainer}>
      <span className={styles.statusItem}>{pageInfo}</span>
      <span className={styles.statusItem}>Words: {wordCount}</span>
      <span className={styles.statusItem}>{currentLanguage}</span>
      <span className={styles.statusItem}>{documentState}</span>
      <select
        className={styles.zoomSelect}
        value={currentZoom}
        onChange={handleZoomChange}
      >
        <option value="0.5">50%</option>
        <option value="0.75">75%</option>
        <option value="1">100%</option>
        <option value="1.25">125%</option>
        <option value="1.5">150%</option>
      </select>
      <button className={styles.viewModeButton} onClick={toggleViewMode}>
        View
      </button>
    </div>
  );
};

export default StatusBar;