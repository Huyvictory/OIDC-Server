using OidcServer.Models;

namespace OidcServer.Repository;

public interface ICodeItemRepository
{
    CodeItem? FindByCode(string code);

    void Add(string code, CodeItem codeItem);

    void Delete(string code);
}