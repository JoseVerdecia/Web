using ClosedXML.Excel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Enums;
using WEB.Features.Area;
using WEB.Features.Proceso;
using WEB.Features.Objetivo;
using WEB.Features.Indicador;
using WEB.Interfaces;

namespace WEB.Services
{
    public class ExcelExportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ExcelExportService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

 
        
        public async Task<byte[]> ExportIndicadoresToExcelAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;

            try
            {
                var indicadores = await unitOfWork.Indicador.GetAll(includeProperties: "Proceso,Objetivos");
                var indicadorDtos = indicadores.Select(i => i.MapToDto()).ToList();
                using var workbook = new XLWorkbook();
                
                if (!indicadores.Any())
                {
                    var emptySheet = workbook.Worksheets.Add("Sin Indicadores");
                    emptySheet.Cell(1, 1).Value = "No hay Indicadores registrados.";
                    using var newStream = new MemoryStream();
                    workbook.SaveAs(newStream);
                    return newStream.ToArray();
                }
                var worksheet = workbook.Worksheets.Add("Indicadores");
                
                worksheet.Cell(1, 1).Value = "Nombre";
                worksheet.Cell(1, 2).Value = "Meta Cumplir";
                worksheet.Cell(1, 3).Value = "Meta Real";
                worksheet.Cell(1, 4).Value = "Evaluación";
                worksheet.Cell(1, 5).Value = "Origen";
                worksheet.Cell(1, 6).Value = "Tipo";
                worksheet.Cell(1, 7).Value = "Proceso";
                worksheet.Cell(1, 8).Value = "Objetivos";
                
                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
                
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                int row = 2;
                foreach (var indicador in indicadorDtos)
                {
                    worksheet.Cell(row, 1).Value = indicador.Nombre;
                    worksheet.Cell(row, 2).Value = indicador.MetaCumplir;
                    worksheet.Cell(row, 3).Value = indicador.MetaReal ?? "-";
                    worksheet.Cell(row, 4).Value = indicador.Evaluacion.ToString();
                    worksheet.Cell(row, 5).Value = indicador.Origen.ToString();
                    worksheet.Cell(row, 6).Value = indicador.Tipo.ToString();
                    worksheet.Cell(row, 7).Value = indicador.Proceso?.Nombre ?? "Sin asignar";
                    worksheet.Cell(row, 8).Value = string.Join("|", indicador.Objetivos.Select(o => o.NumeroObjetivo));
                    
                     if (indicador.Tipo == IndicadorTipo.Escencial)
                     { 
                         worksheet.Cell(row, 1).Style.Font.Bold = true;
                     }
                                        
                    var dataRange = worksheet.Range(row, 1, row, 8);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; 
                    worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; 
                    
                    row++;
                }
                
                worksheet.Columns().AdjustToContents();
                
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally
            {
                UnitOfWorkAccessor.CurrentScope = null;
            }
        }
        public async Task<byte[]> ExportAllProcesosDetailedToExcelAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;
            
            

