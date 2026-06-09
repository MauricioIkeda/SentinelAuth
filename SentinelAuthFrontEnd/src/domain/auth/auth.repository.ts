import type {
  AuthorizeCallback,
  AuthorizeUserInput,
  LoginAdminInput,
  LoginResult,
  RegisterUserInput,
} from './auth.models'

export interface AuthRepository {
  registerUser(input: RegisterUserInput): Promise<void>
  authorizeUser(input: AuthorizeUserInput): Promise<AuthorizeCallback>
  loginAdmin(input: LoginAdminInput): Promise<LoginResult>
}
