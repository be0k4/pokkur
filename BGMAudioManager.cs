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

    //音量調節結果を保持するための変数
    [SerializeField] float volume;

    void Awake()
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
    /// 引数の音源にBGMを入れ替える
    /// </summary>
    public async UniTask SwapTrack(AudioClip newClip)
    {
        if (track0.isPlaying)
        {
            //ダンジョン内では同じBGMがかかるのでここで終了
            if (track0.clip == newClip) return;

            track1.clip = newClip;
            track1.Play();

            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                track1.volume = Mathf.Lerp(0, this.volume, time / duration);
                track0.volume = Mathf.Lerp(this.volume, 0, time / duration);
                await UniTask.Yield();
            }

            track0.Stop();
            track0.clip = null;
        }
        else
        {
            if (track1.clip == newClip) return;

            track0.clip = newClip;
            track0.Play();

            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                track0.volume = Mathf.Lerp(0, this.volume, time / duration);
                track1.volume = Mathf.Lerp(this.volume, 0, time / duration);
                await UniTask.Yield();
            }

            track1.Stop();
            track1.clip = null;
        }
    }

    public void SetBGMVolume(float value)
    {
        this.volume = value;
        track0.volume = this.volume;
        track1.volume = this.volume;
    }

    public float GetBGMVolume()
    {
        return this.volume;
    }
}
