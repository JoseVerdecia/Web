using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WEB.Data.IRepository;
using WEB.Interfaces;

namespace WEB.Data.Repository;

public class Repository<T>:IRepository<T> where T:class
{
    private readonly ApplicationDbContext  _context;
    internal DbSet<T> dbSet;
    
    public Repository(ApplicationDbContext context )
    {
        _context = context;
        dbSet = _context.Set<T>();
    }
    public async Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken , string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        
        query = IncludeProperties(query,includeProperties);
      
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsNoTracking(CancellationToken cancellationToken ,string includeProperties = null)
    {
        IQueryable<T> query = dbSet.AsNoTracking();
        
        query = IncludeProperties(query,includeProperties);
      
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken , string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        
        query = query.Where(predicate);
        
        query = IncludeProperties(query,includeProperties);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllByAsNoTracking(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken ,string includeProperties = null)
    {
        IQueryable<T> query = dbSet.AsNoTracking();
        
        query = query.Where(predicate);
        
        query = IncludeProperties(query,includeProperties);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> Get(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken ,string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        
        query = query.Where(predicate);
        
        query = IncludeProperties(query,includeProperties);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    

    public T GetSync(Expression<Func<T, bool>> predicate , string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        
        query = query.Where(predicate);
        
        query = IncludeProperties(query,includeProperties);

        return query.FirstOrDefault();
    }

    public async Task<T?> GetAsNoTracking(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken , string includeProperties = null)
    {
        IQueryable<T> query = dbSet.AsNoTracking();
        
        query = query.Where(predicate);
        
        query = IncludeProperties(query,includeProperties);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken ,Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<T?> GetById(int id,CancellationToken cancellationToken) =>  await dbSet.FindAsync(id);
    

    public void Delete(T entity) => _context.Remove(entity);

    public async Task<IEnumerable<T>> GetAllIncludingDeleted(CancellationToken cancellationToken ,string includeProperties = null)
    {
        IQueryable<T> query = dbSet.IgnoreQueryFilters();
        
        query = IncludeProperties(query,includeProperties);
      
        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<T>> GetAllByIncludingDeleted(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken, 
        string? includeProperties = null)
    {
        IQueryable<T> query = dbSet.IgnoreQueryFilters();
    
        query = query.Where(predicate);
        query = IncludeProperties(query, includeProperties);
      
        return await query.ToListAsync(cancellationToken);
    }

    public void DeleteRange(IEnumerable<T> entities) => _context.RemoveRange(entities);
    public void UpdateRange(IEnumerable<T> entities)
    {
        _context.UpdateRange(entities);
    }

    public void Add(T entity)
    {
        _context.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _context.AddRange(entities);
    }

    public void SoftDelete(T entity)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            _context.Update(entity); 
        }
        else
        {
            throw new InvalidOperationException($"La entidad {typeof(T).Name} no soporta SoftDelete.");
        }
    }
    
    public async Task<T?> GetIncludingDeleted(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken ,
        string? includeProperties = null)
    {
        IQueryable<T> query = dbSet.IgnoreQueryFilters(); 
        query = IncludeProperties(query,includeProperties);
        return await query.FirstOrDefaultAsync(predicate,cancellationToken);
    }
    
    public IQueryable<T> IncludeProperties(IQueryable<T> query, string includeProperties)
    {
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }
        return query;
    }
    
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken ,
        Expression<Func<T, bool>>? predicate = null,
        string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(prop);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken ,
        Expression<Func<T, bool>>? predicate = null,
        string? includeProperties = null)
    {
        IQueryable<T> query = dbSet.IgnoreQueryFilters(); 

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(prop);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}