            try
            {
                var procesos = await unitOfWork.Proceso.GetAll(includeProperties: "Indicadores.Objetivos");
                using var workbook = new XLWorkbook();
                
                if (!procesos.Any())
                {
                    var emptySheet = workbook.Worksheets.Add("Sin Procesos");
                    emptySheet.Cell(1, 1).Value = "No hay procesos registrados.";
                    using var newStream = new MemoryStream();
                    workbook.SaveAs(newStream);
                    return newStream.ToArray();
                }

                foreach (var proceso in procesos)
                {
                    var worksheet = workbook.Worksheets.Add(proceso.Nombre.Length > 31 ? proceso.Nombre.Substring(0, 31) : proceso.Nombre);


                    worksheet.Cell(1, 1).Value = $"PROCESO: {proceso.Nombre}";
                    worksheet.Range(1, 1, 1, 3).Merge();
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Row(1).Height = 22;


                    worksheet.Cell(3, 1).Value = "OE";
                    worksheet.Cell(3, 2).Value = "INDICADOR";
                    worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
                    var headerRange = worksheet.Range(3, 1, 3, 3);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int row = 4;
                    foreach (var indicador in proceso.Indicadores)
                    {
                        string objetivos = indicador.Objetivos != null && indicador.Objetivos.Any()
                            ? string.Join("|", indicador.Objetivos.Select(o => o.NumeroObjetivo))
                            : "";
                        worksheet.Cell(row, 1).Value = objetivos;
                        worksheet.Cell(row, 2).Value = indicador.Nombre;
                        worksheet.Cell(row, 3).Value = indicador.MetaCumplir;

                        var dataRange = worksheet.Range(row, 1, row, 3);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally
            {
                UnitOfWorkAccessor.CurrentScope = null;
            }
        } 
        public async Task<byte[]> ExportAllObjetivosDetailedToExcelAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivos = await unitOfWork.Objetivo.GetAll(includeProperties: "Indicadores.Proceso");
        using var workbook = new XLWorkbook();

        if (!objetivos.Any())
        {
            var emptySheet = workbook.Worksheets.Add("Sin Objetivos");
            emptySheet.Cell(1, 1).Value = "No hay Objetivos registrados.";
            using var newStream = new MemoryStream();
            workbook.SaveAs(newStream);
            return newStream.ToArray();
        }
        
        foreach (var objetivo in objetivos)
        {
            var worksheet = workbook.Worksheets.Add(
                objetivo.Nombre.Length > 31 ? objetivo.Nombre.Substring(0, 31) : objetivo.Nombre
            );

           
            worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATEGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}";
            worksheet.Range(1, 1, 1, 3).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Row(1).Height = 22;

        
            worksheet.Cell(3, 1).Value = "Proceso";
            worksheet.Cell(3, 2).Value = "INDICADOR";
            worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
            var headerRange = worksheet.Range(3, 1, 3, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;
            var indicadoresPorProceso = objetivo.Indicadores
                .Where(i => i.Proceso != null)
                .GroupBy(i => i.Proceso)
                .OrderBy(g => g.Key.Nombre);

            foreach (var grupo in indicadoresPorProceso)
            {
                var proceso = grupo.Key;
                var indicadores = grupo.ToList();
                int startRow = row;
                bool first = true;

                foreach (var indicador in indicadores)
                {
                    worksheet.Cell(row, 1).Value = first ? proceso.Nombre : "";
                    worksheet.Cell(row, 2).Value = indicador.Nombre;
                    worksheet.Cell(row, 3).Value = indicador.MetaCumplir;

                    var dataRange = worksheet.Range(row, 1, row, 3);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    row++;
                    first = false;
                }
                
                if (indicadores.Count > 1)
                {
                    worksheet.Range(startRow, 1, row - 1, 1).Merge();
                    worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }

        
}


        public async Task<byte[]> ExportAllObjetivosDetailedToPdfAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivos = await unitOfWork.Objetivo.GetAll(includeProperties: "Indicadores.Proceso");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    bool first = true;
                    foreach (var objetivo in objetivos)
                    {
                        if (!first)
                        {
                            column.Item().PageBreak();
                        }
                        first = false;
                        
                        column.Item()
                            .Text($"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}")
                            .FontSize(10)
                            .Bold().Justify();

                        column.Item().PaddingTop(10);
                        
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(1);
                            });

                            table.ExtendLastCellsToTableBottom();

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Padding(4)
                                    .Text("Proceso")
                                    .FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text("INDICADOR")
                                    .FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(DateTime.Now.Year.ToString())
                                    .FontSize(10).Bold();
                            });

                            var indicadoresPorProceso = objetivo.Indicadores
                                .Where(i => i.Proceso != null)
                                .GroupBy(i => i.Proceso)
                                .OrderBy(g => g.Key.Nombre);

