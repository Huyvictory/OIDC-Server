using OidcServer.Models;

namespace OidcServer.Repository;

public class CodeItemRepository : ICodeItemRepository
{
    private readonly Dictionary<string, CodeItem> _codeItems = new Dictionary<string, CodeItem>();

    public CodeItem? FindByCode(string code)
    {
        return _codeItems.GetValueOrDefault(code);
    }

    public void Add(string code, CodeItem codeItem)
    {
        _codeItems.Add(code, codeItem);
    }

    public void Delete(string code)
    {
        _codeItems.Remove(code);
    }
}