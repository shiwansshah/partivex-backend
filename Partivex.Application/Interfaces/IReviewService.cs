using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewListDto>> GetReviewsAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<ReviewDetailDto>> GetReviewAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<ReviewDetailDto>> CreateReviewAsync(
        string customerId,
        CreateReviewDto dto,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<ReviewDetailDto>> UpdateReviewAsync(
        Guid id,
        string customerId,
        UpdateReviewDto dto,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<Guid>> DeleteReviewAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);
}
