using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using BrightNess.Library.Responses;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<Response> RegisterAsync(UserDTO userDTO);
        Task<Response> LoginAsync(LoginDTO loginDTO);
        Task<GetUserDTO?> GetByIdAsync(Guid userId);
        Task<IEnumerable<GetUserDTO>> GetAllAsync();
        Task<Response> UpdateAsync(Guid userId, UpdateUserDTO userDTO);
        Task<Response> DeleteAsync(Guid userId);
    }
}
