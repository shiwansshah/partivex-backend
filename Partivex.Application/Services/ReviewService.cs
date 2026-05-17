using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;

namespace Partivex.Application.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICustomerAppointmentRepository _appointmentRepository;

    public ReviewService(IReviewRepository reviewRepository, ICustomerAppointmentRepository appointmentRepository)
    {
        _reviewRepository = reviewRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<ReviewListDto>> GetReviewsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        return reviews.Select(MapList).ToArray();
    }

    public async Task<CustomerPortalResult<ReviewDetailDto>> GetReviewAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken);

        if (review is null || review.CustomerId != customerId)
        {
            return CustomerPortalResult<ReviewDetailDto>.NotFound("Review not found.");
        }

        return CustomerPortalResult<ReviewDetailDto>.Success(MapDetail(review));
    }

    public async Task<CustomerPortalResult<ReviewDetailDto>> CreateReviewAsync(
        string customerId,
        CreateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateReview(dto.Rating, dto.Comment);
        Appointment? appointment = null;
        var appointmentId = NormalizeAppointmentId(dto.AppointmentId);

        if (appointmentId.HasValue)
        {
            appointment = await _appointmentRepository.GetByIdAsync(appointmentId.Value, cancellationToken);

            if (appointment is null || appointment.CustomerId != customerId)
            {
                return CustomerPortalResult<ReviewDetailDto>.NotFound("Appointment not found.");
            }

            if (appointment.Status != AppointmentStatus.Completed)
            {
                errors.Add(new CustomerPortalError(nameof(dto.AppointmentId), "Only completed appointments can be reviewed."));
            }

            if (await _reviewRepository.ExistsForAppointmentAsync(customerId, appointment.Id, null, cancellationToken))
            {
                errors.Add(new CustomerPortalError(nameof(dto.AppointmentId), "You have already reviewed this appointment."));
            }
        }

        if (errors.Count > 0)
        {
            return CustomerPortalResult<ReviewDetailDto>.Failed(errors, "Review could not be submitted.");
        }

        var now = DateTimeOffset.UtcNow;
        var review = new Review
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            AppointmentId = appointment?.Id,
            Category = appointment is null ? ReviewCategory.General : ReviewCategory.Appointment,
            Rating = dto.Rating,
            Comment = NormalizeRequired(dto.Comment),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        var saved = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return CustomerPortalResult<ReviewDetailDto>.Success(MapDetail(saved!));
    }

    public async Task<CustomerPortalResult<ReviewDetailDto>> UpdateReviewAsync(
        Guid id,
        string customerId,
        UpdateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken);

        if (review is null || review.CustomerId != customerId)
        {
            return CustomerPortalResult<ReviewDetailDto>.NotFound("Review not found.");
        }

        var errors = ValidateReview(dto.Rating, dto.Comment);
        if (errors.Count > 0)
        {
            return CustomerPortalResult<ReviewDetailDto>.Failed(errors, "Review could not be updated.");
        }

        review.Rating = dto.Rating;
        review.Comment = NormalizeRequired(dto.Comment);
        review.UpdatedAt = DateTimeOffset.UtcNow;

        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return CustomerPortalResult<ReviewDetailDto>.Success(MapDetail(review));
    }

    public async Task<CustomerPortalResult<Guid>> DeleteReviewAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken);

        if (review is null || review.CustomerId != customerId)
        {
            return CustomerPortalResult<Guid>.NotFound("Review not found.");
        }

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return CustomerPortalResult<Guid>.Success(id);
    }

    private static List<CustomerPortalError> ValidateReview(int rating, string? comment)
    {
        var errors = new List<CustomerPortalError>();

        if (rating is < 1 or > 5)
        {
            errors.Add(new CustomerPortalError(nameof(rating), "Rating must be between 1 and 5."));
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            errors.Add(new CustomerPortalError(nameof(comment), "Review comment is required."));
        }

        return errors;
    }

    private static ReviewListDto MapList(Review review)
    {
        return new ReviewListDto(
            review.Id,
            review.AppointmentId,
            review.Category.ToString(),
            review.Appointment?.ServiceType,
            review.Appointment is null ? null : DateOnly.FromDateTime(review.Appointment.PreferredAt.DateTime),
            review.Appointment?.Status.ToString(),
            review.Rating,
            review.Comment,
            review.CreatedAt,
            review.UpdatedAt);
    }

    private static ReviewDetailDto MapDetail(Review review)
    {
        return new ReviewDetailDto(
            review.Id,
            review.AppointmentId,
            review.Category.ToString(),
            review.Appointment?.ServiceType,
            review.Appointment is null ? null : DateOnly.FromDateTime(review.Appointment.PreferredAt.DateTime),
            review.Appointment?.Status.ToString(),
            review.Rating,
            review.Comment,
            review.CreatedAt,
            review.UpdatedAt);
    }

    private static Guid? NormalizeAppointmentId(Guid? appointmentId)
    {
        return appointmentId.HasValue && appointmentId.Value != Guid.Empty ? appointmentId : null;
    }

    private static string NormalizeRequired(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
