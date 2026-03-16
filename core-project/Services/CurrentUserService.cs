using System;
using System.Security.Claims;
using MyApp.Services;
using User.Models;

namespace Library.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private User.Models.User? _cachedUser;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public int UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null)
                    return 0;

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? user.FindFirst("sub")?.Value;

                if (int.TryParse(userIdClaim, out int id))
                    return id;

                return 0;
            }
        }

        public string UserName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            }
        }

        public string UserRole
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            }
        }

        public User.Models.User? User
        {
            get
            {
                if (UserId == 0)
                    return null;

                if (_cachedUser == null || _cachedUser.Id != UserId)
                {
                    _cachedUser = _userService.GetById(UserId);
                }

                return _cachedUser;
            }
        }

        public bool IsAuthenticated
        {
            get => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsAdmin
        {
            get => UserRole == "Admin";
        }
    }
}
