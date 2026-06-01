/* eslint-disable react-hooks/exhaustive-deps, react-hooks/set-state-in-effect */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import {
  ArrowRight,
  BadgeCheck,
  BookOpen,
  Boxes,
  CheckCircle2,
  Copy,
  Eye,
  EyeOff,
  KeyRound,
  Lock,
  Mail,
  Pencil,
  Plus,
  RefreshCw,
  Save,
  Search,
  ShieldCheck,
  Trash2,
  UserPlus,
  UserRound,
  X,
} from 'lucide-react'
import './App.css'

type AuthMode = 'login' | 'register'
type AdminTab = 'applications' | 'docs'

type ProblemDetails = {
  title?: string
  detail?: string
  message?: string
}

type AuthorizeResponse = {
  callbackUrl: string
}

type ApplicationSummary = {
  id: number
  name: string
  clientId: string
  audience: string
  isActive: boolean
  roleCount: number
  assignmentCount: number
}

type UserSummary = {
  id: number
  name: string
  email: string
  isActive: boolean
  assignmentCount: number
}

type RoleSummary = {
  id: number
  applicationClientId: number
  name: string
}

type AssignmentSummary = {
  id: number
  userId: number
  userName: string
  userEmail: string
  applicationClientId: number
  applicationName: string
  clientId: string
  roleId: number
  roleName: string
}

type AdminOverview = {
  applications: ApplicationSummary[]
  users: UserSummary[]
  assignments: AssignmentSummary[]
}

type ApplicationDetails = {
  application: ApplicationSummary
  roles: RoleSummary[]
  assignments: AssignmentSummary[]
}

