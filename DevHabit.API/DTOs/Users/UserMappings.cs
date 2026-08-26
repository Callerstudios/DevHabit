using DevHabit.API.DTOs.Auth;
using DevHabit.API.Entities;

namespace DevHabit.API.DTOs.Users
{
    public static class UserMappings
    {
        public static User ToEntity(this RegisterUserDto dto)
        {
            return new User
            {
                Id = $"u_{Guid.CreateVersion7()}",
                Email = dto.Email,
                Name = dto.Name,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
    }
}
