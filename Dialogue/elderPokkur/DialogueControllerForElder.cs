using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// ���V�|�b�N��
/// <para>�����n�̃_�C�A���O�ƁA�I�[�u�̊Ǘ�</para>
/// </summary>
public class DialogueControllerForElder : DialogueController
{
    [SerializeField] List<Orb> collectedOrbs;
    [SerializeField] PlayableDirector happyEnd;
    [SerializeField] PlayableDirector badEnd;

    protected override void Start()
    {
        base.Start();
        DynamicBind(badEnd);
        DynamicBind(happyEnd);
        gameManager.badEndTrigger += () => badEnd.Play();

        void DynamicBind(PlayableDirector director)
        {
            //���ʉ��𗬂����߂�AudioSource���o�C���h����g���b�N�B���̎��_�ł͉����o�C���h����Ă��Ȃ�
            PlayableBinding audioTrack = director.playableAsset.outputs.First(e => e.streamName == "Audio Track");
            //���I�o�C���h���s��
            director.SetGenericBinding(audioTrack.sourceObject, SEAudioManager.instance.GetSeAudio());
        }
    }

    public override async void Interact()
    {
        //��d��b��h��
        if (GameManager.invalid) return;
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;

        //�I�[�u�������Ă���ꍇ�͂���p�̉�b������
        if (GameManager.inventory.Any(e => e is Orb))
        {
            //������n���I�[�u�̎�ނ�񋓌^�Ŏ擾
            Orb orb = (Orb)GameManager.inventory.First(e => e is Orb);
            string type = orb.Type switch
            {
                Orb.Orbs.Blue => "�u���[",
                Orb.Orbs.Red => "���b�h",
                Orb.Orbs.Gold => "�S�[���h",
                Orb.Orbs.Silver => "�V���o�[",
                _ => ""

            };
            //�c��̃I�[�u���v�Z
            //�S�I�[�u�̎�� - (�W�߂��I�[�u + 1)
            int count = Enum.GetValues(typeof(Orb.Orbs)).Length - (collectedOrbs.Where(e => e.gameObject.activeSelf).Count() + 1);

            //�n�b�s�[�G���f�B���O
            if (count is 0)
            {
                var dialogue = new TextAsset($"�����I���ɑS�ẴI�[�u���������킢�B\r\n���O���񂽂��̂���܂ł̓����Ɋ��ӂ��邼�B\r\n���v���Β����A���������̂肶������B\r\n�����Đ��ɁA�킵�̖�]�����A���鎞�������悤����I");
                await gameManager.Dialogue(dialogue, token);
                collectedOrbs.First(e => e.Type == orb.Type).gameObject.SetActive(true);
                happyEnd.Play();
            }
            //�ʏ�
            else
            {
                var dialogue = new TextAsset($"����͂܂�����{type}�I�[�u����! �悭�W�߂Ă��Ă��ꂽ�B\r\n����Ŏc��̃I�[�u�͂���{count}����B\r\n�������A�킵�������Ă���̂����Ƃ킸������B���ꂮ����Y���悤�ɂȁB");
                await gameManager.Dialogue(dialogue, token);
                //�Ή�����I�[�u���擾�ς݂ɂ���
                gameManager.BlackOut();
                var collectedOrb = collectedOrbs.First(e => e.Type == orb.Type);
                collectedOrb.gameObject.SetActive(true);
                collectedOrb.IsCorrected = false;
                GameManager.inventory.Remove(orb);
            }

        }
        //�`���[�g���A���Ɨ��̖ړI��b��
        else
        {
            await gameManager.Dialogue(textFile, token);
        }

        this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
    }
}