type LoginResult = {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

const adminClientId = 'sentinel-auth-admin'
const adminTokenStorageKey = 'sentinel_admin_access_token'

const runtimeApiBaseUrl =
  typeof window === 'undefined'
    ? 'http://localhost:5254'
    : window.location.hostname.startsWith('auth.')
      ? `${window.location.protocol}//${window.location.hostname.replace(/^auth\./, 'auth-api.')}`
      : `${window.location.protocol}//${window.location.hostname}:5254`

const apiBaseUrl = (
  import.meta.env.VITE_SENTINEL_AUTH_API_URL || runtimeApiBaseUrl
).replace(/\/$/, '')

const defaultClientId = import.meta.env.VITE_DEFAULT_CLIENT_ID ?? ''
const defaultRedirectUri = import.meta.env.VITE_DEFAULT_REDIRECT_URI ?? ''
const defaultState = import.meta.env.VITE_DEFAULT_STATE ?? ''

async function readApiError(response: Response) {
  const fallback = 'Nao foi possivel concluir a solicitacao.'

  try {
    const problem = (await response.json()) as ProblemDetails
    return problem.detail || problem.message || problem.title || fallback
  } catch {
    return fallback
  }
}

async function apiRequest<T>(path: string, init?: RequestInit) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    throw new Error(`${response.status}: ${await readApiError(response)}`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

function getStoredAdminToken() {
  if (typeof window === 'undefined') return ''
  return localStorage.getItem(adminTokenStorageKey) ?? ''
}

function storeAdminToken(token: string) {
  localStorage.setItem(adminTokenStorageKey, token)
}

function clearAdminToken() {
  localStorage.removeItem(adminTokenStorageKey)
}

function isAdminAuthError(error: unknown) {
  return (
    error instanceof Error &&
    (error.message.startsWith('401:') || error.message.startsWith('403:'))
  )
}

async function adminApiRequest<T>(path: string, init?: RequestInit) {
  const token = getStoredAdminToken()

  try {
    return await apiRequest<T>(path, {
      ...init,
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...init?.headers,
      },
    })
  } catch (requestError) {
    if (isAdminAuthError(requestError)) {
      clearAdminToken()
    }

    throw requestError
  }
}

function getAuthorizeContext() {
  const params = new URLSearchParams(window.location.search)

  return {
    clientId: params.get('client_id') || params.get('clientId') || defaultClientId,
    redirectUri:
      params.get('redirect_uri') || params.get('redirectUri') || defaultRedirectUri,
    state: params.get('state') || defaultState,
  }
}

function AuthPage() {
  const authorizeContext = useMemo(() => getAuthorizeContext(), [])
  const [mode, setMode] = useState<AuthMode>('login')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [status, setStatus] = useState('')
  const [error, setError] = useState('')

  const canAuthorize = Boolean(authorizeContext.clientId && authorizeContext.redirectUri)
  const appName = authorizeContext.clientId || 'aplicativo'

  async function handleRegister() {
    await apiRequest('/api/User/register', {
      method: 'POST',
      body: JSON.stringify({ name, email, password }),
    })

    setMode('login')
    setStatus('Conta criada. Entre para continuar.')
  }

  async function handleLogin() {
    if (!canAuthorize) {
      throw new Error('Abra esta tela a partir de um aplicativo cadastrado.')
    }

    const data = await apiRequest<AuthorizeResponse>('/api/oauth/authorize', {
      method: 'POST',
      body: JSON.stringify({
        email,
        password,
        clientId: authorizeContext.clientId,
        redirectUri: authorizeContext.redirectUri,
        state: authorizeContext.state || null,
      }),
    })

    window.location.assign(data.callbackUrl)
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setStatus('')
    setIsSubmitting(true)

    try {
      if (mode === 'register') {
        await handleRegister()
      } else {
        await handleLogin()
      }
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Nao foi possivel concluir a solicitacao.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="auth-shell">
      <section className="brand-panel" aria-label="SentinelAuth">
        <div className="brand-mark" aria-hidden="true">
          <ShieldCheck size={36} strokeWidth={1.8} />
        </div>
        <div>
          <p className="eyebrow">SentinelAuth</p>
          <h1>Identidade global para seus aplicativos.</h1>
        </div>
        <div className="trust-list" aria-label="Recursos de seguranca">
          <span>
            <BadgeCheck size={18} />
            Conta unica
          </span>
          <span>
            <KeyRound size={18} />
            Authorization code
          </span>
          <span>
            <Lock size={18} />
            Tokens isolados por app
          </span>
        </div>
      </section>

      <section className="auth-panel" aria-label="Autenticacao">
        <div className="panel-header">
          <div>
            <p className="context-label">Continuar em</p>
            <h2>{appName}</h2>
          </div>
          <div className="mode-switch" role="tablist" aria-label="Modo">
            <button
              type="button"
              className={mode === 'login' ? 'active' : ''}
              onClick={() => {
                setMode('login')
                setError('')
                setStatus('')
              }}
              role="tab"
              aria-selected={mode === 'login'}
            >
              Entrar
            </button>
            <button
              type="button"
              className={mode === 'register' ? 'active' : ''}
              onClick={() => {
                setMode('register')
                setError('')
                setStatus('')
              }}
              role="tab"
              aria-selected={mode === 'register'}
            >
              Criar conta
            </button>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {mode === 'register' && (
            <Field label="Nome" icon={<UserRound size={19} aria-hidden="true" />}>
              <input
                autoComplete="name"
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Seu nome"
                required
              />
            </Field>
          )}

          <Field label="Email" icon={<Mail size={19} aria-hidden="true" />}>
            <input
              autoComplete="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="voce@email.com"
              required
            />
          </Field>

          <Field label="Senha" icon={<Lock size={19} aria-hidden="true" />}>
            <input
              autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Sua senha"
              required
            />
            <button
              type="button"
              className="icon-button"
              onClick={() => setShowPassword((current) => !current)}
              aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </Field>

          {error && <p className="message error">{error}</p>}
          {status && <p className="message success">{status}</p>}

          <button
            type="submit"
            className="primary-action"
            disabled={isSubmitting || (mode === 'login' && !canAuthorize)}
          >
            {isSubmitting
              ? 'Aguarde...'
              : mode === 'login'
                ? 'Continuar'
                : 'Criar conta'}
            <ArrowRight size={19} aria-hidden="true" />
          </button>
        </form>
      </section>
    </main>
  )
}

function Field({
  label,
  icon,
  children,
}: {
  label: string
  icon: ReactNode
  children: ReactNode
}) {
  return (
    <label className="field">
      <span>{label}</span>
      <div className="input-shell">
        {icon}
        {children}
      </div>
    </label>
  )
}

function AdminPage() {
  const initialAdminTab: AdminTab =
    typeof window !== 'undefined' && window.location.pathname.toLowerCase().startsWith('/docs')
      ? 'docs'
      : 'applications'
  const [tab, setTab] = useState<AdminTab>(initialAdminTab)
  const [overview, setOverview] = useState<AdminOverview>({
    applications: [],
    users: [],
    assignments: [],
  })
  const [selectedApplicationId, setSelectedApplicationId] = useState('')
  const [details, setDetails] = useState<ApplicationDetails | null>(null)
  const [query, setQuery] = useState('')
  const [newApplication, setNewApplication] = useState({
    name: '',
    clientId: '',
    audience: '',
    clientSecret: '',
    allowRoleAssignment: true,
  })
  const [newRole, setNewRole] = useState('')
  const [editingRoleId, setEditingRoleId] = useState<number | null>(null)
  const [editingRoleName, setEditingRoleName] = useState('')
  const [newAssignment, setNewAssignment] = useState({
    userId: '',
    roleId: '',
  })
  const [isLoading, setIsLoading] = useState(initialAdminTab === 'applications')
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [status, setStatus] = useState('')
  const [adminToken, setAdminToken] = useState(getStoredAdminToken)

  const filteredApplications = overview.applications.filter((application) => {
    const value = `${application.name} ${application.clientId} ${application.audience}`
      .toLowerCase()

    return value.includes(query.toLowerCase())
  })

  async function loadAdminData(nextApplicationId = selectedApplicationId) {
    setIsLoading(true)
    setError('')

    try {
      const data = await adminApiRequest<AdminOverview>('/api/admin/overview')
      const applicationId = nextApplicationId || data.applications[0]?.id.toString() || ''

      setOverview(data)
      setSelectedApplicationId(applicationId)

      if (applicationId) {
        setDetails(
          await adminApiRequest<ApplicationDetails>(
            `/api/admin/applications/${applicationId}/details`,
          ),
        )
      } else {
        setDetails(null)
      }
    } catch (requestError) {
      if (isAdminAuthError(requestError)) {
        setAdminToken('')
      }

      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Nao foi possivel carregar a administracao.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  async function selectApplication(applicationId: string) {
    setSelectedApplicationId(applicationId)
    setEditingRoleId(null)
    setNewAssignment({ userId: '', roleId: '' })

    if (!applicationId) {
      setDetails(null)
      return
    }

    setDetails(
      await adminApiRequest<ApplicationDetails>(
        `/api/admin/applications/${applicationId}/details`,
      ),
    )
  }

  useEffect(() => {
    if (tab === 'applications' && adminToken) {
      void loadAdminData()
    }
  }, [tab, adminToken])

  async function runAction(action: () => Promise<void>, successMessage: string) {
    setIsSaving(true)
    setError('')
    setStatus('')

    try {
      await action()
      setStatus(successMessage)
      await loadAdminData(selectedApplicationId)
    } catch (requestError) {
      if (isAdminAuthError(requestError)) {
        setAdminToken('')
      }

      setError(requestError instanceof Error ? requestError.message : 'Falha ao salvar.')
    } finally {
      setIsSaving(false)
    }
  }

  async function saveApplication(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await adminApiRequest<ApplicationSummary>('/api/ApplicationClient/register', {
        method: 'POST',
        body: JSON.stringify(newApplication),
      })
      setNewApplication({
        name: '',
        clientId: '',
        audience: '',
        clientSecret: '',
        allowRoleAssignment: true,
      })
    }, 'Aplicacao cadastrada.')
  }

  async function saveRole(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await adminApiRequest<RoleSummary>('/api/Role/register', {
        method: 'POST',
        body: JSON.stringify({
          applicationClientId: Number(selectedApplicationId),
          name: newRole,
        }),
      })
      setNewRole('')
    }, 'Role cadastrada.')
  }

  async function renameRole(roleId: number) {
    await runAction(async () => {
      await adminApiRequest(`/api/admin/roles/${roleId}`, {
        method: 'PUT',
        body: JSON.stringify({ name: editingRoleName }),
      })
      setEditingRoleId(null)
      setEditingRoleName('')
    }, 'Role atualizada.')
  }

  async function deleteRole(roleId: number) {
    await runAction(async () => {
      await adminApiRequest(`/api/admin/roles/${roleId}`, { method: 'DELETE' })
    }, 'Role removida.')
  }

  async function saveAssignment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await adminApiRequest(`/api/admin/applications/${selectedApplicationId}/assignments`, {
        method: 'POST',
        body: JSON.stringify({
          userId: Number(newAssignment.userId),
          roleId: Number(newAssignment.roleId),
        }),
      })
      setNewAssignment({ userId: '', roleId: '' })
    }, 'Role atribuida ao usuario.')
  }

  async function deleteAssignment(assignmentId: number) {
    await runAction(async () => {
      await adminApiRequest(`/api/admin/assignments/${assignmentId}`, { method: 'DELETE' })
    }, 'Atribuicao removida.')
  }

  function handleAdminAuthenticated(token: string) {
    storeAdminToken(token)
    setAdminToken(token)
    setError('')
    setStatus('Sessao administrativa iniciada.')
    setTab('applications')
    void loadAdminData()
  }

  function handleAdminLogout() {
    clearAdminToken()
    setAdminToken('')
    setOverview({ applications: [], users: [], assignments: [] })
    setDetails(null)
    setSelectedApplicationId('')
    setStatus('Sessao administrativa encerrada.')
  }

  return (
    <main className="admin-shell admin-shell-modern">
      <aside className="admin-sidebar">
        <div className="admin-brand">
          <ShieldCheck size={28} />
          <div>
            <strong>SentinelAuth</strong>
            <span>Admin</span>
          </div>
        </div>
        <nav className="admin-nav" aria-label="Administracao">
          <NavButton
            icon={<Boxes />}
            active={tab === 'applications'}
            onClick={() => setTab('applications')}
          >
            Aplicacoes
          </NavButton>
          <NavButton
            icon={<BookOpen />}
            active={tab === 'docs'}
            onClick={() => setTab('docs')}
          >
            Integracao
          </NavButton>
        </nav>
      </aside>

      <section className="admin-content">
        <header className="admin-header">
          <div>
            <p className="context-label">Painel</p>
            <h1>Administracao do SentinelAuth</h1>
          </div>
          <div className="admin-header-actions">
            {tab === 'applications' && adminToken && (
              <button type="button" className="ghost-action" onClick={() => loadAdminData()}>
                <RefreshCw size={18} />
                Atualizar
              </button>
            )}
            {adminToken && (
              <button type="button" className="ghost-action" onClick={handleAdminLogout}>
                Sair
              </button>
            )}
          </div>
        </header>

        {error && <p className="message error">{error}</p>}
        {status && <p className="message success">{status}</p>}

        {tab === 'docs' ? (
          <DocsTab />
        ) : !adminToken ? (
          <AdminLogin onAuthenticated={handleAdminAuthenticated} />
        ) : isLoading ? (
          <div className="empty-state">Carregando dados...</div>
        ) : (
          <section className="admin-workspace">
            <aside className="application-browser">
              <div className="section-heading">
                <h2>Aplicacoes</h2>
                <span>{overview.applications.length}</span>
              </div>
              <label className="search-box">
                <Search size={17} />
                <input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Buscar app"
                />
              </label>
              <div className="application-list">
                {filteredApplications.map((application) => (
                  <button
                    type="button"
                    key={application.id}
                    className={
                      selectedApplicationId === application.id.toString() ? 'active' : ''
                    }
                    onClick={() => void selectApplication(application.id.toString())}
                  >
                    <strong>{application.name}</strong>
                    <code>{application.clientId}</code>
                    <span>
                      {application.roleCount} roles · {application.assignmentCount} usuarios
                    </span>
                  </button>
                ))}
              </div>
              <form className="compact-form create-app-form" onSubmit={saveApplication}>
                <h3>Nova aplicacao</h3>
                <input
                  value={newApplication.name}
                  onChange={(event) =>
                    setNewApplication({ ...newApplication, name: event.target.value })
                  }
                  placeholder="Nome publico"
                  required
                />
                <input
                  value={newApplication.clientId}
                  onChange={(event) =>
                    setNewApplication({ ...newApplication, clientId: event.target.value })
                  }
                  placeholder="client-id"
                  required
                />
                <input
                  value={newApplication.audience}
                  onChange={(event) =>
                    setNewApplication({ ...newApplication, audience: event.target.value })
                  }
                  placeholder="Audience"
                  required
                />
                <input
                  value={newApplication.clientSecret}
                  onChange={(event) =>
                    setNewApplication({ ...newApplication, clientSecret: event.target.value })
                  }
                  placeholder="client secret do backend"
                  type="password"
                />
                <label className="checkbox-row">
                  <input
                    type="checkbox"
                    checked={newApplication.allowRoleAssignment}
                    onChange={(event) =>
                      setNewApplication({
                        ...newApplication,
                        allowRoleAssignment: event.target.checked,
                      })
                    }
                  />
                  Permitir atribuir roles via API
                </label>
                <button type="submit" className="primary-action small" disabled={isSaving}>
                  <Plus size={17} />
                  Criar app
                </button>
              </form>
            </aside>

            <ApplicationDetail
              details={details}
              users={overview.users}
              newRole={newRole}
              newAssignment={newAssignment}
              editingRoleId={editingRoleId}
              editingRoleName={editingRoleName}
              isSaving={isSaving}
              onNewRoleChange={setNewRole}
              onNewAssignmentChange={setNewAssignment}
              onEditRole={(role) => {
                setEditingRoleId(role.id)
                setEditingRoleName(role.name)
              }}
              onCancelRoleEdit={() => {
                setEditingRoleId(null)
                setEditingRoleName('')
              }}
              onEditingRoleNameChange={setEditingRoleName}
              onSaveRole={saveRole}
              onRenameRole={renameRole}
              onDeleteRole={deleteRole}
              onSaveAssignment={saveAssignment}
              onDeleteAssignment={deleteAssignment}
            />
          </section>
        )}
      </section>
    </main>
  )
}

