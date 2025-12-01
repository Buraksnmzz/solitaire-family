using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Collectible;
using Levels;


[Serializable]
public class SavedDataService : ISavedDataService
{
    public SavedDataService()
    {
        Debug.Log("SavedData initialized");
        RegisterModels();
    }

    private Dictionary<Type, IModel> models = new Dictionary<Type, IModel>();

    public void RegisterModels()
    {
        var settingsModel = LoadData<SettingsModel>();
        if (settingsModel == null)
        {
            settingsModel = new SettingsModel();
            SaveData(settingsModel);
        }

        var coinModel = LoadData<CollectibleModel>();
        if (coinModel == null)
        {
            coinModel = new CollectibleModel();
            SaveData(coinModel);
        }
        
        var levelProgressModel = LoadData<LevelProgressModel>();
        if (levelProgressModel == null)
        {
            levelProgressModel = new LevelProgressModel();
            SaveData(levelProgressModel);
        }
    }
    public T GetModel<T>() where T : IModel
    {
        if (models.TryGetValue(typeof(T), out var model))
        {
            return (T)model;
        }
        else
        {
            return default(T);
        }
    }
    public void SaveData<T>(T data) where T : IModel
    {
        var fileName = data.GetType().Name;
        var fullPath = Application.persistentDataPath + "/" + fileName;
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        File.WriteAllText(fullPath, json);
        models[typeof(T)] = data;
    }

    public T LoadData<T>() where T : IModel, new()
    {
        var savableModel = ReadData<T>();
        models[typeof(T)] = savableModel != null ? savableModel : new T();
        return (T)models[typeof(T)];
    }
    public T ReadData<T>()
    {
        var fileName = typeof(T).Name;
        var fullPath = Application.persistentDataPath + "/" + fileName;
        if (!File.Exists(fullPath)) return default;
        var json = File.ReadAllText(fullPath);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }

    public void Dispose()
    {
        // Save all models before disposing
        foreach (var modelPair in models)
        {
            if (modelPair.Value != null)
            {
                SaveData(modelPair.Value);
            }
        }

        models.Clear();
    }

    public void DeleteData<T>() where T : IModel
    {
        var fileName = typeof(T).Name;
        var fullPath = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        if (models.ContainsKey(typeof(T)))
        {
            models.Remove(typeof(T));
        }
    }
}
