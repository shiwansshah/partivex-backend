using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ICustomerAppointmentService
{
    IReadOnlyList<ServiceOptionDto> GetServiceOptions();

    Task<IReadOnlyList<AppointmentListDto>> GetAppointmentsAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentDetailDto>> GetAppointmentAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentDetailDto>> CreateAppointmentAsync(
        string customerId,
        CreateAppointmentDto dto,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentDetailDto>> CancelAppointmentAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);
}
