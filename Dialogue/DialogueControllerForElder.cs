using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 長老ポックル
/// <para>説明系のダイアログと、オーブの管理</para>
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
            //効果音を流すためのAudioSourceをバインドするトラック。この時点では何もバインドされていない
            PlayableBinding audioTrack = director.playableAsset.outputs.First(e => e.streamName == "Audio Track");
            //動的バインドを行う
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

            //オーブを持っている場合はそれ用の会話をする
            if (GameManager.inventory.Any(e => e is Orb))
            {
                //今から渡すオーブの種類を列挙型で取得
                Orb orb = (Orb)GameManager.inventory.First(e => e is Orb);
                Orb.Orbs type = orb.Type;
                //残りのオーブを計算
                //全オーブの種類 - (集めたオーブ + 1)
                int count = Enum.GetValues(typeof(Orb.Orbs)).Length - (collectedOrbs.Where(e => e.gameObject.activeSelf).Count() + 1);

                //ハッピーエンディング
                if (count is 0)
                {
                    var dialogue = new TextAsset($"All the orbs have now been collected.\r\nThanks for working so hard this far.\r\nMy dream will finally come true!");
                    await gameManager.Dialogue(dialogue, token);
                    collectedOrbs.First(e => e.Type == type).gameObject.SetActive(true);
                    happyEnd.Play();
                }
                //通常
                else
                {
                    var dialogue = new TextAsset($"Ohhh! This is {type} Orb! You collected it well.\r\n There are {count} Orbs left. Good luck.");
                    await gameManager.Dialogue(dialogue, token);
                    //対応するオーブを取得済みにする
                    gameManager.BlackOut();
                    var collectedOrb = collectedOrbs.First(e => e.Type == type);
                    collectedOrb.gameObject.SetActive(true);
                    collectedOrb.IsCorrected = false;
                    GameManager.inventory.Remove(orb);
                }

            }
            //チュートリアルと旅の目的を話す
            else
            {
                await gameManager.Dialogue(textFile, token);
            }

            this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
        }
    }
}
