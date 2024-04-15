using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AddComponent����SetUp���Ăяo�����ƂŃo�t��t�^����
/// </summary>
public class Buff : MonoBehaviour
{
    [SerializeField] float buffTimer;
    Buffs type;
    bool isSetUp = false;

    public float BuffTimer { get => buffTimer; }
    public Buffs Type { get => type; }


    /// <summary>
    /// �V���Ƀo�t��ǉ������ۂ̃Z�b�g�A�b�v���s��
    /// <param>�o�t�񋓌^���o�t���X�g�ɒǉ�����</param>
    /// </summary>
    /// <param name="buffTimer"></param>
    /// <param name="type"></param>
    public void SetUp(float buffTimer, Buffs type)
    {
        this.buffTimer = buffTimer;
        this.type = type;
        GetComponentInChildren<CreatureStatus>().Buffs.Add(type);
        isSetUp = true;
    }

    /// <summary>
    /// ���łɃo�t���ǉ�����Ă���ꍇ�ɁA�^�C�}�[���X�V����
    /// </summary>
    /// <param name="buffTimer"></param>
    public void UpdateBuffTimer(float buffTimer)
    {
        this.buffTimer = buffTimer;
    }

    private void Update()
    {
        // AddComponet���������ł͉������Ȃ�
        if (isSetUp is false) return;
        buffTimer -= Time.deltaTime;
        if (buffTimer < 0)
        {
            GetComponentInChildren<CreatureStatus>().Buffs.Remove(type);
            Destroy(this);
        }
    }

    /// <summary>
    /// �U���͂��㏸������
    /// </summary>
    public static void RedBuff(List<Buffs> buffs, ref float damage)
    {
        if (buffs.Contains(Buffs.Red)) damage *= 1.3f;
    }
}

public enum Buffs
{
    Red
}
