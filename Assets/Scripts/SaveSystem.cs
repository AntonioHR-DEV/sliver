using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton that handles all game data persistence.
/// Saves to Application.persistentDataPath/savedata.json.
///
/// Usage:
///   SaveSystem.Instance.GetLevelData(1).stars
///   SaveSystem.Instance.SetLevelCompleted(1, 3);
///   SaveSystem.Instance.GetSettings().musicVolume
///   SaveSystem.Instance.SetMusicVolume(0.8f);
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SAVE_FILE_NAME = "savedata.json";
    public readonly int TOTAL_LEVELS = 10;

    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    private SaveData saveData;

    // =========================================================================
    // Data Structures
    // =========================================================================

    [Serializable]
    public class LevelData
    {
        public int levelIndex;
        public bool isUnlocked;
        public bool isCompleted;
        public int stars;
    }

    [Serializable]
    public class SettingsData
    {
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public string bindingOverrides = "";
    }

    [Serializable]
    private class SaveData
    {
        public LevelData[] levels;
        public SettingsData settings;
    }

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    // =========================================================================
    // Load / Save
    // =========================================================================

    private void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                saveData = JsonUtility.FromJson<SaveData>(json);
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveSystem: Failed to load save file — {e.Message}. Creating new save.");
            }
        }

        // No save file found or failed to parse — create a fresh one
        CreateNewSave();
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: Failed to write save file — {e.Message}");
        }
    }

    private void CreateNewSave()
    {
        saveData = new SaveData
        {
            levels = new LevelData[TOTAL_LEVELS],
            settings = new SettingsData()
        };

        for (int i = 0; i < TOTAL_LEVELS; i++)
        {
            saveData.levels[i] = new LevelData
            {
                levelIndex = i + 1,
                isUnlocked = i == 0, // only Level 1 starts unlocked
                isCompleted = false,
                stars = 0
            };
        }

        Save();
    }

    // =========================================================================
    // Level Data API
    // =========================================================================

    /// <summary>Returns data for a level by 1-based index.</summary>
    public LevelData GetLevelData(int levelIndex)
    {
        if (!IsValidIndex(levelIndex)) return null;
        return saveData.levels[levelIndex - 1];
    }

    /// <summary>
    /// Call when the player completes a level.
    /// Saves stars, marks completed, and unlocks the next level.
    /// Only updates stars if the new rating is higher than the saved one.
    /// </summary>
    public void SetLevelCompleted(int levelIndex, int stars)
    {
        if (!IsValidIndex(levelIndex)) return;

        LevelData data = saveData.levels[levelIndex - 1];
        data.isCompleted = true;
        data.stars = Mathf.Max(data.stars, stars); // never overwrite a higher score

        // Unlock the next level
        if (levelIndex < TOTAL_LEVELS)
            saveData.levels[levelIndex].isUnlocked = true;

        Save();
    }

    /// <summary>Returns true if the level has been unlocked.</summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (!IsValidIndex(levelIndex)) return false;
        return saveData.levels[levelIndex - 1].isUnlocked;
    }

    // =========================================================================
    // Settings API
    // =========================================================================

    public SettingsData GetSettings() => saveData.settings;

    public void SetMusicVolume(float volume)
    {
        saveData.settings.musicVolume = Mathf.Clamp01(volume);
        Save();
    }

    public void SetSfxVolume(float volume)
    {
        saveData.settings.sfxVolume = Mathf.Clamp01(volume);
        Save();
    }

    public void SetBindingOverrides(string json)
    {
        saveData.settings.bindingOverrides = json;
        Save();
    }

    public string GetBindingOverrides() => saveData.settings.bindingOverrides;

    // =========================================================================
    // Debug
    // =========================================================================

    /// <summary>Deletes the save file and resets to a fresh save. Useful for testing.</summary>
    [ContextMenu("Delete Save File")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        CreateNewSave();
        Debug.Log("SaveSystem: Save file deleted and reset.");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private bool IsValidIndex(int levelIndex)
    {
        if (levelIndex >= 1 && levelIndex <= TOTAL_LEVELS) return true;
        Debug.LogWarning($"SaveSystem: Invalid level index {levelIndex}. Must be 1-{TOTAL_LEVELS}.");
        return false;
    }
}