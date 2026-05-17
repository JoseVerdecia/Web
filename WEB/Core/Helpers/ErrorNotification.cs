using WEB.Core.Interfaces;
using WEB.Core.Result;

namespace WEB.Core.Helpers;

public static class ErrorNotification
{
    public static void ErrorToast<T>(AppResult<T> result,INotificationService notificacion)
    {
        if (result.Errors.Any())
        {
            foreach (var error in result.Errors)
            {
                notificacion.ShowError(error.Message);
            }
        }
        else
        {
            notificacion.ShowError(result.Errors.First().Message);
        }
    }
}