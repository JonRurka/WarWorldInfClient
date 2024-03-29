using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public static class HashHelper
{
    public struct KeyPair {
        public byte[] Key;
        public byte[] IV;

        public KeyPair(byte[] key, byte[] iv) {
            Key = key;
            IV = iv;
        }
    }

    private static List<string> _generatedKeys = new List<string> ();
	
	public static string RandomKey(int length){
		char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
		char[] identifier = new char[length];
		byte[] randomData = new byte[length];
		RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider ();
		rng.GetBytes(randomData);

		for (int i = 0; i < identifier.Length; i++) {
			int pos = randomData[i] % chars.Length;
			identifier[i] = chars[pos];
		}
		string key = new string (identifier);
		if (_generatedKeys.Contains (key))
			key = RandomKey (length);
		return key;
	}

    public static byte[] RandomBytes(int seed, int length) {
        Random rand = new Random(seed);
        byte[] data = new byte[length];
        for (int i = 0; i < data.Length; i++) {
            data[i] = (byte)rand.Next(0, 256);
        }
        return data;
    }

	public static string MD5Hash(string value){
		StringBuilder sBuilder = new StringBuilder();
		MD5 md5Hash = MD5.Create (); 
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
		for (int i = 0; i < data.Length; i++){
			sBuilder.Append(data[i].ToString("x2"));
		}
		return sBuilder.ToString ();
	}
	
	public static string HashPasswordClient(string value, string salt){
		return MD5Hash(value + salt);
	}

    public static string Encrypt(string input, string pass) {
        return Encrypt(Encoding.UTF8.GetBytes(input), pass);
    }

    public static string Encrypt(byte[] input, string password) {
        var aesAlg = NewRijndaelManaged(password);

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt)) {
            swEncrypt.Write(Encoding.UTF8.GetString(input));
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string Decrypt(string input, string password) {
        return Decrypt(Convert.FromBase64String(input), password);
    }

    public static string Decrypt(byte[] input, string password) {
        string text;

        var aesAlg = NewRijndaelManaged(password);
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using (var msDecrypt = new MemoryStream(input)) {
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                using (var srDecrypt = new StreamReader(csDecrypt)) {
                    text = srDecrypt.ReadToEnd();
                }
            }
        }
        return text;
    }

    public static bool IsBase64String(string base64String) {
        base64String = base64String.Trim();
        return (base64String.Length % 4 == 0) &&
               Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

    }

    private static RijndaelManaged NewRijndaelManaged(string salt) {
        if (salt == null)
            throw new ArgumentNullException("salt");
        var saltBytes = Encoding.ASCII.GetBytes(salt);
        var key = new Rfc2898DeriveBytes(Inputkey, saltBytes);

        var aesAlg = new RijndaelManaged();
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

        return aesAlg;
    }

    internal const string Inputkey = "2b8e9b21-9b29-4d9e-8131-4faa046b5bb0";
}


