using ChatService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IMessageLimitService
    {
        Task<MessageLimitsDTO> GetMessageLimitsAsync(Guid userId);
        Task<bool> CanSendMessageAsync(Guid userId);
        Task IncrementMessageCountAsync(Guid userId);
    }
}

