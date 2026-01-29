using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "save.dat");

    // 🔑 Ключ и IV для AES (в реальном проекте лучше генерировать и хранить иначе)
    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("MySuperSecretKey123"); // 16/24/32 байта
    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("MySuperSecretIV456");  // 16 байт

    // Сохранение
    public static void Save<T>(T data)
    {
        try
        {
            // 1. сериализация в JSON
            string json = JsonUtility.ToJson(data);

            // 2. шифрование AES
            byte[] encrypted = EncryptString(json);

            // 3. создаём подпись SHA256
            string hash = ComputeHash(encrypted);

            // 4. сохраняем: [HASH]\n[DATA]
            using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                writer.Write(hash);
                writer.Write(encrypted.Length);
                writer.Write(encrypted);
            }

            Debug.Log($"[SaveSystem] Save successful: {savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed: {ex}");
        }
    }

    // Загрузка
    public static T Load<T>() where T : new()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("[SaveSystem] No save file found, returning new instance.");
            return new T();
        }

        try
        {
            using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                string storedHash = reader.ReadString();
                int length = reader.ReadInt32();
                byte[] encrypted = reader.ReadBytes(length);

                // Проверка подписи
                string actualHash = ComputeHash(encrypted);
                if (storedHash != actualHash)
                {
                    Debug.LogError("[SaveSystem] Save corrupted or tampered!");
                    return new T();
                }

                // Расшифровка
                string json = DecryptString(encrypted);
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed: {ex}");
            return new T();
        }
    }

    // Проверка существования сейва
    public static bool SaveExists() => File.Exists(savePath);

    // Удаление сейва
    public static void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
    }

    // 🔒 AES шифрование
    private static byte[] EncryptString(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = aesKey;
            aes.IV = aesIV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Close();
                return ms.ToArray();
            }
        }
    }

    // 🔓 AES расшифровка
    private static string DecryptString(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = aesKey;
            aes.IV = aesIV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    // 🛡️ Хэш для проверки целостности
    private static string ComputeHash(byte[] data)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}
