using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class DataEditingSceneHandler: SceneHandler<DataEditingSceneArgs>
{
    [SerializeField] private PointEditBehaviourBase pointEditPanel;
    [SerializeField] private PointEditBehaviourBase categoryEditPanel;
    [SerializeField] private PointEditBehaviourBase albumEditPanel;
    [Space]
    [SerializeField] private CanvasGroupFader canvasFader;
    [SerializeField] private BlurPanelFader blurFader;
    [SerializeField] private TMP_Dropdown groupDropdown;
    [SerializeField] private int otherGroupIndex;

    public MapPointsContainer Container => Args.container;
    private DataEditSceneCloseMessage msg = new DataEditSceneCloseMessage();

    protected override void Rise()
    {
        Messager.Instance.Subscribe<UpdateOtherGroupLabelMessage>(_ => UpdateDropdown());
        UpdateDropdown();
    }

    private void UpdateDropdown()
    {
        groupDropdown.options[otherGroupIndex].text = GameManager.Instance.Settings.otherGroupName;
    }

    public void Show()
    {
        canvasFader.FadeIn();
        blurFader.FadeIn();
    }

    public void Close()
    {
        CloseAsync();
    }

    public async UniTask CloseAsync()
    {
        await LoaderScreen.ShowAsync(false);
        await LoaderScreen.HideAsync(false);

        canvasFader.FadeOut();
        blurFader.FadeOut();

        await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(canvasFader.FadeOutDuration, blurFader.FadeOutDuration)));

        Messager.Instance.Send<CloseAppMenuMessage>();
        Messager.Instance.Send(msg);
        
        if(gameObject is not null)
        {
            await Traveler.UnloadScene(gameObject.scene.name);
        }
    }

    protected override void SetupScene(DataEditingSceneArgs args)
    {
        pointEditPanel.gameObject.SetActive(false);
        categoryEditPanel.gameObject.SetActive(false);
        albumEditPanel.gameObject.SetActive(false);

        switch (args.editType)
        {
            case PointEditType.Point:
                pointEditPanel.gameObject.SetActive(true);
                pointEditPanel.Edit(args.pointData, null, null, args.isNew);
                msg.albumData = null;
                break;
            case PointEditType.Category:
                categoryEditPanel.gameObject.SetActive(true);
                categoryEditPanel.Edit(args.pointData, args.category, null, args.isNew);
                msg.albumData = null;
                break;
            case PointEditType.Album:
                albumEditPanel.gameObject.SetActive(true);
                albumEditPanel.Edit(args.pointData, args.category, args.album, args.isNew);
                msg.albumData = args.album;
                break;
        }

        Show();
    }

    public void SetAlbumToOpen(AlbumData currentAlbum)
    {
        msg.albumData = currentAlbum;
    }
}

[Serializable]
public class DataEditSceneCloseMessage: Message 
{
    public AlbumData albumData;
}
