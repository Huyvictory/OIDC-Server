using OidcServer.Models;

namespace OidcServer.Repository;

public interface IUserRepository
{
    User? FindByName(string name);
}