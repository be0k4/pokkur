using UnityEngine;

/// <summary>
/// SE�̊Ǘ����s���B���ʉ����i�[����R���e�i�Ƃ��Ďg��
/// </summary>
public class SEAudioManager : MonoBehaviour
{
    public static SEAudioManager instance;
    [SerializeField] AudioSource seAudio;
    [Header("���ʉ��������ɒǉ�")]
    [Tooltip("�{�^���N���b�N")] public AudioClip click;
    [Tooltip("���N���[�g")] public AudioClip recruit;
    [Tooltip("�A�C�e���擾�A����")] public AudioClip lift;
    [Tooltip("�A�C�e���j��")] public AudioClip put;
    [Tooltip("�A�C�e������")] public AudioClip use;

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
    /// ���ʉ����Đ�����
    /// </summary>
    /// <param name="soundEffect">SEAudioManager�ɓo�^���ꂽ���ʉ����w�肷��</param>
    public void PlaySE(AudioClip soundEffect)
    {
        seAudio.PlayOneShot(soundEffect);
    }
}
