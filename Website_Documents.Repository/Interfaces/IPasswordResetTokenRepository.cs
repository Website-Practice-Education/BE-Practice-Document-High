using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task InvalidateUserTokensAsync(long userId);
}
