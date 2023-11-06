using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using VolumeBox.Toolbox;

public class VideoWrapper : MonoCached
{
    [SerializeField] private VideoPlayer player;
    [SerializeField] private RawImage image;
    [SerializeField] private RenderTexture renderTexture;

    public RawImage VideoImage => image;
    public RenderTexture Render => renderTexture;


    private CancellationTokenSource _tokenSource;

    private bool _endPlaying;

    protected override void Rise()
    {
        player.loopPointReached += OnPlayEnd;
    }

    public async UniTask WaitPrepare()
    {
        player.gameObject.SetActive(true);
        //player.sendFrameReadyEvents = true;
        player.Prepare();
        await UniTask.WaitUntil(() => player.isPrepared);
        player.gameObject.SetActive(false);
    }

    public async UniTask Play()
    {
        player.gameObject.SetActive(true);

        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        RenderTexture.active = player.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        player.Play();

        await UniTask.WaitUntil(() => player.frame == player.frameCount - 1f, PlayerLoopTiming.Update, _tokenSource.Token);
    }

    private void OnPlayEnd(VideoPlayer causedVideoPlayer)
    {
        _endPlaying = true;
    }

    public void Stop()
    {
        _tokenSource.Cancel();
    }

    protected override void Destroyed()
    {
        player.loopPointReached -= OnPlayEnd;        
    }
}
