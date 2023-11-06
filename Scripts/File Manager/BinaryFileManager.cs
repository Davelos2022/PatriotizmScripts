using Cysharp.Threading.Tasks;
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VolumeBox.Toolbox;

public class BinaryFileManager//: FileManager
{
    //public async override UniTask<T> Load<T>(string path, bool useSavePath = true) where T : class
    //{
    //    if(useSavePath)
    //    {
    //        path = DataPath + "/" + path;
    //    }

    //    if(!File.Exists(path))
    //    {
    //        return null;
    //    }

    //    using (var stream = new FileStream(path, FileMode.Open))
    //    {
    //        return await MemoryPackSerializer.DeserializeAsync<T>(stream);
    //    }
    //}

    //public async override UniTask Save<T>(T data, string path, bool useSavePath = true) where T: class
    //{
    //    if(useSavePath)
    //    {
    //        path += "/" + DataPath;
    //    }

    //    using (var stream = new FileStream(path, FileMode.OpenOrCreate))
    //    {
    //        await MemoryPackSerializer.SerializeAsync(stream, data);
    //    }
    //}
}
