using System.Text;

namespace WEB.Core.Extensions;

public static class StringExtensions
{
    public static string GetInitials(this string? fullName, int maxInitials = 2)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "?";

        var words = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = new StringBuilder();

        for (int i = 0; i < Math.Min(words.Length, maxInitials); i++)
        {
            if (words[i].Length > 0)
                initials.Append(char.ToUpper(words[i][0]));
        }

        return initials.Length > 0 ? initials.ToString() : "?";
    }
}