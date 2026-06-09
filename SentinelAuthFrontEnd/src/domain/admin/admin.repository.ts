import type {
  AdminOverview,
  ApplicationDetails,
  ApplicationSummary,
  AssignRoleInput,
  RegisterApplicationInput,
  RegisterRoleInput,
  RenameRoleInput,
  RoleSummary,
} from './admin.models'

export interface AdminRepository {
  getOverview(): Promise<AdminOverview>
  getApplicationDetails(applicationClientId: number): Promise<ApplicationDetails>
  registerApplication(input: RegisterApplicationInput): Promise<ApplicationSummary>
  registerRole(input: RegisterRoleInput): Promise<RoleSummary>
  renameRole(input: RenameRoleInput): Promise<void>
  deleteRole(roleId: number): Promise<void>
  assignRole(input: AssignRoleInput): Promise<void>
  deleteAssignment(assignmentId: number): Promise<void>
}
