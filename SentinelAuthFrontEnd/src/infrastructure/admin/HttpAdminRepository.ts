import type { AdminRepository } from '../../domain/admin/admin.repository'
import type {
  AdminOverview,
  ApplicationDetails,
  ApplicationSummary,
  AssignRoleInput,
  RegisterApplicationInput,
  RegisterRoleInput,
  RenameRoleInput,
  RoleSummary,
} from '../../domain/admin/admin.models'
import type { HttpClient } from '../api/httpClient'
import { isAuthorizationError } from '../api/httpClient'
import type { AdminSessionStorage } from '../../domain/auth/admin-session.storage'

export class HttpAdminRepository implements AdminRepository {
  private readonly http: HttpClient

  private readonly tokenStorage: AdminSessionStorage

  constructor(http: HttpClient, tokenStorage: AdminSessionStorage) {
    this.http = http
    this.tokenStorage = tokenStorage
  }

  getOverview() {
    return this.request<AdminOverview>('/api/admin/overview')
  }

  getApplicationDetails(applicationClientId: number) {
    return this.request<ApplicationDetails>(
      `/api/admin/applications/${applicationClientId}/details`,
    )
  }

  registerApplication(input: RegisterApplicationInput) {
    return this.request<ApplicationSummary>('/api/ApplicationClient/register', {
      method: 'POST',
      body: JSON.stringify(input),
    })
  }

  registerRole(input: RegisterRoleInput) {
    return this.request<RoleSummary>('/api/Role/register', {
      method: 'POST',
      body: JSON.stringify(input),
    })
  }

  async renameRole(input: RenameRoleInput) {
    await this.request(`/api/admin/roles/${input.roleId}`, {
      method: 'PUT',
      body: JSON.stringify({ name: input.name }),
    })
  }

  async deleteRole(roleId: number) {
    await this.request(`/api/admin/roles/${roleId}`, { method: 'DELETE' })
  }

  async assignRole(input: AssignRoleInput) {
    await this.request(`/api/admin/applications/${input.applicationClientId}/assignments`, {
      method: 'POST',
      body: JSON.stringify({
        userId: input.userId,
        roleId: input.roleId,
      }),
    })
  }

  async deleteAssignment(assignmentId: number) {
    await this.request(`/api/admin/assignments/${assignmentId}`, { method: 'DELETE' })
  }

  private async request<T>(path: string, init?: RequestInit) {
    const token = this.tokenStorage.get()

    try {
      return await this.http.request<T>(path, {
        ...init,
        headers: {
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
          ...init?.headers,
        },
      })
    } catch (error) {
      if (isAuthorizationError(error)) {
        this.tokenStorage.clear()
      }

      throw error
    }
  }
}
