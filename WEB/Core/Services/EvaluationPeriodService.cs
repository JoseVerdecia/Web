namespace WEB.Core.Services;

public class EvaluationPeriodService
{
    public bool IsActive { get; private set; }

    public event Action? OnChanged;
    
    public void IniciarEvaluacion()
    {
        if (!IsActive)
        {
            IsActive = true;
            OnChanged?.Invoke();
        }
    }
    
    public void FinalizarEvaluacion()
    {
        if (IsActive)
        {
            IsActive = false;
            OnChanged?.Invoke();
        }
    }
}