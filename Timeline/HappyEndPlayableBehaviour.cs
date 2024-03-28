using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// ハッピーエンドのエンディング処理
/// </summary>
public class HappyEndPlayableBehaviour : PlayableBehaviour
{
    public AudioClip endingClip;
    public Camera mainCamera;

    // Called when the owning graph starts playing
    public override async void OnGraphStart(Playable playable)
    {
        //BGMを停止
        await BGMAudioManager.instance.SwapTrack(null);
        GameManager.invalid = true;
        //Uiを非表示
        mainCamera.cullingMask &= ~(1 << 5);
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        SceneManager.LoadSceneAsync(MainMenu.mainmenu);
        GameManager.invalid = false;
    }

    // Called when the state of the playable is set to Play
    public override async void OnBehaviourPlay(Playable playable, FrameData info)
    {
        await BGMAudioManager.instance.SwapTrack(endingClip);
    }
}
