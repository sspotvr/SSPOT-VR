using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles persistence of FCP colors across scenes or gaming sessions
/// </summary>
[RequireComponent(typeof(FlexibleColorPicker))]
public class FCPPersistence : MonoBehaviour {

    private FlexibleColorPicker fcp;
    public string saveName = GenerateID();

    // private static Dictionary<string, Color> savedColors;
    private static string saveFilePath;
    // private static bool saveFileLoaded;
    // private static bool saveFileOutdated;

    private void Awake() {
        fcp = GetComponent<FlexibleColorPicker>();
        saveFilePath ??= Path.Combine(Application.persistentDataPath, "FCP_SavedColors.txt");
        
        if (File.Exists(saveFilePath)) fcp.color = LoadFromFile();
    }

    // private void InitStatic() {
    //     saveFilePath ??= Path.Combine(Application.persistentDataPath, "FCP_SavedColors.txt");
    // }

    private void OnDestroy() {
        SaveToFile(fcp.color);
    }
    private void OnDisable() {
        SaveToFile(fcp.color);
    }

    private void OnEnable() {
        if (File.Exists(saveFilePath)) fcp.color = LoadFromFile();
        else
        {
            Debug.Log("No save file found for slot: " + saveName);
            // Optionally reset to default or a specific color if no file exists
            // fcp.color = Color.white; 
        }
    }

    private void SaveToFile(Color c) {
        string hex = ColorUtility.ToHtmlStringRGBA(c);
        string content = saveName + "#" + hex;

        try {
            File.WriteAllText(saveFilePath, content);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save FCP color: " + e.Message);
        }
    }


    private static Color LoadFromFile()
    {
        if (!File.Exists(saveFilePath)) return Color.black;

        try {
            string[] lines = File.ReadAllLines(saveFilePath);
            if (lines.Length == 0 || !lines[^1].EndsWith("#")) {
                // Handle edge case where file has trailing newlines or is empty
                return Color.black;
            }

            string hexString = lines[^1].Trim();
            
            // Format expected: "ID#RRGGBBAA" based on your original logic
            int splitIndex = hexString.LastIndexOf('#');
            if (splitIndex > 0)
            {
                string colorHex = hexString.Substring(splitIndex + 1).Trim();
                if (!ColorUtility.TryParseHtmlString("#" + colorHex, out Color c)) return c;
            }
            // else
            // {
            //     // If the line doesn't have a # separator, treat whole line as hex? 
            //     // Or assume error. Let's try parsing the whole thing with prefix added if no split found.
            //     if (ColorUtility.TryParseHtmlString("#" + hexString, out Color c)) return c;
            // }
            
            return Color.black;
        }
        catch
        {
            return Color.black;
        }
    }


    private static string GenerateID() {
        return Convert.ToBase64String(BitConverter.GetBytes(DateTime.Now.Ticks));
    }
}
