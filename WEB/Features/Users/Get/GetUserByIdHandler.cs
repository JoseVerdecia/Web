using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Users.Dto;

namespace WEB.Features.Users.Get;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, UserDto>
{
    private readonly ApplicationDbContext _context;

    public GetUserByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var query = from user in _context.Users
            join userRole in _context.UserRoles on user.Id equals userRole.UserId
            join role in _context.Roles on userRole.RoleId equals role.Id
            
            
            join proc in _context.Proceso on user.Id equals proc.JefeProcesoId into procGroup
            from proceso in procGroup.DefaultIfEmpty()
            
            
            join ar in _context.Area on user.Id equals ar.JefeAreaId into arGroup
            from area in arGroup.DefaultIfEmpty()
            
            where user.Id == request.Id
            select new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Role = role.Name,
          
                ProcesoId = proceso != null ? proceso.Id : (int?)null,
                ProcesoNombre = proceso != null ? proceso.Nombre : null,
                AreaId = area != null ? area.Id : (int?)null,
                AreaNombre = area != null ? area.Nombre : null
            };

        var userDto = await query.FirstOrDefaultAsync(cancellationToken);

        if (userDto == null)
            return Result<UserDto>.NotFound("Usuario no encontrado.");

        return Result<UserDto>.Success(userDto);
    }
}