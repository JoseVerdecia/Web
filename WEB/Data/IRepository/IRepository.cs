using System.Linq.Expressions;

namespace WEB.Data.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default,string includeProperties = null);
    
    Task<IEnumerable<T>> GetAllAsNoTracking(CancellationToken cancellationToken = default,string includeProperties = null);
    Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken = default,string includeProperties = null);
    
    Task<IEnumerable<T>> GetAllByAsNoTracking(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken = default,string includeProperties = null);
    Task<T?> Get(Expression<Func<T,bool>> predicate ,CancellationToken cancellationToken = default, string includeProperties = null);
    T GetSync(Expression<Func<T,bool>> predicate ,string includeProperties = null);
    
    Task<T?> GetAsNoTracking(Expression<Func<T,bool>> predicate , CancellationToken cancellationToken = default,string includeProperties = null);
    
    Task<int> CountAsync(CancellationToken cancellationToken = default,Expression<Func<T, bool>>? predicate = null);
    
    Task<T?> GetById(int id,CancellationToken cancellationToken = default);
    void Delete(T entity);
    void SoftDelete(T entity);
    
    Task<T?> GetIncludingDeleted(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken = default,string includeProperties = null);
    Task<IEnumerable<T>> GetAllIncludingDeleted(CancellationToken cancellationToken = default,string includeProperties = null);
    Task<IEnumerable<T>> GetAllByIncludingDeleted(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken, 
        string? includeProperties = null);
    void DeleteRange(IEnumerable<T> entities);
    void UpdateRange(IEnumerable<T> entities);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default,
        Expression<Func<T, bool>>? predicate = null,
        string? includeProperties = null);
    
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default,
        Expression<Func<T, bool>>? predicate = null,
        string? includeProperties = null);
}