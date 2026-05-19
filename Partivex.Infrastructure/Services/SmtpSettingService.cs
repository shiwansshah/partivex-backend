using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class SmtpSettingService : ISmtpSettingService
{
    private readonly AppDbContext _dbContext;

    public SmtpSettingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SmtpSettingDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.SmtpSettings.AsNoTracking().OrderBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        return new SmtpSettingDto(setting?.SenderEmail ?? string.Empty);
    }

    public async Task<SmtpSettingDto> UpdateAsync(UpdateSmtpSettingDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.SmtpSettings.OrderBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        if (setting is null)
        {
            setting = new SmtpSetting();
            await _dbContext.SmtpSettings.AddAsync(setting, cancellationToken);
        }

        setting.SenderEmail = dto.SenderEmail.Trim();
        setting.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SmtpSettingDto(setting.SenderEmail);
    }
}