function AdminLogin({ onAuthenticated }: { onAuthenticated: (token: string) => void }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setError('')

    try {
      const result = await apiRequest<LoginResult>('/api/User/login', {
        method: 'POST',
        body: JSON.stringify({
          email,
          password,
          clientId: adminClientId,
        }),
      })

      onAuthenticated(result.accessToken)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Nao foi possivel entrar na administracao.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="admin-login-card">
      <div>
        <p className="context-label">Acesso restrito</p>
        <h2>Entrar como administrador</h2>
        <p>
          Use uma conta com role <code>SentinelSuperAdmin</code> ou{' '}
          <code>SentinelAdmin</code> no client <code>{adminClientId}</code>.
        </p>
      </div>
      <form className="auth-form" onSubmit={handleSubmit}>
        <Field label="Email" icon={<Mail size={19} aria-hidden="true" />}>
          <input
            autoComplete="email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="admin@email.com"
            required
          />
        </Field>
        <Field label="Senha" icon={<Lock size={19} aria-hidden="true" />}>
          <input
            autoComplete="current-password"
            type={showPassword ? 'text' : 'password'}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Senha administrativa"
            required
          />
          <button
            type="button"
            className="icon-button"
            onClick={() => setShowPassword((current) => !current)}
            aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
          >
            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        </Field>
        {error && <p className="message error">{error}</p>}
        <button type="submit" className="primary-action" disabled={isSubmitting}>
          {isSubmitting ? 'Entrando...' : 'Entrar no painel'}
          <ArrowRight size={19} aria-hidden="true" />
        </button>
      </form>
    </section>
  )
}

