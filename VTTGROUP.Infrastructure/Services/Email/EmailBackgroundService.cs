using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model.Email;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services.Email
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IServiceProvider provider, ILogger<EmailBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var emails = await db.HtSendEmails
                    .AsNoTracking()
                    .Where(e => e.TrangThai == false)
                    .OrderBy(e => e.NgayLap)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (emails.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                foreach (var item in emails)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var atts = await db.HtSendEmailAttachments
                    .AsNoTracking()
                    .Where(a => a.EmailId == item.IdEmail)
                    .Select(a => new { a.Id, a.FileName, a.FileBytes })
                    .ToListAsync(stoppingToken);

                    var attachments = atts.Count > 0
                        ? atts.Select(a => (a.FileBytes, a.FileName)).ToList()
                        : null;

                    var history = new HtEmailHistory
                    {
                        Email = item.Email,
                        TieuDe = item.TieuDe,
                        NoiDung = item.NoiDung,
                        NgayLap = item.NgayLap,
                        NgayGui = null,
                        TrangThai = false,
                        NoiDungLoi = null,
                        FileDinhKem = (attachments is { Count: > 0 })
                            ? string.Join(';', attachments.Select(x => x.FileName))
                            : null
                    };

                    await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                    try
                    {
                        await emailService.SendEmailAsync(new EmailMessageModal
                        {
                            To = new List<string> { item.Email },
                            Subject = item.TieuDe,
                            BodyHtml = item.NoiDung,
                            Attachments = attachments
                        });

                        history.TrangThai = true;
                        history.NgayGui = DateTime.Now;

                        db.HtEmailHistories.Add(history);
                        await db.SaveChangesAsync(stoppingToken);

                        await db.HtSendEmailAttachments
                                .Where(a => a.EmailId == item.IdEmail)
                                .ExecuteDeleteAsync(stoppingToken);

                        await db.HtSendEmails
                            .Where(e => e.Id == item.Id)
                            .ExecuteDeleteAsync(stoppingToken);

                        await tx.CommitAsync(stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync(stoppingToken);

                        try
                        {
                            history.TrangThai = false;
                            history.NoiDungLoi = ex.Message;
                            db.HtEmailHistories.Add(history);
                            await db.SaveChangesAsync(stoppingToken);
                        }
                        catch (Exception writeErr)
                        {
                            _logger.LogError(writeErr, "Ghi lịch sử lỗi thất bại cho email {Email}", item.Email);
                        }

                        _logger.LogError(ex, "Lỗi gửi email tới: {Email}", item.Email);

                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
