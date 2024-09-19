import axios from 'axios';
import { DocumentContent } from '../types/DocumentContent';
import { UserSettings } from '../types/UserSettings';

// Base URL for API requests
const BASE_URL = process.env.REACT_APP_API_BASE_URL || 'https://api.microsoftword.com/v1';

// Axios instance with base configuration
const axiosInstance = axios.create({ baseURL: BASE_URL });

/**
 * Sets the authentication token for API requests
 * @param token The authentication token
 */
export const setAuthToken = (token: string): void => {
  if (token) {
    axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  } else {
    delete axiosInstance.defaults.headers.common['Authorization'];
  }
};

/**
 * Creates a new document on the server
 * @returns Promise with the new document ID and content
 */
export const createNewDocument = async (): Promise<{ documentId: string; content: DocumentContent }> => {
  const response = await axiosInstance.post('/documents');
  return response.data;
};

/**
 * Loads a document from the server
 * @param documentId The ID of the document to load
 * @returns Promise with the document content
 */
export const loadDocument = async (documentId: string): Promise<DocumentContent> => {
  const response = await axiosInstance.get(`/documents/${documentId}`);
  return response.data;
};

/**
 * Saves a document to the server
 * @param documentId The ID of the document to save
 * @param content The content of the document
 */
export const saveDocument = async (documentId: string, content: DocumentContent): Promise<void> => {
  await axiosInstance.put(`/documents/${documentId}`, content);
};

/**
 * Retrieves user settings from the server
 * @returns Promise with the user settings
 */
export const getUserSettings = async (): Promise<UserSettings> => {
  const response = await axiosInstance.get('/user/settings');
  return response.data;
};

/**
 * Updates user settings on the server
 * @param settings Partial user settings to update
 * @returns Promise with the updated user settings
 */
export const updateUserSettings = async (settings: Partial<UserSettings>): Promise<UserSettings> => {
  const response = await axiosInstance.patch('/user/settings', settings);
  return response.data;
};

/**
 * Retrieves the list of collaborators for a document
 * @param documentId The ID of the document
 * @returns Promise with an array of collaborators
 */
export const getDocumentCollaborators = async (documentId: string): Promise<Array<{ id: string; name: string; email: string }>> => {
  const response = await axiosInstance.get(`/documents/${documentId}/collaborators`);
  return response.data;
};

/**
 * Adds a collaborator to a document
 * @param documentId The ID of the document
 * @param email The email of the collaborator to add
 */
export const addCollaborator = async (documentId: string, email: string): Promise<void> => {
  await axiosInstance.post(`/documents/${documentId}/collaborators`, { email });
};

/**
 * Removes a collaborator from a document
 * @param documentId The ID of the document
 * @param collaboratorId The ID of the collaborator to remove
 */
export const removeCollaborator = async (documentId: string, collaboratorId: string): Promise<void> => {
  await axiosInstance.delete(`/documents/${documentId}/collaborators/${collaboratorId}`);
};

/**
 * Retrieves the version history of a document
 * @param documentId The ID of the document
 * @returns Promise with an array of document versions
 */
export const getDocumentVersions = async (documentId: string): Promise<Array<{ id: string; timestamp: string; author: string }>> => {
  const response = await axiosInstance.get(`/documents/${documentId}/versions`);
  return response.data;
};

/**
 * Reverts a document to a specific version
 * @param documentId The ID of the document
 * @param versionId The ID of the version to revert to
 * @returns Promise with the reverted document content
 */
export const revertToVersion = async (documentId: string, versionId: string): Promise<DocumentContent> => {
  const response = await axiosInstance.post(`/documents/${documentId}/versions/${versionId}/revert`);
  return response.data;
};