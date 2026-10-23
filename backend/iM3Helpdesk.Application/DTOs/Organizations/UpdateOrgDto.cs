namespace iM3Helpdesk.Application.DTOs.Organizations;

public class UpdateOrgDto
{
  public string? Name { get; set; }
  public string? SupportEmail { get; set; }
  public string? BrandColor { get; set; }
  public string? LogoUrl { get; set; }
  public string? SmtpHost { get; set; }
  public int? SmtpPort { get; set; }
  public string? SmtpFromEmail { get; set; }
  public string? SmtpFromName { get; set; }
  public string? SmtpUsername { get; set; }
  public string? SmtpPassword { get; set; }
  public string? ImapHost { get; set; }
  public int? ImapPort { get; set; }
  public bool? EmailPollingEnabled { get; set; }
  public int? EmailPollingIntervalSeconds { get; set; }
  public string? Timezone { get; set; }
  public string? SlackWebhookUrl { get; set; }
  public string? TeamsWebhookUrl { get; set; }
  public string? WhatsAppNumber { get; set; }
  public string? TwilioAccountSid { get; set; }
  public string? TwilioAuthToken { get; set; }
  public int? RecycleBinRetentionValue { get; set; }
  public string? RecycleBinRetentionUnit { get; set; }
}
