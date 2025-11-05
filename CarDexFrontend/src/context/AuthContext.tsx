/**
 * Auth Context - Authentication State Management
 * 
 * This is the "brain" for all authentication in the app.
 * It manages who is logged in, their token, and login/logout operations.
 * 
 * WHAT IS CONTEXT?
 * Context is React's built-in way to share data across many components without
 * passing props through every level (called "prop drilling").
 * 
 * Think of it like a whiteboard in an office:
 * - Anyone can walk up and read what's written (useAuth hook)
 * - Anyone can write on it (login/logout functions)
 * - Everyone sees the same information (single source of truth)
 * 
 * WHY WE USE CONTEXT FOR AUTH?
 * - Many components need to know if user is logged in (Header, Pages, etc.)
 * - Without context, you'd pass user/token props through 10+ components
 * - With context, any component can just call useAuth() to get current user
 * 
 * HOW IT WORKS:
 * 1. App wraps everything with <AuthProvider>
 * 2. Provider stores user/token in state
 * 3. Any child component uses useAuth() to access that state
 * 4. When login() is called, state updates and all components re-render with new user
 */

import React, { createContext, useState, useEffect, ReactNode } from 'react';
import { User } from '../types/types';
import { authService, LoginRequest, RegisterRequest } from '../services/authService';

/**
 * AuthContextType: The shape of data this context provides
 * 
 * This interface defines what values/functions components can access
 * when they call useAuth()
 */
interface AuthContextType {
  user: User | null;              // Current logged-in user (null if not logged in)
  token: string | null;           // JWT token for API requests (null if not logged in)
  isAuthenticated: boolean;       // Convenience flag: true if user is logged in
  isLoading: boolean;             // True during initial load (checking localStorage)
  login: (credentials: LoginRequest) => Promise<void>;    // Login function
  register: (userData: RegisterRequest) => Promise<void>; // Register function
  logout: () => Promise<void>;                             // Logout function
}

/**
 * Create the Context
 * 
 * This creates a "container" that will hold our auth state.
 * - Initialized as undefined (before provider wraps the app)
 * - Gets real value when AuthProvider is mounted
 * - Components access it via useAuth() hook
 */
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Props for AuthProvider component
 * - children: All components that need access to auth state
 */
interface AuthProviderProps {
  children: ReactNode;
}

/**
 * AuthProvider Component
 * 
 * This is the "wrapper" component that provides auth state to all children.
 * Think of it like a power strip - you plug it in once, then all devices
 * connected to it get power.
 * 
 * USAGE: In App.tsx, wrap your app:
 *   <AuthProvider>
 *     <YourAppComponents />
 *   </AuthProvider>
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  // State: user - The current logged-in user object (or null if not logged in)
  const [user, setUser] = useState<User | null>(null);
  
  // State: token - The JWT token for authenticating API requests (or null)
  const [token, setToken] = useState<string | null>(null);
  
  // State: isLoading - True while we're checking if user has existing session
  // We set this to false after checking localStorage on first load
  const [isLoading, setIsLoading] = useState(true);

  /**
   * Effect: Load saved session on mount
   * 
   * WHY WE DO THIS:
   * - When user refreshes the page, React state is lost (starts at null)
   * - But we saved their token in localStorage during login
   * - This effect runs once when app starts and restores the session
   * 
   * FLOW:
   * 1. User logs in → token saved to localStorage
   * 2. User refreshes page → state resets to null
   * 3. This effect runs → loads token from localStorage
   * 4. User stays logged in! (no need to log in again)
   */
  useEffect(() => {
    // Try to get saved auth data from browser storage
    const storedToken = localStorage.getItem('authToken');
    const storedUser = localStorage.getItem('user');

    // If we found saved data, restore it to state
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser)); // Parse JSON string back to object
    }
    
    // We're done loading (whether we found data or not)
    setIsLoading(false);
  }, []); // Empty array means this only runs once on mount

  /**
   * login: Authenticate user with email/password
   * 
   * FLOW:
   * 1. Call backend API with credentials
   * 2. Backend validates and returns user + token
   * 3. Save both to state (triggers re-render of all components using useAuth)
   * 4. Save to localStorage (so session persists across refreshes)
   * 
   * ERROR HANDLING:
   * - If backend returns error (wrong password), we throw it
   * - Component that called login() can catch and show error message
   */
  const login = async (credentials: LoginRequest) => {
    try {
      // Call backend API to authenticate
      const response = await authService.login(credentials);
      
      // Update React state with user and token
      // This immediately makes isAuthenticated = true
      setUser(response.user);
      setToken(response.token);
      
      // Save to localStorage so it persists across page refreshes
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    } catch (error) {
      // If login fails (wrong password, network error, etc.), log and re-throw
      console.error('Login failed:', error);
      throw error; // Re-throw so the component can handle it (show error message)
    }
  };

  /**
   * register: Create new user account
   * 
   * FLOW:
   * Same as login, but creates a new account first
   * After successful registration, user is automatically logged in
   */
  const register = async (userData: RegisterRequest) => {
    try {
      // Call backend API to create account
      const response = await authService.register(userData);
      
      // Log the user in immediately after registration
      setUser(response.user);
      setToken(response.token);
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    } catch (error) {
      console.error('Registration failed:', error);
      throw error;
    }
  };

  /**
   * logout: End user session
   * 
   * FLOW:
   * 1. Tell backend to invalidate the session (optional, for security)
   * 2. Clear state (user/token become null)
   * 3. Clear localStorage (so user isn't auto-logged-in on refresh)
   * 
   * NOTE: We use 'finally' block to ensure cleanup happens even if
   * backend call fails (e.g., if user is offline)
   */
  const logout = async () => {
    try {
      // Tell backend to invalidate the session
      await authService.logout();
    } catch (error) {
      // If backend call fails, still log them out locally
      console.error('Logout failed:', error);
    } finally {
      // Always clear local state and storage, regardless of backend response
      setUser(null);
      setToken(null);
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
    }
  };

  /**
   * Build the value object that components will receive
   * 
   * This object is what gets returned when components call useAuth()
   * - Include state values (user, token, etc.)
   * - Include functions (login, logout, etc.)
   * - Include derived values (isAuthenticated = computed from user + token)
   */
  const value: AuthContextType = {
    user,
    token,
    // isAuthenticated is a convenience computed value
    // !! converts truthy/falsy to true/false (null becomes false, object becomes true)
    isAuthenticated: !!user && !!token,
    isLoading,
    login,
    register,
    logout,
  };

  /**
   * Provide the value to all children
   * 
   * Any component nested inside <AuthProvider> (directly or deeply nested)
   * can access this value by calling useAuth()
   */
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
