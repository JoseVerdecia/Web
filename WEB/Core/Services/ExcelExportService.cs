using ClosedXML.Excel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using WEB.Core.Extensions;
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
        
       public async Task<byte[]> ExportProcesoToExcelSIEGEAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");
        if (proceso == null)
            return Array.Empty<byte>();

        var indicadores = proceso.Indicadores.OrderBy(i => i.Id).ToList();
        var indicadoresConValores = indicadores
            .Where(i => i.IsMetaCumplirPorcentaje)
            .ToList();

        using var workbook = new XLWorkbook();
        var sheetName = proceso.Nombre.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre;
        var worksheet = workbook.Worksheets.Add(sheetName);

       
        var headerBgColor = XLColor.FromHtml("#D2D6D9");
        var headerFontColor = XLColor.FromHtml("#212E58");
        var evalCumplido = XLColor.FromHtml("#34B66B");
        var evalSobrecumplido = XLColor.FromHtml("#1B74B6");
        var evalParcialmente = XLColor.FromHtml("#1B74B6");
        var evalIncumplido = XLColor.FromHtml("#ED7425");
        var fontWhite = XLColor.White;

        int row = 1;
        
        var procesoCell = worksheet.Cell(row, 4); 
        procesoCell.Value = proceso.Nombre;
        procesoCell.Style.Font.FontColor = headerFontColor;
        procesoCell.Style.Font.Bold = true;
        procesoCell.Style.Font.FontSize = 14;
        procesoCell.Style.Alignment.WrapText = true; 
        procesoCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top; 
        procesoCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        row++;

       
        worksheet.Cell(row, 2).Value = "ID";
        worksheet.Cell(row, 3).Value = "OE";
        worksheet.Cell(row, 4).Value = "INDICADOR";
        worksheet.Cell(row, 5).Value = "TIPO";
        worksheet.Cell(row, 6).Value = "META";
        worksheet.Cell(row, 7).Value = "REAL";
        worksheet.Cell(row, 8).Value = "% CUMPLIMIENTO";
        worksheet.Cell(row, 9).Value = "EVALUACIÓN";

        var header1 = worksheet.Range(row, 2, row, 9);
        header1.Style.Font.Bold = true;
        header1.Style.Font.FontColor = headerFontColor;
        header1.Style.Fill.BackgroundColor = headerBgColor;
        header1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header1.Style.Alignment.WrapText = true;

        row++;

        foreach (var ind in indicadores)
        {
            var pct = ind.MetaCumplirDecimal > 0 && ind.MetaRealDecimal > 0
                ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100
                : 0;
            string pctText = pct > 0 ? $"{pct:F2}%" : "—";
            var tieneReal = ind.MetaRealDecimal > 0;

            worksheet.Cell(row, 2).Value = ind.Id.ToString("D2");
            worksheet.Cell(row, 3).Value = ind.Objetivos.Any()
                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                : "—";
            worksheet.Cell(row, 4).Value = ind.Nombre;
            worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E (Esencial)" : "N (Necesario)";
            worksheet.Cell(row, 6).Value = ind.MetaCumplir;
            worksheet.Cell(row, 7).Value = tieneReal ? ind.MetaReal : "—";
            worksheet.Cell(row, 8).Value = pctText;
            worksheet.Cell(row, 9).Value = ind.Evaluacion.ToString();

            if (ind.Tipo == IndicadorTipo.Escencial)
                worksheet.Cell(row, 4).Style.Font.Bold = true;

            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            worksheet.Cell(row, 4).Style.Alignment.WrapText = true;
            worksheet.Cell(row, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            worksheet.Range(row, 2, row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(row, 2, row, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var evalStr = ind.Evaluacion.ToString().ToLower().Replace(" ", "");
            var evalCell = worksheet.Cell(row, 9);

            if (evalStr.Contains("incumplido"))
            {
                evalCell.Style.Fill.BackgroundColor = evalIncumplido;
                evalCell.Style.Font.FontColor = fontWhite;
            }
            else if (evalStr.Contains("parcial"))
            {
                evalCell.Style.Fill.BackgroundColor = evalParcialmente;
                evalCell.Style.Font.FontColor = fontWhite;
            }
            else if (evalStr.Contains("sobre"))
            {
                evalCell.Style.Fill.BackgroundColor = evalSobrecumplido;
                evalCell.Style.Font.FontColor = fontWhite;
            }
            else if (evalStr.Contains("cumplido"))
            {
                evalCell.Style.Fill.BackgroundColor = evalCumplido;
                evalCell.Style.Font.FontColor = fontWhite;
            }

            row++;
        }

        if (indicadoresConValores.Any())
        {
            row += 2; 

            worksheet.Cell(row, 2).Value = "ID";
            worksheet.Range(row, 3, row, 5).Merge().Value = "Valores Cuantitativos a introducir:";
            worksheet.Cell(row, 6).Value = "Datos";
            worksheet.Cell(row, 7).Value = "Porcentaje";

            var header2 = worksheet.Range(row, 2, row, 7);
            header2.Style.Font.Bold = true;
            header2.Style.Font.FontColor = headerFontColor;
            header2.Style.Fill.BackgroundColor = headerBgColor;
            header2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row++;

            int itemIndex = 0;
            foreach (var ind in indicadoresConValores)
            {
                bool esPar = itemIndex % 2 == 0;
                var bgColor = esPar ? XLColor.FromHtml("#FFF2CA") : XLColor.White;
                
                worksheet.Cell(row, 2).Value = ind.Id.ToString("D2");
                worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                worksheet.Range(row, 3, row, 5).Merge().Value = ind.ValorTotal ?? "Valor Total";
                worksheet.Cell(row, 3).Style.Alignment.WrapText = true; 
                worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top; 
                
                worksheet.Cell(row, 6).Value = ind.ValorTotalAcumulado.HasValue
                    ? ind.ValorTotalAcumulado.Value.FormatearMiles()
                    : "—";
                    
                var pctPatternCell = worksheet.Cell(row, 7);
                pctPatternCell.Value = "—";
                pctPatternCell.Style.Fill.PatternType = XLFillPatternValues.DarkHorizontal;
                pctPatternCell.Style.Fill.SetPatternColor(XLColor.White);
                pctPatternCell.Style.Fill.PatternColor = XLColor.FromHtml("#BFBFBF");
                pctPatternCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range(row, 2, row, 7).Style.Fill.BackgroundColor = bgColor;
                
                pctPatternCell.Style.Fill.PatternType = XLFillPatternValues.DarkHorizontal;
                pctPatternCell.Style.Fill.SetPatternColor(bgColor);
                pctPatternCell.Style.Fill.PatternColor = XLColor.FromHtml("#BFBFBF");

                worksheet.Range(row, 2, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 2, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
                
                var pctCuant = ind.ValorTotalAcumulado.HasValue && ind.ValorRealAcumulado.HasValue
                    ? (ind.ValorRealAcumulado.Value / ind.ValorTotalAcumulado.Value) * 100
                    : 0;
                string pctCuantText = pctCuant > 0 ? $"{pctCuant:F2}%" : "—";

               
                worksheet.Range(row, 3, row, 5).Merge().Value = ind.ValorReal ?? "Valor Real";
                worksheet.Cell(row, 3).Style.Alignment.WrapText = true;
                worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                
                worksheet.Cell(row, 6).Value = ind.ValorRealAcumulado.HasValue
                    ? ind.ValorRealAcumulado.Value.FormatearMiles()
                    : "—";
                worksheet.Cell(row, 7).Value = pctCuantText;
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range(row, 2, row, 7).Style.Fill.BackgroundColor = bgColor;
                worksheet.Range(row, 2, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 2, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
                worksheet.Range(row - 1, 2, row, 2).Merge();
                worksheet.Cell(row - 1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row - 1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                itemIndex++;
            }
        }

        
        worksheet.Column(1).Width = 2;     
        worksheet.Column(2).Width = 4;   
        worksheet.Column(3).Width = 12;  
        worksheet.Column(4).Width = 40;  
        worksheet.Column(5).Width = 10; 
        worksheet.Column(6).Width = 10;  
        worksheet.Column(7).Width = 10; 
        worksheet.Column(8).Width = 12; 
        worksheet.Column(9).Width = 14;  

      
        worksheet.Rows().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        
        public async Task<byte[]> ExportProcesoToPdfSIEGEAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");
        if (proceso == null)
            return Array.Empty<byte>();

        var indicadores = proceso.Indicadores.OrderBy(i => i.Id).ToList();
        var indicadoresConValores = indicadores
            .Where(i => i.IsMetaCumplirPorcentaje)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Column(column =>
                {
                    column.Item()
                        .Text($"PROCESO: {proceso.Nombre}")
                        .FontSize(11).Bold();

                    column.Item().PaddingTop(12);
                    
                    column.Item().Text("Indicadores del Proceso").FontSize(9).Bold();
                    column.Item().PaddingTop(4);

                    column.Item().Table(table1 =>
                    {
                        table1.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(45);
                            columns.ConstantColumn(40);
                            columns.ConstantColumn(40);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(60);
                        });

                        table1.ExtendLastCellsToTableBottom();
                        
                        table1.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("ID").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("OE").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("INDICADOR").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("TIPO").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("META").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("REAL").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("%CUMPL.").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("EVALUACIÓN").Bold();
                        });

                        foreach (var ind in indicadores)
                        {
                            var pct = ind.MetaCumplirDecimal > 0 && ind.MetaRealDecimal > 0
                                ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100
                                : 0;
                            string pctText = pct > 0 ? $"{pct:F2}%" : "—";
                            var tieneReal = ind.MetaRealDecimal > 0;

                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.Id.ToString("D2"));
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.Objetivos.Any()
                                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                                : "—");
                            
                            if(ind.Tipo == IndicadorTipo.Escencial)
                            {
                                table1.Cell().Border(1).Padding(2).Text(ind.Nombre).Bold();
                            }
                            else
                            {
                                table1.Cell().Border(1).Padding(2).Text(ind.Nombre); 
                            }
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.Tipo == IndicadorTipo.Escencial ? "E" : "N");
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.MetaCumplir);
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(tieneReal ? ind.MetaReal : "—");
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(pctText);
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.Evaluacion.ToString()).FontSize(7);
                        }
                    });
                    
                    if (indicadoresConValores.Any())
                    {
                        column.Item().PageBreak();
                        column.Item().Text("Valores Cuantitativos a introducir").FontSize(9).Bold();
                        column.Item().PaddingTop(4);

                        column.Item().Table(table2 =>
                        {
                            table2.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                            });

                            table2.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("ID").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Valores Cuantitativos").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Datos").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Porcentaje").Bold();
                            });

                            int itemIndex = 0;
                            foreach (var ind in indicadoresConValores)
                            {
                                bool esPar = itemIndex % 2 == 0;
                                var bgColor = esPar ? Colors.Yellow.Lighten5 : Colors.White;
                                
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2).Text(ind.Id.ToString("D2"));
                                table2.Cell().Border(1).Background(bgColor).Padding(2).Text(ind.ValorTotal ?? "Valor Total");
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2)
                                    .Text(ind.ValorTotalAcumulado.HasValue ? ind.ValorTotalAcumulado.Value.FormatearMiles() : "—");
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2).Text("—");
                                
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2).Text("");
                                table2.Cell().Border(1).Background(bgColor).Padding(2).Text(ind.ValorReal ?? "Valor Real");
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2)
                                    .Text(ind.ValorRealAcumulado.HasValue ? ind.ValorRealAcumulado.Value.FormatearMiles() : "—");

                                var pctCuant = ind.ValorTotalAcumulado.HasValue && ind.ValorRealAcumulado.HasValue
                                    ? (ind.ValorRealAcumulado.Value / ind.ValorTotalAcumulado.Value) * 100
                                    : 0;
                                string pctCuantText = pctCuant > 0 ? $"{pctCuant:F2}%" : "—";
                                table2.Cell().Border(1).Background(bgColor).AlignCenter().Padding(2).Text(pctCuantText);

                                itemIndex++;
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
        
        public async Task<byte[]> ExportObjetivoToExcelAsync(int objetivoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivo = await unitOfWork.Objetivo.Get(
            o => o.Id == objetivoId,
            includeProperties: "Indicadores.Proceso"
        );
        if (objetivo == null) return Array.Empty<byte>();

        using var workbook = new XLWorkbook();
        var sheetName = objetivo.Nombre?.Length > 31 
            ? objetivo.Nombre[..31] 
            : objetivo.Nombre ?? "Objetivo";
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Título del objetivo
        worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}";
        worksheet.Range(1, 1, 1, 3).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Row(1).Height = 22;

        // Encabezados de tabla
        worksheet.Cell(3, 1).Value = "Proceso";
        worksheet.Cell(3, 2).Value = "INDICADOR";
        worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
        var headerRange = worksheet.Range(3, 1, 3, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Agrupar indicadores por proceso (solo los que tienen proceso)
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

            for (int i = 0; i < indicadores.Count; i++)
            {
                var ind = indicadores[i];
                worksheet.Cell(row, 1).Value = i == 0 ? proceso.Nombre : "";
                worksheet.Cell(row, 2).Value = ind.Nombre;
                worksheet.Cell(row, 3).Value = ind.MetaCumplir;

                var dataRange = worksheet.Range(row, 1, row, 3);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                row++;
            }

            // Combinar celdas del proceso si hay más de un indicador
            if (indicadores.Count > 1)
            {
                worksheet.Range(startRow, 1, row - 1, 1).Merge();
                worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        worksheet.Columns().AdjustToContents();
        worksheet.Rows().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

        public async Task<byte[]> ExportObjetivoToPdfAsync(int objetivoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivo = await unitOfWork.Objetivo.Get(
            o => o.Id == objetivoId,
            includeProperties: "Indicadores.Proceso"
        );
        if (objetivo == null) return Array.Empty<byte>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    
                    column.Item()
                        .Text($"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}")
                        .FontSize(11)
                        .Bold();

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
                                .Text("Proceso").FontSize(10).Bold();
                            header.Cell()
                                .Background(Colors.Grey.Lighten2)
                                .Border(1).BorderColor(Colors.Grey.Lighten1)
                                .AlignCenter().Padding(4)
                                .Text("INDICADOR").FontSize(10).Bold();
                            header.Cell()
                                .Background(Colors.Grey.Lighten2)
                                .Border(1).BorderColor(Colors.Grey.Lighten1)
                                .AlignCenter().Padding(4)
                                .Text(DateTime.Now.Year.ToString()).FontSize(10).Bold();
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
                            
                            table.Cell()
                                .RowSpan((uint)rowCount)
                                .Border(1).BorderColor(Colors.Grey.Lighten1)
                                .AlignMiddle().AlignCenter().Padding(4)
                                .Text(proceso.Nombre).FontSize(10);

                            table.Cell()
                                .Border(1).BorderColor(Colors.Grey.Lighten1)
                                .AlignMiddle().Padding(4)
                                .Text(indicadores[0].Nombre).FontSize(10);

                            table.Cell()
                                .Border(1).BorderColor(Colors.Grey.Lighten1)
                                .AlignMiddle().AlignCenter().Padding(4)
                                .Text(indicadores[0].MetaCumplir?.ToString() ?? "").FontSize(10);

                           
                            for (int i = 1; i < rowCount; i++)
                            {
                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignMiddle().Padding(4)
                                    .Text(indicadores[i].Nombre).FontSize(10);

                                table.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .AlignMiddle().AlignCenter().Padding(4)
                                    .Text(indicadores[i].MetaCumplir?.ToString() ?? "").FontSize(10);
                            }
                        }
                    });
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

        public async Task<byte[]> ExportProcesoTabToExcelAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(
            p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos"
        );
        if (proceso == null) return Array.Empty<byte>();

        using var workbook = new XLWorkbook();
        var sheetName = proceso.Nombre?.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre ?? "Proceso";
        var worksheet = workbook.Worksheets.Add(sheetName);
       
        worksheet.Cell(1, 1).Value = $"PROCESO: {proceso.Nombre}";
        worksheet.Range(1, 1, 1, 3).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        
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
        var indicadores = proceso.Indicadores.OrderBy(i => i.Id).ToList();
        foreach (var ind in indicadores)
        {
            string oe = ind.Objetivos != null && ind.Objetivos.Any()
                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                : "—";
            worksheet.Cell(row, 1).Value = oe;
            worksheet.Cell(row, 2).Value = ind.Nombre;
            worksheet.Cell(row, 3).Value = ind.MetaCumplir;

            var dataRange = worksheet.Range(row, 1, row, 3);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row++;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.Rows().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

        public async Task<byte[]> ExportProcesoTabToPdfAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(
            p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos"
        );
        if (proceso == null) return Array.Empty<byte>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                   
                    column.Item()
                        .Text($"PROCESO: {proceso.Nombre}")
                        .FontSize(11).Bold();

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
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1)
                                .AlignCenter().Padding(4).Text("OE").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1)
                                .AlignCenter().Padding(4).Text("INDICADOR").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1)
                                .AlignCenter().Padding(4).Text(DateTime.Now.Year.ToString()).Bold();
                        });

                        var indicadores = proceso.Indicadores.OrderBy(i => i.Id).ToList();
                        foreach (var ind in indicadores)
                        {
                            string oe = ind.Objetivos != null && ind.Objetivos.Any()
                                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                                : "—";

                            table.Cell().Border(1).AlignCenter().Padding(4).Text(oe);
                            table.Cell().Border(1).AlignMiddle().Padding(4).Text(ind.Nombre);
                            table.Cell().Border(1).AlignCenter().Padding(4).Text(ind.MetaCumplir?.ToString() ?? "");
                        }
                    });
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