import React, { useState } from 'react';
import DocumentViewModel from '../viewmodels/DocumentViewModel';
import RibbonTab from './RibbonTab';
import HomeTab from './ribbon-tabs/HomeTab';
import InsertTab from './ribbon-tabs/InsertTab';
import LayoutTab from './ribbon-tabs/LayoutTab';
import ReviewTab from './ribbon-tabs/ReviewTab';
import styles from '../styles/RibbonInterface.module.css';

// Define the props interface for the RibbonInterface component
interface RibbonInterfaceProps {
  documentViewModel: DocumentViewModel;
  onSave: () => Promise<void>;
}

// RibbonInterface component
const RibbonInterface: React.FC<RibbonInterfaceProps> = ({ documentViewModel, onSave }) => {
  // State to keep track of the active tab
  const [activeTab, setActiveTab] = useState<string>('Home');

  // Handle changing the active tab
  const handleTabChange = (tabName: string) => {
    setActiveTab(tabName);
  };

  // Handle font changes
  const handleFontChange = (font: string) => {
    documentViewModel.setFont(font);
    // Update the document view to reflect the new font
  };

  // Handle font size changes
  const handleFontSizeChange = (size: number) => {
    documentViewModel.setFontSize(size);
    // Update the document view to reflect the new font size
  };

  // Handle style changes
  const handleStyleChange = (style: string) => {
    documentViewModel.applyStyle(style);
    // Update the document view to reflect the new style
  };

  // Handle inserting a table
  const handleInsertTable = (rows: number, columns: number) => {
    documentViewModel.insertTable(rows, columns);
    // Update the document view to show the inserted table
  };

  // Handle inserting an image
  const handleInsertImage = (imageUrl: string) => {
    documentViewModel.insertImage(imageUrl);
    // Update the document view to show the inserted image
  };

  return (
    <div className={styles.ribbonContainer}>
      <div className={styles.tabContainer}>
        <RibbonTab
          name="Home"
          isActive={activeTab === 'Home'}
          onClick={() => handleTabChange('Home')}
        />
        <RibbonTab
          name="Insert"
          isActive={activeTab === 'Insert'}
          onClick={() => handleTabChange('Insert')}
        />
        <RibbonTab
          name="Layout"
          isActive={activeTab === 'Layout'}
          onClick={() => handleTabChange('Layout')}
        />
        <RibbonTab
          name="Review"
          isActive={activeTab === 'Review'}
          onClick={() => handleTabChange('Review')}
        />
      </div>
      <div className={styles.tabContent}>
        {activeTab === 'Home' && (
          <HomeTab
            documentViewModel={documentViewModel}
            onFontChange={handleFontChange}
            onFontSizeChange={handleFontSizeChange}
            onStyleChange={handleStyleChange}
            onSave={onSave}
          />
        )}
        {activeTab === 'Insert' && (
          <InsertTab
            documentViewModel={documentViewModel}
            onInsertTable={handleInsertTable}
            onInsertImage={handleInsertImage}
          />
        )}
        {activeTab === 'Layout' && (
          <LayoutTab documentViewModel={documentViewModel} />
        )}
        {activeTab === 'Review' && (
          <ReviewTab documentViewModel={documentViewModel} />
        )}
      </div>
    </div>
  );
};

export default RibbonInterface;