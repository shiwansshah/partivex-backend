namespace Partivex.Application.DTOs;

public sealed record StaffFeatureAccessDto(
    string FeatureKey,
    string DisplayName,
    bool IsEnabled);

public sealed record StaffFeatureAccessSummaryDto(
    string UserId,
    IReadOnlyCollection<StaffFeatureAccessDto> Features);

public sealed record UpdateStaffFeatureAccessDto(
    IReadOnlyCollection<string> EnabledFeatureKeys);
