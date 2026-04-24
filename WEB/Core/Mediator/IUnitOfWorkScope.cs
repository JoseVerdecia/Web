using WEB.Data.IRepository;

namespace WEB.Core.Mediator;

public interface IUnitOfWorkScope
{
    IUnitOfWork UnitOfWork { get; }
}