                            foreach (var grupo in indicadoresPorProceso)
                            {
                                var proceso = grupo.Key;
                                var indicadores = grupo.ToList();
                                int rowCount = indicadores.Count;

                                var bgColor = Colors.White;

                                table.Cell()
                                    .RowSpan((uint)rowCount)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(bgColor)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(proceso.Nombre)
                                    .FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(bgColor)
                                    .AlignMiddle()
                                    .Padding(4)
                                    .Text(indicadores[0].Nombre)
                                    .FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(bgColor)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(indicadores[0].MetaCumplir)
                                    .FontSize(10);

                                for (int i = 1; i < rowCount; i++)
                                {
                                    bgColor = Colors.White;

                                    if (indicadores[i].Tipo == IndicadorTipo.Escencial)
                                    {
                                        table.Cell()
                                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                                            .Background(bgColor)
                                            .AlignMiddle()
                                            .Padding(4)
                                            .Text(indicadores[i].Nombre).Bold()
                                            .FontSize(10);
                                    }
                                    else
                                    {
                                        table.Cell()
                                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                                            .Background(bgColor)
                                            .AlignMiddle()
                                            .Padding(4)
                                            .Text(indicadores[i].Nombre)
                                            .FontSize(10);
                                    }

                                    table.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .Background(bgColor)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Padding(4)
                                        .Text(indicadores[i].MetaCumplir?.ToString() ?? "")
                                        .FontSize(10);
                                }
                            }
                        });
                        
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

        public async Task<byte[]> ExportAllProcesosDetailedToPdfAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var procesos = await unitOfWork.Proceso.GetAll(includeProperties: "Indicadores.Objetivos");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    bool first = true;
                    foreach (var proceso in procesos)
                    {
                        if (!first)
                        {
                            column.Item().PageBreak();
                        }
                        first = false;

                        column.Item()
                            .Text($"PROCESO: {proceso.Nombre}")
                            .FontSize(11)
                            .Bold();

                        column.Item().PaddingTop(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); 
                                columns.RelativeColumn(3); 
                                columns.RelativeColumn(1); 
                            });

                            table.ExtendLastCellsToTableBottom();

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text("OE")
                                    .FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text("INDICADOR")
                                    .FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(DateTime.Now.Year.ToString())
                                    .FontSize(10).Bold();
                            });

                            foreach (var indicador in proceso.Indicadores)
                            {
                                var bgColor = Colors.White;

                                string objetivos = indicador.Objetivos != null && indicador.Objetivos.Any()
                                    ? string.Join("|", indicador.Objetivos.Select(o => o.NumeroObjetivo))
                                    : "";

                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(bgColor)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(objetivos)
                                    .FontSize(10);

                                if (indicador.Tipo == IndicadorTipo.Escencial)
                                {
                                    table.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .Background(bgColor)
                                        .AlignMiddle()
                                        .Padding(4)
                                        .Text(indicador.Nombre).Bold()
                                        .FontSize(10);
                                }
                                else
                                {
                                    table.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .Background(bgColor)
                                        .AlignMiddle()
                                        .Padding(4)
                                        .Text(indicador.Nombre)
                                        .FontSize(10);
                                }

                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(bgColor)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Padding(4)
                                    .Text(indicador.MetaCumplir?.ToString() ?? "")
                                    .FontSize(10);
                            }
                        });
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        
        public async Task<byte[]> ExportIndicadoresToPdfAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var indicadores = await unitOfWork.Indicador.GetAll(includeProperties: "Proceso,Objetivos");
        
        var indicadoresOrdenados = indicadores
            .OrderBy(i => i.Proceso?.Nombre ?? "Sin asignar")
            .ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                
                page.Content().Table(table =>
                {
                    
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);  
                        columns.RelativeColumn(1); 
                        columns.RelativeColumn(1); 
                        columns.RelativeColumn(1); 
                        columns.RelativeColumn(1); 
                        columns.RelativeColumn(1); 
                        columns.RelativeColumn(2); 
                        columns.RelativeColumn(1); 
                    });

                    table.ExtendLastCellsToTableBottom();

                    
                    table.Header(header =>
                    {
                       
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(4)
                            .Text("Nombre")
                            .FontSize(10).Bold();
                       
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Meta Cumplir")
                            .FontSize(10).Bold();
                        
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Meta Real")
                            .FontSize(10).Bold();
                       
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Evaluación")
                            .FontSize(10).Bold();
                       
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Origen")
                            .FontSize(10).Bold();
                       
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Tipo")
                            .FontSize(10).Bold();
                        
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Proceso")
                            .FontSize(10).Bold();
                        
                        header.Cell()
                            .Background(Colors.Grey.Lighten2)
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .AlignCenter()
                            .Padding(4)
                            .Text("Objetivos")
                            .FontSize(10).Bold();
                    });
                    
                    uint rowIndex = 0;
                    foreach (var indicador in indicadoresOrdenados)
                    {
                        var bgColor = (rowIndex % 2 == 0) ? Colors.Grey.Lighten3 : Colors.White;
                        bool esEsencial = indicador.Tipo == IndicadorTipo.Escencial;
                        
                        var nombreCell = table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .Padding(4);
                        if (esEsencial)
                            nombreCell.Text(indicador.Nombre).Bold().FontSize(10);
                        else
                            nombreCell.Text(indicador.Nombre).FontSize(10);
                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(indicador.MetaCumplir?.ToString() ?? "")
                            .FontSize(10);
                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(indicador.MetaReal?.ToString() ?? "-")
                            .FontSize(10);
                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .Padding(4)
                            .Text(indicador.Evaluacion.ToString())
                            .FontSize(10);
                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(indicador.Origen.ToString())
                            .FontSize(10);
                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(indicador.Tipo.ToString())
                            .FontSize(10);

                        
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(indicador.Proceso?.Nombre ?? "Sin asignar")
                            .FontSize(10);

                        
                        string objetivos = indicador.Objetivos != null && indicador.Objetivos.Any()
                            ? string.Join("|", indicador.Objetivos.Select(o => o.NumeroObjetivo))
                            : "";
                        table.Cell()
                            .Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Background(bgColor)
                            .AlignMiddle()
                            .AlignCenter()
                            .Padding(4)
                            .Text(objetivos)
                            .FontSize(10);

                        rowIndex++;
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        
        public async Task<byte[]> ExportAllAreasDetailedToExcelAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
       
        var areas = await unitOfWork.Area.GetAll(includeProperties: "IndicadoresDeArea.Indicador");
        using var workbook = new XLWorkbook();
        
        if (!areas.Any())
        {
            var emptySheet = workbook.Worksheets.Add("Sin Areas");
            emptySheet.Cell(1, 1).Value = "No hay Areas registrados.";
            using var newStream = new MemoryStream();
            workbook.SaveAs(newStream);
            return newStream.ToArray();
        }
                
        foreach (var area in areas)
        {
            var worksheet = workbook.Worksheets.Add(
                area.Nombre.Length > 31 ? area.Nombre.Substring(0, 31) : area.Nombre
            );

            
            worksheet.Cell(1, 1).Value = $"ÁREA: {area.Nombre}";
            worksheet.Range(1, 1, 1, 3).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Row(1).Height = 22;

            
            worksheet.Cell(3, 1).Value = "Área";
            worksheet.Cell(3, 2).Value = "Indicador";
            worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
            var headerRange = worksheet.Range(3, 1, 3, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;
            var indicadoresDeArea = area.IndicadoresDeArea;

            if (indicadoresDeArea.Any())
            {
                int startRow = row;
                bool first = true;

                foreach (var indArea in indicadoresDeArea)
                {
                   
                    worksheet.Cell(row, 1).Value = first ? area.Nombre : "";
                   
                    worksheet.Cell(row, 2).Value = indArea.Indicador?.Nombre ?? "Sin indicador";
                    
                    worksheet.Cell(row, 3).Value = indArea.MetaCumplir;

                    var dataRange = worksheet.Range(row, 1, row, 3);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    row++;
                    first = false;
                }

                
                if (indicadoresDeArea.Count > 1)
                {
                    worksheet.Range(startRow, 1, row - 1, 1).Merge();
                    worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
            }
            else
            {
                
                worksheet.Cell(row, 1).Value = area.Nombre;
                worksheet.Range(row, 1, row, 3).Merge(); 
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Value = "Sin indicadores de área";
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        
        public async Task<byte[]> ExportAllAreasDetailedToPdfAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var areas = await unitOfWork.Area.GetAll(includeProperties: "IndicadoresDeArea.Indicador");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    bool firstArea = true;
                    foreach (var area in areas)
                    {
                        if (!firstArea)
                            column.Item().PageBreak();
                        firstArea = false;

                       
                        column.Item()
                            .Text($"ÁREA: {area.Nombre}")
                            .FontSize(11).Bold();

                        column.Item().PaddingTop(10);

                       
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); 
                                columns.RelativeColumn(5); 
                                columns.RelativeColumn(1); 
                            });

                            table.ExtendLastCellsToTableBottom();

                            
                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter().AlignMiddle().Padding(4)
                                    .Text("Área").FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter().Padding(4)
                                    .Text("Indicador").FontSize(10).Bold();
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignCenter().Padding(4)
                                    .Text(DateTime.Now.Year.ToString()).FontSize(10).Bold();
                            });

                            var indicadoresDeArea = area.IndicadoresDeArea;
                            if (indicadoresDeArea.Any())
                            {
                                int rowCount = indicadoresDeArea.Count;

                                
                                table.Cell()
                                    .RowSpan((uint)rowCount)
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().AlignCenter().Padding(4)
                                    .Text(area.Nombre).FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().Padding(4)
                                    .Text(indicadoresDeArea[0].Indicador?.Nombre ?? "Sin indicador").FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().AlignCenter().Padding(4)
                                    .Text(indicadoresDeArea[0].MetaCumplir?.ToString() ?? "").FontSize(10);

                              
                                for (int i = 1; i < rowCount; i++)
                                {
                                    table.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .Background(Colors.White)
                                        .AlignMiddle().Padding(4)
                                        .Text(indicadoresDeArea[i].Indicador?.Nombre ?? "Sin indicador").FontSize(10);
                                    table.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .Background(Colors.White)
                                        .AlignMiddle().AlignCenter().Padding(4)
                                        .Text(indicadoresDeArea[i].MetaCumplir?.ToString() ?? "").FontSize(10);
                                }
                            }
                            else
                            {
                                
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().AlignCenter().Padding(4)
                                    .Text(area.Nombre).FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().Padding(4)
                                    .Text("Sin indicadores").FontSize(10);
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.White)
                                    .AlignMiddle().AlignCenter().Padding(4)
                                    .Text("").FontSize(10);
                            }
                        });
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        private class UnitOfWorkScope : IUnitOfWorkScope
        {
            public IUnitOfWork UnitOfWork { get; }
            public UnitOfWorkScope(IUnitOfWork uow) => UnitOfWork = uow;
        }
    }
}