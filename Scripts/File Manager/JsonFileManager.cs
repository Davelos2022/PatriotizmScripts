using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class JsonFileManager: FileManager
{
    public async override UniTask<T> Load<T>(string path, bool useSavePath = true, CancellationToken token = default) where T : class
    {
        if(useSavePath)
        {
            path = DataPath + "/" + path;
        }

        if(!File.Exists(path))
        {
            return null;
        }

        var textData = await File.ReadAllTextAsync(path, token);

        return JsonConvert.DeserializeObject<T>(textData);
    }

    public async override UniTask Save<T>(T data, string path, bool useSavePath = true, CancellationToken token = default) where T: class
    {
        if(useSavePath)
        {
            path = DataPath + "/" + path;
        }

        var serializedData = JsonConvert.SerializeObject(data);

        await File.WriteAllTextAsync(path, serializedData, token);
    }
}
