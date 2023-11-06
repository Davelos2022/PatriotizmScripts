using MemoryPack;
using NaughtyAttributes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using VolumeBox.Toolbox;

public class JSONToBinaryConvert : MonoBehaviour
{
    [Button("Convert")]
    public async void Convert()
    {
        FileManager saver = new JsonFileManager();

        var desData = await saver.Load<List<MapPointData>>("User_Map_Points/map_points");

        foreach(var mapPointData in desData)
        {
            mapPointData.photos.Clear();

            foreach(var photoPath in mapPointData.pointPhotosPath)
            {
                //mapPointData.photos.Add(FileManager.CreateSprite(await FileManager.LoadTextureAsync(Application.streamingAssetsPath + "/" + photoPath), SpriteMeshType.FullRect));
            }

            if(mapPointData.iconPath.IsValuable())
            {
                //mapPointData.icon = FileManager.CreateSprite(await FileManager.LoadTextureAsync(Application.streamingAssetsPath + "/" + mapPointData.iconPath));
            }
        }

        //saver = new BinaryFileManager();

        //await saver.Save(desData, Application.streamingAssetsPath + "/User_Map_Points/map_points_bin", false);

        //File.WriteAllBytes(Application.streamingAssetsPath + "/User_Map_Points/map_points_bin", MemoryPackSerializer.Serialize(desData));
    }
}
