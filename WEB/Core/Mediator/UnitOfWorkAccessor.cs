using WEB.Data.IRepository;
using WEB.Core.Interfaces;

namespace WEB.Core.Mediator;

public class UnitOfWorkAccessor : IUnitOfWorkAccessor
{
    private static readonly AsyncLocal<IUnitOfWorkScope?> _currentScope = new();

    public static IUnitOfWorkScope? CurrentScope
    {
        get => _currentScope.Value;
        set => _currentScope.Value = value;
    }

    public IUnitOfWork? Current => CurrentScope?.UnitOfWork;
}