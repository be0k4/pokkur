using UnityEngine;

/// <summary>
/// SEの管理を行う。効果音を格納するコンテナとして使う
/// </summary>
public class SEAudioManager : MonoBehaviour
{
    public static SEAudioManager instance;
    [SerializeField] AudioSource seAudio;
    [Header("効果音をここに追加")]
    [Tooltip("ボタンクリック")] public AudioClip click;
    [Tooltip("リクルート")] public AudioClip recruit;
    [Tooltip("アイテム取得、装備")] public AudioClip lift;
    [Tooltip("アイテム破棄")] public AudioClip put;
    [Tooltip("アイテム消費")] public AudioClip use;

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

    public void SetSEVolume(float value)
    {
        seAudio.volume = value;
    }

    public float GetSEVolume()
    {
        return seAudio.volume;
    }

    public AudioSource GetSeAudio()
    {
        return seAudio;
    }

    /// <summary>
    /// 効果音を再生する
    /// </summary>
    /// <param name="soundEffect">SEAudioManagerに登録された効果音を指定する</param>
    public void PlaySE(AudioClip soundEffect)
    {
        seAudio.PlayOneShot(soundEffect);
    }
}
