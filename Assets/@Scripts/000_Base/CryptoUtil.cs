using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CryptoUtil
{
    // 32바이트 Key (256bit), 16바이트 IV (128bit)
    // 실제 서비스 시 고유한 임의의 문자열로 변경하여 사용
    private static readonly byte[] Key;
    private static readonly byte[] IV;                 

    // 암호화 (Plain Text -> Encrypted Base64 String)
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // 복호화 (Encrypted Base64 String -> Plain Text)
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}