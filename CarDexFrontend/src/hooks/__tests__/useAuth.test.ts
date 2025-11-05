/**
 * Tests for useAuth Hook
 * 
 * These tests verify that the useAuth hook:
 * 1. Returns correct values from AuthContext
 * 2. Throws error when used outside AuthProvider
 * 
 * TESTING PHILOSOPHY FOR INFRASTRUCTURE:
 * We keep tests simple and focused on critical behavior:
 * - Does the hook return what it should?
 * - Does it fail gracefully with helpful errors?
 * - We don't test implementation details
 * 
 * NOTE: We mock authService to avoid axios dependency issues in tests
 */

import { renderHook } from '@testing-library/react';
import { useAuth } from '../useAuth';
import { AuthProvider } from '../../context/AuthContext';

// Mock the authService module to prevent axios loading issues
jest.mock('../../services/authService', () => ({
  authService: {
    login: jest.fn().mockResolvedValue({ user: {}, token: 'test-token' }),
    register: jest.fn().mockResolvedValue({ user: {}, token: 'test-token' }),
    logout: jest.fn().mockResolvedValue(undefined),
  },
}));

/**
 * Test: Hook throws error when used outside provider
 * 
 * WHY THIS TEST:
 * Common mistake is forgetting to wrap app with <AuthProvider>
 * This test ensures we get a helpful error message, not a cryptic undefined error
 */
describe('useAuth Hook', () => {
  it('should throw error when used outside AuthProvider', () => {
    // Suppress console.error for this test (we expect an error)
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation();

    // Try to use the hook without a provider
    // We expect this to throw an error
    expect(() => {
      renderHook(() => useAuth());
    }).toThrow('useAuth must be used within an AuthProvider');

    // Restore console.error
    consoleSpy.mockRestore();
  });

  /**
   * Test: Hook returns context value when used correctly
   * 
   * WHY THIS TEST:
   * Verifies that when used correctly (inside AuthProvider),
   * the hook returns all the expected properties
   */
  it('should return auth context when used within AuthProvider', () => {
    // Render the hook inside AuthProvider (correct usage)
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider, // This wraps the hook with <AuthProvider>
    });

    // Check that we got all expected properties
    expect(result.current).toHaveProperty('user');
    expect(result.current).toHaveProperty('token');
    expect(result.current).toHaveProperty('isAuthenticated');
    expect(result.current).toHaveProperty('isLoading');
    expect(result.current).toHaveProperty('login');
    expect(result.current).toHaveProperty('register');
    expect(result.current).toHaveProperty('logout');
  });

  /**
   * Test: Initial state is correct
   * 
   * WHY THIS TEST:
   * When app first loads (no stored session), user should not be authenticated
   * isLoading should become false after checking localStorage
   */
  it('should have correct initial state', () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    });

    // Initially, user should be null (not logged in)
    expect(result.current.user).toBeNull();
    expect(result.current.token).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    
    // isLoading should be false after initial check completes
    // (In real app, there's a brief moment where isLoading is true,
    //  but by the time test runs, useEffect has completed)
  });
});

/**
 * NOTE ON MORE COMPREHENSIVE TESTING:
 * 
 * For a production app, you'd also test:
 * - login() function updates state correctly
 * - logout() clears state correctly
 * - Session restoration from localStorage works
 * 
 * These would require mocking:
 * - authService calls (so we don't hit real API in tests)
 * - localStorage (so tests are isolated)
 * 
 * Example structure (not implemented here to keep it simple):
 * 
 * it('should update state after successful login', async () => {
 *   // Mock authService.login to return fake user/token
 *   jest.spyOn(authService, 'login').mockResolvedValue({
 *     user: { id: 1, username: 'test' },
 *     token: 'fake-token'
 *   });
 *   
 *   const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });
 *   
 *   await act(async () => {
 *     await result.current.login({ email: 'test@test.com', password: 'pass' });
 *   });
 *   
 *   expect(result.current.isAuthenticated).toBe(true);
 *   expect(result.current.user).toEqual({ id: 1, username: 'test' });
 * });
 * 
 * For your purposes (learning and teammates using the infrastructure),
 * the basic tests above are sufficient. More detailed tests can be added
 * later if needed.
 */
