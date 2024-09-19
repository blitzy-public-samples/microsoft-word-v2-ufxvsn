import React, { useState, useEffect, useRef } from 'react';
import { DocumentViewModel } from '../viewmodels/DocumentViewModel';
import useDebounce from '../hooks/useDebounce';
import styles from '../styles/DocumentArea.module.css';

// Define the props interface for the DocumentArea component
interface DocumentAreaProps {
  documentViewModel: DocumentViewModel;
  onContentChange: (content: string) => void;
}

// DocumentArea component for Microsoft Word web application
const DocumentArea: React.FC<DocumentAreaProps> = ({ documentViewModel, onContentChange }) => {
  // State for document content and selection
  const [content, setContent] = useState<string>('');
  const [selection, setSelection] = useState<{ start: number; end: number }>({ start: 0, end: 0 });

  // Reference for the contentEditable div
  const contentEditableRef = useRef<HTMLDivElement>(null);

  // Effect for initializing document content and setting up event listeners
  useEffect(() => {
    // Set initial content from documentViewModel
    setContent(documentViewModel.getContent());

    // Add event listener for selection changes
    document.addEventListener('selectionchange', handleSelectionChange);

    // Cleanup function to remove event listener
    return () => {
      document.removeEventListener('selectionchange', handleSelectionChange);
    };
  }, [documentViewModel]);

  // Debounce effect for content changes
  const debouncedContent = useDebounce(content, 500);

  useEffect(() => {
    // Call onContentChange with debounced content
    onContentChange(debouncedContent);
    // Update documentViewModel with new content
    documentViewModel.updateContent(debouncedContent);
  }, [debouncedContent, onContentChange, documentViewModel]);

  // Handle changes in the document content
  const handleContentChange = (event: React.ChangeEvent<HTMLDivElement>) => {
    const newContent = event.target.innerHTML;
    setContent(newContent);
    documentViewModel.updateContent(newContent);
  };

  // Handle changes in text selection
  const handleSelectionChange = () => {
    const selection = window.getSelection();
    if (selection && contentEditableRef.current) {
      const range = selection.getRangeAt(0);
      const start = range.startOffset;
      const end = range.endOffset;
      setSelection({ start, end });
      documentViewModel.updateSelection({ start, end });
    }
  };

  // Apply formatting to selected text
  const applyFormatting = (format: string, value: string) => {
    const selection = window.getSelection();
    if (selection && !selection.isCollapsed) {
      document.execCommand(format, false, value);
      setContent(contentEditableRef.current?.innerHTML || '');
      documentViewModel.updateContent(contentEditableRef.current?.innerHTML || '');
    }
  };

  // Insert a table at the current cursor position
  const insertTable = (rows: number, columns: number) => {
    let tableHtml = '<table border="1">';
    for (let i = 0; i < rows; i++) {
      tableHtml += '<tr>';
      for (let j = 0; j < columns; j++) {
        tableHtml += '<td>&nbsp;</td>';
      }
      tableHtml += '</tr>';
    }
    tableHtml += '</table>';

    document.execCommand('insertHTML', false, tableHtml);
    setContent(contentEditableRef.current?.innerHTML || '');
    documentViewModel.updateContent(contentEditableRef.current?.innerHTML || '');
  };

  // Insert an image at the current cursor position
  const insertImage = (imageUrl: string) => {
    const imageHtml = `<img src="${imageUrl}" alt="Inserted image" />`;
    document.execCommand('insertHTML', false, imageHtml);
    setContent(contentEditableRef.current?.innerHTML || '');
    documentViewModel.updateContent(contentEditableRef.current?.innerHTML || '');
  };

  // Render the document editing area
  return (
    <div className={styles.documentContainer}>
      <div
        className={styles.page}
        contentEditable={true}
        dangerouslySetInnerHTML={{ __html: content }}
        onInput={handleContentChange}
        onSelect={handleSelectionChange}
        ref={contentEditableRef}
      />
    </div>
  );
};

export default DocumentArea;