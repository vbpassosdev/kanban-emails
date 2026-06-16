using System.Security.Cryptography;
using System.Text;

namespace KanbanEmails.Infrastructure.Security;

public class CriptografiaService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public CriptografiaService(string chave)
    {
        // SHA256 garante 32 bytes de chave, MD5 garante 16 bytes de IV
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(chave));
        _iv = MD5.HashData(Encoding.UTF8.GetBytes(chave));
    }

    public string Criptografar(string texto)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encriptador = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(texto);
        var cifrado = encriptador.TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(cifrado);
    }

    public string Descriptografar(string textoCifrado)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var decriptador = aes.CreateDecryptor();
        var bytes = Convert.FromBase64String(textoCifrado);
        var plain = decriptador.TransformFinalBlock(bytes, 0, bytes.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
