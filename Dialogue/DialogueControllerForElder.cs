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

    async void Update()
    {
        if (GameManager.invalid) return;

        localUI.transform.rotation = Camera.main.transform.rotation;

        if (interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (gameManager.CheckPartyIsReady(this.transform) is false)
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            //�I�[�u�������Ă���ꍇ�͂���p�̉�b������
            if (GameManager.inventory.Any(e => e is Orb))
            {
                //������n���I�[�u�̎�ނ�񋓌^�Ŏ擾
                Orb orb = (Orb)GameManager.inventory.First(e => e is Orb);
                Orb.Orbs type = orb.Type;
                //�c��̃I�[�u���v�Z
                //�S�I�[�u�̎�� - (�W�߂��I�[�u + 1)
                int count = Enum.GetValues(typeof(Orb.Orbs)).Length - (collectedOrbs.Where(e => e.gameObject.activeSelf).Count() + 1);

                //�n�b�s�[�G���f�B���O
                if (count is 0)
                {
                    var dialogue = new TextAsset($"All the orbs have now been collected.\r\nThanks for working so hard this far.\r\nMy dream will finally come true!");
                    await gameManager.Dialogue(dialogue, token);
                    collectedOrbs.First(e => e.Type == type).gameObject.SetActive(true);
                    happyEnd.Play();
                }
                //�ʏ�
                else
                {
                    var dialogue = new TextAsset($"Ohhh! This is {type} Orb! You collected it well.\r\n There are {count} Orbs left. Good luck.");
                    await gameManager.Dialogue(dialogue, token);
                    //�Ή�����I�[�u���擾�ς݂ɂ���
                    gameManager.BlackOut();
                    var collectedOrb = collectedOrbs.First(e => e.Type == type);
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
}
