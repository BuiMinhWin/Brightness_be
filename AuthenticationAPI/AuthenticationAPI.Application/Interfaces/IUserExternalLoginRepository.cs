using System;
using System.Threading.Tasks;
using AuthenticationAPI.Domain.Entities;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IUserExternalLoginRepository
    {
        Task<UserExternalLogin?> GetByProviderAsync(string provider, string providerKey);
        Task<UserExternalLogin?> GetByEmailAsync(string provider, string email);
        Task<UserExternalLogin?> GetByUserAsync(string provider, Guid userId);
        Task<UserExternalLogin?> CreateAsync(UserExternalLogin externalLogin);
    }
}
