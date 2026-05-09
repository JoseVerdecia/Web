using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Mediator;
using WEB.Core.Services;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Enums;
using WEB.Features.Indicador;
using WEB.Features.IndicadorDeArea;
using WEB.Models;

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
               headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
               headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
               headerRange.Style.Alignment.WrapText = true;
       
              
               worksheet.Column(1).Width = 60; // Nombre
               worksheet.Column(2).Width = 15; // Meta Cumplir
               worksheet.Column(3).Width = 15; // Meta Real
               worksheet.Column(4).Width = 18; // Evaluación
               worksheet.Column(5).Width = 15; // Origen
               worksheet.Column(6).Width = 15; // Tipo
               worksheet.Column(7).Width = 30; // Proceso
               worksheet.Column(8).Width = 20; // Objetivos
       
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
                   worksheet.Cell(row, 8).Value = string.Join("/", indicador.Objetivos.Select(o => o.NumeroObjetivo));
                   
                   if (indicador.Tipo == IndicadorTipo.Escencial)
                   { 
                       worksheet.Cell(row, 1).Style.Font.Bold = true;
                   }
                   
                   var dataRange = worksheet.Range(row, 1, row, 8);
                   dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                   dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                   dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                   dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                   
                   worksheet.Cell(row, 1).Style.Alignment.WrapText = true; // Nombre
                   worksheet.Cell(row, 7).Style.Alignment.WrapText = true; // Proceso
                   worksheet.Cell(row, 8).Style.Alignment.WrapText = true; // Objetivos
                   
                   row++;
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

                    worksheet.Column(1).Width = 18;     
                    worksheet.Column(2).Width = 80;   
                    worksheet.Column(3).Width = 10; 
                                        
                    int row = 4;
                    foreach (var indicador in proceso.Indicadores)
                    {
                        string objetivos = indicador.Objetivos != null && indicador.Objetivos.Any()
                            ? string.Join("/", indicador.Objetivos.Select(o => o.NumeroObjetivo))
                            : "";
                        worksheet.Cell(row, 1).Value = objetivos;
                        worksheet.Cell(row, 2).Value = indicador.Nombre;
                        worksheet.Cell(row, 3).Value = indicador.MetaCumplir;
                        worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                        
                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; 
                        worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top; 
                        worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; 
                        worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        
                        var dataRange = worksheet.Range(row, 1, row, 3);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        row++;
                    }

                     
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

            // Título del objetivo
            worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATEGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}";
            worksheet.Range(1, 1, 1, 3).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Row(1).Height = 22;

            // Cabeceras
            worksheet.Cell(3, 1).Value = "Proceso";
            worksheet.Cell(3, 2).Value = "INDICADOR";
            worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
            
            var headerRange = worksheet.Range(3, 1, 3, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; 

           
            worksheet.Column(1).Width = 30; // Proceso
            worksheet.Column(2).Width = 80; // Indicador
            worksheet.Column(3).Width = 18; // Año / Meta

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
                    
                    worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; 
                    
                    worksheet.Cell(row, 1).Style.Alignment.WrapText = true; 
                    worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                     worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                     worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                
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
                    worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Mejor visibilidad
                }
            }
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
            worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
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
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; // Centrado vertical

            
            worksheet.Column(1).Width = 30; // Área
            worksheet.Column(2).Width = 80; // Indicador
            worksheet.Column(3).Width = 18; // Año / Meta

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

                   
                    worksheet.Cell(row, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    
                    worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    
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
                    worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }
            else
            {
                
                worksheet.Cell(row, 1).Value = "Sin indicadores de área";
                worksheet.Range(row, 1, row, 3).Merge(); 
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            
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
        
        // Export/Excel Proceso(X) con sus indicadores modelo SIGES
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
        
        worksheet.Range(row, 2,row,9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Range(row, 2,row,9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        
        var header1 = worksheet.Range(row, 2, row, 9);
        header1.Style.Font.Bold = true;
        header1.Style.Font.FontColor = headerFontColor;
        header1.Style.Fill.BackgroundColor = headerBgColor;
        header1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header1.Style.Alignment.WrapText = true;

        row++;
        
        worksheet.Column(1).Width = 2;     
        worksheet.Column(2).Width = 4;   
        worksheet.Column(3).Width = 18;  
        worksheet.Column(4).Width = 60;  
        worksheet.Column(5).Width = 10; 
        worksheet.Column(6).Width = 10;  
        worksheet.Column(7).Width = 10; 
        worksheet.Column(8).Width = 12; 
        worksheet.Column(9).Width = 14;  
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
            worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
            worksheet.Cell(row, 6).Value = ind.MetaCumplir;
            worksheet.Cell(row, 7).Value = tieneReal ? ind.MetaReal : "—";
            worksheet.Cell(row, 8).Value = pctText;
            worksheet.Cell(row, 9).Value = ind.Evaluacion.ToString();

            if (ind.Tipo == IndicadorTipo.Escencial)
                worksheet.Cell(row, 4).Style.Font.Bold = true;

            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
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
                worksheet.Cell(row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
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
                worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                
                worksheet.Cell(row, 6).Value = ind.ValorRealAcumulado.HasValue
                    ? ind.ValorRealAcumulado.Value.FormatearMiles()
                    : "—";
                worksheet.Cell(row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                worksheet.Cell(row, 7).Value = pctCuantText;
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range(row, 2, row, 7).Style.Fill.BackgroundColor = bgColor;
                worksheet.Range(row, 2, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 2, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 2, row, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                
                worksheet.Range(row - 1, 2, row, 2).Merge();
                worksheet.Cell(row - 1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row - 1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                itemIndex++;
            }
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
        // Export/PDF Proceso(X) con sus indicadores modelo SIGES (ADMIN-JEFE PROCESO)
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
                            table1.Cell().Border(1).AlignCenter().Padding(2).Text(ind.Evaluacion.GetDisplayName()).FontSize(7);
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
        
        // Exportar/Excel -> Objetivos con Todos los Procesos e Indicadores (ADMIN) -> Proyecto Estrategico
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

       
        worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo} : {objetivo.Nombre}";
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

        worksheet.Column(1).Width = 40;
        worksheet.Column(2).Width = 80;
        worksheet.Column(3).Width = 10;
        
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
                worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                worksheet.Cell(row, 3).Value = ind.MetaCumplir;

                var dataRange = worksheet.Range(row, 1, row, 3);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                row++;
            }

            if (indicadores.Count > 1)
            {
                worksheet.Range(startRow, 1, row - 1, 1).Merge();
                worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
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
        
        //Exportar/PDF -> Objetivos con todos los Procesos e Indicadores (ADMIN) -> Proyecto Estrategico
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

        //Exportar/Excel Proceso(X) con todos los indicadores de ese Proceso (Admin-Jefe Proceso) -> Proyecto Estrategico
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
        
        worksheet.Column(1).Width = 18;     
        worksheet.Column(2).Width = 80;   
        worksheet.Column(3).Width = 18;  
        
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
            worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
            row++;
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
        
        //Export/Excel Evaluacion del Proceso(X) -> (Admin-JefeProceso) -> Evaluacion
        public async Task<byte[]> ExportEvaluacionProcesoToExcelAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores");
        if (proceso == null) return Array.Empty<byte>();

        var data = EvaluateObjetivosAndProcesos.CalcularEvaluacionProceso(proceso);

        using var workbook = new XLWorkbook();
        var sheetName = proceso.Nombre?.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre ?? "Evaluación";
        var worksheet = workbook.Worksheets.Add(sheetName);

        
        worksheet.Cell(1, 1).Value = $"Proceso: {data.NombreProceso}";
        worksheet.Range(1, 1, 1, 10).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Row(1).Height = 22;

        
        string[] headers = { "Indicadores", "Cantidad", "Cantidad S", "% S", "Cantidad C", "% C", "Cantidad PC", "% PC", "Cantidad I", "% I" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = headers[i];
        }
        var headerRange = worksheet.Range(3, 1, 3, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Row(3).Height = 18;

     
        int row = 4;
        foreach (var fila in data.Filas)
        {
            worksheet.Cell(row, 1).Value = fila.Nombre;
            worksheet.Cell(row, 2).Value = fila.Total;
            worksheet.Cell(row, 3).Value = fila.Sobrecumplidos;
            worksheet.Cell(row, 4).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeS:F2}%";
            worksheet.Cell(row, 5).Value = fila.Cumplidos;
            worksheet.Cell(row, 6).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeC:F2}%";
            worksheet.Cell(row, 7).Value = fila.ParcialmenteCumplidos;
            worksheet.Cell(row, 8).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajePC:F2}%";
            worksheet.Cell(row, 9).Value = fila.Incumplidos;
            worksheet.Cell(row, 10).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeI:F2}%";

            var dataRange = worksheet.Range(row, 1, row, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
          
            for (int c = 2; c <= 10; c++)
                worksheet.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
           
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
        }

      
        row++;
        worksheet.Cell(row, 1).Value = $"EVALUACIÓN: {data.Evaluacion}";
        worksheet.Range(row, 1, row, 10).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Font.FontSize = 12;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

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
        
        // Exportar/PDF Evaluacion del Proceso(X) -> ADMIN Y JEFE PROCESO
        public async Task<byte[]> ExportEvaluacionProcesoToPdfAsync(int procesoId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;
        
            try
            {
                var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores");
                if (proceso == null) return Array.Empty<byte>();
        
                var data = EvaluateObjetivosAndProcesos.CalcularEvaluacionProceso(proceso);
        
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));
        
                        page.Content().Column(column =>
                        {
                         
                            column.Item().Text($"Proceso: {data.NombreProceso}").FontSize(12).Bold();
                            column.Item().PaddingTop(8);
        
                       
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Indicadores
                                    columns.RelativeColumn(1); // Cantidad
                                    columns.RelativeColumn(1); // S
                                    columns.RelativeColumn(1); // %S
                                    columns.RelativeColumn(1); // C
                                    columns.RelativeColumn(1); // %C
                                    columns.RelativeColumn(1); // PC
                                    columns.RelativeColumn(1); // %PC
                                    columns.RelativeColumn(1); // I
                                    columns.RelativeColumn(1); // %I
                                });
        
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Indicadores").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Cantidad").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Cant. S").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("% S").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Cant. C").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("% C").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Cant. PC").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("% PC").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("Cant. I").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).AlignCenter().Padding(2).Text("% I").Bold();
                                });
        
                                foreach (var fila in data.Filas)
                                {
                                    table.Cell().Border(1).Padding(2).Text(fila.Nombre);
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Total.ToString());
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Sobrecumplidos.ToString());
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeS:F2}%");
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Cumplidos.ToString());
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeC:F2}%");
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.ParcialmenteCumplidos.ToString());
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajePC:F2}%");
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Incumplidos.ToString());
                                    table.Cell().Border(1).AlignCenter().Padding(2).Text(fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeI:F2}%");
                                }
                            });
                            
                            column.Item().PaddingTop(12);
                            column.Item().Text($"EVALUACIÓN: {data.Evaluacion}").FontSize(12).Bold();
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
        
        //Export/Excel Resumen de Evaluacion de los Procesos (ADMIN)
        public async Task<byte[]> ExportResumenEvaluacionProcesosToExcelAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;
        
            try
            {
                var procesos = await unitOfWork.Proceso.GetAll(includeProperties: "Indicadores");
                if (!procesos.Any())
                {
                    using var emptyWorkbook = new XLWorkbook();
                    var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
                    emptySheet.Cell(1, 1).Value = "No hay procesos registrados.";
                    using var newStream = new MemoryStream();
                    emptyWorkbook.SaveAs(newStream);
                    return newStream.ToArray();
                }
        
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Resumen de Procesos");
        
              
                worksheet.Cell(1, 1).Value = "RESUMEN DE LA EVALUACIÓN DE LOS PROCESOS";
                worksheet.Range(1, 1, 1, 17).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Height = 22;
        
               
                int headerRow1 = 3;
                int headerRow2 = 4;
        
                string[] headers1 = { "PROCESOS", "CATEGORÍA", "TOTAL", "SOBRE-CUMPLIDOS", "", "CUMPLIDOS", "", "SC+C", "", "PARCIALMENTE CUMPLIDO", "", "SC+C+PC", "", "INCUMPLIDOS", "", "NO EVALUADOS", "EVALUACIÓN DEL PROCESO" };
                for (int i = 0; i < headers1.Length; i++)
                    worksheet.Cell(headerRow1, i + 1).Value = headers1[i];
        
                
                worksheet.Range(headerRow1, 4, headerRow1, 5).Merge(); // SOBRE-CUMPLIDOS
                worksheet.Range(headerRow1, 6, headerRow1, 7).Merge(); // CUMPLIDOS
                worksheet.Range(headerRow1, 8, headerRow1, 9).Merge(); // SC+C
                worksheet.Range(headerRow1, 10, headerRow1, 11).Merge(); // PARCIALMENTE CUMPLIDO
                worksheet.Range(headerRow1, 12, headerRow1, 13).Merge(); // SC+C+PC
                worksheet.Range(headerRow1, 14, headerRow1, 15).Merge(); // INCUMPLIDOS
        
             
                string[] subHeaders = { "", "", "", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "", "" };
                for (int i = 0; i < subHeaders.Length; i++)
                {
                    var cell = worksheet.Cell(headerRow2, i + 1);
                    cell.Value = subHeaders[i];
                }
        
                
                worksheet.Range(headerRow1, 1, headerRow2, 1).Merge(); // PROCESOS
                worksheet.Range(headerRow1, 2, headerRow2, 2).Merge(); // CATEGORÍA
                worksheet.Range(headerRow1, 3, headerRow2, 3).Merge(); // TOTAL
                worksheet.Range(headerRow1, 16, headerRow2, 16).Merge(); // NO EVALUADOS
                worksheet.Range(headerRow1, 17, headerRow2, 17).Merge(); // EVALUACIÓN DEL PROCESO
        
               
                var headerRange = worksheet.Range(headerRow1, 1, headerRow2, 17);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
                headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;
        
          
                int row = 5;
                var redFontColor = XLColor.Red;
        
                foreach (var proceso in procesos.OrderBy(p => p.Nombre))
                {
                    var indicadores = proceso.Indicadores;
                    var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
                    var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
        
                    var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
                        indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());
        
                    var data = new List<(string Nombre, List<IndicadorModel> Lista)>
                    {
                        ("Indicadores esenciales", esenciales),
                        ("Indicadores necesarios", necesarios)
                    };
        
                    int startRow = row;
                    foreach (var (nombre, lista) in data)
                    {
                        int total = lista.Count;
                        int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
                        int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
                        int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
                        int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
                        int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);
                        int scPlusC = sobre + cumple;
                        int scPlusCPlusPC = sobre + cumple + parcial;
        
                        bool esEsencial = nombre == "Indicadores esenciales";
                        var fontColor = esEsencial ? redFontColor : XLColor.Black;
        
                      
                        if (row == startRow)
                            worksheet.Cell(row, 1).Value = proceso.Nombre;
                        else
                            worksheet.Cell(row, 1).Value = "";
        
                     
                        worksheet.Cell(row, 2).Value = nombre;
                        worksheet.Cell(row, 2).Style.Font.FontColor = fontColor;
        
                      
                        worksheet.Cell(row, 3).Value = total;
                        worksheet.Cell(row, 3).Style.Font.FontColor = fontColor;
                      
                        (int valor, string porcentaje)[] celdas = new[] {
                            (sobre, total > 0 ? (sobre / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (cumple, total > 0 ? (cumple / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (scPlusC, total > 0 ? (scPlusC / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (parcial, total > 0 ? (parcial / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (scPlusCPlusPC, total > 0 ? (scPlusCPlusPC / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (incumple, total > 0 ? (incumple / (double)total * 100).ToString("F2") + "%" : "0,00%")
                        };
                        int colBase = 4;
                        for (int i = 0; i < celdas.Length; i++)
                        {
                            worksheet.Cell(row, colBase + i * 2).Value = celdas[i].valor;
                            worksheet.Cell(row, colBase + i * 2).Style.Font.FontColor = fontColor;
                            worksheet.Cell(row, colBase + i * 2 + 1).Value = celdas[i].porcentaje;
                            worksheet.Cell(row, colBase + i * 2 + 1).Style.Font.FontColor = fontColor;
                        }
        
                    
                        worksheet.Cell(row, 16).Value = noEvaluado;
                        worksheet.Cell(row, 16).Style.Font.FontColor = fontColor;
        
                        
                        if (row == startRow)
                            worksheet.Cell(row, 17).Value = evaluacion.GetDisplayName();
                        else
                            worksheet.Cell(row, 17).Value = "";
        
                       
                        for (int c = 1; c <= 17; c++)
                        {
                            var cell = worksheet.Cell(row, c);
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            if (c >= 3 && c <= 16)
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                       
                        if (esEsencial)
                        {
                            for (int c = 2; c <= 16; c++)
                                worksheet.Cell(row, c).Style.Font.Bold = true;
                        }
        
                        row++;
                    }
        
                    
                    if (row - startRow > 1)
                    {
                        worksheet.Range(startRow, 1, row - 1, 1).Merge();
                        worksheet.Range(startRow, 17, row - 1, 17).Merge();
                        worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(startRow, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                }
        
               
                worksheet.Column(1).Width = 28;   
                worksheet.Column(2).Width = 20;   
                worksheet.Column(3).Width = 8;    
                for (int c = 4; c <= 15; c++)     
                    worksheet.Column(c).Width = c % 2 == 0 ? 8 : 8;
                worksheet.Column(16).Width = 10;  
                worksheet.Column(17).Width = 22;  
        
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
        
        // Exportar/Excel ->  Objetivo Seleccionado con todos sus Procesos e Indicadores (Admin)
        public async Task<byte[]> ExportObjetivoSIGEToExcelAsync(int objetivoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivo = await unitOfWork.Objetivo.Get(
            o => o.Id == objetivoId,
            includeProperties: "Indicadores,Indicadores.Proceso"
        );
        if (objetivo == null) return Array.Empty<byte>();

        var indicadores = objetivo.Indicadores.Where(i => !i.IsDeleted).ToList();
        if (!indicadores.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin indicadores");
            emptySheet.Cell(1, 1).Value = $"El objetivo '{objetivo.Nombre}' no tiene indicadores.";
            using var emptyStream = new MemoryStream();
            emptyWorkbook.SaveAs(emptyStream);
            return emptyStream.ToArray();
        }

        using var workbook = new XLWorkbook();
        var sheetName = objetivo.Nombre?.Length > 31
            ? objetivo.Nombre[..31]
            : objetivo.Nombre ?? "Objetivo";
        var worksheet = workbook.Worksheets.Add(sheetName);
        
        worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo}: {objetivo.Nombre}";
        worksheet.Range(1, 1, 1, 9).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Row(1).Height = 22;

        
        var headerRow = 3;
        string[] headers = { "No.", "PROCESO", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPLIMIENTO", "EVALUACIÓN" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
        }
        var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
        headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

      
        var procesos = indicadores.GroupBy(i => i.Proceso).OrderBy(g => g.Key.Nombre);
        int row = 4;
        int rowIndex = 0;

        foreach (var grupo in procesos)
        {
            var proceso = grupo.Key;
            var indicadoresProceso = grupo.OrderBy(i => i.Id).ToList();
            int startRow = row;
            bool first = true;

            worksheet.Column(1).Width = 8; 
            worksheet.Column(2).Width = 40; 
            worksheet.Column(3).Width = 8; 
            worksheet.Column(4).Width = 80; 
            worksheet.Column(5).Width = 8; 
            worksheet.Column(6).Width = 10; 
            worksheet.Column(7).Width = 10; 
            worksheet.Column(8).Width = 12; 
            worksheet.Column(8).Style.Alignment.WrapText = true; 
            worksheet.Column(9).Width = 30; 
            
            foreach (var ind in indicadoresProceso)
            {
                rowIndex++;
                // No.
                worksheet.Cell(row, 1).Value = rowIndex;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
              
                // Proceso
                if (first)
                {
                    worksheet.Cell(row, 2).Value = proceso.Nombre;
                    first = false;
                }

                // Ind.
                worksheet.Cell(row, 3).Value = ind.Id;
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).Style.Font.Bold = true;

                // Indicador
                worksheet.Cell(row, 4).Value = ind.Nombre;
                if (ind.Tipo == IndicadorTipo.Escencial)
                    worksheet.Cell(row, 4).Style.Font.Bold = true;
                worksheet.Cell(row, 4).Style.Alignment.WrapText = true;
                // Tipo
                worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Meta
                worksheet.Cell(row, 6).Value = ind.MetaCumplir;
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Real
                worksheet.Cell(row, 7).Value = ind.MetaReal ?? "—";
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // % Cumplimiento
                decimal pct = ind.MetaCumplirDecimal != 0
                    ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100)
                    : 0;
                worksheet.Cell(row, 8).Value = pct.ToString("F2") + "%";
                worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                var evalCell = worksheet.Cell(row, 9);
                evalCell.Value = ind.Evaluacion.GetDisplayName();
                evalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                evalCell.Style.Font.Bold = true;
                evalCell.Style.Font.FontColor = XLColor.White;

                switch (ind.Evaluacion)
                {
                    case Evaluacion.Sobrecumplido:
                        evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
                        break;
                    case Evaluacion.Cumplido:
                        evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#16a34a");
                        break;
                    case Evaluacion.ParcialmenteCumplido:
                        evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#eab308");
                        break;
                    case Evaluacion.Incumplido:
                        evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#ea580c");
                        break;
                    default:
                        evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#9ca3af");
                        break;
                }

                
                var dataRange = worksheet.Range(row, 1, row, 9);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
            }
            
            if (indicadoresProceso.Count > 1)
            {
                worksheet.Range(startRow, 2, row - 1, 2).Merge();
                worksheet.Cell(startRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(startRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(startRow, 2).Style.Font.Bold = true;
                worksheet.Cell(startRow, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#f4f4f4");
            }
            else
            {
                worksheet.Cell(startRow, 2).Style.Font.Bold = true;
                worksheet.Cell(startRow, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#f4f4f4");
            }
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

        public async Task<byte[]> ExportProcesoSIGEToExcelAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores,Indicadores.Objetivos");
        if (proceso == null) return Array.Empty<byte>();

        var indicadores = proceso.Indicadores.Where(i => !i.IsDeleted).OrderBy(i => i.Id).ToList();
        if (!indicadores.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin indicadores");
            emptySheet.Cell(1, 1).Value = $"El proceso '{proceso.Nombre}' no tiene indicadores.";
            using var ms = new MemoryStream();
            emptyWorkbook.SaveAs(ms);
            return ms.ToArray();
        }

        using var workbook = new XLWorkbook();
        var sheetName = proceso.Nombre?.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre ?? "Proceso";
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Título
        worksheet.Cell(1, 1).Value = $"PROCESO: {proceso.Nombre}";
        worksheet.Range(1, 1, 1, 9).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Encabezados
        string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPLIMIENTO", "EVALUACIÓN" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(3, i + 1).Value = headers[i];
        var headerRange = worksheet.Range(3, 1, 3, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
        headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 4;
        int total = indicadores.Count;
        bool procesoCellWritten = false;

        for (int i = 0; i < total; i++)
        {
            var ind = indicadores[i];
            int seq = i + 1;
            decimal pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100) : 0;
            string pctText = pct.ToString("F2") + "%";

            worksheet.Cell(row, 1).Value = seq;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            if (!procesoCellWritten)
            {
                worksheet.Cell(row, 2).Value = proceso.Nombre;
                worksheet.Range(row, 2, row + total - 1, 2).Merge();
                worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                procesoCellWritten = true;
            }

            worksheet.Cell(row, 3).Value = ind.Id;
            worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).Style.Font.Bold = true;
            worksheet.Cell(row, 4).Value = ind.Nombre;
            if (ind.Tipo == IndicadorTipo.Escencial)
                worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
            worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 6).Value = ind.MetaCumplir;
            worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 7).Value = ind.MetaReal ?? "—";
            worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 8).Value = pctText;
            worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var evalCell = worksheet.Cell(row, 9);
            evalCell.Value = ind.Evaluacion.GetDisplayName();
            evalCell.Style.Font.Bold = true;
            evalCell.Style.Font.FontColor = XLColor.White;
            evalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            switch (ind.Evaluacion)
            {
                case Evaluacion.Sobrecumplido:
                    evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb"); break;
                case Evaluacion.Cumplido:
                    evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#16a34a"); break;
                case Evaluacion.ParcialmenteCumplido:
                    evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#eab308"); break;
                case Evaluacion.Incumplido:
                    evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#ea580c"); break;
                default:
                    evalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#9ca3af"); break;
            }

            var dataRange = worksheet.Range(row, 1, row, 9);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

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
        
        public async Task<byte[]> ExportJAObjetivoSIGEToExcelAsync(int objetivoId, int areaId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var uow = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(uow);
            UnitOfWorkAccessor.CurrentScope = scope;
        
            try
            {
                var objetivo = await uow.Objetivo.Get(o => o.Id == objetivoId, includeProperties: "Indicadores.Proceso");
                if (objetivo == null) return Array.Empty<byte>();
        
                var area = await uow.Area.Get(a => a.Id == areaId, includeProperties: "IndicadoresDeArea");
                if (area == null) return Array.Empty<byte>();
        
                var indicadoresAreaDict = area.IndicadoresDeArea.ToDictionary(ia => ia.IndicadorId, ia => ia);
        
                var indicadores = objetivo.Indicadores.OrderBy(i => i.Id).ToList();
                var grupos = indicadores.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);
        
                using var workbook = new XLWorkbook();
                var sheetName = $"Obj_{objetivo.NumeroObjetivo}";
                var worksheet = workbook.Worksheets.Add(sheetName);
        
                int row = 1;
                worksheet.Cell(row, 1).Value = $"Objetivo {objetivo.NumeroObjetivo} - Área {area.Nombre}";
                worksheet.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
                row += 2;
        
                string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPL.", "EVALUACIÓN" };
                for (int i = 0; i < headers.Length; i++)
                    worksheet.Cell(row, i + 1).Value = headers[i];
                var headerRange = worksheet.Range(row, 1, row, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                row++;
        
                int secuencia = 0;
                foreach (var grupo in grupos)
                {
                    var inds = grupo.ToList();
                    int rowspan = inds.Count;
                    int startRow = row;
                    bool primera = true;
                    foreach (var ind in inds)
                    {
                        secuencia++;
                        string numero = $"{objetivo.NumeroObjetivo}.{secuencia:D2}";
                        var areaInd = indicadoresAreaDict.GetValueOrDefault(ind.Id);
        
                        worksheet.Cell(row, 1).Value = numero;
                        if (primera)
                        {
                            worksheet.Cell(row, 2).Value = ind.Proceso?.Nombre;
                            primera = false;
                        }
                        worksheet.Cell(row, 3).Value = ind.Id;
                        worksheet.Cell(row, 4).Value = ind.Nombre;
                        worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                        worksheet.Cell(row, 6).Value = areaInd?.MetaCumplir ?? "—";
                        string metaReal = areaInd?.MetaReal ?? "—";
                        worksheet.Cell(row, 7).Value = metaReal;
                        decimal? pct = null;
                        if (areaInd != null && areaInd.MetaCumplirDecimal != 0 && areaInd.MetaRealDecimal != 0)
                            pct = (areaInd.MetaRealDecimal / areaInd.MetaCumplirDecimal) * 100;
                        worksheet.Cell(row, 8).Value = pct.HasValue ? $"{pct:F2}%" : "—";
                        worksheet.Cell(row, 9).Value = areaInd?.Evaluacion.GetDisplayName() ?? "—";
        
                        var dataRange = worksheet.Range(row, 1, row, 9);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        for (int c = 1; c <= 9; c++)
                            worksheet.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
                        row++;
                    }
                    
                    if (rowspan > 1)
                    {
                        worksheet.Range(startRow, 2, row - 1, 2).Merge();
                        worksheet.Cell(startRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                }
                
                worksheet.Column(1).Width = 8;   // No.
                worksheet.Column(2).Width = 28;  // PROCESOS
                worksheet.Column(3).Width = 6;   // Ind.
                worksheet.Column(4).Width = 40;  // INDICADOR
                worksheet.Column(5).Width = 6;   // TIPO
                worksheet.Column(6).Width = 10;  // META
                worksheet.Column(7).Width = 10;  // REAL
                worksheet.Column(8).Width = 12;  // % CUMPL.
                worksheet.Column(9).Width = 16;  // EVALUACIÓN
                
                worksheet.Column(2).Style.Alignment.WrapText = true;
                worksheet.Column(4).Style.Alignment.WrapText = true;
                
        
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally { UnitOfWorkAccessor.CurrentScope = null; }
        }
        public async Task<byte[]> ExportObjetivoProcesoSIGEToExcelAsync(int objetivoId, int procesoId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;
        
            try
            {
                var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId);
                var objetivo = await unitOfWork.Objetivo.Get(o => o.Id == objetivoId);
                if (proceso == null || objetivo == null) return Array.Empty<byte>();
        
                var indicadores = (await unitOfWork.Indicador.GetAll(includeProperties: "Objetivos,Proceso"))
                    .Where(i => i.ProcesoId == procesoId && i.Objetivos.Any(o => o.Id == objetivoId) && !i.IsDeleted)
                    .OrderBy(i => i.Id)
                    .ToList();
        
                if (!indicadores.Any())
                {
                    using var emptyWorkbook = new XLWorkbook();
                    var emptySheet = emptyWorkbook.Worksheets.Add("Sin indicadores");
                    emptySheet.Cell(1, 1).Value = $"No hay indicadores para el objetivo '{objetivo.Nombre}' en el proceso '{proceso.Nombre}'.";
                    using var ms = new MemoryStream();
                    emptyWorkbook.SaveAs(ms);
                    return ms.ToArray();
                }
        
                using var workbook = new XLWorkbook();
                string sheetName = objetivo.Nombre?.Length > 31 ? objetivo.Nombre[..31] : objetivo.Nombre ?? "Objetivo";
                var worksheet = workbook.Worksheets.Add(sheetName);
        
                // Título
                worksheet.Cell(1, 1).Value = $"OBJETIVO ESTRATÉGICO {objetivo.NumeroObjetivo}: {objetivo.Nombre} | Proceso: {proceso.Nombre}";
                worksheet.Range(1, 1, 1, 9).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        
                // Encabezados
                string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPLIMIENTO", "EVALUACIÓN" };
                for (int i = 0; i < headers.Length; i++)
                    worksheet.Cell(3, i + 1).Value = headers[i];
                var headerRange = worksheet.Range(3, 1, 3, 9);
                
        
                int row = 4;
                int total = indicadores.Count;
                bool procesoCellWritten = false;
                for (int idx = 0; idx < total; idx++)
                {
                    var ind = indicadores[idx];
                    int seq = idx + 1;
                    
                }
        
                worksheet.Columns().AdjustToContents();
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally { UnitOfWorkAccessor.CurrentScope = null; }
        }
        
        
        // Exportar/Excel -> Resumen de Evaluacion de Todos los Objetivos (ADMIN)
        public async Task<byte[]> ExportResumenEvaluacionObjetivosToExcelAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var unitOfWork = new UnitOfWork(dbContext);
            var scope = new UnitOfWorkScope(unitOfWork);
            UnitOfWorkAccessor.CurrentScope = scope;
        
            try
            {
                var objetivos = await unitOfWork.Objetivo.GetAll(includeProperties: "Indicadores");
                if (!objetivos.Any())
                {
                    using var emptyWorkbook = new XLWorkbook();
                    var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
                    emptySheet.Cell(1, 1).Value = "No hay objetivos registrados.";
                    using var newStream = new MemoryStream();
                    emptyWorkbook.SaveAs(newStream);
                    return newStream.ToArray();
                }
        
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Resumen de Objetivos");
        
                worksheet.Cell(1, 1).Value = "RESUMEN DE LA EVALUACIÓN DE LOS OBJETIVOS";
                worksheet.Range(1, 1, 1, 17).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Height = 22;
        
                int headerRow1 = 3;
                string[] headers1 = { "OBJETIVOS", "CATEGORÍA", "TOTAL", "SOBRE-CUMPLIDOS", "", "CUMPLIDOS", "", "SC+C", "", "PARCIALMENTE CUMPLIDOS", "", "SC+C+PC", "", "INCUMPLIDOS", "", "NO EVALUADOS", "EVALUACIÓN DEL OBJETIVO ESTRATÉGICO" };
                for (int i = 0; i < headers1.Length; i++)
                    worksheet.Cell(headerRow1, i + 1).Value = headers1[i];
        
                worksheet.Range(headerRow1, 4, headerRow1, 5).Merge();
                worksheet.Range(headerRow1, 6, headerRow1, 7).Merge();
                worksheet.Range(headerRow1, 8, headerRow1, 9).Merge();
                worksheet.Range(headerRow1, 10, headerRow1, 11).Merge();
                worksheet.Range(headerRow1, 12, headerRow1, 13).Merge();
                worksheet.Range(headerRow1, 14, headerRow1, 15).Merge();
        
                int headerRow2 = 4;
                string[] subHeaders = { "", "", "", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "TOTAL", "%", "", "" };
                for (int i = 0; i < subHeaders.Length; i++)
                    worksheet.Cell(headerRow2, i + 1).Value = subHeaders[i];
        
                worksheet.Range(headerRow1, 1, headerRow2, 1).Merge();
                worksheet.Range(headerRow1, 2, headerRow2, 2).Merge();
                worksheet.Range(headerRow1, 3, headerRow2, 3).Merge();
                worksheet.Range(headerRow1, 16, headerRow2, 16).Merge();
                worksheet.Range(headerRow1, 17, headerRow2, 17).Merge();
        
                var headerRange = worksheet.Range(headerRow1, 1, headerRow2, 17);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
                headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;
        
                int row = 5;
                var redFontColor = XLColor.Red;
        
                foreach (var objetivo in objetivos.OrderBy(o => o.NumeroObjetivo))
                {
                    var indicadores = objetivo.Indicadores;
                    var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
                    var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
        
                    var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
                        indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());
        
                    var data = new List<(string Nombre, List<IndicadorModel> Lista)>
                    {
                        ("Indicadores esenciales", esenciales),
                        ("Indicadores necesarios", necesarios)
                    };
        
                    int startRow = row;
                    foreach (var (nombre, lista) in data)
                    {
                        int total = lista.Count;
                        int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
                        int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
                        int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
                        int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
                        int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);
                        int scPlusC = sobre + cumple;
                        int scPlusCPlusPC = sobre + cumple + parcial;
        
                        bool esEsencial = nombre == "Indicadores esenciales";
                        var fontColor = esEsencial ? redFontColor : XLColor.Black;
        
                        if (row == startRow)
                            worksheet.Cell(row, 1).Value = $"Objetivo {objetivo.NumeroObjetivo}";
                        else
                            worksheet.Cell(row, 1).Value = "";
        
                        worksheet.Cell(row, 2).Value = nombre;
                        worksheet.Cell(row, 2).Style.Font.FontColor = fontColor;
        
                        worksheet.Cell(row, 3).Value = total;
                        worksheet.Cell(row, 3).Style.Font.FontColor = fontColor;
        
                        (int valor, string porcentaje)[] celdas = new[] {
                            (sobre, total > 0 ? (sobre / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (cumple, total > 0 ? (cumple / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (scPlusC, total > 0 ? (scPlusC / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (parcial, total > 0 ? (parcial / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (scPlusCPlusPC, total > 0 ? (scPlusCPlusPC / (double)total * 100).ToString("F2") + "%" : "0,00%"),
                            (incumple, total > 0 ? (incumple / (double)total * 100).ToString("F2") + "%" : "0,00%")
                        };
                        int colBase = 4;
                        for (int i = 0; i < celdas.Length; i++)
                        {
                            worksheet.Cell(row, colBase + i * 2).Value = celdas[i].valor;
                            worksheet.Cell(row, colBase + i * 2).Style.Font.FontColor = fontColor;
                            worksheet.Cell(row, colBase + i * 2 + 1).Value = celdas[i].porcentaje;
                            worksheet.Cell(row, colBase + i * 2 + 1).Style.Font.FontColor = fontColor;
                        }
        
                        worksheet.Cell(row, 16).Value = noEvaluado;
                        worksheet.Cell(row, 16).Style.Font.FontColor = fontColor;
        
                        if (row == startRow)
                            worksheet.Cell(row, 17).Value = evaluacion.GetDisplayName();
                        else
                            worksheet.Cell(row, 17).Value = "";
        
                        for (int c = 1; c <= 17; c++)
                        {
                            var cell = worksheet.Cell(row, c);
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            if (c >= 3 && c <= 16)
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
        
                        if (esEsencial)
                        {
                            for (int c = 2; c <= 16; c++)
                                worksheet.Cell(row, c).Style.Font.Bold = true;
                        }
        
                        row++;
                    }
        
                    if (row - startRow > 1)
                    {
                        worksheet.Range(startRow, 1, row - 1, 1).Merge();
                        worksheet.Range(startRow, 17, row - 1, 17).Merge();
                        worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(startRow, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                }
        
                worksheet.Column(1).Width = 28;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 8;
                for (int c = 4; c <= 15; c++)
                    worksheet.Column(c).Width = c % 2 == 0 ? 8 : 8;
                worksheet.Column(16).Width = 10;
                worksheet.Column(17).Width = 22;
                
        
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            finally
            {
                UnitOfWorkAccessor.CurrentScope = null;
            }
        }
        
        
        //Export/Excel -> Proyecto Estrategico del Jefe de Area con sus indicadores de area de un Proceso(X)
            public async Task<byte[]> ExportPEJATabToExcelAsync(int procesoId, int areaId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var unitOfWork = new UnitOfWork(dbContext);
        var scope = new UnitOfWorkScope(unitOfWork);
        UnitOfWorkAccessor.CurrentScope = scope;

        try
        {
            var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
                includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");
            if (proceso == null) return Array.Empty<byte>();
            
            var indicadoresConArea = proceso.Indicadores
                .Where(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId))
                .OrderBy(i => i.Id)
                .ToList();

            if (!indicadoresConArea.Any())
            {
                using var emptyWorkbook = new XLWorkbook();
                var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
                emptySheet.Cell(1, 1).Value = "No hay indicadores para esta área.";
                using var mem = new MemoryStream();
                emptyWorkbook.SaveAs(mem);
                return mem.ToArray();
            }

            using var workbook = new XLWorkbook();
            var sheetName = proceso.Nombre.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre;
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Título
            worksheet.Cell(1, 1).Value = $"Proceso: {proceso.Nombre}";
            worksheet.Range(1, 1, 1, 3).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Row(1).Height = 22;

            // Encabezados
            worksheet.Cell(3, 1).Value = "OE";
            worksheet.Cell(3, 2).Value = "INDICADOR";
            worksheet.Cell(3, 3).Value = DateTime.Now.Year.ToString();
            var headerRange = worksheet.Range(3, 1, 3, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
            headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 80;
            worksheet.Column(3).Width = 14;
            
            int row = 4;
            foreach (var ind in indicadoresConArea)
            {
                var area = ind.IndicadoresDeArea.First(ia => ia.AreaId == areaId);
                string oe = ind.Objetivos?.Any() == true
                    ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                    : "—";

                worksheet.Cell(row, 1).Value = oe;
                worksheet.Cell(row, 2).Value = ind.Nombre;
                worksheet.Cell(row, 3).Value = area.MetaCumplir;

                worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                
                if (ind.Tipo == IndicadorTipo.Escencial)
                    worksheet.Cell(row, 2).Style.Font.Bold = true;

                var dataRange = worksheet.Range(row, 1, row, 3);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
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
        
        public async Task<byte[]> ExportEvaluacionAreaToExcelAsync(int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var area = await unitOfWork.Area.Get(a => a.Id == areaId,
            includeProperties: "IndicadoresDeArea.Indicador");
        if (area == null) return Array.Empty<byte>();

        var indicadoresArea = area.IndicadoresDeArea;
        var esenciales = indicadoresArea.Where(ia => ia.Indicador.Tipo == IndicadorTipo.Escencial).ToList();
        var necesarios = indicadoresArea.Where(ia => ia.Indicador.Tipo == IndicadorTipo.Necesario).ToList();

        // Cálculo de filas
        EvaluacionAreaRow CrearFila(string nombre, List<IndicadorDeAreaModel> lista)
        {
            int total = lista.Count;
            int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
            int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
            int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
            int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
            return new EvaluacionAreaRow
            {
                Nombre = nombre,
                Total = total,
                Sobrecumplidos = sobre,
                Cumplidos = cumple,
                ParcialmenteCumplidos = parcial,
                Incumplidos = incumple,
                PorcentajeS  = total > 0 ? (double)sobre / total * 100 : 0,
                PorcentajeC  = total > 0 ? (double)cumple / total * 100 : 0,
                PorcentajePC = total > 0 ? (double)parcial / total * 100 : 0,
                PorcentajeI  = total > 0 ? (double)incumple / total * 100 : 0
            };
        }

        var filaEsenciales = CrearFila("Indicadores esenciales", esenciales);
        var filaNecesarios = CrearFila("Indicadores necesarios", necesarios);
        var filaTotales = new EvaluacionAreaRow
        {
            Nombre = "Totales",
            Total = filaEsenciales.Total + filaNecesarios.Total,
            Sobrecumplidos = filaEsenciales.Sobrecumplidos + filaNecesarios.Sobrecumplidos,
            Cumplidos = filaEsenciales.Cumplidos + filaNecesarios.Cumplidos,
            ParcialmenteCumplidos = filaEsenciales.ParcialmenteCumplidos + filaNecesarios.ParcialmenteCumplidos,
            Incumplidos = filaEsenciales.Incumplidos + filaNecesarios.Incumplidos,
            PorcentajeS = 0, PorcentajeC = 0, PorcentajePC = 0, PorcentajeI = 0
        };

        var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
            indicadoresArea.Select(ia => new IndicadorEvaluacionData(ia.Indicador.Tipo, ia.Evaluacion)).ToList()
        );

        using var workbook = new XLWorkbook();
        var sheetName = area.Nombre?.Length > 31 ? area.Nombre[..31] : area.Nombre ?? "Evaluación";
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Título
        worksheet.Cell(1, 1).Value = $"Área: {area.Nombre}";
        worksheet.Range(1, 1, 1, 10).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Encabezados
        string[] headers = { "Indicadores", "Cantidad", "Cantidad S", "% S", "Cantidad C", "% C", "Cantidad PC", "% PC", "Cantidad I", "% I" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(3, i + 1).Value = headers[i];
        var headerRange = worksheet.Range(3, 1, 3, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos
        int row = 4;
        foreach (var fila in new[] { filaEsenciales, filaNecesarios, filaTotales })
        {
            worksheet.Cell(row, 1).Value = fila.Nombre;
            worksheet.Cell(row, 2).Value = fila.Total;
            worksheet.Cell(row, 3).Value = fila.Sobrecumplidos;
            worksheet.Cell(row, 4).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeS:F2}%";
            worksheet.Cell(row, 5).Value = fila.Cumplidos;
            worksheet.Cell(row, 6).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeC:F2}%";
            worksheet.Cell(row, 7).Value = fila.ParcialmenteCumplidos;
            worksheet.Cell(row, 8).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajePC:F2}%";
            worksheet.Cell(row, 9).Value = fila.Incumplidos;
            worksheet.Cell(row, 10).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeI:F2}%";

            var dataRange = worksheet.Range(row, 1, row, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            for (int c = 2; c <= 10; c++)
                worksheet.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
        }

        row++;
        worksheet.Cell(row, 1).Value = $"EVALUACIÓN: {evaluacion.GetDisplayName()}";
        worksheet.Range(row, 1, row, 10).Merge();
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Font.FontSize = 12;

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
        
        public async Task<byte[]> ExportJAProcesoSIGEToExcelAsync(int procesoId, int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos");
        if (proceso == null) return Array.Empty<byte>();

        var area = await unitOfWork.Area.Get(a => a.Id == areaId,
            includeProperties: "IndicadoresDeArea");
        if (area == null) return Array.Empty<byte>();

        
        var indicadoresAreaDict = area.IndicadoresDeArea
            .Where(ia => ia.IndicadorId != null)
            .ToDictionary(ia => ia.IndicadorId, ia => ia);

        var indicadores = proceso.Indicadores
            .Where(i => indicadoresAreaDict.ContainsKey(i.Id))
            .OrderBy(i => i.Id)
            .ToList();

        if (!indicadores.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin datos");
            emptySheet.Cell(1, 1).Value = "No hay indicadores para esta área en este proceso.";
            using var ms = new MemoryStream();
            emptyWorkbook.SaveAs(ms);
            return ms.ToArray();
        }

        var headerBgColor = XLColor.FromHtml("#D2D6D9");
        var headerFontColor = XLColor.FromHtml("#212E58");
        var evalCumplido = XLColor.FromHtml("#34B66B");
        var evalSobrecumplido = XLColor.FromHtml("#1B74B6");
        var evalParcialmente = XLColor.FromHtml("#F7BC20");
        var evalIncumplido = XLColor.FromHtml("#ED7425");
        var fontWhite = XLColor.White;

        using var workbook = new XLWorkbook();
        var sheetName = proceso.Nombre?.Length > 31 ? proceso.Nombre[..31] : proceso.Nombre;
        var worksheet = workbook.Worksheets.Add(sheetName);

        int row = 1;

      
        worksheet.Cell(row, 4).Value = proceso.Nombre;
        var procesoCell = worksheet.Cell(row, 4);
        procesoCell.Style.Font.FontColor = headerFontColor;
        procesoCell.Style.Font.Bold = true;
        procesoCell.Style.Font.FontSize = 14;
        procesoCell.Style.Alignment.WrapText = true;
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
            var areaInd = indicadoresAreaDict[ind.Id];
            var pct = areaInd.MetaCumplirDecimal != 0 && areaInd.MetaRealDecimal != 0
                ? (areaInd.MetaRealDecimal / areaInd.MetaCumplirDecimal) * 100
                : 0;
            string pctText = areaInd.MetaRealDecimal != 0 ? $"{pct:F2}%" : "—";
            var tieneReal = areaInd.MetaRealDecimal != 0;

            worksheet.Cell(row, 2).Value = ind.Id.ToString("D2");
            worksheet.Cell(row, 3).Value = ind.Objetivos.Any()
                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                : "—";
            worksheet.Cell(row, 4).Value = ind.Nombre;
            worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
            worksheet.Cell(row, 6).Value = areaInd.MetaCumplir;
            worksheet.Cell(row, 7).Value = tieneReal ? areaInd.MetaReal : "—";
            worksheet.Cell(row, 8).Value = pctText;
            worksheet.Cell(row, 9).Value = areaInd.Evaluacion.GetDisplayName();

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
            worksheet.Range(row, 2, row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(row, 2, row, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
            var evalCell = worksheet.Cell(row, 9);
            switch (areaInd.Evaluacion)
            {
                case Evaluacion.Sobrecumplido:
                    evalCell.Style.Fill.BackgroundColor = evalSobrecumplido;
                    evalCell.Style.Font.FontColor = fontWhite;
                    break;
                case Evaluacion.Cumplido:
                    evalCell.Style.Fill.BackgroundColor = evalCumplido;
                    evalCell.Style.Font.FontColor = fontWhite;
                    break;
                case Evaluacion.ParcialmenteCumplido:
                    evalCell.Style.Fill.BackgroundColor = evalParcialmente;
                    evalCell.Style.Font.FontColor = fontWhite;
                    break;
                case Evaluacion.Incumplido:
                    evalCell.Style.Fill.BackgroundColor = evalIncumplido;
                    evalCell.Style.Font.FontColor = fontWhite;
                    break;
            }

            row++;
        }
        
        var indicadoresConValores = indicadores
            .Select(ind => new { Indicador = ind, AreaInd = indicadoresAreaDict[ind.Id] })
            .Where(x => x.AreaInd.IsMetaCumplirPorcentaje)
            .ToList();

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
            foreach (var item in indicadoresConValores)
            {
                var ind = item.Indicador;
                var areaInd = item.AreaInd.MapToDto();
                bool esPar = itemIndex % 2 == 0;
                var bgColor = esPar ? XLColor.FromHtml("#FFF2CA") : XLColor.White;

                
                worksheet.Cell(row, 2).Value = ind.Id.ToString("D2");
                worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                var rangoTotal = worksheet.Range(row, 3, row, 5);
                rangoTotal.Merge();
                rangoTotal.Value = areaInd.ValorTotalLabel ?? "Valor Total";
                rangoTotal.Style.Alignment.WrapText = true;
                rangoTotal.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                worksheet.Cell(row, 6).Value = areaInd.ValorTotal?.FormatearDecimal() ?? "—";
                worksheet.Cell(row, 7).Value = "—";

                worksheet.Range(row, 2, row, 7).Style.Fill.BackgroundColor = bgColor;
                worksheet.Range(row, 2, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 2, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;

                
                var pctCuant = areaInd.ValorTotal.HasValue && areaInd.ValorReal.HasValue
                    ? (areaInd.ValorReal.Value / areaInd.ValorTotal.Value) * 100
                    : 0;
                string pctCuantText = pctCuant > 0 ? $"{pctCuant:F2}%" : "—";

                var rangoReal = worksheet.Range(row, 3, row, 5);
                rangoReal.Merge();
                rangoReal.Value = areaInd.ValorRealLabel ?? "Valor Real";
                rangoReal.Style.Alignment.WrapText = true;
                rangoReal.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                worksheet.Cell(row, 6).Value = areaInd.ValorReal?.FormatearDecimal() ?? "—";
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
        worksheet.Column(2).Width = 8;   // ID
        worksheet.Column(3).Width = 15;  // OE
        worksheet.Column(4).Width = 80;  // Indicador
        worksheet.Column(5).Width = 6;   // Tipo
        worksheet.Column(6).Width = 10;  // Meta
        worksheet.Column(7).Width = 10;  // Real
        worksheet.Column(8).Width = 18;  // % Cumplimiento
        worksheet.Column(9).Width = 14;  // Evaluación
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
        
            // Export/Excel Objetivo con el Proceso(X) -> Modelo SIGES (JEFE PROCESO)
            public async Task<byte[]> ExportJPObjetivoSIGEToExcelAsync(int objetivoId, int procesoId)
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var unitOfWork = new UnitOfWork(dbContext);
                var scope = new UnitOfWorkScope(unitOfWork);
                UnitOfWorkAccessor.CurrentScope = scope;
            
                try
                {
                    var objetivo = await unitOfWork.Objetivo.Get(o => o.Id == objetivoId, includeProperties: "Indicadores.Proceso");
                    if (objetivo == null) return Array.Empty<byte>();
            
                    var indicadores = objetivo.Indicadores
                        .Where(i => i.ProcesoId == procesoId)
                        .OrderBy(i => i.Id)
                        .ToList();
            
                    if (!indicadores.Any())
                    {
                        using var emptyWorkbook = new XLWorkbook();
                        var emptySheet = emptyWorkbook.Worksheets.Add("Sin datos");
                        emptySheet.Cell(1, 1).Value = "No hay indicadores para este objetivo en el proceso.";
                        using var ms = new MemoryStream();
                        emptyWorkbook.SaveAs(ms);
                        return ms.ToArray();
                    }
            
                    string nombreProceso = indicadores.First().Proceso?.Nombre ?? "Proceso";
            
                    using var workbook = new XLWorkbook();
                    string sheetName = $"Obj_{objetivo.NumeroObjetivo}";
                    var worksheet = workbook.Worksheets.Add(sheetName);
            
                    int row = 1;
                    worksheet.Cell(row, 1).Value = $"Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre} - Proceso: {nombreProceso}";
                    worksheet.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
                    worksheet.Range(row, 1, row, 9).Merge().Style.Alignment.WrapText = true;
                    row += 2;
            
                    string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPL.", "EVALUACIÓN" };
                    for (int i = 0; i < headers.Length; i++)
                        worksheet.Cell(row, i + 1).Value = headers[i];
            
                    var headerRange = worksheet.Range(row, 1, row, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
                    headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;
            
                    int secuencia = 0;
                    int total = indicadores.Count;
                    bool procesoCellWritten = false;
                    int startDataRow = row;     
            
                    foreach (var ind in indicadores)
                    {
                        secuencia++;
                        decimal? pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100 : null;
                        string pctText = pct.HasValue ? $"{pct:F2}%" : "—";
            
                        worksheet.Cell(row, 1).Value = secuencia.ToString();
                        if (!procesoCellWritten)
                        {
                            worksheet.Cell(row, 2).Value = nombreProceso;
                            procesoCellWritten = true;
                        }
                        worksheet.Cell(row, 3).Value = ind.Id;
                        worksheet.Cell(row, 4).Value = ind.Nombre;
                        worksheet.Cell(row, 4).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, 5).Value = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                        worksheet.Cell(row, 6).Value = ind.MetaCumplir;
                        worksheet.Cell(row, 7).Value = ind.MetaReal ?? "—";
                        worksheet.Cell(row, 8).Value = pctText;
                        worksheet.Cell(row, 9).Value = ind.Evaluacion.GetDisplayName();
            
                        var dataRange = worksheet.Range(row, 1, row, 9);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        for (int c = 1; c <= 9; c++)
                            worksheet.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        if (ind.Tipo == IndicadorTipo.Escencial)
                            worksheet.Cell(row, 4).Style.Font.Bold = true;
            
                        row++;
                    }
            
                   
                    if (total > 1)
                    {
                        worksheet.Range(startDataRow, 2, row - 1, 2).Merge();
                        worksheet.Cell(startDataRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
            
                    worksheet.Column(1).Width = 8;
                    worksheet.Column(2).Width = 28;
                    worksheet.Column(3).Width = 6;
                    worksheet.Column(4).Width = 50;
                    worksheet.Column(5).Width = 6;
                    worksheet.Column(6).Width = 12;
                    worksheet.Column(7).Width = 12;
                    worksheet.Column(8).Width = 12;
                    worksheet.Column(9).Width = 14;
                    worksheet.Column(4).Style.Alignment.WrapText = true;
            
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
                finally
                {
                    UnitOfWorkAccessor.CurrentScope = null;
                }
            }
            
            
            public async Task<byte[]> ExportResumenProcesoJefeToExcelAsync(int procesoId)
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var unitOfWork = new UnitOfWork(dbContext);
                var scope = new UnitOfWorkScope(unitOfWork);
                UnitOfWorkAccessor.CurrentScope = scope;
            
                try
                {
                    var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores");
                    if (proceso == null) return Array.Empty<byte>();
            
                   
                    var data = EvaluateObjetivosAndProcesos.CalcularEvaluacionProceso(proceso);
            
                  
                    var filasExtendidas = new List<FilaResumen>();
                    foreach (var fila in data.Filas)
                    {
                        int scPlusC = fila.Sobrecumplidos + fila.Cumplidos;
                        int scPlusCPlusPC = scPlusC + fila.ParcialmenteCumplidos;
                        int noEvaluados = fila.Total - (fila.Sobrecumplidos + fila.Cumplidos + fila.ParcialmenteCumplidos + fila.Incumplidos);
            
                        filasExtendidas.Add(new FilaResumen
                        {
                            Nombre = fila.Nombre,
                            Total = fila.Total,
                            Sobrecumplidos = fila.Sobrecumplidos,
                            PorcentajeS = fila.PorcentajeS,
                            Cumplidos = fila.Cumplidos,
                            PorcentajeC = fila.PorcentajeC,
                            SCplusC = scPlusC,
                            PorcentajeSCplusC = fila.Total > 0 ? Math.Round((double)scPlusC / fila.Total * 100, 2) : 0,
                            ParcialmenteCumplidos = fila.ParcialmenteCumplidos,
                            PorcentajePC = fila.PorcentajePC,
                            SCplusCplusPC = scPlusCPlusPC,
                            PorcentajeSCplusCplusPC = fila.Total > 0 ? Math.Round((double)scPlusCPlusPC / fila.Total * 100, 2) : 0,
                            Incumplidos = fila.Incumplidos,
                            PorcentajeI = fila.PorcentajeI,
                            NoEvaluados = noEvaluados
                        });
                    }
            
                    using var workbook = new XLWorkbook();
                    var sheetName = (data.NombreProceso?.Length > 31 ? data.NombreProceso[..31] : data.NombreProceso) ?? "Resumen";
                    var worksheet = workbook.Worksheets.Add(sheetName);
            
                   
                    worksheet.Cell(1, 1).Value = $"Proceso: {data.NombreProceso}";
                    worksheet.Range(1, 1, 1, 17).Merge(); 
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                    worksheet.Row(1).Height = 22;
                    
            
                    int headerRow1 = 2;
                    int headerRow2 = 3; 
            
                  
                    worksheet.Range(headerRow1, 1, headerRow2, 1).Merge();
                    worksheet.Cell(headerRow1, 1).Value = "PROCESOS";
                    worksheet.Cell(headerRow1, 1).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
                   
                    worksheet.Range(headerRow1, 2, headerRow2, 2).Merge();
                    worksheet.Cell(headerRow1, 2).Value = "CATEGORÍA";
                    worksheet.Cell(headerRow1, 2).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
                  
                    worksheet.Range(headerRow1, 3, headerRow2, 3).Merge();
                    worksheet.Cell(headerRow1, 3).Value = "TOTAL";
                    worksheet.Cell(headerRow1, 3).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow1, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
                   
                    worksheet.Range(headerRow1, 4, headerRow2, 5).Merge();
                  
                    worksheet.Range(headerRow1, 4, headerRow1, 5).Merge();
                    worksheet.Cell(headerRow1, 4).Value = "SOBRE-CUMPLIDOS";
                    worksheet.Cell(headerRow1, 4).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                   
                    worksheet.Cell(headerRow2, 4).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 4).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 5).Value = "%";
                    worksheet.Cell(headerRow2, 5).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                   
                    worksheet.Range(headerRow1, 6, headerRow1, 7).Merge();
                    worksheet.Cell(headerRow1, 6).Value = "CUMPLIDOS";
                    worksheet.Cell(headerRow1, 6).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 6).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 6).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 7).Value = "%";
                    worksheet.Cell(headerRow2, 7).Style.Font.Bold = true;
            
                   
                    worksheet.Range(headerRow1, 8, headerRow1, 9).Merge();
                    worksheet.Cell(headerRow1, 8).Value = "SC+C";
                    worksheet.Cell(headerRow1, 8).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 8).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 8).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 9).Value = "%";
                    worksheet.Cell(headerRow2, 9).Style.Font.Bold = true;
            
                   
                    worksheet.Range(headerRow1, 10, headerRow1, 11).Merge();
                    worksheet.Cell(headerRow1, 10).Value = "PARCIALMENTE CUMPLIDO";
                    worksheet.Cell(headerRow1, 10).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 10).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 10).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 11).Value = "%";
                    worksheet.Cell(headerRow2, 11).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 10).Style.Alignment.WrapText = true;
                   
                    worksheet.Range(headerRow1, 12, headerRow1, 13).Merge();
                    worksheet.Cell(headerRow1, 12).Value = "SC+C+PC";
                    worksheet.Cell(headerRow1, 12).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 12).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 12).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 13).Value = "%";
                    worksheet.Cell(headerRow2, 13).Style.Font.Bold = true;
            
                  
                    worksheet.Range(headerRow1, 14, headerRow1, 15).Merge();
                    worksheet.Cell(headerRow1, 14).Value = "INCUMPLIDOS";
                    worksheet.Cell(headerRow1, 14).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow2, 14).Value = "TOTAL";
                    worksheet.Cell(headerRow2, 14).Style.Font.Bold = true;
                    worksheet.Cell(headerRow2, 15).Value = "%";
                    worksheet.Cell(headerRow2, 15).Style.Font.Bold = true;
            
                   
                    worksheet.Range(headerRow1, 16, headerRow2, 16).Merge();
                    worksheet.Cell(headerRow1, 16).Value = "NO EVALUADOS";
                    worksheet.Cell(headerRow1, 16).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow1, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(headerRow1, 16).Style.Alignment.WrapText = true;
            
                   
                    worksheet.Range(headerRow1, 17, headerRow2, 17).Merge();
                    worksheet.Cell(headerRow1, 17).Value = "EVALUACIÓN DEL PROCESO";
                    worksheet.Cell(headerRow1, 17).Style.Font.Bold = true;
                    worksheet.Cell(headerRow1, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(headerRow1, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            
                    
                    var headerRange = worksheet.Range(headerRow1, 1, headerRow2, 17);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
                    worksheet.Column(1).Width = 30;
                    worksheet.Column(2).Width = 15;
                    worksheet.Column(3).Width = 8;
                    worksheet.Column(4).Width = 10;
                    worksheet.Column(5).Width = 10;
                    worksheet.Column(6).Width = 10;
                    worksheet.Column(7).Width = 10;
                    worksheet.Column(8).Width = 10;
                    worksheet.Column(9).Width = 10;
                    worksheet.Column(10).Width = 10;
                    worksheet.Column(11).Width = 10;
                    worksheet.Column(12).Width = 10;
                    worksheet.Column(13).Width = 10;
                    worksheet.Column(14).Width = 10;
                    worksheet.Column(15).Width = 10;
                    worksheet.Column(16).Width = 10;
                    worksheet.Column(17).Width = 30;
                    
                    int currentRow = 4;
                    int primeraFila = 4;
                    foreach (var fila in filasExtendidas)
                    {
                        bool esPrimera = (currentRow == primeraFila);
            
                     
                        if (esPrimera)
                        {
                            
                            worksheet.Range(currentRow, 1, currentRow + filasExtendidas.Count - 1, 1).Merge();
                            worksheet.Cell(currentRow, 1).Value = data.NombreProceso;
                            worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            
                         
                            worksheet.Range(currentRow, 17, currentRow + filasExtendidas.Count - 1, 17).Merge();
                            worksheet.Cell(currentRow, 17).Value = data.Evaluacion;
                            worksheet.Cell(currentRow, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 17).Style.Font.Bold = true;
                        }
            
                        // Categoría (col B)
                        worksheet.Cell(currentRow, 2).Value = fila.Nombre;
                        
                        if (fila.Nombre.Contains("esenciales"))
                        {
                            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightYellow; // opcional
                        }
            
                        // Total (col C)
                        worksheet.Cell(currentRow, 3).Value = fila.Total;
                        worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // SOBRE-CUMPLIDOS
                        worksheet.Cell(currentRow, 4).Value = fila.Sobrecumplidos;
                        worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 5).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeS:F2}%";
                        worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // CUMPLIDOS
                        worksheet.Cell(currentRow, 6).Value = fila.Cumplidos;
                        worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 7).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeC:F2}%";
                        worksheet.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // SC+C
                        worksheet.Cell(currentRow, 8).Value = fila.SCplusC;
                        worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 9).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeSCplusC:F2}%";
                        worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // PARCIALMENTE CUMPLIDO
                        worksheet.Cell(currentRow, 10).Value = fila.ParcialmenteCumplidos;
                        worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 11).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajePC:F2}%";
                        worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 11).Style.Alignment.WrapText = true;
            
                        // SC+C+PC
                        worksheet.Cell(currentRow, 12).Value = fila.SCplusCplusPC;
                        worksheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 13).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeSCplusCplusPC:F2}%";
                        worksheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // INCUMPLIDOS
                        worksheet.Cell(currentRow, 14).Value = fila.Incumplidos;
                        worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 15).Value = fila.Nombre == "Totales" ? "—" : $"{fila.PorcentajeI:F2}%";
                        worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
                        // NO EVALUADOS
                        worksheet.Cell(currentRow, 16).Value = fila.NoEvaluados;
                        worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(currentRow, 16).Style.Alignment.WrapText = true;
                       
                        var dataRange = worksheet.Range(currentRow, 1, currentRow, 17);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
                        currentRow++;
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

private class EvaluacionAreaRow
{
    public string Nombre { get; set; }
    public int Total { get; set; }
    public int Sobrecumplidos { get; set; }
    public double PorcentajeS { get; set; }
    public int Cumplidos { get; set; }
    public double PorcentajeC { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public double PorcentajePC { get; set; }
    public int Incumplidos { get; set; }
    public double PorcentajeI { get; set; }
}
        private class UnitOfWorkScope : IUnitOfWorkScope
        {
            public IUnitOfWork UnitOfWork { get; }
            public UnitOfWorkScope(IUnitOfWork uow) => UnitOfWork = uow;
        }
        
       
    }
   
}