using System;
using System.Text;
using UnityEngine;

public static class MusicSaveCodeUtility
{
    private const string SAVE_PREFIX_V1 = "DG1:";
    private const string SAVE_PREFIX_V2 = "DG2:";

    public static string ExportCode(MusicSaveData data)
    {
        if (data == null)
            return string.Empty;

        string json = JsonUtility.ToJson(data);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);

        return SAVE_PREFIX_V2 + base64;
    }

    public static bool TryImportCode(string code, out MusicSaveData data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(code))
        {
            GameManager.Instance?.LogWarning("Import failed: code is empty.");
            return false;
        }

        code = code.Trim();

        GameManager.Instance?.Log($"Import code start: {code.Substring(0, Mathf.Min(20, code.Length))}");
        GameManager.Instance?.Log($"Import code length: {code.Length}");

        if (!code.StartsWith("DG1:"))
        {
            GameManager.Instance?.LogWarning("Import failed: invalid prefix.");
            return false;
        }

        string base64 = code.Substring("DG1:".Length);

        try
        {
            byte[] bytes = System.Convert.FromBase64String(base64);
            string json = System.Text.Encoding.UTF8.GetString(bytes);

            GameManager.Instance?.Log($"Import json start: {json.Substring(0, Mathf.Min(100, json.Length))}");

            data = JsonUtility.FromJson<MusicSaveData>(json);

            if (data == null)
            {
                GameManager.Instance?.LogWarning("Import failed: data is null.");
                return false;
            }

            GameManager.Instance?.Log($"Import version: {data.version}");
            GameManager.Instance?.Log($"Import track count: {(data.tracks != null ? data.tracks.Count : -1)}");

            if (data.version < 1 || data.version > 2)
            {
                GameManager.Instance?.LogWarning($"Import failed: unsupported version {data.version}");
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            data = null;
            GameManager.Instance?.LogWarning($"Import failed exception: {e.Message}");
            return false;
        }
    }
}