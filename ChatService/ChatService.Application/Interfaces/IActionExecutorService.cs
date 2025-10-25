using ChatService.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IActionExecutorService
    {
        Task<ActionResult> ExecuteActionAsync(Guid userId, IntentDetectionResult intent, UserContextDTO userContext);
    }
}

