import axios from 'axios';
import jwtDecode from 'jwt-decode';
import { api } from './api';
import { UserCredentials } from '../types/UserCredentials';
import { AuthToken } from '../types/AuthToken';

// Key for storing the auth token in local storage
const TOKEN_KEY = 'ms_word_auth_token';

// Base URL for authentication requests
const AUTH_BASE_URL = process.env.REACT_APP_AUTH_BASE_URL || 'https://auth.microsoftword.com';

/**
 * Authenticates a user and stores the auth token
 * @param credentials User credentials for authentication
 */
export const login = async (credentials: UserCredentials): Promise<void> => {
  try {
    // Make a POST request to `${AUTH_BASE_URL}/login` with credentials
    const response = await axios.post(`${AUTH_BASE_URL}/login`, credentials);
    
    // Extract the token from the response
    const { token } = response.data;
    
    // Store the token in local storage using TOKEN_KEY
    localStorage.setItem(TOKEN_KEY, token);
    
    // Call setAuthToken with the new token
    setAuthToken(token);
  } catch (error) {
    console.error('Login failed:', error);
    throw error;
  }
};

/**
 * Logs out the current user
 */
export const logout = (): void => {
  // Remove the token from local storage
  localStorage.removeItem(TOKEN_KEY);
  
  // Call setAuthToken with null to clear the auth header
  setAuthToken(null);
};

/**
 * Retrieves the current auth token
 * @returns The auth token if it exists, otherwise null
 */
export const getToken = (): string | null => {
  // Retrieve the token from local storage using TOKEN_KEY
  return localStorage.getItem(TOKEN_KEY);
};

/**
 * Checks if the user is currently authenticated
 * @returns True if the user is authenticated, false otherwise
 */
export const isAuthenticated = (): boolean => {
  // Get the token using getToken()
  const token = getToken();
  
  if (token) {
    try {
      // If token exists, decode it and check if it's not expired
      const decodedToken: AuthToken = jwtDecode(token);
      return decodedToken.exp > Date.now() / 1000;
    } catch (error) {
      console.error('Error decoding token:', error);
    }
  }
  
  return false;
};

/**
 * Retrieves the user information from the auth token
 * @returns User information if available, null otherwise
 */
export const getUserInfo = (): { id: string; email: string; name: string } | null => {
  // Get the token using getToken()
  const token = getToken();
  
  if (token) {
    try {
      // If token exists, decode it and extract user information
      const decodedToken: AuthToken = jwtDecode(token);
      return {
        id: decodedToken.sub,
        email: decodedToken.email,
        name: decodedToken.name
      };
    } catch (error) {
      console.error('Error decoding token:', error);
    }
  }
  
  return null;
};

/**
 * Refreshes the auth token
 */
export const refreshToken = async (): Promise<void> => {
  try {
    // Get the current token using getToken()
    const currentToken = getToken();
    
    if (!currentToken) {
      throw new Error('No token to refresh');
    }
    
    // Make a POST request to `${AUTH_BASE_URL}/refresh` with the current token
    const response = await axios.post(`${AUTH_BASE_URL}/refresh`, { token: currentToken });
    
    // Extract the new token from the response
    const { token: newToken } = response.data;
    
    // Store the new token in local storage
    localStorage.setItem(TOKEN_KEY, newToken);
    
    // Call setAuthToken with the new token
    setAuthToken(newToken);
  } catch (error) {
    console.error('Token refresh failed:', error);
    throw error;
  }
};

/**
 * Sets up an interceptor to handle token expiration
 */
export const setupAuthInterceptor = (): void => {
  // Add a response interceptor to the axios instance
  api.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      
      // If a request fails due to an expired token:
      if (error.response.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;
        
        try {
          // Call refreshToken()
          await refreshToken();
          
          // Retry the original request with the new token
          return api(originalRequest);
        } catch (refreshError) {
          // If token refresh fails, logout the user
          logout();
          throw refreshError;
        }
      }
      
      return Promise.reject(error);
    }
  );
};

/**
 * Sets the authentication token for API requests
 * @param token The auth token to set, or null to clear
 */
const setAuthToken = (token: string | null): void => {
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  } else {
    delete api.defaults.headers.common['Authorization'];
  }
};