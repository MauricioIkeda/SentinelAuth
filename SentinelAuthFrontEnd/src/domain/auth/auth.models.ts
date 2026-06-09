export type AuthMode = 'login' | 'register'

export type AuthorizeContext = {
  clientId: string
  redirectUri: string
  state: string
}

export type AuthorizeCallback = {
  callbackUrl: string
}

export type LoginResult = {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export type RegisterUserInput = {
  name: string
  email: string
  password: string
}

export type AuthorizeUserInput = {
  email: string
  password: string
  clientId: string
  redirectUri: string
  state: string | null
}

export type LoginAdminInput = {
  email: string
  password: string
  clientId: string
}
