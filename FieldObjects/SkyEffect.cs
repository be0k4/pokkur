using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// �V��G�t�F�N�g
/// </summary>
public class SkyEffect : MonoBehaviour
{
    [SerializeField, Tooltip("�V��G�t�F�N�g�̍���")] Vector3 offset;

    void Update()
    {
        //�A�N�e�B�u�I�u�W�F�N�g�ɂ��Ă���
        if (GameManager.activeObject != null) transform.position = GameManager.activeObject.transform.position + offset;
    }

    /// <summary>
    /// �V��ω����ɌĂяo����A�C���X�^���X��j������
    /// </summary>
    public async void OnWeatherChanged()
    {
        this.GetComponent<ParticleSystem>().Stop();
        //���݂���p�[�e�B�N�����S�ď��ł���̂�҂�
        await UniTask.Delay(2000);
        Destroy(this.gameObject);
    }
}
