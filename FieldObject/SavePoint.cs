using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// セーブ・回復が可能なオブジェクト
/// </summary>
public class SavePoint : AbstractInteractable
{
    [SerializeField] InGameMenu inGameMenu;
    [SerializeField] Button activateSaveSlotsButton;

    public override void Interact()
    {
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;
        Rest();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        //セーブ可能にする
        activateSaveSlotsButton.interactable = true;

    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        //セーブ不能にする
        activateSaveSlotsButton.interactable = false;
    }

    void Rest()
    {
        //パーティの全員を全回復
        foreach (var pokkur in gameManager.Party)
        {
            Herb.Use(pokkur, 999);
        }

        //メインメニューを表示
        inGameMenu.ActivateMainMenu(true);
    }
}
