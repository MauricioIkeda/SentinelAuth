export type AdminTab = 'applications' | 'docs'

export type ApplicationSummary = {
  id: number
  name: string
  clientId: string
  audience: string
  isActive: boolean
  roleCount: number
  assignmentCount: number
}

export type UserSummary = {
  id: number
  name: string
  email: string
  isActive: boolean
  assignmentCount: number
}

export type RoleSummary = {
  id: number
  applicationClientId: number
  name: string
}

export type AssignmentSummary = {
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

export type AdminOverview = {
  applications: ApplicationSummary[]
  users: UserSummary[]
  assignments: AssignmentSummary[]
}

export type ApplicationDetails = {
  application: ApplicationSummary
  roles: RoleSummary[]
  assignments: AssignmentSummary[]
}

export type RegisterApplicationInput = {
  name: string
  clientId: string
  audience: string
  clientSecret: string
  allowRoleAssignment: boolean
}

export type RegisterRoleInput = {
  applicationClientId: number
  name: string
}

export type RenameRoleInput = {
  roleId: number
  name: string
}

export type AssignRoleInput = {
  applicationClientId: number
  userId: number
  roleId: number
}
