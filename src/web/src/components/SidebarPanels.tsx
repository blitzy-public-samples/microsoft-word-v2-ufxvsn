import React, { useState } from 'react';
import { DocumentViewModel } from '../viewmodels/DocumentViewModel';
import NavigationPanel from './sidebar/NavigationPanel';
import StylesPanel from './sidebar/StylesPanel';
import ReviewPanel from './sidebar/ReviewPanel';
import ReferencesPanel from './sidebar/ReferencesPanel';
import styles from '../styles/SidebarPanels.module.css';

// Define the props interface for the SidebarPanels component
interface SidebarPanelsProps {
  documentViewModel: DocumentViewModel;
}

// SidebarPanels component for Microsoft Word web application
const SidebarPanels: React.FC<SidebarPanelsProps> = ({ documentViewModel }) => {
  // State to keep track of the active panel
  const [activePanel, setActivePanel] = useState<string>('navigation');

  // Function to handle changing the active panel
  const handlePanelChange = (panelName: string): void => {
    setActivePanel(panelName);
  };

  // Function to navigate to a selected section in the document
  const navigateToSection = (sectionId: string): void => {
    documentViewModel.navigateToSection(sectionId);
    // Update document view to show selected section
    // (This would typically be handled by the DocumentViewModel)
  };

  // Function to apply a selected style to the current selection
  const applyStyle = (styleId: string): void => {
    documentViewModel.applyStyle(styleId);
    // Update document view to reflect applied style
    // (This would typically be handled by the DocumentViewModel)
  };

  // Function to add a comment to the current selection
  const addComment = (commentText: string): void => {
    documentViewModel.addComment(commentText);
    // Update review panel to show new comment
    // (This would typically be handled by the DocumentViewModel)
  };

  // Function to insert a citation at the current cursor position
  const insertCitation = (citationData: object): void => {
    documentViewModel.insertCitation(citationData);
    // Update document view to show inserted citation
    // (This would typically be handled by the DocumentViewModel)
  };

  return (
    <div className={styles.sidebarContainer}>
      <div className={styles.panelTabs}>
        <button
          onClick={() => handlePanelChange('navigation')}
          className={activePanel === 'navigation' ? styles.activeTab : ''}
        >
          Navigation
        </button>
        <button
          onClick={() => handlePanelChange('styles')}
          className={activePanel === 'styles' ? styles.activeTab : ''}
        >
          Styles
        </button>
        <button
          onClick={() => handlePanelChange('review')}
          className={activePanel === 'review' ? styles.activeTab : ''}
        >
          Review
        </button>
        <button
          onClick={() => handlePanelChange('references')}
          className={activePanel === 'references' ? styles.activeTab : ''}
        >
          References
        </button>
      </div>
      <div className={styles.panelContent}>
        {activePanel === 'navigation' && (
          <NavigationPanel
            documentViewModel={documentViewModel}
            onNavigate={navigateToSection}
          />
        )}
        {activePanel === 'styles' && (
          <StylesPanel
            documentViewModel={documentViewModel}
            onApplyStyle={applyStyle}
          />
        )}
        {activePanel === 'review' && (
          <ReviewPanel
            documentViewModel={documentViewModel}
            onAddComment={addComment}
          />
        )}
        {activePanel === 'references' && (
          <ReferencesPanel
            documentViewModel={documentViewModel}
            onInsertCitation={insertCitation}
          />
        )}
      </div>
    </div>
  );
};

export default SidebarPanels;