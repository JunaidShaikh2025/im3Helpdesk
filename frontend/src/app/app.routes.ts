import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { superAdminGuard } from './core/guards/super-admin.guard';
import { companyAdminGuard } from './core/guards/company-admin.guard';
import { setupOrgGuard } from './core/guards/setup-org.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { featureGuard } from './core/guards/feature.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'lead',
    loadComponent: () =>
      import('./features/public-onboarding/lead-capture/lead-capture.component').then(m => m.LeadCaptureComponent)
  },
  {
    path: 'setup-org',
    loadComponent: () =>
      import('./features/public-onboarding/setup-org/setup-org.component').then(m => m.SetupOrgComponent),
    canActivate: [setupOrgGuard]
  },
  {
    path: 'setup-org-error',
    loadComponent: () =>
      import('./features/public-onboarding/setup-org-error/setup-org-error.component').then(m => m.SetupOrgErrorComponent)
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'users',
    loadChildren: () =>
      import('./features/users/users.routes').then(m => m.USERS_ROUTES)
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./features/auth/verify-email/verify-email.component').then(m => m.VerifyEmailComponent)
  },
  {
    path: 'register-customer',
    loadComponent: () =>
      import('./features/auth/register-customer/register-customer.component').then(m => m.RegisterCustomerComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'contacts',
    loadComponent: () =>
      import('./features/contacts/contacts-page/contacts-page.component').then(m => m.ContactsPageComponent),
    canActivate: [authGuard, permissionGuard('contacts')]
  },
  {
    path: 'todo',
    loadComponent: () =>
      import('./features/todo/todo-panel/todo-list.component').then(m => m.TodoListComponent),
    canActivate: [authGuard, permissionGuard('todo')]
  },
  {
    path: 'notes',
    loadComponent: () =>
      import('./features/notes/notes-workspace/notes-workspace.component').then(m => m.NotesWorkspaceComponent),
    canActivate: [authGuard, permissionGuard('notes')]
  },
  {
    path: 'chat',
    loadComponent: () =>
      import('./features/chat/chat-page/chat-page.component').then(m => m.ChatPageComponent),
    canActivate: [authGuard, permissionGuard('chat')]
  },
  {
    path: 'onboarding',
    loadComponent: () =>
      import('./features/onboarding/onboarding-wizard/onboarding-wizard.component').then(m => m.OnboardingWizardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'dashboard',
    loadChildren: () =>
      import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES),
    canActivate: [authGuard, permissionGuard('dashboard')]
  },
  {
    path: 'ai-dashboard',
    loadComponent: () =>
      import('./features/dashboard/ai-dashboard/ai-dashboard.component').then(m => m.AIDashboardComponent),
    canActivate: [authGuard, permissionGuard('ai-insights'), featureGuard('ai-insights')]
  },
  {
    path: 'tickets/create',
    loadComponent: () =>
      import('./features/tickets/ticket-create/ticket-create.component').then(m => m.TicketCreateComponent),
    canActivate: [authGuard, permissionGuard('tickets')]
  },
  {
    path: 'tickets',
    loadChildren: () =>
      import('./features/tickets/tickets.routes').then(m => m.TICKETS_ROUTES),
    canActivate: [authGuard, permissionGuard('tickets')]
  },
  {
    path: 'agents/groups',
    loadComponent: () =>
      import('./features/agents/agent-groups/agent-groups.component').then(m => m.AgentGroupsComponent),
    canActivate: [authGuard, companyAdminGuard, permissionGuard('agent-groups')]
  },
  {
    path: 'agents/invite',
    loadComponent: () =>
      import('./features/agents/agent-invite/agent-invite.component').then(m => m.AgentInviteComponent),
    canActivate: [authGuard, companyAdminGuard, permissionGuard('agents')]
  },
  {
    path: 'agents',
    loadComponent: () =>
      import('./features/agents/agent-list/agent-list.component').then(m => m.AgentsComponent),
    canActivate: [authGuard, companyAdminGuard, permissionGuard('agents')]
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/profile-page/profile-page.component').then(m => m.ProfilePageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'organization-profile',
    loadComponent: () =>
      import('./features/organization/organization-profile/organization-profile.component').then(
        m => m.OrganizationProfileComponent
      ),
    canActivate: [authGuard, companyAdminGuard, permissionGuard('organization-profile'), featureGuard('organization-profile')]
  },
  {
    path: 'holiday-setup',
    loadComponent: () =>
      import('./features/organization/holiday-setup/holiday-setup.component').then(
        m => m.HolidaySetupComponent
      ),
    canActivate: [authGuard, companyAdminGuard, permissionGuard('holiday-setup'), featureGuard('holiday-setup')]
  },
  {
    path: 'role-rights',
    loadComponent: () =>
      import('./features/settings/role-rights/role-rights.component').then(
        m => m.RoleRightsComponent
      ),
    canActivate: [authGuard, companyAdminGuard, featureGuard('role-rights')]
  },
  {
    path: 'recycle-bin/:id',
    loadComponent: () =>
      import('./features/recycle-bin/recycle-bin-detail-page/recycle-bin-detail-page.component').then(
        m => m.RecycleBinDetailPageComponent
      ),
    canActivate: [authGuard, permissionGuard('recycle-bin'), featureGuard('recycle-bin')]
  },
  {
    path: 'recycle-bin',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/recycle-bin/recycle-bin-page/recycle-bin-page.component').then(
        m => m.RecycleBinPageComponent
      ),
    canActivate: [authGuard, permissionGuard('recycle-bin'), featureGuard('recycle-bin')]
  },
  {
    path: 'sla-policies/:id/edit',
    loadComponent: () =>
      import('./features/sla-policies/sla-policy-edit/sla-policy-edit.component').then(
        m => m.SlaPolicyEditComponent
      ),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'business-hours/:id/edit',
    loadComponent: () =>
      import('./features/business-hours/business-hours-edit/business-hours-edit.component').then(
        m => m.BusinessHoursEditComponent
      ),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'business-hours',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/business-hours/business-hours-page/business-hours-page.component').then(
        m => m.BusinessHoursPageComponent
      ),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'sla-policies',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/sla-policies/sla-policies-page/sla-policies-page.component').then(
        m => m.SlaPoliciesPageComponent
      ),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'notifications',
    loadComponent: () =>
      import('./features/notifications/notifications-page/notifications-page.component').then(m => m.NotificationsPageComponent),
    canActivate: [authGuard, permissionGuard('notifications')]
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./features/reports/reports-page/reports-page.component').then(m => m.ReportsComponent),
    canActivate: [authGuard, permissionGuard('reports'), featureGuard('reports')]
  },
  {
    path: 'admin',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/super-admin/super-admin-dashboard/super-admin-dashboard.component').then(m => m.SuperAdminDashboardComponent),
    canActivate: [superAdminGuard]
  },
  {
    path: 'admin/organizations',
    loadComponent: () =>
      import('./features/super-admin/organizations-list/organizations-list.component').then(m => m.OrganizationsListComponent),
    canActivate: [superAdminGuard]
  },
  {
    path: 'admin/users',
    loadComponent: () =>
      import('./features/super-admin/all-users/all-users.component').then(m => m.AllUsersComponent),
    canActivate: [superAdminGuard]
  },
  {
    path: 'admin/leads',
    loadComponent: () =>
      import('./features/super-admin/lead-management/lead-management.component').then(m => m.LeadManagementComponent),
    canActivate: [superAdminGuard]
  },
  {
    path: 'customer/ticket/:id',
    loadComponent: () =>
      import('./features/customer/customer-ticket-detail/customer-ticket-detail.component').then(m => m.CustomerTicketDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: 'customer',
    loadComponent: () =>
      import('./features/customer/customer-portal/customer-portal.component').then(m => m.CustomerPortalComponent),
    canActivate: [authGuard]
  },
  {
    path: 'kb/create',
    loadComponent: () =>
      import('./features/knowledge-base/kb-create/kb-create.component').then(m => m.KbCreateComponent),
    canActivate: [authGuard, permissionGuard('knowledge-base')]
  },
  {
    path: 'kb/edit/:id',
    loadComponent: () =>
      import('./features/knowledge-base/kb-create/kb-create.component').then(m => m.KbCreateComponent),
    canActivate: [authGuard, permissionGuard('knowledge-base')]
  },
  {
    path: 'kb/:id',
    loadComponent: () =>
      import('./features/knowledge-base/kb-detail/kb-detail.component').then(m => m.KbDetailComponent),
    canActivate: [authGuard, permissionGuard('knowledge-base')]
  },
  {
    path: 'kb',
    loadComponent: () =>
      import('./features/knowledge-base/kb-list/kb-list.component').then(m => m.KbListComponent),
    canActivate: [authGuard, permissionGuard('knowledge-base')]
  },
  {
    path: 'insights/heatmap',
    loadComponent: () =>
      import('./features/insights/heatmap/heatmap.component').then(m => m.HeatmapComponent),
    canActivate: [authGuard, permissionGuard('analytics-heatmap'), featureGuard('analytics-heatmap')]
  },
  {
    path: 'settings/templates',
    loadComponent: () =>
      import('./features/settings/ticket-templates/ticket-templates.component').then(m => m.TicketTemplatesComponent),
    canActivate: [authGuard, permissionGuard('ticket-templates')]
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('./features/settings/settings-page/settings-page.component').then(m => m.SettingsPageComponent),
    canActivate: [authGuard, permissionGuard('settings')]
  },
  {
    path: 'audit',
    loadComponent: () =>
      import('./features/settings/audit-log/audit-log.component').then(m => m.AuditLogComponent),
    canActivate: [authGuard, permissionGuard('audit-log'), featureGuard('audit-log')]
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('./features/calendar/calendar-event/calendar-event.component').then(m => m.CalendarEventComponent),
    canActivate: [authGuard, permissionGuard('calendar')]
  },
  {
    path: 'explore-plans',
    loadComponent: () =>
      import('./features/subscription-plans/explore-plans/explore-plans.component').then(m => m.ExplorePlansComponent),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'plans-billing',
    loadComponent: () =>
      import('./features/subscription-plans/plans-billing/plans-billing.component').then(m => m.PlansBillingComponent),
    canActivate: [authGuard, companyAdminGuard]
  },
  {
    path: 'superadmin/pricing',
    loadComponent: () =>
      import('./features/super-admin/pricing-management/pricing-management.component').then(m => m.PricingManagementComponent),
    canActivate: [authGuard, superAdminGuard]
  },
  {
    path: 'superadmin/payments',
    loadComponent: () =>
      import('./features/super-admin/pending-payments/pending-payments.component').then(m => m.PendingPaymentsComponent),
    canActivate: [authGuard, superAdminGuard]
  },
  { path: '**', redirectTo: 'auth/login' }
];
