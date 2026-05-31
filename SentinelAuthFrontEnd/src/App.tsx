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

const runtimeApiBaseUrl =
  typeof window === 'undefined'
    ? 'http://localhost:5254'
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
    throw new Error(await readApiError(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
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
  const [tab, setTab] = useState<AdminTab>('applications')
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
  })
  const [newRole, setNewRole] = useState('')
  const [editingRoleId, setEditingRoleId] = useState<number | null>(null)
  const [editingRoleName, setEditingRoleName] = useState('')
  const [newAssignment, setNewAssignment] = useState({
    userId: '',
    roleId: '',
  })
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const [status, setStatus] = useState('')

  const filteredApplications = overview.applications.filter((application) => {
    const value = `${application.name} ${application.clientId} ${application.audience}`
      .toLowerCase()

    return value.includes(query.toLowerCase())
  })

  async function loadAdminData(nextApplicationId = selectedApplicationId) {
    setIsLoading(true)
    setError('')

    try {
      const data = await apiRequest<AdminOverview>('/api/admin/overview')
      const applicationId = nextApplicationId || data.applications[0]?.id.toString() || ''

      setOverview(data)
      setSelectedApplicationId(applicationId)

      if (applicationId) {
        setDetails(
          await apiRequest<ApplicationDetails>(
            `/api/admin/applications/${applicationId}/details`,
          ),
        )
      } else {
        setDetails(null)
      }
    } catch (requestError) {
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
      await apiRequest<ApplicationDetails>(
        `/api/admin/applications/${applicationId}/details`,
      ),
    )
  }

  useEffect(() => {
    void loadAdminData()
  }, [])

  async function runAction(action: () => Promise<void>, successMessage: string) {
    setIsSaving(true)
    setError('')
    setStatus('')

    try {
      await action()
      setStatus(successMessage)
      await loadAdminData(selectedApplicationId)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Falha ao salvar.')
    } finally {
      setIsSaving(false)
    }
  }

  async function saveApplication(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await apiRequest<ApplicationSummary>('/api/ApplicationClient/register', {
        method: 'POST',
        body: JSON.stringify(newApplication),
      })
      setNewApplication({ name: '', clientId: '', audience: '' })
    }, 'Aplicacao cadastrada.')
  }

  async function saveRole(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await apiRequest<RoleSummary>('/api/Role/register', {
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
      await apiRequest(`/api/admin/roles/${roleId}`, {
        method: 'PUT',
        body: JSON.stringify({ name: editingRoleName }),
      })
      setEditingRoleId(null)
      setEditingRoleName('')
    }, 'Role atualizada.')
  }

  async function deleteRole(roleId: number) {
    await runAction(async () => {
      await apiRequest(`/api/admin/roles/${roleId}`, { method: 'DELETE' })
    }, 'Role removida.')
  }

  async function saveAssignment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    await runAction(async () => {
      await apiRequest('/api/user-roles/assign', {
        method: 'POST',
        body: JSON.stringify({
          userId: Number(newAssignment.userId),
          applicationClientId: Number(selectedApplicationId),
          roleId: Number(newAssignment.roleId),
        }),
      })
      setNewAssignment({ userId: '', roleId: '' })
    }, 'Role atribuida ao usuario.')
  }

  async function deleteAssignment(assignmentId: number) {
    await runAction(async () => {
      await apiRequest(`/api/admin/assignments/${assignmentId}`, { method: 'DELETE' })
    }, 'Atribuicao removida.')
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
          <button type="button" className="ghost-action" onClick={() => loadAdminData()}>
            <RefreshCw size={18} />
            Atualizar
          </button>
        </header>

        {error && <p className="message error">{error}</p>}
        {status && <p className="message success">{status}</p>}

        {isLoading ? (
          <div className="empty-state">Carregando dados...</div>
        ) : tab === 'docs' ? (
          <DocsTab />
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
  const authorizeUrl =
    'http://localhost:5173/authorize?client_id=ingressinhos-api&redirect_uri=ingressinhos://auth/callback&state=STATE_ALEATORIO'
  const createApplication = `POST http://localhost:5254/api/ApplicationClient/register
Content-Type: application/json

{
  "name": "Ingressinhos API",
  "clientId": "ingressinhos-api",
  "audience": "Ingressinhos.API"
}

Resposta 200:
{
  "id": 1,
  "name": "Ingressinhos API",
  "clientId": "ingressinhos-api",
  "audience": "Ingressinhos.API",
  "isActive": true
}`
  const createRoles = `POST http://localhost:5254/api/Role/register
Content-Type: application/json

{
  "applicationClientId": 1,
  "name": "client"
}

POST http://localhost:5254/api/Role/register
Content-Type: application/json

{
  "applicationClientId": 1,
  "name": "seller"
}

Resposta 200:
{
  "id": 10,
  "applicationClientId": 1,
  "name": "client"
}`
  const registerAndLogin = `POST http://localhost:5254/api/User/register
Content-Type: application/json

{
  "name": "Maria Silva",
  "email": "maria@email.com",
  "password": "Senha@123"
}

POST http://localhost:5254/api/User/login
Content-Type: application/json

{
  "email": "maria@email.com",
  "password": "Senha@123",
  "clientId": "ingressinhos-api"
}

Resposta do login:
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "7d2f...",
  "expiresAt": "2026-05-31T15:20:00-03:00"
}`
  const openCentralLogin = `const state = crypto.randomUUID(); // salve antes do redirect
const redirectUri = 'ingressinhos://auth/callback';

const url = new URL('http://localhost:5173/authorize');
url.searchParams.set('client_id', 'ingressinhos-api');
url.searchParams.set('redirect_uri', redirectUri);
url.searchParams.set('state', state);

window.location.href = url.toString();`
  const authorizePost = `POST http://localhost:5254/api/oauth/authorize
Content-Type: application/json

{
  "email": "maria@email.com",
  "password": "Senha@123",
  "clientId": "ingressinhos-api",
  "redirectUri": "ingressinhos://auth/callback",
  "state": "STATE_ALEATORIO"
}

Resposta 200:
{
  "code": "codigo-curto-de-uso-unico",
  "redirectUri": "ingressinhos://auth/callback",
  "callbackUrl": "ingressinhos://auth/callback?code=codigo-curto-de-uso-unico&state=STATE_ALEATORIO",
  "expiresAt": "2026-05-31T15:10:00-03:00"
}`
  const callbackHandling = `// Callback recebido:
// ingressinhos://auth/callback?code=abc123&state=STATE_ALEATORIO

const code = callbackUrl.searchParams.get('code');
const receivedState = callbackUrl.searchParams.get('state');
const expectedState = sessionStorage.getItem('sentinel_oauth_state');

if (!code) throw new Error('SentinelAuth nao retornou code.');
if (receivedState !== expectedState) {
  throw new Error('State invalido. Possivel callback antigo ou tentativa de CSRF.');
}

// code nao e token. Ele so serve para pedir accessToken e refreshToken.`
  const exchangeCode = `POST http://localhost:5254/api/oauth/token
Content-Type: application/json

{
  "code": "codigo-curto-de-uso-unico",
  "clientId": "ingressinhos-api",
  "redirectUri": "ingressinhos://auth/callback"
}

Resposta 200:
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "7d2f...",
  "expiresAt": "2026-05-31T15:20:00-03:00"
}`
  const assignRole = `POST http://localhost:5254/api/user-roles/assign
Content-Type: application/json

{
  "userId": 22,
  "applicationClientId": 1,
  "roleId": 10
}

Resposta 200:
{
  "id": 55,
  "userId": 22,
  "applicationClientId": 1,
  "roleId": 10
}`
  const refreshToken = `POST http://localhost:5254/api/User/refresh
Content-Type: application/json

{
  "token": "access-token-atual-ou-expirado",
  "refreshToken": "refresh-token-salvo"
}

Resposta 200:
{
  "accessToken": "novo-jwt-com-roles-atualizadas",
  "refreshToken": "novo-refresh-token",
  "expiresAt": "2026-05-31T16:20:00-03:00"
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
            ValidAudience = "Ingressinhos.API",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

app.UseAuthentication();
app.UseAuthorization();`
  const protectEndpoint = `[Authorize]
[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok();

    [Authorize(Roles = "seller")]
    [HttpPost]
    public IActionResult Create(CreateEventRequest request) => Ok();
}`
  const onboardingApi = `GET /api/onboarding/profile-status
Authorization: Bearer accessToken

Resposta:
{
  "hasClientProfile": false,
  "hasSellerProfile": false
}

// Se nao tem perfil:
// 1. Pergunte "cliente ou vendedor?"
// 2. Crie o perfil local com CPF/CNPJ, loja etc.
// 3. Atribua role client ou seller no SentinelAuth.
// 4. Peça novo token para receber a role atualizada.`
  const migration = `1. Cadastre sua API no SentinelAuth e guarde applicationClientId, clientId e audience.
2. Crie roles equivalentes ao sistema antigo: admin, client, seller, manager etc.
3. Para cada usuario existente, crie ou vincule uma conta global SentinelAuth.
4. Crie as atribuicoes em /api/user-roles/assign.
5. Troque sua tela local de senha pelo redirect para /authorize.
6. Na volta do callback, troque code por tokens em /api/oauth/token.
7. Valide JWT na API local e mantenha dados de negocio no banco local.
8. Depois de mudar role, solicite novo token via refresh ou novo login.`

  return (
    <section className="docs-layout docs-pro">
      <article className="admin-section wide">
        <div className="section-heading">
          <h2>Tutorial completo de integracao</h2>
          <BookOpen size={18} />
        </div>
        <p className="doc-lead">
          Pense no SentinelAuth como o Google/GitHub do seu ecossistema: ele guarda a conta
          global, emite tokens e controla roles por aplicacao. A sua API continua dona dos
          dados de negocio, como cliente, vendedor, perfil, pedido, evento ou assinatura.
        </p>
        <div className="doc-flow">
          <DocStep title="1. Cadastre a aplicacao">
            Crie um application client para cada API/app que vai confiar no SentinelAuth.
          </DocStep>
          <DocStep title="2. Crie roles">
            Cadastre permissoes como client, seller, admin ou qualquer papel do seu dominio.
          </DocStep>
          <DocStep title="3. Abra o login central">
            O frontend redireciona para /authorize com client_id, redirect_uri e state.
          </DocStep>
          <DocStep title="4. Troque code por tokens">
            O callback retorna code e state. Valide o state e troque o code por tokens.
          </DocStep>
          <DocStep title="5. Faca onboarding local">
            Se o usuario nao tem perfil na sua API, pergunte o tipo e crie os dados locais.
          </DocStep>
          <DocStep title="6. Renove o token">
            Depois de atribuir role, peca um novo token para a role aparecer nas claims.
          </DocStep>
        </div>
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>Quem faz o que?</h2>
          <CheckCircle2 size={18} />
        </div>
        <div className="doc-table">
          <div><strong>SentinelAuth</strong><span>Conta global, senha, cadastro, login, refresh token e roles globais por aplicacao.</span></div>
          <div><strong>Frontend consumidor</strong><span>Abre /authorize, recebe callback, troca code por tokens e salva tokens com seguranca.</span></div>
          <div><strong>API consumidora</strong><span>Valida JWT, protege endpoints, cria perfis locais e atribui roles no SentinelAuth.</span></div>
          <div><strong>Banco local da API</strong><span>Guarda dados de negocio: CPF/CNPJ, loja, eventos, pedidos e preferencias.</span></div>
        </div>
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>1. Cadastre sua aplicacao</h2>
        </div>
        <p className="doc-copy">
          O <code>clientId</code> identifica o app no login. O <code>audience</code> identifica
          a API que vai validar o JWT.
        </p>
        <CodeBlock value={createApplication} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>2. Crie roles para a aplicacao</h2>
        </div>
        <p className="doc-copy">
          Role pertence a uma aplicacao. Um usuario pode ser seller no Ingressinhos e nao ter
          nenhuma permissao em outro app.
        </p>
        <CodeBlock value={createRoles} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>3. Cadastro e login direto pela API</h2>
        </div>
        <p className="doc-copy">
          Funciona para testes e clientes confiaveis. Para uma experiencia profissional estilo
          Google/GitHub, prefira o login central em /authorize.
        </p>
        <CodeBlock value={registerAndLogin} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>4. Login central: abrir o SentinelAuth</h2>
        </div>
        <p className="doc-copy">
          A URL publica do frontend SentinelAuth recebe client_id, redirect_uri e state. O
          state deve ser aleatorio e salvo antes do redirect.
        </p>
        <CodeBlock value={authorizeUrl} />
        <CodeBlock value={openCentralLogin} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>5. O que /authorize faz por baixo</h2>
        </div>
        <p className="doc-copy">
          Quando o usuario informa e-mail e senha no frontend SentinelAuth, ele chama o backend
          abaixo. O backend cria um authorization code e monta a URL de callback.
        </p>
        <CodeBlock value={authorizePost} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>6. Callback, code e state</h2>
        </div>
        <div className="doc-callout">
          <strong>Regra de ouro:</strong>
          <span>
            <code>code</code> nao e token. Ele e uma credencial temporaria, de uso unico, usada
            apenas para pedir os tokens reais. <code>state</code> prova que o callback pertence
            ao login que o seu app iniciou.
          </span>
        </div>
        <CodeBlock value={callbackHandling} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>7. Troque code por tokens</h2>
        </div>
        <p className="doc-copy">
          Depois de validar o state, chame /api/oauth/token. Salve accessToken para chamar APIs
          e refreshToken para renovar sessao sem pedir senha de novo.
        </p>
        <CodeBlock value={exchangeCode} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>8. Onboarding local</h2>
        </div>
        <p className="doc-copy">
          SentinelAuth sabe quem e o usuario global. A sua API decide o que esse usuario e dentro
          do seu produto. No Ingressinhos, perguntamos cliente ou vendedor, criamos o perfil local
          e atribuimos a role correta.
        </p>
        <CodeBlock value={onboardingApi} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>9. Atribua role ao usuario</h2>
        </div>
        <p className="doc-copy">
          Depois de criar o perfil local, chame o SentinelAuth para atribuir a role. Ex.: cliente
          recebe role client; vendedor recebe role seller.
        </p>
        <CodeBlock value={assignRole} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>10. Preciso pedir novo token depois da role?</h2>
        </div>
        <div className="doc-callout">
          <strong>Sim.</strong>
          <span>
            O JWT ja emitido nao muda sozinho. Se voce atribuiu ou removeu uma role, o app precisa
            receber um novo accessToken para que as claims venham atualizadas.
          </span>
        </div>
        <ol className="steps">
          <li>Depois do onboarding, chame refresh ou refaca login.</li>
          <li>Substitua accessToken e refreshToken salvos pelos novos valores.</li>
          <li>So depois disso libere telas que dependem de role.</li>
        </ol>
        <CodeBlock value={refreshToken} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>11. Valide token na API consumidora</h2>
        </div>
        <p className="doc-copy">
          Toda API integrada deve validar issuer, audience, assinatura e expiracao. Depois use
          [Authorize] e [Authorize(Roles = "...")] nos endpoints.
        </p>
        <CodeBlock value={validateApi} />
        <CodeBlock value={protectEndpoint} />
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>12. Endpoints administrativos uteis</h2>
        </div>
        <div className="doc-table">
          <div><strong>GET /api/admin/overview</strong><span>Lista aplicacoes, usuarios e atribuicoes em uma resposta.</span></div>
          <div><strong>GET /api/admin/applications</strong><span>Lista aplicacoes cadastradas.</span></div>
          <div><strong>GET /api/admin/users</strong><span>Lista usuarios globais.</span></div>
          <div><strong>GET /api/admin/applications/{'{id}'}/details</strong><span>Mostra roles e atribuicoes de uma aplicacao.</span></div>
          <div><strong>PUT /api/admin/roles/{'{roleId}'}</strong><span>Renomeia uma role. Body: {'{ "name": "seller" }'}.</span></div>
          <div><strong>DELETE /api/admin/roles/{'{roleId}'}</strong><span>Remove uma role.</span></div>
          <div><strong>DELETE /api/admin/assignments/{'{assignmentId}'}</strong><span>Remove uma atribuicao de role.</span></div>
        </div>
      </article>

      <article className="admin-section wide">
        <div className="section-heading">
          <h2>13. Migrando de auth proprio</h2>
        </div>
        <p className="doc-copy">
          Se seu projeto ja tem login proprio, nao jogue fora os dados de dominio. Migre a
          identidade para o SentinelAuth e mantenha os perfis locais na sua API.
        </p>
        <CodeBlock value={migration} />
      </article>
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
