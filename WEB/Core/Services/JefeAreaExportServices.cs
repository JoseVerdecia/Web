using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using WEB.Core.Extensions;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Enums;

namespace WEB.Core.Services;

public class JefeAreaExportServices
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    
        public JefeAreaExportServices(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

    #region Exportar -> Todos los indicadores de un área (de todos los procesos)
    public async Task<byte[]> ExportIndicadoresAreaToExcelAsync(int areaId, string nombreArea = null)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var procesos = await unitOfWork.Proceso.GetAll(
            includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");

        var procesosConIndicadores = procesos
            .Where(p => p.Indicadores.Any(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId)))
            .OrderBy(p => p.Nombre)
            .ToList();

        if (!procesosConIndicadores.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
            emptySheet.Cell(1, 1).Value = "No hay indicadores para esta área en ningún proceso.";
            using var mem = new MemoryStream();
            emptyWorkbook.SaveAs(mem);
            return mem.ToArray();
        }

        using var workbook = new XLWorkbook();
        string hojaNombre = !string.IsNullOrEmpty(nombreArea) ? nombreArea : "Indicadores Area";
        if (hojaNombre.Length > 31) hojaNombre = hojaNombre[..31];
        var worksheet = workbook.Worksheets.Add(hojaNombre);

        
        string titulo = !string.IsNullOrEmpty(nombreArea)
                        ? $"Indicadores del área: {nombreArea}"
                        : "Indicadores del Área";
        
        worksheet.Cell(1, 1).Value = titulo;
        worksheet.Range(1, 1, 1, 4).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Row(1).Height = 22;

       
        worksheet.Cell(3, 1).Value = "PROCESO";
        worksheet.Cell(3, 2).Value = "OE";
        worksheet.Cell(3, 3).Value = "INDICADOR";
        worksheet.Cell(3, 4).Value = DateTime.Now.Year.ToString();
        var headerRange = worksheet.Range(3, 1, 3, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
        headerRange.Style.Font.FontColor = XLColor.FromHtml("#212E58");
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Column(1).Width = 30;
        worksheet.Column(2).Width = 15;
        worksheet.Column(3).Width = 80;
        worksheet.Column(4).Width = 14;
        
        var mergeRanges = new List<(int startRow, int endRow)>();

        int row = 4;
        foreach (var proceso in procesosConIndicadores)
        {
            var indicadoresDelProceso = proceso.Indicadores
                .Where(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId))
                .OrderBy(i => i.Id)
                .ToList();

            if (!indicadoresDelProceso.Any()) continue;

            int processStartRow = row;

            
            worksheet.Cell(processStartRow, 1).Value = proceso.Nombre;
            worksheet.Cell(processStartRow, 1).Style.Font.Bold = true;
            worksheet.Cell(processStartRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            foreach (var ind in indicadoresDelProceso)
            {
                var indicadorArea = ind.IndicadoresDeArea.First(ia => ia.AreaId == areaId);
                string oe = ind.Objetivos?.Any() == true
                    ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                    : "—";
                
                worksheet.Cell(row, 2).Value = oe;
                worksheet.Cell(row, 3).Value = ind.Nombre;
                worksheet.Cell(row, 3).Style.Alignment.WrapText=true;
                worksheet.Cell(row, 4).Value = indicadorArea.MetaCumplir;
                
                worksheet.Cell(row, 3).Style.Alignment.WrapText = true;

               
                if (ind.Tipo == IndicadorTipo.Escencial)
                    worksheet.Cell(row, 3).Style.Font.Bold = true;

                
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
            }

            int processEndRow = row - 1;
            mergeRanges.Add((processStartRow, processEndRow));
        }

    
        foreach (var (start, end) in mergeRanges)
        {
            worksheet.Range(start, 1, end, 1).Merge();
            
            worksheet.Range(start, 1, end, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        
        if (row > 4)
        {
            var dataRange = worksheet.Range(4, 1, row - 1, 4);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
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
    public async Task<string> GetIndicadoresAreaHtmlAsync(int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var area = await unitOfWork.Area.GetById(areaId);
        string nombreArea = area?.Nombre ?? "Área";

        var procesos = await unitOfWork.Proceso.GetAll(
            includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");

        var procesosConIndicadores = procesos
            .Where(p => p.Indicadores.Any(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId)))
            .OrderBy(p => p.Nombre)
            .ToList();

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='UTF-8'><style>");
        sb.Append("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.Append("h2 { color: #212E58; }");
        sb.Append("table { border-collapse: collapse; width: 100%; margin-top: 15px; }");
        sb.Append("th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 6px 8px; border: 1px solid #999; text-align: center; font-size: 12px; }");
        sb.Append("td { padding: 5px 8px; border: 1px solid #ccc; font-size: 11px; vertical-align: middle; }");
        sb.Append("td:first-child { text-align: left; }");
        sb.Append("td:not(:first-child) { text-align: center; }");
        sb.Append(".negrita { font-weight: bold; }");
        sb.Append("</style></head><body>");

        sb.Append($"<h2>Indicadores del área: {nombreArea}</h2>");

        if (!procesosConIndicadores.Any())
        {
            sb.Append("<p>No hay indicadores para esta área en ningún proceso.</p>");
        }
        else
        {
            sb.Append("<table>");
            sb.Append("<thead><tr>");
            sb.Append("<th>PROCESO</th>");
            sb.Append("<th>OE</th>");
            sb.Append("<th>INDICADOR</th>");
            sb.Append($"<th>{DateTime.Now.Year}</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var proceso in procesosConIndicadores)
            {
                var indicadoresDelProceso = proceso.Indicadores
                    .Where(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId))
                    .OrderBy(i => i.Id)
                    .ToList();

                int count = indicadoresDelProceso.Count;
                bool first = true;

                foreach (var ind in indicadoresDelProceso)
                {
                    var indicadorArea = ind.IndicadoresDeArea.First(ia => ia.AreaId == areaId);

                    string oe = ind.Objetivos?.Any() == true
                        ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                        : "—";

                    sb.Append("<tr>");

                    if (first)
                    {
                        sb.Append($"<td rowspan=\"{count}\">{proceso.Nombre}</td>");
                        first = false;
                    }

                    sb.Append($"<td>{oe}</td>");

                    string nombreInd = ind.Nombre;
                    if (ind.Tipo == IndicadorTipo.Escencial)
                        nombreInd = $"<span class='negrita'>{nombreInd}</span>";
                    sb.Append($"<td>{nombreInd}</td>");
                    sb.Append($"<td>{indicadorArea.MetaCumplir}</td>");
                    sb.Append("</tr>");
                }
            }
            sb.Append("</tbody></table>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
    public async Task<byte[]> ExportIndicadoresAreaToPdfAsync(int areaId)
{
    string html = await GetIndicadoresAreaHtmlAsync(areaId);

    var launchOptions = new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        Args = new[] { "--no-sandbox" }
    };

    using var browser = await Puppeteer.LaunchAsync(launchOptions);
    using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html, new NavigationOptions
    {
        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
    });

    var pdf = await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        Landscape = false,
        MarginOptions = { Top = "15mm", Bottom = "15mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });

    return pdf;
}
        
    #endregion
    
    public async Task<byte[]> ExportJAAllObjetivosSIGEToExcelAsync(int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var uow = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(uow);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var area = await uow.Area.Get(a => a.Id == areaId, includeProperties: "IndicadoresDeArea");
        if (area == null) return Array.Empty<byte>();

        var indicadoresAreaDict = area.IndicadoresDeArea.ToDictionary(ia => ia.IndicadorId, ia => ia);

        var objetivos = await uow.Objetivo.GetAll(includeProperties: "Indicadores.Proceso");
        var objetivosFiltrados = objetivos
            .Where(o => o.Indicadores.Any(i => indicadoresAreaDict.ContainsKey(i.Id)))
            .OrderBy(o => o.NumeroObjetivo)
            .ToList();

        if (!objetivosFiltrados.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
            emptySheet.Cell(1, 1).Value = "No hay objetivos con indicadores para esta área.";
            using var stream = new MemoryStream();
            emptyWorkbook.SaveAs(stream);
            return stream.ToArray();
        }

        using var workbook = new XLWorkbook();
        string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPL.", "EVALUACIÓN" };

        foreach (var objetivo in objetivosFiltrados)
        {
            // Crear una hoja por objetivo
            var sheetName = $"Obj {objetivo.NumeroObjetivo}";
            var worksheet = workbook.Worksheets.Add(sheetName);

            int row = 1;
            worksheet.Cell(row, 1).Value = $"Objetivo {objetivo.NumeroObjetivo}:{objetivo.Nombre} - Área {area.Nombre}";
            worksheet.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
            row += 2;

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cell(row, i + 1).Value = headers[i];
            var headerRange = worksheet.Range(row, 1, row, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D2D6D9");
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            row++;

            var indicadores = objetivo.Indicadores
                .Where(i => indicadoresAreaDict.ContainsKey(i.Id))
                .OrderBy(i => i.Id)
                .ToList();
            var grupos = indicadores.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);

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
            var defaultSheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Sheet1");
            if (defaultSheet != null)
                defaultSheet.Delete();
           
            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 28;
            worksheet.Column(3).Width = 6;
            worksheet.Column(4).Width = 40;
            worksheet.Column(5).Width = 6;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 10;
            worksheet.Column(8).Width = 12;
            worksheet.Column(9).Width = 16;
            worksheet.Column(2).Style.Alignment.WrapText = true;
            worksheet.Column(4).Style.Alignment.WrapText = true;
        }

        using var streamX = new MemoryStream();
        workbook.SaveAs(streamX);
        return streamX.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
    
    public async Task<byte[]> ExportJAAllObjetivosSIGEToPdfAsync(int areaId)
{
    string html = await GetJAAllObjetivosSIGEHtmlAsync(areaId);

    var launchOptions = new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        Args = new[] { "--no-sandbox" }
    };

    using var browser = await Puppeteer.LaunchAsync(launchOptions);
    using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

    return await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        Landscape = true,
        MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });
}
    private async Task<string> GetJAAllObjetivosSIGEHtmlAsync(int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var uow = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(uow);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var area = await uow.Area.Get(a => a.Id == areaId, includeProperties: "IndicadoresDeArea");
        if (area == null) return "<html><body>Área no encontrada.</body></html>";

        var indicadoresAreaDict = area.IndicadoresDeArea.ToDictionary(ia => ia.IndicadorId, ia => ia);

        var objetivos = await uow.Objetivo.GetAll(includeProperties: "Indicadores.Proceso");
        var objetivosFiltrados = objetivos
            .Where(o => o.Indicadores.Any(i => indicadoresAreaDict.ContainsKey(i.Id)))
            .OrderBy(o => o.NumeroObjetivo)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .siges-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; margin-bottom: 30px; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; }
            .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
            h3 { margin-top: 25px; }
            .page-break { page-break-before: always; }
        ");
        sb.AppendLine("</style></head><body>");

        bool first = true;
        foreach (var objetivo in objetivosFiltrados)
        {
            if (!first)
            {
                sb.AppendLine("<div class='page-break'></div>");
            }
            first = false;

            sb.AppendLine($"<h3>Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre} – Área {area.Nombre}</h3>");
            sb.AppendLine("<table class='siges-table'><thead><tr>");
            sb.AppendLine("<th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPL.</th><th>EVALUACIÓN</th>");
            sb.AppendLine("</tr></thead><tbody>");

            var indicadores = objetivo.Indicadores
                .Where(i => indicadoresAreaDict.ContainsKey(i.Id))
                .OrderBy(i => i.Id)
                .ToList();
            var grupos = indicadores.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);

            int secuencia = 0;
            foreach (var grupo in grupos)
            {
                var inds = grupo.ToList();
                bool primera = true;
                foreach (var ind in inds)
                {
                    secuencia++;
                    string numero = $"{objetivo.NumeroObjetivo}.{secuencia:D2}";
                    var areaInd = indicadoresAreaDict.GetValueOrDefault(ind.Id);
                    string proceso = primera ? ind.Proceso?.Nombre ?? "" : "";
                    string tipo = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                    string meta = areaInd?.MetaCumplir ?? "—";
                    string real = areaInd?.MetaReal ?? "—";
                    string pct = "—";
                    if (areaInd != null && areaInd.MetaCumplirDecimal != 0 && areaInd.MetaRealDecimal != 0)
                        pct = ((areaInd.MetaRealDecimal / areaInd.MetaCumplirDecimal) * 100).ToString("F2") + "%";
                    string eval = areaInd?.Evaluacion.GetDisplayName() ?? "—";

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{numero}</td>");
                    if (primera) sb.AppendLine($"<td rowspan='{inds.Count}'>{proceso}</td>");
                    sb.AppendLine($"<td>{ind.Id}</td><td>{ind.Nombre}</td><td>{tipo}</td>");
                    sb.AppendLine($"<td>{meta}</td><td>{real}</td><td>{pct}</td><td>{eval}</td>");
                    sb.AppendLine("</tr>");
                    primera = false;
                }
            }

            sb.AppendLine("</tbody></table>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
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