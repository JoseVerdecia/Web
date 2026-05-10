using System.Text;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Enums;
using WEB.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea;

namespace WEB.Core.Services;

public class ExportPdfService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public ExportPdfService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    //Export/PDF Resumen de Evaluacion de los Procesos (Admin)
    public async Task<byte[]> ExportResumenEvaluacionProcesosToPdfAsync()
    {
        string html = await GetResumenHtmlAsync();

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
            Landscape = true,
            MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
            PrintBackground = true
        });

        return pdf;
    }
    private async Task<string> GetResumenHtmlAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var unitOfWork = new UnitOfWork(dbContext);
        var scope = new UnitOfWorkScope(unitOfWork);
        UnitOfWorkAccessor.CurrentScope = scope;
        try
        {
            var procesos = await unitOfWork.Proceso.GetAll(includeProperties: "Indicadores");

            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
            sb.AppendLine(@"
                .resumen-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; }
                .resumen-table th, .resumen-table td { border: 1px solid #ccc; padding: 6px 4px; vertical-align: middle; text-align: center; }
                .resumen-table th { background-color: #f0f0f0; font-weight: bold; text-transform: uppercase; }
                .celda-proceso { text-align: left !important; font-weight: bold; }
                .celda-evaluacion { font-weight: bold; background-color: #f0f0f0; }
                .fila-esencial td:not(.celda-proceso):not(.celda-evaluacion) { color: red; font-weight: bold; }
            ");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<table class='resumen-table'>");

            // Encabezados
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr><th rowspan='2'>PROCESOS</th><th rowspan='2'>CATEGORÍA</th><th rowspan='2'>TOTAL</th>");
            sb.AppendLine("<th colspan='2'>SOBRE-CUMPLIDOS</th><th colspan='2'>CUMPLIDOS</th><th colspan='2'>SC+C</th>");
            sb.AppendLine("<th colspan='2'>PARCIALMENTE CUMPLIDO</th><th colspan='2'>SC+C+PC</th><th colspan='2'>INCUMPLIDOS</th>");
            sb.AppendLine("<th rowspan='2'>NO EVALUADOS</th><th rowspan='2'>EVALUACIÓN DEL PROCESO</th></tr>");
            sb.AppendLine("<tr><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th></tr>");
            sb.AppendLine("</thead><tbody>");

            foreach (var proceso in procesos.OrderBy(p => p.Nombre))
            {
                var indicadores = proceso.Indicadores;
                var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
                var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
                var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
                    indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());
                var filas = new List<(string nombre, List<IndicadorModel> lista)>
                {
                    ("Indicadores esenciales", esenciales),
                    ("Indicadores necesarios", necesarios)
                };

                bool primeraFila = true;
                foreach (var (nombre, lista) in filas)
                {
                    int total = lista.Count;
                    int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
                    int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
                    int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
                    int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
                    int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);
                    int scPlusC = sobre + cumple;
                    int scPlusCPlusPC = sobre + cumple + parcial;

                   
                    string Pct(int val) => total > 0 ? (val / (double)total * 100).ToString("F2") + "%" : "0.00%";

                    string colorClass = nombre.Contains("esenciales") ? "class='fila-esencial'" : "";
                    sb.AppendLine($"<tr {colorClass}>");
                    if (primeraFila)
                    {
                        sb.AppendLine($"<td class='celda-proceso' rowspan='{filas.Count}'>{proceso.Nombre}</td>");
                        primeraFila = false;
                    }
                    sb.AppendLine($"<td>{nombre}</td>");
                    sb.AppendLine($"<td>{total}</td>");
                    sb.AppendLine($"<td>{sobre}</td><td>{Pct(sobre)}</td>");
                    sb.AppendLine($"<td>{cumple}</td><td>{Pct(cumple)}</td>");
                    sb.AppendLine($"<td>{scPlusC}</td><td>{Pct(scPlusC)}</td>");
                    sb.AppendLine($"<td>{parcial}</td><td>{Pct(parcial)}</td>");
                    sb.AppendLine($"<td>{scPlusCPlusPC}</td><td>{Pct(scPlusCPlusPC)}</td>");
                    sb.AppendLine($"<td>{incumple}</td><td>{Pct(incumple)}</td>");
                    sb.AppendLine($"<td>{noEvaluado}</td>");
                    if (nombre == "Indicadores esenciales")
                        sb.AppendLine($"<td class='celda-evaluacion' rowspan='2'>{evaluacion.GetDisplayName()}</td>");
                    sb.AppendLine("</tr>");
                }
            }

            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }
        finally
        {
            UnitOfWorkAccessor.CurrentScope = null;
        }
    }
    
    // Export/PDF Resumen Evaluacion del Proceso(X) Jefe Proceso
    public async Task<byte[]> ExportEvaluacionProcesoToPdfAsync(int procesoId)
{
    string html = await GetEvaluacionProcesoHtmlAsync(procesoId);

    var launchOptions = new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // Ajusta la ruta si es necesario
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
        Landscape = true,
        MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });

    return pdf;
}

    private async Task<string> GetEvaluacionProcesoHtmlAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores");
        if (proceso == null || !proceso.Indicadores.Any())
            return "<html><body>No se encontraron datos del proceso.</body></html>";

        var indicadores = proceso.Indicadores;
        var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
        var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();

        var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
            indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .resumen-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; }
            .resumen-table th, .resumen-table td { border: 1px solid #ccc; padding: 6px 4px; vertical-align: middle; text-align: center; }
            .resumen-table th { background-color: #f0f0f0; font-weight: bold; text-transform: uppercase; }
            .celda-proceso { text-align: left !important; font-weight: bold; }
            .celda-evaluacion { font-weight: bold; background-color: #f0f0f0; }
            .fila-esencial td:not(.celda-proceso):not(.celda-evaluacion) { color: red; font-weight: bold; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<table class='resumen-table'>");
        
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr><th rowspan='2'>PROCESOS</th><th rowspan='2'>CATEGORÍA</th><th rowspan='2'>TOTAL</th>");
        sb.AppendLine("<th colspan='2'>SOBRE-CUMPLIDOS</th><th colspan='2'>CUMPLIDOS</th><th colspan='2'>SC+C</th>");
        sb.AppendLine("<th colspan='2'>PARCIALMENTE CUMPLIDO</th><th colspan='2'>SC+C+PC</th><th colspan='2'>INCUMPLIDOS</th>");
        sb.AppendLine("<th rowspan='2'>NO EVALUADOS</th><th rowspan='2'>EVALUACIÓN DEL PROCESO</th></tr>");
        sb.AppendLine("<tr><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th></tr>");
        sb.AppendLine("</thead><tbody>");
        
        var filas = new List<(string nombre, List<IndicadorModel> lista)>
        {
            ("Indicadores esenciales", esenciales),
            ("Indicadores necesarios", necesarios)
        };

        bool primeraFila = true;
        foreach (var (nombre, lista) in filas)
        {
            int total = lista.Count;
            int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
            int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
            int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
            int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
            int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);
            int scPlusC = sobre + cumple;
            int scPlusCPlusPC = sobre + cumple + parcial;

            string Pct(int val) => total > 0 ? (val / (double)total * 100).ToString("F2") + "%" : "0.00%";

            string colorClass = nombre.Contains("esenciales") ? "class='fila-esencial'" : "";
            sb.AppendLine($"<tr {colorClass}>");
            if (primeraFila)
            {
                sb.AppendLine($"<td class='celda-proceso' rowspan='2'>{proceso.Nombre}</td>");
                primeraFila = false;
            }
            sb.AppendLine($"<td>{nombre}</td>");
            sb.AppendLine($"<td>{total}</td>");
            sb.AppendLine($"<td>{sobre}</td><td>{Pct(sobre)}</td>");
            sb.AppendLine($"<td>{cumple}</td><td>{Pct(cumple)}</td>");
            sb.AppendLine($"<td>{scPlusC}</td><td>{Pct(scPlusC)}</td>");
            sb.AppendLine($"<td>{parcial}</td><td>{Pct(parcial)}</td>");
            sb.AppendLine($"<td>{scPlusCPlusPC}</td><td>{Pct(scPlusCPlusPC)}</td>");
            sb.AppendLine($"<td>{incumple}</td><td>{Pct(incumple)}</td>");
            sb.AppendLine($"<td>{noEvaluado}</td>");
            if (nombre == "Indicadores esenciales")
                sb.AppendLine($"<td class='celda-evaluacion' rowspan='2'>{evaluacion.GetDisplayName()}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
    
    public async Task<byte[]> ExportObjetivosSIGEsToPdfAsync()
{
    string html = await GetObjetivosSIGEsHtmlAsync();

    var launchOptions = new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // ajusta la ruta si es necesario
        Args = new[] { "--no-sandbox" }
    };

    using var browser = await Puppeteer.LaunchAsync(launchOptions);
    using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html, new NavigationOptions
    {
        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
    });

    return await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        Landscape = true,
        MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });
}

    private async Task<string> GetObjetivosSIGEsHtmlAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;
    try
    {
        var objetivos = await unitOfWork.Objetivo.GetAll(
            includeProperties: "Indicadores,Indicadores.Proceso"
        );

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .siges-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; border: 1px solid #d0d0d0; margin-bottom: 20px; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
            .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .celda-numero { text-align: center; font-variant-numeric: tabular-nums; }
            .celda-proceso { font-weight: 600; background-color: #f4f4f4; text-align: left !important; }
            .celda-id { font-weight: 700; text-align: center; }
            .celda-tipo { text-align: center; }
            .tipo-tag { display: inline-block; padding: 1px 7px; border-radius: 3px; font-size: 0.75rem; font-weight: 700; }
            .tipo-e { background-color: #fff3cd; color: #856404; border: 1px solid #ffc107; }
            .tipo-n { background-color: #d1ecf1; color: #0c5460; border: 1px solid #bee5eb; }
            .evaluacion-cell { display: inline-block; padding: 3px 12px; border-radius: 4px; color: white; font-weight: 600; font-size: 0.82rem; }
            .ev-sobrecumplido { background-color: #2563eb; }
            .ev-cumplido      { background-color: #16a34a; }
            .ev-parcialmente-cumplido { background-color: #eab308; }
            .ev-incumplido    { background-color: #ea580c; }
            .ev-noevaluado    { background-color: #9ca3af; }
            h2 { color: #212E58; }
        ");
        sb.AppendLine("</style></head><body>");

        foreach (var objetivo in objetivos.OrderBy(o => o.NumeroObjetivo))
        {
            var indicadoresObjetivo = objetivo.Indicadores.Where(i => !i.IsDeleted).ToList();
            if (!indicadoresObjetivo.Any()) continue;

            sb.AppendLine($"<h2>Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre}</h2>");
            
            var procesos = indicadoresObjetivo.GroupBy(i => i.Proceso).OrderBy(g => g.Key.Nombre);

            int rowIndex = 0;
            sb.AppendLine("<table class='siges-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr><th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPLIMIENTO</th><th>EVALUACIÓN</th></tr>");
            sb.AppendLine("</thead><tbody>");

            foreach (var procesoGroup in procesos)
            {
                var indicadoresProceso = procesoGroup.ToList();
                bool first = true;
                foreach (var ind in indicadoresProceso)
                {
                    rowIndex++;

                    string meta = ind.MetaCumplir; // ya contiene formato (ej: "100%")
                    string real = ind.MetaReal ?? "—";

                    // Cálculo del porcentaje
                    decimal pct = 0;
                    if (ind.MetaCumplirDecimal != 0)
                    {
                        pct = ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100;
                    }
                    string porcentajeTexto = (pct >= 0 ? pct.ToString("F2") : "0.00") + "%";

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td class='celda-numero'>{rowIndex}</td>");
                    if (first)
                    {
                        sb.AppendLine($"<td class='celda-proceso' rowspan='{indicadoresProceso.Count}'>{procesoGroup.Key.Nombre}</td>");
                        first = false;
                    }
                    sb.AppendLine($"<td class='celda-id'>{ind.Id}</td>");
                    sb.AppendLine($"<td>{ind.Nombre}</td>");
                    string tipoTag = ind.Tipo == IndicadorTipo.Escencial ? "tipo-e" : "tipo-n";
                    string tipoTexto = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                    sb.AppendLine($"<td class='celda-tipo'><span class='tipo-tag {tipoTag}'>{tipoTexto}</span></td>");
                    sb.AppendLine($"<td class='celda-numero'>{meta}</td>");
                    sb.AppendLine($"<td class='celda-numero'>{real}</td>");
                    sb.AppendLine($"<td class='celda-numero'>{porcentajeTexto}</td>");

                    string evalClass = ind.Evaluacion switch
                    {
                        Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                        Evaluacion.Cumplido => "ev-cumplido",
                        Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                        Evaluacion.Incumplido => "ev-incumplido",
                        _ => "ev-noevaluado"
                    };
                    string evalText = ind.Evaluacion.GetDisplayName();
                    sb.AppendLine($"<td class='celda-numero'><span class='evaluacion-cell {evalClass}'>{evalText}</span></td>");
                    sb.AppendLine("</tr>");
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
    
    // Exportar/PDF -> Objetivos con todos los Procesos e Indicadores (ADMIN)
    public async Task<byte[]> ExportObjetivoSIGEToPdfAsync(int objetivoId)
    {
        string html = await GetObjetivoSIGEHtmlAsync(objetivoId);
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // Ajusta según tu instalación
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
    
    private async Task<string> GetObjetivoSIGEHtmlAsync(int objetivoId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var unitOfWork = new UnitOfWork(dbContext);
        var scope = new UnitOfWorkScope(unitOfWork);
        UnitOfWorkAccessor.CurrentScope = scope;
        try
        {
            var objetivo = await unitOfWork.Objetivo.Get(o => o.Id == objetivoId, includeProperties: "Indicadores,Indicadores.Proceso");
            if (objetivo == null) return "<html><body>Objetivo no encontrado.</body></html>";
    
            var indicadores = objetivo.Indicadores.Where(i => !i.IsDeleted).ToList();
            if (!indicadores.Any()) return $"<html><body>El objetivo '{objetivo.Nombre}' no tiene indicadores.</body></html>";
    
            var procesos = indicadores.GroupBy(i => i.Proceso).OrderBy(g => g.Key.Nombre);
    
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
            sb.AppendLine(@"
                .siges-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; border: 1px solid #d0d0d0; }
                .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
                .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
                .celda-numero { text-align: center; font-variant-numeric: tabular-nums; }
                .celda-proceso { font-weight: 600; background-color: #f4f4f4; text-align: left !important; }
                .celda-id { font-weight: 700; text-align: center; }
                .celda-tipo { text-align: center; }
                .tipo-tag { display: inline-block; padding: 1px 7px; border-radius: 3px; font-size: 0.75rem; font-weight: 700; }
                .tipo-e { background-color: #fff3cd; color: #856404; border: 1px solid #ffc107; }
                .tipo-n { background-color: #d1ecf1; color: #0c5460; border: 1px solid #bee5eb; }
                .evaluacion-cell { display: inline-block; padding: 3px 12px; border-radius: 4px; color: white; font-weight: 600; font-size: 0.82rem; }
                .ev-sobrecumplido { background-color: #2563eb; }
                .ev-cumplido      { background-color: #16a34a; }
                .ev-parcialmente-cumplido { background-color: #eab308; }
                .ev-incumplido    { background-color: #ea580c; }
                .ev-noevaluado    { background-color: #9ca3af; }
                h2 { color: #212E58; }
            ");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine($"<h2>Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre}</h2>");
    
            int rowIndex = 0;
            sb.AppendLine("<table class='siges-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr><th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPLIMIENTO</th><th>EVALUACIÓN</th></tr>");
            sb.AppendLine("</thead><tbody>");
    
            foreach (var procesoGroup in procesos)
            {
                var indicadoresProceso = procesoGroup.ToList();
                bool first = true;
                foreach (var ind in indicadoresProceso)
                {
                    rowIndex++;
                    string meta = ind.MetaCumplir;
                    string real = ind.MetaReal ?? "—";
                    decimal pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100) : 0;
                    string porcentajeTexto = pct.ToString("F2") + "%";
    
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td class='celda-numero'>{rowIndex}</td>");
                    if (first)
                    {
                        sb.AppendLine($"<td class='celda-proceso' rowspan='{indicadoresProceso.Count}'>{procesoGroup.Key.Nombre}</td>");
                        first = false;
                    }
                    sb.AppendLine($"<td class='celda-id'>{ind.Id}</td>");
                    sb.AppendLine($"<td>{ind.Nombre}</td>");
                    string tipoTag = ind.Tipo == IndicadorTipo.Escencial ? "tipo-e" : "tipo-n";
                    string tipoTexto = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
                    sb.AppendLine($"<td class='celda-tipo'><span class='tipo-tag {tipoTag}'>{tipoTexto}</span></td>");
                    sb.AppendLine($"<td class='celda-numero'>{meta}</td>");
                    sb.AppendLine($"<td class='celda-numero'>{real}</td>");
                    sb.AppendLine($"<td class='celda-numero'>{porcentajeTexto}</td>");
    
                    string evalClass = ind.Evaluacion switch
                    {
                        Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                        Evaluacion.Cumplido => "ev-cumplido",
                        Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                        Evaluacion.Incumplido => "ev-incumplido",
                        _ => "ev-noevaluado"
                    };
                    sb.AppendLine($"<td class='celda-numero'><span class='evaluacion-cell {evalClass}'>{ind.Evaluacion.GetDisplayName()}</span></td>");
                    sb.AppendLine("</tr>");
                }
            }
    
            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }
        finally
        {
            UnitOfWorkAccessor.CurrentScope = null;
        }
    }
    
    //Export/PDF -> Proceso con todos sus indicadores (Admin-JefeProceso) -> Proyecto Estrategico
    public async Task<byte[]> ExportProcesoTabToPdfAsync(int procesoId)
{
    string html = await GetProcesoTabHtmlAsync(procesoId);

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

    public async Task<string> GetProcesoTabHtmlAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(
            p => p.Id == procesoId,
            includeProperties: "Indicadores,Indicadores.Objetivos"
        );

        if (proceso == null)
            return "<html><body>Proceso no encontrado.</body></html>";

        var indicadores = proceso.Indicadores.Where(i => !i.IsDeleted).ToList();
        if (!indicadores.Any())
            return $"<html><body>El proceso '{proceso.Nombre}' no tiene indicadores.</body></html>";

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .pe-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; border: 1px solid #d0d0d0; }
            .pe-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; font-size: 0.8rem; padding: 8px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
            .pe-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .celda-oe { font-weight: 600; text-align: center; color: #0078d4; }
            .celda-meta { text-align: center; }
            .sin-oe { color: #9ca3af; }
            .fila-escencial { font-weight: 600; }
            h2 { color: #212E58; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h2>Proceso: {proceso.Nombre}</h2>");
        sb.AppendLine("<table class='pe-table'>");
        sb.AppendLine("<thead>");
        sb.AppendLine($"<tr><th>OE</th><th>INDICADOR</th><th>{DateTime.Now.Year}</th></tr>");
        sb.AppendLine("</thead><tbody>");

        foreach (var ind in indicadores.OrderBy(i => i.Id))
        {
            string oeText = "—";
            if (ind.Objetivos.Any())
            {
                oeText = string.Join("/", ind.Objetivos
                    .OrderBy(o => o.NumeroObjetivo)
                    .Select(o => o.NumeroObjetivo));
            }

            string rowClass = ind.Tipo == IndicadorTipo.Escencial ? "fila-escencial" : "";
            sb.AppendLine($"<tr class='{rowClass}'>");
            sb.AppendLine($"<td class='celda-oe'>{oeText}</td>");
            sb.AppendLine($"<td>{ind.Nombre}</td>");
            sb.AppendLine($"<td class='celda-meta'>{ind.MetaCumplir}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}
    public async Task<byte[]> ExportProcesoSIGEToPdfAsync(int procesoId)
{
    string html = await GetProcesoSIGEHtmlAsync(procesoId);

    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // ajustar
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

private async Task<string> GetProcesoSIGEHtmlAsync(int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;
    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId, includeProperties: "Indicadores.Objetivos");
        if (proceso == null) return "<html><body>Proceso no encontrado.</body></html>";

        var indicadores = proceso.Indicadores.Where(i => !i.IsDeleted).OrderBy(i => i.Id).ToList();
        if (!indicadores.Any()) return $"<html><body>El proceso '{proceso.Nombre}' no tiene indicadores.</body></html>";

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .siges-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; border: 1px solid #d0d0d0; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
            .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .celda-numero { text-align: center; font-variant-numeric: tabular-nums; }
            .celda-proceso { font-weight: 600; background-color: #f4f4f4; text-align: left !important; }
            .celda-id { font-weight: 700; text-align: center; }
            .celda-tipo { text-align: center; }
            .tipo-tag { display: inline-block; padding: 1px 7px; border-radius: 3px; font-size: 0.75rem; font-weight: 700; }
            .tipo-e { background-color: #fff3cd; color: #856404; border: 1px solid #ffc107; }
            .tipo-n { background-color: #d1ecf1; color: #0c5460; border: 1px solid #bee5eb; }
            .evaluacion-cell { display: inline-block; padding: 3px 12px; border-radius: 4px; color: white; font-weight: 600; font-size: 0.82rem; }
            .ev-sobrecumplido { background-color: #2563eb; }
            .ev-cumplido      { background-color: #16a34a; }
            .ev-parcialmente-cumplido { background-color: #eab308; }
            .ev-incumplido    { background-color: #ea580c; }
            .ev-noevaluado    { background-color: #9ca3af; }
            h2 { color: #212E58; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h2>Proceso: {proceso.Nombre}</h2>");
        sb.AppendLine("<table class='siges-table'>");
        sb.AppendLine("<thead><tr><th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPLIMIENTO</th><th>EVALUACIÓN</th></tr></thead><tbody>");

        int rowIndex = 0;
       
        int totalFilas = indicadores.Count;
        bool procesoCellWritten = false;

        foreach (var ind in indicadores)
        {
            rowIndex++;
            string meta = ind.MetaCumplir;
            string real = ind.MetaReal ?? "—";
            decimal pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100) : 0;
            string porcentajeTexto = pct.ToString("F2") + "%";

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class='celda-numero'>{rowIndex}</td>");
            if (!procesoCellWritten)
            {
                sb.AppendLine($"<td class='celda-proceso' rowspan='{totalFilas}'>{proceso.Nombre}</td>");
                procesoCellWritten = true;
            }
            sb.AppendLine($"<td class='celda-id'>{ind.Id}</td>");
            sb.AppendLine($"<td>{ind.Nombre}</td>");
            string tipoTag = ind.Tipo == IndicadorTipo.Escencial ? "tipo-e" : "tipo-n";
            string tipoTexto = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
            sb.AppendLine($"<td class='celda-tipo'><span class='tipo-tag {tipoTag}'>{tipoTexto}</span></td>");
            sb.AppendLine($"<td class='celda-numero'>{meta}</td>");
            sb.AppendLine($"<td class='celda-numero'>{real}</td>");
            sb.AppendLine($"<td class='celda-numero'>{porcentajeTexto}</td>");

            string evalClass = ind.Evaluacion switch
            {
                Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                Evaluacion.Cumplido => "ev-cumplido",
                Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                Evaluacion.Incumplido => "ev-incumplido",
                _ => "ev-noevaluado"
            };
            sb.AppendLine($"<td class='celda-numero'><span class='evaluacion-cell {evalClass}'>{ind.Evaluacion.GetDisplayName()}</span></td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

// Export/PDF Objetivo con el Proceso(X)-> Modelo SIGES (Jefe Proceso)
public async Task<byte[]> ExportObjetivoProcesoSIGEToPdfAsync(int objetivoId, int procesoId)
{
    string html = await GetObjetivoProcesoSIGEHtmlAsync(objetivoId, procesoId);

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

private async Task<string> GetObjetivoProcesoSIGEHtmlAsync(int objetivoId, int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId);
        var objetivo = await unitOfWork.Objetivo.Get(o => o.Id == objetivoId);
        if (proceso == null || objetivo == null) return "<html><body>Datos no encontrados.</body></html>";

        var indicadores = (await unitOfWork.Indicador.GetAll(includeProperties: "Objetivos,Proceso"))
            .Where(i => i.ProcesoId == procesoId && i.Objetivos.Any(o => o.Id == objetivoId) && !i.IsDeleted)
            .OrderBy(i => i.Id)
            .ToList();

        if (!indicadores.Any()) return $"<html><body>No hay indicadores para el objetivo '{objetivo.Nombre}' en el proceso '{proceso.Nombre}'.</body></html>";

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
                .siges-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; border: 1px solid #d0d0d0; }
                .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
                .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
                .celda-numero { text-align: center; font-variant-numeric: tabular-nums; }
                .celda-proceso { font-weight: 600; background-color: #f4f4f4; text-align: left !important; }
                .celda-id { font-weight: 700; text-align: center; }
                .celda-tipo { text-align: center; }
                .tipo-tag { display: inline-block; padding: 1px 7px; border-radius: 3px; font-size: 0.75rem; font-weight: 700; }
                .tipo-e { background-color: #fff3cd; color: #856404; border: 1px solid #ffc107; }
                .tipo-n { background-color: #d1ecf1; color: #0c5460; border: 1px solid #bee5eb; }
                .evaluacion-cell { display: inline-block; padding: 3px 12px; border-radius: 4px; color: white; font-weight: 600; font-size: 0.82rem; }
                .ev-sobrecumplido { background-color: #2563eb; }
                .ev-cumplido      { background-color: #16a34a; }
                .ev-parcialmente-cumplido { background-color: #eab308; }
                .ev-incumplido    { background-color: #ea580c; }
                .ev-noevaluado    { background-color: #9ca3af; }
                h2 { color: #212E58; }
            ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h2>Objetivo {objetivo.NumeroObjetivo}: {objetivo.Nombre} (Proceso: {proceso.Nombre})</h2>");
        sb.AppendLine("<table class='siges-table'>");
        sb.AppendLine("<thead><tr><th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPLIMIENTO</th><th>EVALUACIÓN</th></tr></thead><tbody>");

        int rowIndex = 0;
        int total = indicadores.Count;
        bool procesoCellWritten = false;
        foreach (var ind in indicadores)
        {
            rowIndex++;
            string meta = ind.MetaCumplir;
            string real = ind.MetaReal ?? "—";
            decimal pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal * 100) : 0;
            string porcentajeTexto = pct.ToString("F2") + "%";

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class='celda-numero'>{rowIndex}</td>");
            if (!procesoCellWritten)
            {
                sb.AppendLine($"<td class='celda-proceso' rowspan='{total}'>{proceso.Nombre}</td>");
                procesoCellWritten = true;
            }
            sb.AppendLine($"<td class='celda-id'>{ind.Id}</td>");
            sb.AppendLine($"<td>{ind.Nombre}</td>");
            string tipoTag = ind.Tipo == IndicadorTipo.Escencial ? "tipo-e" : "tipo-n";
            string tipoTexto = ind.Tipo == IndicadorTipo.Escencial ? "E" : "N";
            sb.AppendLine($"<td class='celda-tipo'><span class='tipo-tag {tipoTag}'>{tipoTexto}</span></td>");
            sb.AppendLine($"<td class='celda-numero'>{meta}</td>");
            sb.AppendLine($"<td class='celda-numero'>{real}</td>");
            sb.AppendLine($"<td class='celda-numero'>{porcentajeTexto}</td>");
            string evalClass = ind.Evaluacion switch
            {
                Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                Evaluacion.Cumplido => "ev-cumplido",
                Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                Evaluacion.Incumplido => "ev-incumplido",
                _ => "ev-noevaluado"
            };
            sb.AppendLine($"<td class='celda-numero'><span class='evaluacion-cell {evalClass}'>{ind.Evaluacion.GetDisplayName()}</span></td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally { UnitOfWorkAccessor.CurrentScope = null; }
}
    
// Exportar/PDF -> Resumen de Evaluacion de Todos los Objetivos (ADMIN)
public async Task<byte[]> ExportResumenEvaluacionObjetivosToPdfAsync()
{
    string html = await GetResumenObjetivosHtmlAsync();

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
        Landscape = true,
        MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });

    return pdf;
}

private async Task<string> GetResumenObjetivosHtmlAsync()
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivos = await unitOfWork.Objetivo.GetAll(includeProperties: "Indicadores");
        if (!objetivos.Any()) return "<html><body>No hay objetivos registrados.</body></html>";

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .resumen-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; }
            .resumen-table th, .resumen-table td { border: 1px solid #ccc; padding: 6px 4px; vertical-align: middle; text-align: center; }
            .resumen-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; text-transform: uppercase; }
            .celda-proceso { text-align: left !important; font-weight: bold; background-color: #f0f0f0; }
            .celda-evaluacion { font-weight: bold; background-color: #f0f0f0; }
            .fila-esencial td:not(.celda-proceso):not(.celda-evaluacion) { color: red; font-weight: bold; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<table class='resumen-table'>");

        // Headers
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr><th rowspan='2'>OBJETIVOS</th><th rowspan='2'>CATEGORÍA</th><th rowspan='2'>TOTAL</th>");
        sb.AppendLine("<th colspan='2'>SOBRE-CUMPLIDOS</th><th colspan='2'>CUMPLIDOS</th><th colspan='2'>SC+C</th>");
        sb.AppendLine("<th colspan='2'>PARCIALMENTE CUMPLIDOS</th><th colspan='2'>SC+C+PC</th><th colspan='2'>INCUMPLIDOS</th>");
        sb.AppendLine("<th rowspan='2'>NO EVALUADOS</th><th rowspan='2'>EVALUACIÓN DEL OBJETIVO ESTRATÉGICO</th></tr>");
        sb.AppendLine("<tr><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th><th>TOTAL</th><th>%</th></tr>");
        sb.AppendLine("</thead><tbody>");

        foreach (var objetivo in objetivos.OrderBy(o => o.NumeroObjetivo))
        {
            var indicadores = objetivo.Indicadores;
            var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
            var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
            var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
                indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());

            bool primeraFila = true;
            foreach (var (nombre, lista) in new[] { ("Indicadores esenciales", esenciales), ("Indicadores necesarios", necesarios) })
            {
                int total = lista.Count;
                int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
                int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
                int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
                int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
                int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);
                int scPlusC = sobre + cumple;
                int scPlusCPlusPC = sobre + cumple + parcial;

                string Pct(int val) => total > 0 ? (val / (double)total * 100).ToString("F2") + "%" : "0.00%";
                string colorClass = nombre.Contains("esenciales") ? "class='fila-esencial'" : "";

                sb.AppendLine($"<tr {colorClass}>");
                if (primeraFila)
                {
                    sb.AppendLine($"<td class='celda-proceso' rowspan='2'>Objetivo {objetivo.NumeroObjetivo}</td>");
                    primeraFila = false;
                }
                sb.AppendLine($"<td>{nombre}</td>");
                sb.AppendLine($"<td>{total}</td>");
                sb.AppendLine($"<td>{sobre}</td><td>{Pct(sobre)}</td>");
                sb.AppendLine($"<td>{cumple}</td><td>{Pct(cumple)}</td>");
                sb.AppendLine($"<td>{scPlusC}</td><td>{Pct(scPlusC)}</td>");
                sb.AppendLine($"<td>{parcial}</td><td>{Pct(parcial)}</td>");
                sb.AppendLine($"<td>{scPlusCPlusPC}</td><td>{Pct(scPlusCPlusPC)}</td>");
                sb.AppendLine($"<td>{incumple}</td><td>{Pct(incumple)}</td>");
                sb.AppendLine($"<td>{noEvaluado}</td>");
                if (nombre == "Indicadores esenciales")
                    sb.AppendLine($"<td class='celda-evaluacion' rowspan='2'>{evaluacion.GetDisplayName()}</td>");
                sb.AppendLine("</tr>");
            }
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

//Export/PDF -> Proyecto Estrategico del Jefe de Area con sus indicadores de area de un Proceso(X)
public async Task<byte[]> ExportPEJATabToPdfAsync(int procesoId, int areaId)
{
    string html = await GetPEJAHtmlAsync(procesoId, areaId);

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

public async Task<string> GetPEJAHtmlAsync(int procesoId, int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos,Indicadores.IndicadoresDeArea");
        if (proceso == null) return "<html><body>Proceso no encontrado.</body></html>";

        var indicadoresConArea = proceso.Indicadores
            .Where(i => i.IndicadoresDeArea.Any(ia => ia.AreaId == areaId))
            .OrderBy(i => i.Id)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            body { font-family: Arial, sans-serif; }
            .pe-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; }
            .pe-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; }
            .pe-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .celda-oe { font-weight: 600; text-align: center; color: #0078d4; }
            .celda-meta { text-align: center; }
            .sin-oe { color: #9ca3af; }
            .fila-escencial { font-weight: 600; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h3>Proceso: {proceso.Nombre}</h3>");
        sb.AppendLine("<table class='pe-table'>");
        sb.AppendLine("<thead><tr><th>OE</th><th>INDICADOR</th><th>" + DateTime.Now.Year + "</th></tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (var ind in indicadoresConArea)
        {
            var area = ind.IndicadoresDeArea.First(ia => ia.AreaId == areaId);
            string oe = ind.Objetivos?.Any() == true
                ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo))
                : "—";

            string estiloFila = ind.Tipo == IndicadorTipo.Escencial ? " class='fila-escencial'" : "";
            sb.AppendLine($"<tr{estiloFila}>");
            sb.AppendLine($"<td class='celda-oe'>{oe}</td>");
            sb.AppendLine($"<td>{ind.Nombre}</td>");
            sb.AppendLine($"<td class='celda-meta'>{area.MetaCumplir}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally
    {
        UnitOfWorkAccessor.CurrentScope = null;
    }
}

public async Task<byte[]> ExportEvaluacionAreaToPdfAsync(int areaId)
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
        
        FilaEvaluacion CrearFila(string nombre, List<IndicadorDeAreaModel> lista)
        {
            int total = lista.Count;
            int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
            int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
            int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
            int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
            return new FilaEvaluacion
            {
                Nombre = nombre,
                Total = total,
                Sobrecumplidos = sobre,
                Cumplidos = cumple,
                ParcialmenteCumplidos = parcial,
                Incumplidos = incumple,
                PorcentajeS = total > 0 ? (double)sobre / total * 100 : 0,
                PorcentajeC = total > 0 ? (double)cumple / total * 100 : 0,
                PorcentajePC = total > 0 ? (double)parcial / total * 100 : 0,
                PorcentajeI = total > 0 ? (double)incumple / total * 100 : 0
            };
        }

        var filaEsenciales = CrearFila("Indicadores esenciales", esenciales);
        var filaNecesarios = CrearFila("Indicadores necesarios", necesarios);
        var filaTotales = new FilaEvaluacion
        {
            Nombre = "Totales",
            Total = filaEsenciales.Total + filaNecesarios.Total,
            Sobrecumplidos = filaEsenciales.Sobrecumplidos + filaNecesarios.Sobrecumplidos,
            Cumplidos = filaEsenciales.Cumplidos + filaNecesarios.Cumplidos,
            ParcialmenteCumplidos = filaEsenciales.ParcialmenteCumplidos + filaNecesarios.ParcialmenteCumplidos,
            Incumplidos = filaEsenciales.Incumplidos + filaNecesarios.Incumplidos
        };

        var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
            indicadoresArea.Select(ia => new IndicadorEvaluacionData(ia.Indicador.Tipo, ia.Evaluacion)).ToList());

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Item().Text($"Área: {area.Nombre}").FontSize(12).Bold();
                    column.Item().PaddingTop(8);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); columns.RelativeColumn(1); columns.RelativeColumn(1);
                            columns.RelativeColumn(1); columns.RelativeColumn(1); columns.RelativeColumn(1);
                            columns.RelativeColumn(1); columns.RelativeColumn(1); columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
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

                        foreach (var fila in new[] { filaEsenciales, filaNecesarios, filaTotales })
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
                    column.Item().Text($"EVALUACIÓN: {evaluacion.GetDisplayName()}").FontSize(12).Bold();
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
public async Task<byte[]> ExportJAObjetivoSIGEToPdfAsync(int objetivoId, int areaId)
{
    string html = await GetJAObjetivoSIGEHtmlAsync(objetivoId, areaId);

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

private async Task<string> GetJAObjetivoSIGEHtmlAsync(int objetivoId, int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var uow = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(uow);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivo = await uow.Objetivo.Get(o => o.Id == objetivoId, includeProperties: "Indicadores.Proceso");
        if (objetivo == null) return "<html><body>Objetivo no encontrado.</body></html>";

        var area = await uow.Area.Get(a => a.Id == areaId, includeProperties: "IndicadoresDeArea");
        if (area == null) return "<html><body>Área no encontrada.</body></html>";

        var indicadoresAreaDict = area.IndicadoresDeArea.ToDictionary(ia => ia.IndicadorId, ia => ia);
        var indicadores = objetivo.Indicadores.OrderBy(i => i.Id).ToList();
        var grupos = indicadores.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            .siges-table { width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #ccc; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 8px; border: 1px solid #c0c0c0; text-align: center; }
            .siges-table td { padding: 6px 8px; border: 1px solid #d0d0d0; vertical-align: middle; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h3>Objetivo {objetivo.NumeroObjetivo} – Área {area.Nombre}</h3>");
        sb.AppendLine("<table class='siges-table'><thead><tr>");
        sb.AppendLine("<th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPL.</th><th>EVALUACIÓN</th>");
        sb.AppendLine("</tr></thead><tbody>");

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

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
    finally { UnitOfWorkAccessor.CurrentScope = null; }
}

public async Task<byte[]> ExportJAProcesoSIGEToPdfAsync(int procesoId, int areaId)
{
    string html = await GetJAProcesoSIGEHtmlAsync(procesoId, areaId);

    var launchOptions = new LaunchOptions
    {
        Headless = true,
        ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // Ajusta la ruta si es necesario
        Args = new[] { "--no-sandbox" }
    };

    using var browser = await Puppeteer.LaunchAsync(launchOptions);
    using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html, new NavigationOptions
    {
        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
    });

    return await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        Landscape = true,
        MarginOptions = { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" },
        PrintBackground = true
    });
}

private async Task<string> GetJAProcesoSIGEHtmlAsync(int procesoId, int areaId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var proceso = await unitOfWork.Proceso.Get(p => p.Id == procesoId,
            includeProperties: "Indicadores.Objetivos");
        if (proceso == null) return "<html><body>Proceso no encontrado.</body></html>";

        var area = await unitOfWork.Area.Get(a => a.Id == areaId,
            includeProperties: "IndicadoresDeArea");
        if (area == null) return "<html><body>Área no encontrada.</body></html>";

        var indicadoresAreaDict = area.IndicadoresDeArea
            .Where(ia => ia.IndicadorId != null)
            .ToDictionary(ia => ia.IndicadorId, ia => ia);

        var indicadores = proceso.Indicadores
            .Where(i => indicadoresAreaDict.ContainsKey(i.Id))
            .OrderBy(i => i.Id)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            body { font-family: Arial, sans-serif; font-size: 10px; }
            .siges-table { width: 100%; border-collapse: collapse; border: 1px solid #ccc; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 6px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
            .siges-table td { padding: 5px 6px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .col-id { width: 40px; text-align: center; }
            .col-oe { width: 80px; text-align: center; }
            .col-indicador { min-width: 200px; }
            .col-tipo { width: 40px; text-align: center; }
            .col-meta, .col-real, .col-porcentaje { width: 70px; text-align: center; }
            .col-evaluacion { width: 90px; text-align: center; }
            .evaluacion-cell { display: inline-block; padding: 2px 8px; border-radius: 3px; color: white; font-weight: bold; }
            .ev-sobrecumplido { background-color: #1B74B6; }
            .ev-cumplido { background-color: #34B66B; }
            .ev-parcialmente-cumplido { background-color: #F7BC20; }
            .ev-incumplido { background-color: #ED7425; }
            .ev-noevaluado { background-color: #9ca3af; }
            .celda-vacia { color: #9ca3af; }
            .fila-par { background-color: #FFF2CA; }
            .fila-impar { background-color: #ffffff; }
            .valor-label { font-style: italic; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h3>Proceso: {proceso.Nombre} – Área: {area.Nombre}</h3>");

       
        sb.AppendLine("<table class='siges-table'><thead><tr>");
        sb.AppendLine("<th>ID</th><th>OE</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPL.</th><th>EVALUACIÓN</th>");
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var ind in indicadores)
        {
            var areaInd = indicadoresAreaDict[ind.Id];
            var pct = areaInd.MetaCumplirDecimal != 0 && areaInd.MetaRealDecimal != 0
                ? (areaInd.MetaRealDecimal / areaInd.MetaCumplirDecimal) * 100
                : 0;
            string pctText = areaInd.MetaRealDecimal != 0 ? $"{pct:F2}%" : "—";
            var tieneReal = areaInd.MetaRealDecimal != 0;
            string evaluacionClase = areaInd.Evaluacion switch
            {
                Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                Evaluacion.Cumplido => "ev-cumplido",
                Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                Evaluacion.Incumplido => "ev-incumplido",
                _ => "ev-noevaluado"
            };

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class='col-id'>{ind.Id:D2}</td>");
            sb.AppendLine($"<td class='col-oe'>{(ind.Objetivos.Any() ? string.Join("/", ind.Objetivos.OrderBy(o => o.NumeroObjetivo).Select(o => o.NumeroObjetivo)) : "—")}</td>");
            sb.AppendLine($"<td class='col-indicador'>{(ind.Tipo == IndicadorTipo.Escencial ? "<b>" + ind.Nombre + "</b>" : ind.Nombre)}</td>");
            sb.AppendLine($"<td class='col-tipo'>{(ind.Tipo == IndicadorTipo.Escencial ? "E" : "N")}</td>");
            sb.AppendLine($"<td class='col-meta'>{areaInd.MetaCumplir}</td>");
            sb.AppendLine($"<td class='col-real'>{(tieneReal ? areaInd.MetaReal : "—")}</td>");
            sb.AppendLine($"<td class='col-porcentaje'>{pctText}</td>");
            sb.AppendLine($"<td class='col-evaluacion'><span class='evaluacion-cell {evaluacionClase}'>{areaInd.Evaluacion.GetDisplayName()}</span></td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        
        var indicadoresConValores = indicadores
            .Select(ind => new { Indicador = ind, AreaInd = indicadoresAreaDict[ind.Id] })
            .Where(x => x.AreaInd.IsMetaCumplirPorcentaje)
            .ToList();

        if (indicadoresConValores.Any())
        {
            sb.AppendLine("<br><br>");
            sb.AppendLine("<table class='siges-table'><thead><tr>");
            sb.AppendLine("<th>ID</th><th>Valores Cuantitativos</th><th>Datos</th><th>Porcentaje</th>");
            sb.AppendLine("</tr></thead><tbody>");

            int index = 0;
            foreach (var item in indicadoresConValores)
            {
                var ind = item.Indicador;
                var areaInd = item.AreaInd.MapToDto();
                bool esPar = index % 2 == 0;
                string claseFila = esPar ? "fila-par" : "fila-impar";
                
                sb.AppendLine($"<tr class='{claseFila}'>");
                sb.AppendLine($"<td class='col-id' rowspan='2' style='vertical-align: middle;'>{ind.Id:D2}</td>");
                sb.AppendLine($"<td><span class='valor-label'>{"Valor Total"}</span></td>");
                sb.AppendLine($"<td>{areaInd.ValorTotal?.FormatearDecimal() ?? "—"}</td>");
                sb.AppendLine($"<td>—</td>");
                sb.AppendLine("</tr>");
                
                var pctCuant = areaInd.ValorTotal.HasValue && areaInd.ValorReal.HasValue
                    ? (areaInd.ValorReal.Value / areaInd.ValorTotal.Value) * 100
                    : 0;
                string pctCuantText = pctCuant > 0 ? $"{pctCuant:F2}%" : "—";

                sb.AppendLine($"<tr class='{claseFila}'>");
                sb.AppendLine($"<td><span class='valor-label'>{"Valor Real"}</span></td>");
                sb.AppendLine($"<td>{areaInd.ValorReal?.FormatearDecimal() ?? "—"}</td>");
                sb.AppendLine($"<td>{pctCuantText}</td>");
                sb.AppendLine("</tr>");

                index++;
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

public async Task<byte[]> ExportJPObjetivoSIGEToPdfAsync(int objetivoId, int procesoId)
{
    string html = await GetJPObjetivoSIGEHtmlAsync(objetivoId, procesoId);

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

private async Task<string> GetJPObjetivoSIGEHtmlAsync(int objetivoId, int procesoId)
{
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    var unitOfWork = new UnitOfWork(dbContext);
    var scope = new UnitOfWorkScope(unitOfWork);
    UnitOfWorkAccessor.CurrentScope = scope;

    try
    {
        var objetivo = await unitOfWork.Objetivo.Get(o => o.Id == objetivoId, includeProperties: "Indicadores.Proceso");
        if (objetivo == null) return "<html><body>Objetivo no encontrado.</body></html>";

        var indicadores = objetivo.Indicadores
            .Where(i => i.ProcesoId == procesoId)
            .OrderBy(i => i.Id)
            .ToList();

        if (!indicadores.Any())
            return "<html><body>No hay indicadores para este objetivo en el proceso.</body></html>";

        string nombreProceso = indicadores.First().Proceso?.Nombre ?? "Proceso";

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
        sb.AppendLine(@"
            body { font-family: Arial, sans-serif; font-size: 10px; }
            .siges-table { width: 100%; border-collapse: collapse; border: 1px solid #ccc; }
            .siges-table th { background-color: #D2D6D9; color: #212E58; font-weight: bold; padding: 6px; border: 1px solid #c0c0c0; text-align: center; text-transform: uppercase; }
            .siges-table td { padding: 5px 6px; border: 1px solid #d0d0d0; vertical-align: middle; }
            .col-numero { width: 40px; text-align: center; }
            .col-proceso { width: 120px; text-align: center; }
            .col-id { width: 40px; text-align: center; }
            .col-indicador { }
            .col-tipo { width: 40px; text-align: center; }
            .col-meta, .col-real, .col-porcentaje { width: 70px; text-align: center; }
            .col-evaluacion { width: 90px; text-align: center; }
            .evaluacion-cell { display: inline-block; padding: 2px 8px; border-radius: 3px; color: white; font-weight: bold; }
            .ev-sobrecumplido { background-color: #2563eb; }
            .ev-cumplido { background-color: #16a34a; }
            .ev-parcialmente-cumplido { background-color: #eab308; }
            .ev-incumplido { background-color: #ea580c; }
            .ev-noevaluado { background-color: #9ca3af; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h3>Objetivo {objetivo.NumeroObjetivo} – Proceso: {nombreProceso}</h3>");

        sb.AppendLine("<table class='siges-table'><thead><tr>");
        sb.AppendLine("<th>No.</th><th>PROCESOS</th><th>Ind.</th><th>INDICADOR</th><th>TIPO</th><th>META</th><th>REAL</th><th>% CUMPL.</th><th>EVALUACIÓN</th>");
        sb.AppendLine("</tr></thead><tbody>");

        int secuencia = 0;
        bool procesoCellWritten = false;
        int total = indicadores.Count;

        foreach (var ind in indicadores)
        {
            secuencia++;
            decimal? pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100 : null;
            string pctText = pct.HasValue ? $"{pct:F2}%" : "—";
            string evalClase = ind.Evaluacion switch
            {
                Evaluacion.Sobrecumplido => "ev-sobrecumplido",
                Evaluacion.Cumplido => "ev-cumplido",
                Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
                Evaluacion.Incumplido => "ev-incumplido",
                _ => "ev-noevaluado"
            };

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class='col-numero'>{secuencia}</td>");
            if (!procesoCellWritten)
            {
                sb.AppendLine($"<td class='col-proceso' rowspan='{total}'>{nombreProceso}</td>");
                procesoCellWritten = true;
            }
            sb.AppendLine($"<td class='col-id'>{ind.Id}</td>");
            sb.AppendLine($"<td class='col-indicador'>{(ind.Tipo == IndicadorTipo.Escencial ? "<b>" + ind.Nombre + "</b>" : ind.Nombre)}</td>");
            sb.AppendLine($"<td class='col-tipo'>{(ind.Tipo == IndicadorTipo.Escencial ? "E" : "N")}</td>");
            sb.AppendLine($"<td class='col-meta'>{ind.MetaCumplir}</td>");
            sb.AppendLine($"<td class='col-real'>{ind.MetaReal ?? "—"}</td>");
            sb.AppendLine($"<td class='col-porcentaje'>{pctText}</td>");
            sb.AppendLine($"<td class='col-evaluacion'><span class='evaluacion-cell {evalClase}'>{ind.Evaluacion.GetDisplayName()}</span></td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
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