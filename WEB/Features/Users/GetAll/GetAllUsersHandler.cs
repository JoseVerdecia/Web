using Microsoft.EntityFrameworkCore;
using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Users.Dto;

namespace WEB.Features.Users.GetAll;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, PagedResult<UserDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllUsersHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
       
        var query = from user in _context.Users
            join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
            from ur in userRoles.DefaultIfEmpty()
            join role in _context.Roles on ur.RoleId equals role.Id into roles
            from r in roles.DefaultIfEmpty()
            
            join area in _context.Area.Where(a=>!a.IsDeleted) on user.Id equals area.JefeAreaId into areas
            from a in areas.DefaultIfEmpty()
            
            join proceso in _context.Proceso.Where(a=>!a.IsDeleted) on user.Id equals proceso.JefeProcesoId into procesos
            from p in procesos.DefaultIfEmpty()
            
            select new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Role = r != null ? r.Name : null,
                AreaId = a != null ? a.Id : null,
                AreaNombre = a != null ? a.Nombre : null,
                ProcesoId = p != null ? p.Id : null,
                ProcesoNombre = p != null ? p.Nombre : null
            };

      
        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(u => u.Role == request.Role);
        }
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(u => u.FullName.Contains(request.Name));
        }
        
        if (string.IsNullOrEmpty(request.SortBy))
        {
            query = query.OrderBy(u => u.Id);
        }
        else
        {
            bool isDescending = request.SortDirection?.ToLower() == "desc";
           
            query = request.SortBy switch
            {
                "FullName" => isDescending 
                    ? query.OrderByDescending(u => u.FullName) 
                    : query.OrderBy(u => u.FullName),
                
                "Email" => isDescending 
                    ? query.OrderByDescending(u => u.Email) 
                    : query.OrderBy(u => u.Email),
                
                "Role" => isDescending 
                    ? query.OrderByDescending(u => u.Role) 
                    : query.OrderBy(u => u.Role),
                
                _ => query.OrderBy(u => u.Id) 
            };
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<UserDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
}