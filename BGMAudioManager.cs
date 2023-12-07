using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// BGMの管理を行う
/// </summary>
public class BGMAudioManager : MonoBehaviour
{
    /// <summary>
    /// シングルトン
    /// </summary>
    public static BGMAudioManager instance;

    [SerializeField] AudioSource track0, track1;
    [SerializeField, Tooltip("BGMがフェードイン、アウトする時間")] float duration;

    void Start()
    {
        if (instance is not null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 引数のクリップに音楽を入れ替える
    /// </summary>
    public async UniTask SwapTrack(AudioClip newClip)
    {
        if (track0.isPlaying)
        {
            track1.clip = newClip;
            track1.Play();

            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                track1.volume = Mathf.Lerp(0, 1, time / duration);
                track0.volume = Mathf.Lerp(1, 0, time / duration);
                await UniTask.Yield();
            }

            track0.Stop();
        }
        else
        {
            track0.clip = newClip;
            track0.Play();

            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                track0.volume = Mathf.Lerp(0, 1, time / duration);
                track1.volume = Mathf.Lerp(1, 0, time / duration);
                await UniTask.Yield();
            }

            track1.Stop();
        }
    }

    public async void PlayDungeonClip(AudioClip clip)
    {
        //ダンジョン内では同じBGMはそのまま
        if (track0.clip == clip || track1.clip == clip) return;
        await SwapTrack(clip);
    }
}
