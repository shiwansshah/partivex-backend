using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ISmtpSettingService
{
    Task<SmtpSettingDto> GetAsync(CancellationToken cancellationToken = default);

    Task<SmtpSettingDto> UpdateAsync(UpdateSmtpSettingDto dto, CancellationToken cancellationToken = default);
}
