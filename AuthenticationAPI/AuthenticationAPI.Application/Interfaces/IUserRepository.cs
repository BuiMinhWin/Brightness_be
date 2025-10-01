using System;
using System.Threading.Tasks;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Interface;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IUserRepository : IGenericInterface<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UserNameExistsAsync(string userName, Guid? excludeUserId = null);
    }
}
