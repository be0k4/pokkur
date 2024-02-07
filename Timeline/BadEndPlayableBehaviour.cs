using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

/// <summary>
/// �o�b�h�G���h�̃G���f�B���O����
/// </summary>
public class BadEndPlayableBehaviour : PlayableBehaviour
{
    public PostProcessVolume postProcessing;
    ColorGrading colorGrading;

    // Called when the owning graph starts playing
    public override async void OnGraphStart(Playable playable)
    {
        //BGM���~
        await BGMAudioManager.instance.SwapTrack(null);
        //����ł��Ȃ�����
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
        //�J���[�O���[�f�B���O���擾
        foreach (var setting in postProcessing.profile.settings)
        {
            if (setting is ColorGrading colorGrading)
            {
                //���m�N���ɂ���
                colorGrading.saturation.value = -100;
            }
        }
    }
}
