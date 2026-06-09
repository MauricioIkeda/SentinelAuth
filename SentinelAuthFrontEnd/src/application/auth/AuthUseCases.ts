import type { AuthRepository } from '../../domain/auth/auth.repository'
import type { AuthorizeContext, LoginAdminInput, RegisterUserInput } from '../../domain/auth/auth.models'

export class AuthUseCases {
  private readonly authRepository: AuthRepository

  constructor(authRepository: AuthRepository) {
    this.authRepository = authRepository
  }

  registerUser(input: RegisterUserInput) {
    return this.authRepository.registerUser(input)
  }

  authorizeUserLogin(input: {
    email: string
    password: string
    authorizeContext: AuthorizeContext
  }) {
    return this.authRepository.authorizeUser({
      email: input.email,
      password: input.password,
      clientId: input.authorizeContext.clientId,
      redirectUri: input.authorizeContext.redirectUri,
      state: input.authorizeContext.state || null,
    })
  }

  loginAdmin(input: LoginAdminInput) {
    return this.authRepository.loginAdmin(input)
  }
}
