
namespace iM3Helpdesk.Domain.Enums;

public enum UserRole
{
    SuperAdmin,
    CompanyAdmin,
    Agent,
    [Obsolete("Customer role is deprecated. External customers communicate via email without user accounts.")]
    Customer
}