export const TOKEN_KEY = 'auth_token';
export const REFRESH_TOKEN_KEY = 'refresh_token';
export const TWO_FACTOR_ENABLED_KEY = 'im3_two_factor_enabled';
export const AUTH_USER_KEY = 'im3_user';
export const AUTH_IS_LOGGED_IN = 'im3_is_logged_in';

export const AUTH_EXCLUDED_URLS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/forgot-password',
  '/api/auth/reset-password',
  '/api/auth/refresh'
];
