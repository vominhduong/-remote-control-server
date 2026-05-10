using System.Security.Cryptography;
using remoteControllerApp.DTOs;
using remoteControllerApp.Models;
using remoteControllerApp.Repositories;

namespace remoteControllerApp.Services;

public class UserService
{
    private readonly IRealtimeDatabaseRepository _repository;

    public UserService(IRealtimeDatabaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserInfoDto> CreateUserAsync(CreateUserDto request)
    {
        var now = DateTime.UtcNow;

        var userId = string.IsNullOrWhiteSpace(request.UserId)
            ? Guid.NewGuid().ToString("N")
            : request.UserId.Trim();

        var normalizedEmail = request.Email.Trim().ToLower();

        var existingUser = await FindUserByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new AppUser
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.SetAsync($"users/{user.UserId}", user);

        await _repository.SetAsync($"user_emails/{ToEmailKey(normalizedEmail)}", new
        {
            user.UserId,
            user.Email
        });

        return ToDto(user);
    }

    public async Task<UserInfoDto?> LoginAsync(LoginUserDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        var user = await FindUserByEmailAsync(normalizedEmail);

        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        var validPassword = VerifyPassword(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt
        );

        if (!validPassword)
        {
            return null;
        }

        return ToDto(user);
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
    {
        var user = await _repository.GetAsync<AppUser>($"users/{userId}");

        return user == null ? null : ToDto(user);
    }

    public async Task<List<UserInfoDto>> GetAllUsersAsync()
    {
        var users = await _repository.GetAsync<Dictionary<string, AppUser>>("users");

        if (users == null)
        {
            return new List<UserInfoDto>();
        }

        return users.Values
            .OrderBy(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto request)
    {
        var existingUser = await _repository.GetAsync<AppUser>($"users/{userId}");

        if (existingUser == null)
        {
            return false;
        }

        var updateData = new Dictionary<string, object?>
        {
            ["updatedAt"] = DateTime.UtcNow
        };

        if (request.DisplayName != null)
        {
            updateData["displayName"] = request.DisplayName;
        }

        if (request.Email != null)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            updateData["email"] = normalizedEmail;
        }

        if (request.Role != null)
        {
            updateData["role"] = request.Role;
        }

        if (request.IsActive.HasValue)
        {
            updateData["isActive"] = request.IsActive.Value;
        }

        await _repository.PatchAsync($"users/{userId}", updateData);

        return true;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var existingUser = await _repository.GetAsync<AppUser>($"users/{userId}");

        if (existingUser == null)
        {
            return false;
        }

        await _repository.PatchAsync($"users/{userId}", new
        {
            isActive = false,
            updatedAt = DateTime.UtcNow
        });

        return true;
    }

    private async Task<AppUser?> FindUserByEmailAsync(string email)
    {
        var emailKey = ToEmailKey(email);

        var emailIndex = await _repository.GetAsync<Dictionary<string, string>>(
            $"user_emails/{emailKey}"
        );

        if (emailIndex == null || !emailIndex.TryGetValue("userId", out var userId))
        {
            return null;
        }

        return await _repository.GetAsync<AppUser>($"users/{userId}");
    }

    private static void CreatePasswordHash(
        string password,
        out string passwordHash,
        out string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var saltBytes = RandomNumberGenerator.GetBytes(16);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );

        var hashBytes = pbkdf2.GetBytes(32);

        passwordHash = Convert.ToBase64String(hashBytes);
        passwordSalt = Convert.ToBase64String(saltBytes);
    }

    private static bool VerifyPassword(
        string password,
        string storedHash,
        string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );

        var hashBytes = pbkdf2.GetBytes(32);
        var hash = Convert.ToBase64String(hashBytes);

        return hash == storedHash;
    }

    private static string ToEmailKey(string email)
    {
        return email
            .Trim()
            .ToLower()
            .Replace(".", "_dot_")
            .Replace("@", "_at_");
    }

    private static UserInfoDto ToDto(AppUser user)
    {
        return new UserInfoDto
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}