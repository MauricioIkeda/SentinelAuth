import type { AdminSessionStorage } from '../../domain/auth/admin-session.storage'

const adminTokenStorageKey = 'sentinel_admin_access_token'

export class AdminTokenStorage implements AdminSessionStorage {
  get() {
    if (typeof window === 'undefined') return ''
    return localStorage.getItem(adminTokenStorageKey) ?? ''
  }

  set(token: string) {
    localStorage.setItem(adminTokenStorageKey, token)
  }

  clear() {
    localStorage.removeItem(adminTokenStorageKey)
  }
}
