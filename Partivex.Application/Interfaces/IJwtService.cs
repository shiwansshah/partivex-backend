using System.Collections.Generic;
using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
