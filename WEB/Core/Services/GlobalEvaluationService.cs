using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WEB.Common;
using WEB.Core.Helpers;
using WEB.Data;
using WEB.Data.Hub;
using WEB.Enums;

namespace WEB.Core.Services;


public class GlobalEvaluationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificacionHub> _hubContext;

    public GlobalEvaluationService(IServiceScopeFactory scopeFactory, IHubContext<NotificacionHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    public async Task EjecutarEvaluacionCompletaAsync()
    {
       
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await EvaluarTodosLosIndicadoresAsync(context);
        await EvaluarTodosLosProcesosAsync(context);
        await EvaluarTodosLosObjetivosAsync(context);

        
        await _hubContext.Clients.All.SendAsync("RefreshPage");
    }

    private async Task EvaluarTodosLosIndicadoresAsync(ApplicationDbContext context)
    {
     
        var indicadoresGlobales = await context.Indicador
            .Include(i => i.IndicadoresDeArea)
            .Where(i => !i.IsDeleted)
            .ToListAsync();

        if (indicadoresGlobales == null) return;

        foreach (var indicador in indicadoresGlobales.Where(i => i != null))
        {
            try
            {
                if (indicador.Evaluacion == Evaluacion.NoEvaluado)
                {
                    indicador.Evaluacion = Evaluacion.Incumplido;
                }
                else if ((indicador.MetaRealDecimal > 0 || indicador.MetaReal != null) && indicador.MetaCumplirDecimal > 0) 
                {
                    indicador.Evaluacion = EvaluacionHelper.EvaluarIndicador(
                        indicador.MetaRealDecimal, 
                        indicador.MetaCumplirDecimal
                    );
                }
                
                if (indicador.IndicadoresDeArea != null)
                {
                    foreach (var indicadorArea in indicador.IndicadoresDeArea.Where(ia => ia != null))
                    {
                        if (indicadorArea.Evaluacion == Evaluacion.NoEvaluado)
                        {
                            indicadorArea.Evaluacion = Evaluacion.Incumplido;
                        }
                        else if (indicadorArea.MetaRealDecimal > 0 && indicadorArea.MetaCumplirDecimal > 0)
                        {
                            indicadorArea.Evaluacion = EvaluacionHelper.EvaluarIndicador(
                                indicadorArea.MetaRealDecimal,
                                indicadorArea.MetaCumplirDecimal
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluando indicador ID {indicador.Id}: {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task EvaluarTodosLosProcesosAsync(ApplicationDbContext context)
    {
        var procesos = await context.Proceso
            .Include(p => p.Indicadores)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        if (procesos == null) return;

        foreach (var proceso in procesos.Where(p => p != null))
        {
            try
            {
                if (proceso.Indicadores == null || !proceso.Indicadores.Any())
                {
                    proceso.Evaluacion = Evaluacion.Incumplido;
                }
                else
                {
                    var evaluationData = proceso.Indicadores
                        .Where(i => i != null)
                        .Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion))
                        .ToList();

                    proceso.Evaluacion = EvaluateObjetivosAndProcesos.Evaluar(evaluationData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluando proceso ID {proceso.Id}: {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task EvaluarTodosLosObjetivosAsync(ApplicationDbContext context)
    {
        var objetivos = await context.Objetivo
            .Include(o => o.Indicadores)
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        if (objetivos == null) return;

        foreach (var objetivo in objetivos.Where(o => o != null))
        {
            try
            {
                if (objetivo.Indicadores == null || !objetivo.Indicadores.Any())
                {
                    objetivo.Evaluacion = Evaluacion.Incumplido;
                }
                else
                {
                    var evaluationData = objetivo.Indicadores
                        .Where(i => i != null)
                        .Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion))
                        .ToList();

                    objetivo.Evaluacion = EvaluateObjetivosAndProcesos.Evaluar(evaluationData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluando objetivo ID {objetivo.Id}: {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
    }
}