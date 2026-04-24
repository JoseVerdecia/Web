namespace WEB.Core.Helpers;

[AttributeUsage(AttributeTargets.Field)]
public class BadgeColorAttribute : Attribute
{
    public string Color { get; }

    public BadgeColorAttribute(string color)
    {
        Color = color;
    }
}