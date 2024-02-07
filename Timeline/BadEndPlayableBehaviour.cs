using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

/// <summary>
/// バッドエンドのエンディング処理
/// </summary>
public class BadEndPlayableBehaviour : PlayableBehaviour
{
    public PostProcessVolume postProcessing;
    ColorGrading colorGrading;

    // Called when the owning graph starts playing
    public override async void OnGraphStart(Playable playable)
    {
        //BGMを停止
        await BGMAudioManager.instance.SwapTrack(null);
        //操作できなくする
        GameManager.invalid = true;
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        SceneManager.LoadSceneAsync(MainMenu.mainmenu);
        GameManager.invalid = false;
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //カラーグレーディングを取得
        foreach (var setting in postProcessing.profile.settings)
        {
            if (setting is ColorGrading colorGrading)
            {
                //モノクロにする
                colorGrading.saturation.value = -100;
            }
        }
    }
}
