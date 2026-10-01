using System.Security.Cryptography;
using System.Text;

namespace Gateway.Infrastructure.Helpers;

public static class Md5Helper
{
    /// <summary>
    /// Computes the MD5 hash of a plain text string if it's not already a 32-character hex string.
    /// </summary>
    public static string EnsureMd5(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // If it's already a 32-character hex string, assume it's MD5
        if (input.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9a-fA-F]{32}$"))
            return input.ToLowerInvariant();

        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        StringBuilder sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
