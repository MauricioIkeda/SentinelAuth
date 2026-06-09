export const runtimeApiBaseUrl =
  typeof window === 'undefined'
    ? 'http://localhost:5254'
    : window.location.hostname.startsWith('auth.')
      ? `${window.location.protocol}//${window.location.hostname.replace(/^auth\./, 'auth-api.')}`
      : `${window.location.protocol}//${window.location.hostname}:5254`

export const apiBaseUrl = (
  import.meta.env.VITE_SENTINEL_AUTH_API_URL || runtimeApiBaseUrl
).replace(/\/$/, '')

export const defaultAuthorizeContext = {
  clientId: import.meta.env.VITE_DEFAULT_CLIENT_ID ?? '',
  redirectUri: import.meta.env.VITE_DEFAULT_REDIRECT_URI ?? '',
  state: import.meta.env.VITE_DEFAULT_STATE ?? '',
}

export const adminClientId = 'sentinel-auth-admin'
