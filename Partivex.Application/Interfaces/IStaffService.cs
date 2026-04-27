using Partivex.Application.DTOs; // Imports staff DTO contracts.

namespace Partivex.Application.Interfaces; // Defines interface namespace.

public interface IStaffService // Defines staff service contract.
{
    Task<StaffDto> CreateStaffAsync(CreateStaffDto dto); // Creates staff account.

    Task<IEnumerable<StaffDto>> GetAllStaffAsync(); // Gets staff collection.

    Task<StaffDto> GetStaffByIdAsync(string id); // Gets staff by id.

    Task UpdateStaffAsync(string id, UpdateStaffDto dto); // Updates staff fields.

    Task DeleteStaffAsync(string id); // Deletes staff account.
}
