using Hangfire;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MimeKit.Utils;
using System.Text.Json;

namespace iM3Helpdesk.Infrastructure.Services;

public class EmailQueueService : IEmailQueueService
{
  private static readonly TimeSpan[] RetrySchedule =
  {
    TimeSpan.FromMinutes(5),
    TimeSpan.FromMinutes(30),
    TimeSpan.FromHours(2)
  };

  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<EmailQueueService> _logger;

  public EmailQueueService(
      IServiceScopeFactory scopeFactory,
      ILogger<EmailQueueService> logger)
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  public async Task<EmailQueueEnqueueResult> QueueEmailAsync(QueuedEmailDispatch dispatch)
  {
    ArgumentNullException.ThrowIfNull(dispatch);

    var generatedMessageId = !string.IsNullOrWhiteSpace(dispatch.MessageId)
      ? dispatch.MessageId!.Trim()
      : MimeUtils.GenerateMessageId("im3helpdesk.local");

    dispatch.MessageId = generatedMessageId;

    using var scope = _scopeFactory.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var email = new EmailQueue
    {
      OrganizationId = dispatch.OrganizationId,
      ToEmail = dispatch.To,
      Subject = dispatch.Subject,
      Body = dispatch.HtmlBody,
      Status = "Queued",
      MessageId = generatedMessageId,
      PayloadJson = JsonSerializer.Serialize(dispatch),
      NextRetryAt = DateTime.UtcNow
    };

    context.EmailQueues.Add(email);
    await context.SaveChangesAsync();

    var jobId = BackgroundJob.Enqueue<IEmailQueueService>(s =>
        s.ProcessQueuedEmailAsync(email.Id));

    email.HangfireJobId = jobId;
    await context.SaveChangesAsync();

    return new EmailQueueEnqueueResult(email.Id, generatedMessageId);
  }

  public async Task ProcessQueuedEmailAsync(Guid emailQueueId)
  {
    using var scope = _scopeFactory.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    var dispatchService = scope.ServiceProvider
        .GetRequiredService<IEmailDispatchService>();

    var email = await context.EmailQueues
        .FirstOrDefaultAsync(e => e.Id == emailQueueId);

    if (email == null)
      return;

    if (email.Status is "Cancelled" or "Sent" or "Delivered" or "Bounced")
      return;

    if (email.NextRetryAt.HasValue && email.NextRetryAt.Value > DateTime.UtcNow)
    {
      var wait = email.NextRetryAt.Value - DateTime.UtcNow;
      email.HangfireJobId = BackgroundJob.Schedule<IEmailQueueService>(s =>
          s.ProcessQueuedEmailAsync(email.Id), wait).ToString();
      await context.SaveChangesAsync();
      return;
    }

    QueuedEmailDispatch? payload = null;
    try
    {
      payload = string.IsNullOrWhiteSpace(email.PayloadJson)
          ? new QueuedEmailDispatch
          {
            To = email.ToEmail,
            Subject = email.Subject,
            HtmlBody = email.Body,
            OrganizationId = email.OrganizationId,
            MessageId = email.MessageId
          }
          : JsonSerializer.Deserialize<QueuedEmailDispatch>(email.PayloadJson!);

      if (payload == null)
        throw new InvalidOperationException("Invalid queued email payload.");

      payload.MessageId ??= email.MessageId;

      email.Status = "Processing";
      email.LastAttemptAt = DateTime.UtcNow;
      await context.SaveChangesAsync();

      var providerMessageId = await dispatchService.SendDirectAsync(payload);

      email.Status = "Sent";
      email.IsSent = true;
      email.SentAt = DateTime.UtcNow;
      email.NextRetryAt = null;
      email.ErrorMessage = null;
      email.ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId)
          ? email.MessageId
          : providerMessageId;
    }
    catch (Exception ex)
    {
      email.RetryCount++;
      email.ErrorMessage = ex.Message;

      if (email.RetryCount <= RetrySchedule.Length)
      {
        email.Status = "Queued";
        email.NextRetryAt = DateTime.UtcNow.Add(RetrySchedule[email.RetryCount - 1]);
        email.HangfireJobId = BackgroundJob.Schedule<IEmailQueueService>(s =>
            s.ProcessQueuedEmailAsync(email.Id), RetrySchedule[email.RetryCount - 1]);
      }
      else
      {
        email.Status = "Failed";
        email.FailedAt = DateTime.UtcNow;
        email.NextRetryAt = null;
      }

      _logger.LogError(ex, "Email queue item {EmailId} failed on attempt {Attempt}", email.Id, email.RetryCount);
    }

    await context.SaveChangesAsync();
  }

  public async Task RetryEmailAsync(Guid emailQueueId)
  {
    using var scope = _scopeFactory.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var email = await context.EmailQueues
        .FirstOrDefaultAsync(e => e.Id == emailQueueId);

    if (email == null)
      throw new KeyNotFoundException("Queued email not found.");

    email.Status = "Queued";
    email.RetryCount = 0;
    email.ErrorMessage = null;
    email.FailedAt = null;
    email.CancelledAt = null;
    email.NextRetryAt = DateTime.UtcNow;

    email.HangfireJobId = BackgroundJob.Enqueue<IEmailQueueService>(s =>
        s.ProcessQueuedEmailAsync(email.Id));

    await context.SaveChangesAsync();
  }

  public async Task CancelQueuedEmailAsync(Guid emailQueueId)
  {
    using var scope = _scopeFactory.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var email = await context.EmailQueues
        .FirstOrDefaultAsync(e => e.Id == emailQueueId);

    if (email == null)
      throw new KeyNotFoundException("Queued email not found.");

    if (email.Status is "Sent" or "Delivered" or "Bounced")
      return;

    email.Status = "Cancelled";
    email.CancelledAt = DateTime.UtcNow;
    email.NextRetryAt = null;

    if (!string.IsNullOrWhiteSpace(email.HangfireJobId))
      BackgroundJob.Delete(email.HangfireJobId);

    await context.SaveChangesAsync();
  }
}
