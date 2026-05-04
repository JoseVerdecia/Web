    using WEB.Common;

    namespace WEB.Features.Proceso.Dto;

    public record ProcesoDto(
        int Id,
        string Nombre,
        UserSummaryDto Responsable,
        string ResponsableId,
        Enums.Evaluacion Evaluacion,
        IEnumerable<IndicadoresDeProcesoDto> Indicadores){
        public  bool IsSelected { get; set; }
    };