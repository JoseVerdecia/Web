using WEB.Enums;

namespace WEB.Common;

public class ErrorDetail
{
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
    public ErrorType Type { get; set; } 
}