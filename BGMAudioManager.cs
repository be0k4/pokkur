using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// BGM�̊Ǘ����s��
/// </summary>
public class BGMAudioManager : MonoBehaviour
{
    /// <summary>
    /// �V���O���g��
    /// </summary>
    public static BGMAudioManager instance;

    [SerializeField] AudioSource track0, track1;
    [SerializeField, Tooltip("BGM���t�F�[�h�C���A�A�E�g���鎞��")] float duration;

    //���ʒ��ߌ��ʂ�ێ����邽�߂̕ϐ�
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
    /// �����̉�����BGM�����ւ���
    /// </summary>
    public async UniTask SwapTrack(AudioClip newClip)
    {
        if (track0.isPlaying)
        {
            //�_���W�������ł͓���BGM��������̂ł����ŏI��
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
