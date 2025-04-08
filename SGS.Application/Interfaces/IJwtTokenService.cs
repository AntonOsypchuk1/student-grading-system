using SGS.Domain.Entities;

namespace SGS.Application.Interfaces;

public interface ITokenService
{
    /// <summary>
    ///     Generates a JWT token for the given user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A JWT token as a string.</returns>
    string GenerateToken(User user);
}