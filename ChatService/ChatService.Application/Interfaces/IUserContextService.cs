using ChatService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IUserContextService
    {
        Task<UserContextDTO> BuildUserContextAsync(Guid userId, Guid? sessionId = null);
    }
}

