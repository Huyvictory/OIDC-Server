using OidcServer.Models;

namespace OidcServer.Repository;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new List<User>()
    {
        new User() { Name = "user1" },
        new User() { Name = "user2" },
    };

    public User? FindByName(string name)
    {
        return _users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}