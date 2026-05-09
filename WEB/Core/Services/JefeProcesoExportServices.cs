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

public class JefeProcesoExportServices
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    
    public JefeProcesoExportServices(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    private class UnitOfWorkScope : IUnitOfWorkScope
    {
        public IUnitOfWork UnitOfWork { get; }
        public UnitOfWorkScope(IUnitOfWork uow) => UnitOfWork = uow;
    }
    
    
    public async Task<byte[]> ExportAllObjetivosProcesoSIGEToExcelAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var uow = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(uow);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await uow.Proceso.GetById(procesoId);
        if (proceso == null) return Array.Empty<byte>();
        string nombreProceso = proceso.Nombre;

        var objetivos = await uow.Objetivo.GetAll(includeProperties: "Indicadores");
        var objetivosFiltrados = objetivos
            .Where(o => o.Indicadores.Any(i => i.ProcesoId == procesoId))
            .OrderBy(o => o.NumeroObjetivo)
            .ToList();

        if (!objetivosFiltrados.Any())
        {
            using var emptyWorkbook = new XLWorkbook();
            var emptySheet = emptyWorkbook.Worksheets.Add("Sin Datos");
            emptySheet.Cell(1, 1).Value = "No hay objetivos con indicadores para este proceso.";
            using var stream = new MemoryStream();
            emptyWorkbook.SaveAs(stream);
            return stream.ToArray();
        }

        using var workbook = new XLWorkbook();
        string[] headers = { "No.", "PROCESOS", "Ind.", "INDICADOR", "TIPO", "META", "REAL", "% CUMPL.", "EVALUACIÓN" };

        foreach (var objetivo in objetivosFiltrados)
        {
            var indicadores = objetivo.Indicadores
                .Where(i => i.ProcesoId == procesoId)
                .OrderBy(i => i.Id)
                .ToList();

            if (!indicadores.Any()) continue;

            var sheetName = $"Obj {objetivo.NumeroObjetivo}";
            var worksheet = workbook.Worksheets.Add(sheetName);

            int row = 1;
            worksheet.Cell(row, 1).Value = $"Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre} - {nombreProceso}";
            worksheet.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 9).Merge().Style.Alignment.WrapText = true;
            row += 2;

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
            int startDataRow = row;

            foreach (var ind in indicadores)
            {
                secuencia++;
                decimal? pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100 : null;
                string pctText = pct.HasValue ? $"{pct:F2}%" : "—";

                worksheet.Cell(row, 1).Value = $"{objetivo.NumeroObjetivo}.{secuencia:D2}";
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

            // Columna "PROCESOS" (col 2) con rowspan
            if (total > 0)
            {
                worksheet.Cell(startDataRow, 2).Value = nombreProceso;
                if (total > 1)
                {
                    worksheet.Range(startDataRow, 2, row - 1, 2).Merge();
                    worksheet.Cell(startDataRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(startDataRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
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
            worksheet.Column(2).Style.Alignment.WrapText = true;
            worksheet.Column(4).Style.Alignment.WrapText = true;
        }
        
        var defaultSheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Sheet1");
        if (defaultSheet != null)
            defaultSheet.Delete();

        using var streamX = new MemoryStream();
        workbook.SaveAs(streamX);
        return streamX.ToArray();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
    
    public async Task<byte[]> ExportAllObjetivosProcesoSIGEToPdfAsync(int procesoId)
{
    string html = await GetAllObjetivosProcesoSIGEHtmlAsync(procesoId);

    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        Args = new[] { "--no-sandbox" }
    });
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

    private async Task<string> GetAllObjetivosProcesoSIGEHtmlAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var uow = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(uow);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await uow.Proceso.GetById(procesoId);
        if (proceso == null) return "<html><body>Proceso no encontrado.</body></html>";
        string nombreProceso = proceso.Nombre;

        var objetivos = await uow.Objetivo.GetAll(includeProperties: "Indicadores");
        var objetivosFiltrados = objetivos
            .Where(o => o.Indicadores.Any(i => i.ProcesoId == procesoId))
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
            var indicadores = objetivo.Indicadores
                .Where(i => i.ProcesoId == procesoId)
                .OrderBy(i => i.Id)
                .ToList();

            if (!indicadores.Any()) continue;

            if (!first)
                sb.AppendLine("<div class='page-break'></div>");
            first = false;

            sb.AppendLine($"<h3>Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre} – {nombreProceso}</h3>");
            sb.AppendLine("<table class='siges-table'><thead><tr>");
            sb.AppendLine("<th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPL.</th><th>EVALUACIÓN</th>");
            sb.AppendLine("</tr></thead><tbody>");

            int secuencia = 0;
            bool primeraFila = true;
            foreach (var ind in indicadores)
            {
                secuencia++;
                string numero = $"{objetivo.NumeroObjetivo}.{secuencia:D2}";
                string tipo = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                string meta = ind.MetaCumplir;
                string real = ind.MetaReal ?? "—";
                string pct = "—";
                if (ind.MetaCumplirDecimal != 0 && ind.MetaRealDecimal != 0)
                    pct = ((ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100).ToString("F2") + "%";
                string eval = ind.Evaluacion.GetDisplayName();

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{numero}</td>");
                if (primeraFila)
                    sb.AppendLine($"<td rowspan='{indicadores.Count}'>{nombreProceso}</td>");
                sb.AppendLine($"<td>{ind.Id}</td><td>{ind.Nombre}</td><td>{tipo}</td>");
                sb.AppendLine($"<td>{meta}</td><td>{real}</td><td>{pct}</td><td>{eval}</td>");
                sb.AppendLine("</tr>");
                primeraFila = false;
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
}