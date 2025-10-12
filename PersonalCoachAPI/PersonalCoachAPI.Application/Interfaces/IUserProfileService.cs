using System;
using System.Threading.Tasks;
using PersonalCoachAPI.Application.DTOs;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileResponse> GetUserProfileAsync(Guid userId);
        Task<UserProfileResponse> CreateOrUpdateProfileAsync(CreateUserProfileRequest request);
    }
}

