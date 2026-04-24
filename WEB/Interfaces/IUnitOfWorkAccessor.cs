using WEB.Data.IRepository;

namespace WEB.Interfaces;

public interface IUnitOfWorkAccessor
{
    IUnitOfWork? Current { get; }
}