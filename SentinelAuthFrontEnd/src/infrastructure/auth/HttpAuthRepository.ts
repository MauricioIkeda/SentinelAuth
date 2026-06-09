import type { AuthRepository } from '../../domain/auth/auth.repository'
import type {
  AuthorizeCallback,
  AuthorizeUserInput,
  LoginAdminInput,
  LoginResult,
  RegisterUserInput,
} from '../../domain/auth/auth.models'
import type { HttpClient } from '../api/httpClient'

export class HttpAuthRepository implements AuthRepository {
  private readonly http: HttpClient

  constructor(http: HttpClient) {
    this.http = http
  }

  async registerUser(input: RegisterUserInput) {
    await this.http.request('/api/User/register', {
      method: 'POST',
      body: JSON.stringify(input),
    })
  }

  authorizeUser(input: AuthorizeUserInput) {
    return this.http.request<AuthorizeCallback>('/api/oauth/authorize', {
      method: 'POST',
      body: JSON.stringify(input),
    })
  }

  loginAdmin(input: LoginAdminInput) {
    return this.http.request<LoginResult>('/api/User/login', {
      method: 'POST',
      body: JSON.stringify(input),
    })
  }
}
