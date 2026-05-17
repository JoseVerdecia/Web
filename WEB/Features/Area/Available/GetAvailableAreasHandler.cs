using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Users.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Available;

public class GetAvailableAreasHandler 
    : IRequestHandler<GetAvailableAreasRequest, List<AvailableUserDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAvailableAreasHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<List<AvailableUserDto>>> Handle(GetAvailableAreasRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<AreaModel> areas = await _uow.Current.Area.GetAllBy(a => a.JefeAreaId == null,cancellationToken);

        List<AvailableUserDto> AppResult = areas.Select(a => new AvailableUserDto(a.Id, a.Nombre)).ToList();

        return AppResult<List<AvailableUserDto>>.Success(AppResult);
    }
}