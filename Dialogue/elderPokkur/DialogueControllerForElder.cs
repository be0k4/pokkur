using Cysharp.Threading.Tasks;
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

    public override async void Interact()
    {
        //二重会話を防ぐ
        if (GameManager.invalid) return;
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;

        //オーブを持っている場合はそれ用の会話をする
        if (GameManager.inventory.Any(e => e is Orb))
        {
            //今から渡すオーブの種類を列挙型で取得
            Orb orb = (Orb)GameManager.inventory.First(e => e is Orb);
            string type = orb.Type switch
            {
                Orb.Orbs.Blue => "ブルー",
                Orb.Orbs.Red => "レッド",
                Orb.Orbs.Gold => "ゴールド",
                Orb.Orbs.Silver => "シルバー",
                _ => ""

            };
            //残りのオーブを計算
            //全オーブの種類 - (集めたオーブ + 1)
            int count = Enum.GetValues(typeof(Orb.Orbs)).Length - (collectedOrbs.Where(e => e.gameObject.activeSelf).Count() + 1);

            //ハッピーエンディング
            if (count is 0)
            {
                var dialogue = new TextAsset($"おお！ついに全てのオーブが揃ったわい。\r\nさて、これで本当におしまいじゃ。\r\nお前さん、いや、あなた様のこれまでの働きに感謝するぞ。よくぞここまでプレイしてくれた。" +
                    $"\r\n今思えば一瞬の出来事じゃったろうが、楽しんでいただけたかの。そうだとよいのじゃが...\r\n遂にわしの野望が成就する時が来た...。\r\n神よ、願いを叶えたまえ！");
                await gameManager.Dialogue(dialogue, token);
                collectedOrbs.First(e => e.Type == orb.Type).gameObject.SetActive(true);
                happyEnd.Play();
            }
            //通常
            else
            {
                string text = $"これはまさしく{type}オーブじゃ! よく集めてきてくれた。\r\nこれで残りのオーブはあと{count}つじゃ。\r\nいいか、わしが生きているのもあとわずかじゃ。くれぐれも忘れんようにな。";
                switch (count)
                {
                    case 1:
                        text += "\r\n残るはあとひとつ..。そろそろ話してもよいころかの。実はヒーローマスクには先代がいたんじゃ。\r\nヒーローマスクというのは最も才能ある、選ばれしポックルの証なんじゃ。\r\nその昔、やつとわしは一緒に旅をしていての。そこにいる軍曹と三人で無敵のパーティじゃった。" +
                        "\r\nじゃが、何の願いを叶えるか揉めてしまい、結局愛想をつかされてしまったんじゃ。惜しい友人を無くした、今でも後悔しておるわい。\r\nおお、すまんすまん。湿っぽくて悪かったのぉ。年寄りの悪い癖じゃ。\r\nお前さんなら最後のオーブもきっと手に入れられるはずじゃ。期待しとるぞ。";
                        break;
                    case 2:
                        text += "\r\nこれで折り返し地点じゃ。4つのオーブを集められるのは、選ばれし者だけじゃとわしは考えとる。\r\nお前さんがその選ばれし者かどうかは分からんがの、フォッフォッ！\r\n時間は有限！焦らず急げじゃ。この調子で頼むぞ～";
                        break;
                    case 3:
                        text += "\r\nむむ？\r\nわしらにそっくりな黒いポックルを見たじゃと？それはきっとコボルドのことじゃな。\r\nやつらはどういうわけか意識が無く、見かけるや襲い掛かってくる危険な連中じゃ。" +
                            "\r\nやつらがどこから来たのか、何が目的なのか全てが分からん。\r\nとにかく気を付けるんじゃぞ、まだオーブ集めは始まったばかりじゃからな。";
                        break;
                }
                TextAsset dialogue = new(text);
                await gameManager.Dialogue(dialogue, token);
                //対応するオーブを取得済みにする
                gameManager.BlackOut();
                var collectedOrb = collectedOrbs.First(e => e.Type == orb.Type);
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
