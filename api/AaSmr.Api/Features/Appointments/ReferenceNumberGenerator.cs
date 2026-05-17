using System.Security.Cryptography;

namespace AaSmr.Api.Features.Appointments;

public static class ReferenceNumberGenerator
{
    // No I, O, 1, 0 to avoid visual ambiguity
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int year) => $"SMR-{year}-{RandomSuffix(6)}";

    private static string RandomSuffix(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(chars);
    }
}
