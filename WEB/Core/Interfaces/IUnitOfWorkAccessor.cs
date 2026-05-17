using WEB.Data.IRepository;

namespace WEB.Core.Interfaces;

public interface IUnitOfWorkAccessor
{
    IUnitOfWork? Current { get; }
}