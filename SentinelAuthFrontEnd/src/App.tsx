import { AdminUseCases } from './application/admin/AdminUseCases'
import { AuthUseCases } from './application/auth/AuthUseCases'
import { AuthorizeContextService } from './application/auth/AuthorizeContextService'
import { HttpAdminRepository } from './infrastructure/admin/HttpAdminRepository'
import {
  adminClientId,
  apiBaseUrl,
  defaultAuthorizeContext,
} from './infrastructure/api/apiConfig'
import { HttpClient, isAuthorizationError } from './infrastructure/api/httpClient'
import { HttpAuthRepository } from './infrastructure/auth/HttpAuthRepository'
import { AdminTokenStorage } from './infrastructure/storage/AdminTokenStorage'
import AppRouter from './presentation/AppRouter'

const httpClient = new HttpClient(apiBaseUrl)
const adminTokenStorage = new AdminTokenStorage()

const authRepository = new HttpAuthRepository(httpClient)
const adminRepository = new HttpAdminRepository(httpClient, adminTokenStorage)

const authUseCases = new AuthUseCases(authRepository)
const adminUseCases = new AdminUseCases(adminRepository)
const authorizeContextService = new AuthorizeContextService(defaultAuthorizeContext)

function App() {
  return (
    <AppRouter
      adminClientId={adminClientId}
      adminTokenStorage={adminTokenStorage}
      adminUseCases={adminUseCases}
      authUseCases={authUseCases}
      authorizeContextService={authorizeContextService}
      docsAuthApiUrl={apiBaseUrl}
      isAuthorizationError={isAuthorizationError}
    />
  )
}

export default App
