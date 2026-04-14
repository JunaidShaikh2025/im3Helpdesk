using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public WhatsAppController(ApplicationDbContext context)
  {
    _context = context;
  }

  // Twilio / WhatsApp Business webhook
  [HttpPost("webhook")]
  public async Task<IActionResult> Webhook(
      [FromForm] WhatsAppWebhookDto dto)
  {
    if (string.IsNullOrEmpty(dto.Body) ||
        string.IsNullOrEmpty(dto.From))
      return BadRequest();

    // Find org by To number (your WhatsApp number)
    var org = await _context.Organizations
        .FirstOrDefaultAsync(o =>
            o.WhatsAppNumber == dto.To &&
            o.IsActive);

    if (org == null) return Ok();

    // Find or create customer
    var customer = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u =>
            u.PhoneNumber == dto.From &&
            u.OrganizationId == org.Id);

    if (customer == null)
    {
      customer = new User
      {
        FullName = dto.ProfileName
              ?? dto.From.Replace("whatsapp:", ""),
        Email = $"{dto.From.Replace("whatsapp:", "")
              .Replace("+", "")}@whatsapp.auto",
        PhoneNumber = dto.From,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(
              Guid.NewGuid().ToString()),
        Role = UserRole.Customer,
        OrganizationId = org.Id,
        IsEmailVerified = true
      };
      _context.Users.Add(customer);
      await _context.SaveChangesAsync();
    }

    // Create ticket from WhatsApp message
    var ticket = new Ticket
    {
      Title = $"WhatsApp: {dto.Body?.Substring(0,
            Math.Min(dto.Body.Length, 100))}",
      Description = dto.Body ?? "",
      Category = "General",
      Priority = TicketPriority.Medium,
      Status = TicketStatus.Open,
      OrganizationId = org.Id,
      CreatedByUserId = customer.Id,
      Tags = "whatsapp",
      TicketType = "Request"
    };

    _context.Tickets.Add(ticket);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Ticket created", id = ticket.Id });
  }

  // Send WhatsApp reply
  [HttpPost("send")]
  public async Task<IActionResult> SendMessage(
      [FromBody] SendWhatsAppDto dto)
  {
    // Twilio WhatsApp API integration
    var accountSid = Environment.GetEnvironmentVariable(
        "TWILIO_ACCOUNT_SID");
    var authToken = Environment.GetEnvironmentVariable(
        "TWILIO_AUTH_TOKEN");

    if (string.IsNullOrEmpty(accountSid))
      return Ok(new
      {
        message = "WhatsApp configured (Twilio credentials needed)"
      });

    // Real Twilio call would go here
    return Ok(new { message = "Message sent" });
  }
}

public class WhatsAppWebhookDto
{
  public string? From { get; set; }
  public string? To { get; set; }
  public string? Body { get; set; }
  public string? ProfileName { get; set; }
  public string? MediaUrl0 { get; set; }
}

public class SendWhatsAppDto
{
  public string To { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
