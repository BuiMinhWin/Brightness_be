using System;
using AuthenticationAPI.Application.DTOs;
using BrightNess.Library.Responses;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IUser
    {
        Task<Response> Register(UserDTO userDTO);
        Task<Response> Login(LoginDTO loginDTO);
        Task<GetUserDTO?> GetUser(Guid userId);
    }
}
