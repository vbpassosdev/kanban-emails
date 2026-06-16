using System.Security.Cryptography;
using System.Text;

namespace KanbanEmails.Infrastructure.Security;

// Formato armazenado: Base64(IV[16 bytes] + CipherText)
// IV é gerado aleatoriamente por operação para evitar ataques de replay/análise de padrões.
public class CriptografiaService
{
    private readonly byte[] _key;

    public CriptografiaService(string chave)
    {
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(chave));
    }

    public string Criptografar(string texto)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        var encriptador = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(texto);
        var cifrado = encriptador.TransformFinalBlock(bytes, 0, bytes.Length);

        var resultado = new byte[aes.IV.Length + cifrado.Length];
        Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
        Buffer.BlockCopy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);

        return Convert.ToBase64String(resultado);
    }

    public string Descriptografar(string textoCifrado)
    {
        var dados = Convert.FromBase64String(textoCifrado);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[16];
        Buffer.BlockCopy(dados, 0, iv, 0, 16);
        aes.IV = iv;

        var decriptador = aes.CreateDecryptor();
        var plain = decriptador.TransformFinalBlock(dados, 16, dados.Length - 16);
        return Encoding.UTF8.GetString(plain);
    }
}
