using System.Security.Cryptography;

namespace Qr.Application.Common;

public static class QrCodeGenerator
{
    private const string Charset = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public const int Length = 24;

    public static string Generate()
    {
        var characters = new char[Length];
        for (var i = 0; i < Length; i++)
            characters[i] = Charset[RandomNumberGenerator.GetInt32(Charset.Length)];
        return new string(characters);
    }
}