import type { AuthorizeContext } from '../../domain/auth/auth.models'

export class AuthorizeContextService {
  private readonly defaultAuthorizeContext: AuthorizeContext

  constructor(defaultAuthorizeContext: AuthorizeContext) {
    this.defaultAuthorizeContext = defaultAuthorizeContext
  }

  fromCurrentUrl(): AuthorizeContext {
    const params = new URLSearchParams(window.location.search)

    return {
      clientId:
        params.get('client_id') ||
        params.get('clientId') ||
        this.defaultAuthorizeContext.clientId,
      redirectUri:
        params.get('redirect_uri') ||
        params.get('redirectUri') ||
        this.defaultAuthorizeContext.redirectUri,
      state: params.get('state') || this.defaultAuthorizeContext.state,
    }
  }
}
