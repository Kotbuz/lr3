using UsersProxy.Models;

namespace UsersProxy
{
    // HTTP-прокси к API смежной системы учёта пользователей
    public interface IUsersApiClient
    {
        // GET /users/{id}; null — пользователь не найден
        Task<ExternalUser?> GetUserAsync(string id, CancellationToken ct = default);

        // POST /users/true
        Task<ExternalUser> CreateUserAsync(ExternalUser user, CancellationToken ct = default);

        // POST /users/false
        Task<ExternalUser> UpdateUserAsync(ExternalUser user, CancellationToken ct = default);

        // GET /roles
        Task<List<string>> GetRolesAsync(CancellationToken ct = default);

        // POST /roles
        Task CreateRoleAsync(string name, CancellationToken ct = default);
    }
}