function NavButton({
  active,
  icon,
  children,
  onClick,
}: {
  active: boolean
  icon: ReactNode
  children: ReactNode
  onClick: () => void
}) {
  return (
    <button type="button" className={active ? 'active' : ''} onClick={onClick}>
      {icon}
      {children}
    </button>
  )
}

function ApplicationDetail({
  details,
  users,
  newRole,
  newAssignment,
  editingRoleId,
  editingRoleName,
  isSaving,
  onNewRoleChange,
  onNewAssignmentChange,
  onEditRole,
  onCancelRoleEdit,
  onEditingRoleNameChange,
  onSaveRole,
  onRenameRole,
  onDeleteRole,
  onSaveAssignment,
  onDeleteAssignment,
}: {
  details: ApplicationDetails | null
  users: UserSummary[]
  newRole: string
  newAssignment: { userId: string; roleId: string }
  editingRoleId: number | null
  editingRoleName: string
  isSaving: boolean
  onNewRoleChange: (value: string) => void
  onNewAssignmentChange: (value: { userId: string; roleId: string }) => void
  onEditRole: (role: RoleSummary) => void
  onCancelRoleEdit: () => void
  onEditingRoleNameChange: (value: string) => void
  onSaveRole: (event: FormEvent<HTMLFormElement>) => void
  onRenameRole: (roleId: number) => void
  onDeleteRole: (roleId: number) => void
  onSaveAssignment: (event: FormEvent<HTMLFormElement>) => void
  onDeleteAssignment: (assignmentId: number) => void
}) {
  if (!details) {
    return <div className="empty-state">Selecione ou cadastre uma aplicacao.</div>
  }

  return (
    <section className="application-detail">
      <div className="application-hero">
        <div>
          <p className="context-label">Aplicacao selecionada</p>
          <h2>{details.application.name}</h2>
          <div className="detail-meta">
            <code>{details.application.clientId}</code>
            <span>{details.application.audience}</span>
            <span>{details.application.isActive ? 'Ativa' : 'Inativa'}</span>
          </div>
        </div>
        <div className="detail-stats">
          <MetricMini label="Roles" value={details.roles.length} />
          <MetricMini label="Usuarios" value={details.assignments.length} />
        </div>
      </div>

      <div className="detail-grid">
        <section className="admin-section">
          <div className="section-heading">
            <h2>Roles</h2>
            <KeyRound size={18} />
          </div>
          <form className="inline-form" onSubmit={onSaveRole}>
            <input
              value={newRole}
              onChange={(event) => onNewRoleChange(event.target.value)}
              placeholder="Nova role"
              required
            />
            <button type="submit" className="icon-action primary" disabled={isSaving}>
              <Plus size={17} />
            </button>
          </form>
          <div className="role-list">
            {details.roles.map((role) => (
              <div className="role-item" key={role.id}>
                {editingRoleId === role.id ? (
                  <input
                    value={editingRoleName}
                    onChange={(event) => onEditingRoleNameChange(event.target.value)}
                    autoFocus
                  />
                ) : (
                  <strong>{role.name}</strong>
                )}
                <div className="row-actions">
                  {editingRoleId === role.id ? (
                    <>
                      <button
                        type="button"
                        className="icon-action"
                        onClick={() => onRenameRole(role.id)}
                      >
                        <Save size={16} />
                      </button>
                      <button type="button" className="icon-action" onClick={onCancelRoleEdit}>
                        <X size={16} />
                      </button>
                    </>
                  ) : (
                    <>
                      <button
                        type="button"
                        className="icon-action"
                        onClick={() => onEditRole(role)}
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        type="button"
                        className="icon-action danger"
                        onClick={() => onDeleteRole(role.id)}
                      >
                        <Trash2 size={16} />
                      </button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="admin-section">
          <div className="section-heading">
            <h2>Atribuir usuario</h2>
            <UserPlus size={18} />
          </div>
          <form className="compact-form" onSubmit={onSaveAssignment}>
            <select
              value={newAssignment.userId}
              onChange={(event) =>
                onNewAssignmentChange({ ...newAssignment, userId: event.target.value })
              }
              required
            >
              <option value="">Usuario</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name} - {user.email}
                </option>
              ))}
            </select>
            <select
              value={newAssignment.roleId}
              onChange={(event) =>
                onNewAssignmentChange({ ...newAssignment, roleId: event.target.value })
              }
              required
            >
              <option value="">Role</option>
              {details.roles.map((role) => (
                <option key={role.id} value={role.id}>
                  {role.name}
                </option>
              ))}
            </select>
            <button type="submit" className="primary-action small" disabled={isSaving}>
              Atribuir
            </button>
          </form>
        </section>
      </div>

      <section className="admin-section">
        <div className="section-heading">
          <h2>Usuarios atribuidos</h2>
          <span>{details.assignments.length} vinculos</span>
        </div>
        <div className="assignment-grid">
          {details.assignments.map((assignment) => (
            <article className="assignment-card" key={assignment.id}>
              <div>
                <strong>{assignment.userName}</strong>
                <span>{assignment.userEmail}</span>
              </div>
              <code>{assignment.roleName}</code>
              <button
                type="button"
                className="icon-action danger"
                onClick={() => onDeleteAssignment(assignment.id)}
              >
                <Trash2 size={16} />
              </button>
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}

function MetricMini({ label, value }: { label: string; value: number }) {
  return (
    <div className="metric-mini">
      <strong>{value}</strong>
      <span>{label}</span>
    </div>
  )
}

function DocsTab() {
  const docsAuthFrontendUrl =
    typeof window === 'undefined' ? 'https://auth.seu-dominio.com.br' : window.location.origin
  const docsAuthApiUrl = apiBaseUrl

  const registerApplication = `# Operacao administrativa: cadastre uma API/app confiavel
POST ${docsAuthApiUrl}/api/ApplicationClient/register
Authorization: Bearer TOKEN_SENTINEL_ADMIN
Content-Type: application/json

{
  "name": "MeuApp API",
  "clientId": "meuapp-api",
  "audience": "MeuApp.API",
  "clientSecret": "segredo-forte-que-fica-so-no-backend",
  "allowRoleAssignment": true
}

Resposta 200:
{
  "id": 10,
  "name": "MeuApp API",
  "clientId": "meuapp-api",
  "audience": "MeuApp.API",
  "isActive": true
}

# Guarde o clientSecret apenas no backend do app consumidor.
# Ele sera usado no grant client_credentials.`

  const createRoles = `# Operacao administrativa: crie roles dentro da aplicacao
POST ${docsAuthApiUrl}/api/Role/register
Authorization: Bearer TOKEN_SENTINEL_ADMIN
Content-Type: application/json

{
  "applicationClientId": 10,
  "name": "Admin"
}

POST ${docsAuthApiUrl}/api/Role/register
Authorization: Bearer TOKEN_SENTINEL_ADMIN
Content-Type: application/json

{
  "applicationClientId": 10,
  "name": "User"
}

# Role sempre pertence a um ApplicationClient.
# Um usuario pode ser Admin no MeuApp e Seller no Ingressinhos.`

  const frontendConfig = `SENTINEL_AUTH_FRONTEND_URL=https://auth.seu-dominio.com.br
SENTINEL_AUTH_API_URL=https://auth-api.seu-dominio.com.br
CLIENT_ID=meuapp-api
REDIRECT_URI=meuapp://auth/callback

# Mobile: meuapp://auth/callback
# Web:    https://meuapp.com/auth/callback`

  const backendConfig = `SentinelAuthClient__BaseUrl=https://auth-api.seu-dominio.com.br
SentinelAuthClient__ClientId=meuapp-api
SentinelAuthClient__ClientSecret=segredo-forte-somente-no-backend
SentinelAuthClient__ApplicationClientId=10
SentinelAuthClient__AdminRoleId=25
SentinelAuthClient__UserRoleId=26

Jwt__Issuer=SentinelAuth
Jwt__Audience=MeuApp.API
Jwt__SecretKey=mesma-chave-usada-no-SentinelAuth`

  const openCentralLogin = `const state = crypto.randomUUID();
sessionStorage.setItem('sentinel_oauth_state', state);

const redirectUri = 'meuapp://auth/callback';
const url = new URL('${docsAuthFrontendUrl}/authorize');
url.searchParams.set('client_id', 'meuapp-api');
url.searchParams.set('redirect_uri', redirectUri);
url.searchParams.set('state', state);

window.location.href = url.toString();`

  const callbackHandling = `// Callback recebido pelo app:
// meuapp://auth/callback?code=abc123&state=STATE_ALEATORIO

const code = callbackUrl.searchParams.get('code');
const receivedState = callbackUrl.searchParams.get('state');
const expectedState = sessionStorage.getItem('sentinel_oauth_state');

if (!code) throw new Error('SentinelAuth nao retornou code.');
if (receivedState !== expectedState) {
  throw new Error('State invalido. Possivel callback antigo ou tentativa de CSRF.');
}

// code nao e access token.
// code e temporario, uso unico, e serve apenas para pedir os tokens reais.`

  const exchangeCode = `POST ${docsAuthApiUrl}/api/oauth/token
Content-Type: application/json

{
  "grantType": "authorization_code",
  "code": "codigo-curto-de-uso-unico",
  "clientId": "meuapp-api",
  "redirectUri": "meuapp://auth/callback"
}

Resposta 200:
{
  "accessToken": "jwt-do-usuario",
  "refreshToken": "refresh-token-do-usuario",
  "expiresAt": "2026-06-01T17:00:00Z"
}`

  const userJwt = `{
  "sub": "123",
  "email": "usuario@email.com",
  "name": "Usuario",
  "client_id": "meuapp-api",
  "role": ["User"],
  "iss": "SentinelAuth",
  "aud": "MeuApp.API",
  "exp": 1780333200
}`

  const validateApi = `builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "SentinelAuth",
            ValidateAudience = true,
            ValidAudience = "MeuApp.API",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

app.UseAuthentication();
app.UseAuthorization();`

  const protectEndpoint = `[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult MyOrders() => Ok();

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminList() => Ok();
}`

  const clientCredentials = `# Chamado somente pelo backend do seu app. Nunca pelo frontend.
POST ${docsAuthApiUrl}/api/oauth/token
Content-Type: application/json

{
  "grantType": "client_credentials",
  "clientId": "meuapp-api",
  "clientSecret": "segredo-forte-somente-no-backend",
  "scope": "roles:assign"
}

Resposta 200:
{
  "accessToken": "jwt-da-aplicacao",
  "refreshToken": "",
  "expiresAt": "2026-06-01T17:10:00Z"
}`

  const appJwt = `{
  "sub": "app:meuapp-api",
  "client_id": "meuapp-api",
  "application_client_id": "10",
  "token_use": "client_credentials",
  "scope": "roles:assign",
  "aud": "SentinelAuth.API"
}`

  const assignRole = `POST ${docsAuthApiUrl}/api/user-roles/assign
Authorization: Bearer jwt-da-aplicacao
Content-Type: application/json

{
  "userId": 123,
  "applicationClientId": 10,
  "roleId": 26
}

# O SentinelAuth valida:
# 1. token_use == client_credentials
# 2. scope contem roles:assign
# 3. application_client_id do token == applicationClientId do body
# 4. a role pertence a essa aplicacao
# 5. o usuario existe`

  const onboardingFlow = `GET /api/onboarding/profile-status
Authorization: Bearer jwt-do-usuario

Resposta:
{
  "hasClientProfile": false,
  "hasSellerProfile": false
}

Fluxo recomendado:
1. Usuario loga pelo SentinelAuth.
2. Seu backend identifica o usuario pelo JWT.
3. Se nao existe perfil local, pergunte o tipo de perfil.
4. Crie Client/Seller/Member no banco local.
5. Seu backend usa client_credentials e chama assignRole.
6. Peca refresh ou novo login para o JWT vir com a role atualizada.`

  const refreshToken = `POST ${docsAuthApiUrl}/api/User/refresh
Content-Type: application/json

{
  "token": "access-token-atual-ou-expirado",
  "refreshToken": "refresh-token-salvo"
}

Resposta 200:
{
  "accessToken": "novo-jwt-com-roles-atualizadas",
  "refreshToken": "novo-refresh-token",
  "expiresAt": "2026-06-01T18:00:00Z"
}`

  const ingressinhosSeed = `// SentinelAuth.API/Program.cs
await SeedIngressinhosClientAsync(app);

// O seed garante:
ApplicationClient:
- Name: Ingressinhos API
- ClientId: ingressinhos-api
- Audience: Ingressinhos.API
- ClientSecretHash: hash do Seed__Ingressinhos__ClientSecret
- AllowRoleAssignment: true

Roles:
- Admin
- Seller
- Client`

  const ingressinhosCompose = `# docker-compose.yml
sentinel-auth-api:
  environment:
    Seed__Ingressinhos__Enabled: "true"
    Seed__Ingressinhos__ClientSecret: \${SENTINEL_AUTH_INGRESSINHOS_CLIENT_SECRET:?Set no .env}

ingressinhos-api:
  environment:
    SentinelAuthClient__BaseUrl: http://sentinel-auth-api:8080
    SentinelAuthClient__ClientId: ingressinhos-api
    SentinelAuthClient__ClientSecret: \${SENTINEL_AUTH_INGRESSINHOS_CLIENT_SECRET:?Set no .env}
    SentinelAuthClient__ApplicationClientId: 1
    SentinelAuthClient__AdminRoleId: 1
    SentinelAuthClient__SellerRoleId: 2
    SentinelAuthClient__ClientRoleId: 3`

  const ingressinhosRequestAuth = `// Generic.Application/Utils/Services/RequestAuth.cs
CreateUser(...):
1. POST api/User/register
2. Recebe userId
3. POST api/oauth/token com grantType=client_credentials
4. POST api/user-roles/assign com Authorization: Bearer token-da-aplicacao

AssignRole(...):
1. Busca roleId na configuracao
2. Pede/cacheia token client_credentials
3. Chama assignRole protegido`

  const migration = `Checklist para um novo projeto:
[ ] Criar ApplicationClient no SentinelAuth.
[ ] Criar roles para esse ApplicationClient.
[ ] Configurar clientSecret forte apenas no backend.
[ ] Marcar AllowRoleAssignment=true se o app puder atribuir roles.
[ ] Configurar frontend com clientId e redirectUri.
[ ] Configurar backend com clientId, clientSecret, applicationClientId e roleIds.
[ ] Backend validar JWT: issuer, audience, assinatura e expiracao.
[ ] Backend usar client_credentials para assignRole.
[ ] Nunca expor clientSecret no frontend.
[ ] Depois de mudar role, emitir novo accessToken via refresh ou novo login.`

  return (
    <section className="docs-layout docs-pro">
      <article className="admin-section wide">
        <div className="section-heading">
          <h2>Integracao completa com SentinelAuth</h2>
          <BookOpen size={18} />
        </div>
        <p className="doc-lead">
          O SentinelAuth centraliza conta, senha, OAuth, refresh token e roles por aplicacao.
          A API consumidora continua dona dos dados de negocio e deve fazer operacoes sensiveis
          server-to-server, nunca pelo frontend.
        </p>
        <div className="doc-flow">
          <DocStep title="1. Registre a aplicacao">
            Crie um ApplicationClient com clientId, audience e secret de backend.
          </DocStep>
          <DocStep title="2. Configure o login">
            O frontend abre o SentinelAuth, recebe code e troca por accessToken e refreshToken.
          </DocStep>
          <DocStep title="3. Valide JWT na API">
            Sua API valida issuer, audience, assinatura, expiracao e roles.
          </DocStep>
          <DocStep title="4. Crie perfil local">
            Dados de negocio ficam no seu banco: cliente, vendedor, assinatura, pedido etc.
          </DocStep>
          <DocStep title="5. Atribua role com token de app">
            Backend usa client_credentials e chama assignRole com Bearer token da aplicacao.
          </DocStep>
          <DocStep title="6. Renove o token do usuario">
            Role nova so aparece em um accessToken novo.
          </DocStep>
        </div>
      </article>

      <article className="admin-section wide">
        <div className="section-heading"><h2>Responsabilidades</h2><CheckCircle2 size={18} /></div>
        <div className="doc-table">
          <div><strong>SentinelAuth</strong><span>Conta global, login, OAuth, refresh token e roles por aplicacao.</span></div>
          <div><strong>Frontend consumidor</strong><span>Abre login central, valida state, troca code por token e envia Bearer para sua API.</span></div>
          <div><strong>API consumidora</strong><span>Valida JWT, protege rotas, cria perfis locais e atribui roles com client_credentials.</span></div>
          <div><strong>Banco local da API</strong><span>Guarda dados de negocio que nao pertencem ao auth.</span></div>
        </div>
      </article>

      <article className="admin-section wide">
        <div className="section-heading"><h2>Token de usuario vs token de aplicacao</h2></div>
        <div className="doc-table">
          <div><strong>Token de usuario</strong><span>Representa uma pessoa. Usado para acessar a API do produto.</span></div>
          <div><strong>Token de aplicacao</strong><span>Representa um backend confiavel. Usado para server-to-server, como assignRole.</span></div>
          <div><strong>Regra de seguranca</strong><span>Frontend nunca recebe clientSecret e nunca chama assignRole diretamente.</span></div>
        </div>
      </article>

      <article className="admin-section wide"><div className="section-heading"><h2>1. Cadastre a aplicacao</h2></div><p className="doc-copy">Operacao administrativa. Cada API/app confiavel precisa de um ApplicationClient proprio.</p><CodeBlock value={registerApplication} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>2. Crie roles da aplicacao</h2></div><p className="doc-copy">Roles sao isoladas por ApplicationClient.</p><CodeBlock value={createRoles} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>3. Configure frontend e backend</h2></div><p className="doc-copy">O frontend recebe apenas valores publicos. O clientSecret fica somente no backend.</p><CodeBlock value={frontendConfig} /><CodeBlock value={backendConfig} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>4. Abra o login central</h2></div><p className="doc-copy">Use state aleatorio para proteger o callback contra CSRF e callbacks antigos.</p><CodeBlock value={openCentralLogin} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>5. Callback, code e state</h2></div><div className="doc-callout"><strong>Importante:</strong><span>code nao e token. Ele e temporario e de uso unico.</span></div><CodeBlock value={callbackHandling} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>6. Troque code por tokens de usuario</h2></div><p className="doc-copy">O accessToken representa o usuario logado e vem com roles daquela aplicacao.</p><CodeBlock value={exchangeCode} /><CodeBlock value={userJwt} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>7. Valide o JWT na sua API</h2></div><p className="doc-copy">A API consumidora deve validar o token antes de confiar em usuario ou role.</p><CodeBlock value={validateApi} /><CodeBlock value={protectEndpoint} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>8. Onboarding local</h2></div><p className="doc-copy">SentinelAuth sabe quem e a pessoa. Sua API decide qual perfil de negocio ela possui.</p><CodeBlock value={onboardingFlow} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>9. Peça token de aplicacao</h2></div><p className="doc-copy">Antes de atribuir role, o backend autentica como aplicacao usando client_credentials.</p><CodeBlock value={clientCredentials} /><CodeBlock value={appJwt} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>10. Atribua role com seguranca</h2></div><p className="doc-copy">assignRole aceita somente token de aplicacao com scope roles:assign e applicationClientId compatível.</p><CodeBlock value={assignRole} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>11. Renove token depois de mudar role</h2></div><p className="doc-copy">JWT ja emitido nao muda. Depois de atribuir ou remover role, gere um accessToken novo.</p><CodeBlock value={refreshToken} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>12. Como o Ingressinhos esta integrado</h2></div><p className="doc-copy">O Ingressinhos usa seed no SentinelAuth e configuracao no backend para pre-cadastrar client, roles e secret.</p><CodeBlock value={ingressinhosSeed} /><CodeBlock value={ingressinhosCompose} /><CodeBlock value={ingressinhosRequestAuth} /></article>
      <article className="admin-section wide"><div className="section-heading"><h2>13. Checklist de integracao</h2></div><CodeBlock value={migration} /></article>
    </section>
  )
}
function DocStep({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="doc-step">
      <strong>{title}</strong>
      <span>{children}</span>
    </div>
  )
}

function CodeBlock({ value }: { value: string }) {
  return (
    <div className="code-block">
      <button type="button" onClick={() => void navigator.clipboard.writeText(value)}>
        <Copy size={16} />
      </button>
      <pre>{value}</pre>
    </div>
  )
}

function App() {
  const path = window.location.pathname.toLowerCase()

  if (path.startsWith('/admin') || path.startsWith('/docs')) {
    return <AdminPage />
  }

  return <AuthPage />
}

export default App

