import React from 'react';
import { render, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import { MainApp } from '../../src/components/MainApp';
import { DocumentViewModel } from '../../src/viewmodels/DocumentViewModel';
import * as api from '../../src/utils/api';

// Mock the api module
jest.mock('../../src/utils/api');

// Helper function to set up MainApp with mocked dependencies
const setupMainApp = (initialDocumentId: string | null) => {
  const renderResult = render(<MainApp initialDocumentId={initialDocumentId} />);
  return {
    ...renderResult,
    mockedApi: api as jest.Mocked<typeof api>,
  };
};

// Helper function to simulate a document edit
const simulateDocumentEdit = (documentArea: HTMLElement, content: string) => {
  fireEvent.input(documentArea, { target: { textContent: content } });
};

describe('MainApp Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders without crashing', () => {
    const { container } = render(<MainApp />);
    expect(container).toBeInTheDocument();
  });

  test('initializes with a new document when no initialDocumentId is provided', async () => {
    const newDocument = new DocumentViewModel('New Document');
    (api.createNewDocument as jest.Mock).mockResolvedValue(newDocument);

    const { getByTestId, mockedApi } = setupMainApp(null);

    await waitFor(() => {
      expect(mockedApi.createNewDocument).toHaveBeenCalled();
    });

    const documentArea = getByTestId('document-area');
    expect(documentArea).toHaveTextContent('New Document');
  });

  test('loads an existing document when initialDocumentId is provided', async () => {
    const existingDocument = new DocumentViewModel('Existing Document');
    (api.loadDocument as jest.Mock).mockResolvedValue(existingDocument);

    const { getByTestId, mockedApi } = setupMainApp('existing-doc-id');

    await waitFor(() => {
      expect(mockedApi.loadDocument).toHaveBeenCalledWith('existing-doc-id');
    });

    const documentArea = getByTestId('document-area');
    expect(documentArea).toHaveTextContent('Existing Document');
  });

  test('handles save operation correctly', async () => {
    const { getByTestId, getByText, mockedApi } = setupMainApp(null);

    const documentArea = getByTestId('document-area');
    simulateDocumentEdit(documentArea, 'Updated content');

    const saveButton = getByText('Save');
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(mockedApi.saveDocument).toHaveBeenCalledWith(expect.objectContaining({
        content: 'Updated content',
      }));
    });

    expect(getByText('Saved')).toBeInTheDocument();
  });

  test('updates zoom level correctly', () => {
    const { getByTestId, getByLabelText } = setupMainApp(null);

    const zoomSelect = getByLabelText('Zoom');
    fireEvent.change(zoomSelect, { target: { value: '150' } });

    const documentArea = getByTestId('document-area');
    expect(documentArea).toHaveStyle('transform: scale(1.5)');
  });

  test('renders all major components', () => {
    const { getByTestId } = setupMainApp(null);

    expect(getByTestId('ribbon-interface')).toBeInTheDocument();
    expect(getByTestId('document-area')).toBeInTheDocument();
    expect(getByTestId('sidebar-panels')).toBeInTheDocument();
    expect(getByTestId('status-bar')).toBeInTheDocument();
  });

  test('handles errors during document loading', async () => {
    (api.loadDocument as jest.Mock).mockRejectedValue(new Error('Failed to load document'));

    const { getByText, mockedApi } = setupMainApp('error-doc-id');

    await waitFor(() => {
      expect(mockedApi.loadDocument).toHaveBeenCalledWith('error-doc-id');
    });

    expect(getByText('Error: Failed to load document')).toBeInTheDocument();
    expect(getByText('New Document')).toBeInTheDocument(); // Fallback to new document
  });

  test('updates document state correctly', async () => {
    const { getByTestId, getByText } = setupMainApp(null);

    const documentArea = getByTestId('document-area');
    simulateDocumentEdit(documentArea, 'New content with multiple words');

    await waitFor(() => {
      expect(getByText('Words: 5')).toBeInTheDocument();
      expect(getByText('Unsaved changes')).toBeInTheDocument();
    });
  });
});