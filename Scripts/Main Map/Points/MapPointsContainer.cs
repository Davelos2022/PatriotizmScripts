using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;
using Random = UnityEngine.Random;

public class MapPointsContainer : MonoCached
{
    [SerializeField] private List<PointsSelector> selectors;
    [SerializeField] private string defaultPointTag;
    [SerializeField] private string cityPointTag;
    [SerializeField] private Transform basePointsRoot;
    [SerializeField] private Transform userPointsRoot;

    [Inject] private FileManager _file;
    [Inject] private Pooler _pool;

    private List<MapPointData> _basePoints;
    private List<MapPointData> _userPoints;
    private List<MapPoint> _drawedPoints;
    public UnityEvent Initialized;
    private Group _currentGroup;

    private bool _draggingPoints;

    public bool DraggingPoints => _draggingPoints;
    public Group CurrentGroup => _currentGroup;
    public List<MapPoint> DrawedPoints => _drawedPoints;

    public UnityEvent PointsDragStarted;
    public UnityEvent PointsDragEnded;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<DeleteMapPointMessage>(m => DeletePoint(m.point), gameObject.scene.name);
        Messager.Instance.Subscribe<SaveUserPointsMessage>(m => ClearUnusedFolders(), gameObject.scene.name);
        Messager.Instance.Subscribe<PointSavedMessage>(_ => RedrawPoints(true), gameObject.scene.name);
        Messager.Instance.Subscribe<EnablePointDragMessage>(_ => EnablePointsDrag(), gameObject.scene.name);
        Messager.Instance.Subscribe<DisablePointDragMessage>(_ => DisablePointsDrag(), gameObject.scene.name);
        Messager.Instance.Subscribe<DestroyPointsMapViewMessage>(_ => DestroyPointsMapViewData(), gameObject.scene.name);
        Messager.Instance.Subscribe<LoadPointsMapViewMessage>(_ => LoadPointsMapViewData(), gameObject.scene.name);
        Messager.Instance.Subscribe<TryClickPinMessage>(x => TryClickPin(x.name), gameObject.scene.name);
        Messager.Instance.Subscribe<TryOpenPinExtended>(x => TryOpenPinExtended(x.name), gameObject.scene.name);
    }

    public float GetLoadingProgress()
    {
        int allCount = 0;

        if(_userPoints != null)
        {
            allCount += _userPoints.Count;
        }

        if(_basePoints != null)
        {
            allCount += _basePoints.Count;
        }

        if (allCount == 0)
        {
            return 1;
        }

        if (_drawedPoints == null)
        {
            return 1;
        }
        else
        {
            return (float)_drawedPoints.Count(x => x.Loaded) / (float)allCount;
        }
    }

    public void ToggleShowAll()
    {
        RedrawPoints();
    }

    [Button("Resize Icons")]
    public async void ResizeIcons()
    {
        var t = new List<UniTask>();
        _drawedPoints.ForEach(p => 
        {
            t.Add(p.Data.Save());
        });

        foreach (var point in _drawedPoints)
        {
            await SavePoint(point.Data);
        }

        await UniTask.WhenAll(t);
    }

    public async UniTask Initialize(bool showLoader = true)
    {
        _userPoints = await _file.Load<List<MapPointData>>(GameManager.Instance.UserPointsPath + "/map_points");
        _basePoints = await _file.Load<List<MapPointData>>(GameManager.Instance.BasePointsPath + "/map_points");

        if (_userPoints == null)
        {
            _userPoints = new List<MapPointData>();
        }

        if (_basePoints == null)
        {
            _basePoints = new List<MapPointData>();
        }

        _userPoints.ForEach(x =>
        {
            x.UpdateRelations();
        });

        _basePoints.ForEach(x =>
        {
            x.UpdateRelations();
        });

        await InitPoints(showLoader);

        SetFilter(Group.None);

        Initialized.Invoke();

        //foreach (var point in _userPoints)
        //{
        //    var fullP = FileManager.DataPath + "/" + point.iconPath;
        //    var name = Path.GetFileNameWithoutExtension(fullP);
        //    point.iconPath = point.iconPath.Replace(name, "icon");
        //}
        //RefreshSave();
    }

    private void RenamePhotos(PhotoData photo)
    {
        var newPhotoName = FileManager.GetUID();
        var photoPath = FileManager.DataPath + "/" + photo.path;
        var minPath = FileManager.DataPath + "/" + photo.miniaturePath;

        var photoExt = Path.GetExtension(photoPath);
        var minExt = Path.GetExtension(minPath);

        var newPhotoPath = Path.GetDirectoryName(photoPath) + "/" + newPhotoName + photoExt;
        var newMinPath = Path.GetDirectoryName(minPath) + "/" + newPhotoName + "_min" + minExt;

        File.Move(photoPath, newPhotoPath);
        File.Move(minPath, newMinPath);

        var oldPhotoName = Path.GetFileNameWithoutExtension(photoPath);

        photo.path = photo.path.Replace(oldPhotoName, newPhotoName);
        photo.miniaturePath = photo.miniaturePath.Replace(oldPhotoName, newPhotoName);
    }

    private void RenamePointUID(MapPointData data)
    {
        var newUID = FileManager.GetUID();
        var newFolderName = "p_" + newUID;
        var oldFolderName = "point_" + data.UID;

        data.iconPath = data.iconPath.Replace("point_" + data.UID, "p_" + newUID);

        for (int i = 0; i < data.pointPhotosPath.Count; i++)
        {
            if (data.pointPhotosPath[i].path.IsValuable())
            {
                data.pointPhotosPath[i].path = data.pointPhotosPath[i].path.Replace("point_" + data.UID, "p_" + newUID);

                data.pointPhotosPath[i].miniaturePath = data.pointPhotosPath[i].miniaturePath.Replace("point_" + data.UID, "p_" + newUID);
            }
        }

        data.audioDescriptionPath = data.audioDescriptionPath.Replace("point_" + data.UID, "p_" + newUID);
        data.UID = newUID;
        Directory.Move(FileManager.DataPath + "/User_Map_Points/" + oldFolderName, FileManager.DataPath + "/User_Map_Points/" + newFolderName);
        
    }

    public async UniTask DestroyPointsMapViewData()
    {
        var destroyTasks = new List<UniTask>();

        _drawedPoints.ForEach(async p =>
        {
            destroyTasks.Add(p.Data.DestroyAllResources());
        });

        await UniTask.WhenAll(destroyTasks);
    }

    public async UniTask LoadPointsMapViewData()
    {
        var loadTasks = new List<UniTask>();

        _drawedPoints.ForEach(p =>
        {
            if(p != null)
            {
                loadTasks.Add(p.Data.LoadIcon());
            }
        });

        await UniTask.WhenAll(loadTasks);
    }

    public void DisablePointInteractions()
    {
        _drawedPoints.ForEach(p => p.DisableInteractions());
    }

    public void EnablePointInteractions()
    {
        _drawedPoints.ForEach(p => p.EnableInteractions());
    }

    private async UniTask InitPoints(bool showLoader = true)
    {
        _userPoints.ForEach(x =>
        {
            x.userPoint = true;
        });

        _basePoints.ForEach(x =>
        {
            x.userPoint = false;
        });

        await RedrawPoints(false, showLoader);
    }

    public void EnablePointsDrag()
    {
        if (_draggingPoints) return;

        _drawedPoints.ForEach(p => 
        {
            p.EnableDrag();
        });

        _draggingPoints = true;
        PointsDragStarted?.Invoke();
    }

    public async UniTask DisablePointsDrag()
    {
        if (!_draggingPoints) return;

        _drawedPoints.ForEach(p => 
        {
            p.UpdatePosition();
            p.DisableDrag();
        });

        await LoaderScreen.ShowAsync(false);
        await RefreshSave();
        await RedrawPoints(true);
        await LoaderScreen.HideAsync(false);

        _draggingPoints = false;
        PointsDragEnded?.Invoke();
    }

    public void SetFilter(Group group)
    {
        _drawedPoints.ForEach(p => p.Hide());

        if(group == Group.None)
        {
            _drawedPoints.Where(x => x.Data.showByDefault || x.Data.group == Group.City).ToList().ForEach(p => p.Show());
        }
        else
        {
            _drawedPoints.Where(x => x.Data.group == group || x.Data.group == Group.City).ToList().ForEach(p => p.Show());
        }

        _currentGroup = group;
    }

    //Jandro was here
    //Here you can control the load on the main black screen, check if it is not conflicting with anything else
    public async UniTask RedrawPoints(bool instantRedraw = false, bool showLoader = true)
    {
        if(showLoader)
        {
            if (instantRedraw)
            {
                await LoaderScreen.ShowAsync(false);
            }
            else
            {
                await LoaderScreen.ShowAsync();
            }
        }

        _drawedPoints = RedrawPointsList(_basePoints, basePointsRoot);
        _drawedPoints.AddRange(RedrawPointsList(_userPoints, userPointsRoot));

        var loadingTasks = new List<UniTask>();

        _drawedPoints.ForEach(p =>
        {
            loadingTasks.Add(p.LoadCurrentData());
        });


        await UniTask.WhenAll(loadingTasks);

        _drawedPoints.ForEach(x => x.UpdatePosition());

        SetFilter(_currentGroup);
        //await CheckAspect();
        //CheckDescription();

        //foreach(var user in _userPoints)
        //{
        //    await user.RegenerateMiniatures();
        //}

        //await RefreshSave();

        if (instantRedraw)
        {
            await LoaderScreen.HideAsync(false);
        }
        else
        {
            await LoaderScreen.HideAsync();
        }
    }

    private List<MapPoint> RedrawPointsList(List<MapPointData> points, Transform root)
    {
        List<MapPoint> pointsList = new List<MapPoint>();

        while (root.childCount > 0)
        {
            if(root.childCount <= 0) break;
            
            _pool.DespawnOrDestroy(root.GetChild(0).gameObject);
        }

        if (points == null || points.Count <= 0) return pointsList;

        for (int i = 0; i < points.Count; i++)
        {
            string pointTag = defaultPointTag;

            if (points[i].group == Group.City)
            {
                pointTag = cityPointTag;
            }

            pointsList.Add(SpawnPoint(pointTag, points[i], root));
        }

        return pointsList;
    }

    private async UniTask CheckAspect()
    {
        foreach (var point in _drawedPoints)
        {
            await point.Data.LoadDetailedData();

            foreach (var photo in point.Data.photos)
            {
                if(Mathf.Abs(((float)photo.width / (float)photo.height) - (1920.0f/1080.0f)) > 0.01f)
                {
                    print($"Incorrect aspect: point: {point.Data.name}, photo size: {photo.width}x{photo.height}, must be aspect: {1920.0f / 1080.0f}, but aspect is {(float)photo.width / (float)photo.height}");
                }
            }

            await point.Data.DestroyAllResources();
        }
    }

    private void CheckDescription()
    {
        foreach (var point in _drawedPoints)
        {
            if(point.Data.description.Length < 350)
            {
                print($"Not filled description: point: {point.Data.name}");
            }
        }
    }

    private MapPoint SpawnPoint(string pointTag, MapPointData data, Transform root)
    {
        var pointGO = _pool.Spawn(pointTag, Vector3.zero, Quaternion.identity, root, data);

        var point = pointGO.GetComponent<MapPoint>();
        point.transform.localScale = Vector3.one;
        var p = point.transform.localPosition;
        p.z = 0;
        point.transform.localPosition = p;
        return point;
    }

    public async UniTask DeletePoint(MapPointData point)
    {
        if(!_userPoints.Any(x => x.UID == point.UID)) return;

        _userPoints.Remove(point);
        ClearUnusedFolders();
        await _file.Save(_userPoints, "User_Map_Points/map_points");
        await RedrawPoints();
    }

    public async UniTask SaveAllDirty()
    {
        await RefreshSave();

        foreach (var point in _userPoints)
        {
            if(!_userPoints.Any(p => p.UID == point.UID))
            {

            }
        }
    }

    public async UniTask SavePoint(MapPointData point)
    {
        await RefreshSave();

        MapPoint existingPoint = _drawedPoints.FirstOrDefault(p => p.Data.UID == point.UID);

        if(existingPoint == null)
        {
            _userPoints.Add(point);

            string pointTag = defaultPointTag;

            if (point.group == Group.City)
            {
                pointTag = cityPointTag;
            }

            _drawedPoints.Add(SpawnPoint(pointTag, point, userPointsRoot));

            await RefreshSave();
        }
        else
        {
            await existingPoint.LoadCurrentData();
        }

        ClearUnusedFolders();
    }

    public async UniTask RefreshSave()
    {
        await _file.Save(_userPoints, "User_Map_Points/map_points");
    }

    public void TryClickPin(string name)
    {
        var p = _drawedPoints.FirstOrDefault(x => x.Data.name == name);

        if (p != null)
        {
            p.OnClick();
        }
    }

    public void TryClickPin(MapPointData point)
    {
        var p = _drawedPoints.FirstOrDefault(x => x.Data == point);

        if (p != null)
        {
            p.OnClick();
        }
    }

    public void TryOpenPinExtended(string name)
    {
        var p = _drawedPoints.FirstOrDefault(x => x.Data.name == name);

        if(p != null)
        {
            Messager.Instance.Send(new ExtendedMapPointClickedMessage { point = p, preloaded = false });
        }
    }
        
    private void ClearUnusedFolders()
    {
        var paths = Directory.GetDirectories(FileManager.DataPath + "/User_Map_Points/");

        foreach (var path in paths)
        {
            string pointUID = Path.GetFileName(path);
            pointUID = pointUID.Replace("p_", "");

            if (!_userPoints.Any(p => p.UID == pointUID))
            {
                Directory.Delete(path, true);
            }
        }
    }
}

[Serializable]
public class DeleteMapPointMessage : Message
{
    public MapPointData point;
}

[Serializable]
public class SaveUserPointsMessage: Message
{
    
}

[Serializable]
public class RedrawPointsMessage: Message
{

}

[Serializable]
public class DestroyPointsMapViewMessage: Message
{

}

[Serializable]
public class LoadPointsMapViewMessage: Message
{

}

[Serializable]
public class TryClickPinMessage: Message
{
    public string name;
}

[Serializable]
public class TryOpenPinExtended: Message
{
    public string name;
}
