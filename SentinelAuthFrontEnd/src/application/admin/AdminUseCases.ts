import type { AdminRepository } from '../../domain/admin/admin.repository'
import type {
  AdminOverview,
  ApplicationDetails,
  AssignRoleInput,
  RegisterApplicationInput,
  RegisterRoleInput,
  RenameRoleInput,
} from '../../domain/admin/admin.models'

export type AdminWorkspace = {
  overview: AdminOverview
  details: ApplicationDetails | null
  selectedApplicationId: string
}

export class AdminUseCases {
  private readonly adminRepository: AdminRepository

  constructor(adminRepository: AdminRepository) {
    this.adminRepository = adminRepository
  }

  async loadWorkspace(selectedApplicationId = ''): Promise<AdminWorkspace> {
    const overview = await this.adminRepository.getOverview()
    const nextApplicationId =
      selectedApplicationId || overview.applications[0]?.id.toString() || ''

    return {
      overview,
      selectedApplicationId: nextApplicationId,
      details: nextApplicationId
        ? await this.adminRepository.getApplicationDetails(Number(nextApplicationId))
        : null,
    }
  }

  async selectApplication(applicationId: string) {
    if (!applicationId) return null
    return this.adminRepository.getApplicationDetails(Number(applicationId))
  }

  registerApplication(input: RegisterApplicationInput) {
    return this.adminRepository.registerApplication(input)
  }

  registerRole(input: RegisterRoleInput) {
    return this.adminRepository.registerRole(input)
  }

  renameRole(input: RenameRoleInput) {
    return this.adminRepository.renameRole(input)
  }

  deleteRole(roleId: number) {
    return this.adminRepository.deleteRole(roleId)
  }

  assignRole(input: AssignRoleInput) {
    return this.adminRepository.assignRole(input)
  }

  deleteAssignment(assignmentId: number) {
    return this.adminRepository.deleteAssignment(assignmentId)
  }
}
