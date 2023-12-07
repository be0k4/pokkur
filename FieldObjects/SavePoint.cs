using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// セーブ・回復が可能なオブジェクト
/// </summary>
public class SavePoint : AbstractInteractable
{
    [SerializeField] InGameMenu inGameMenu;
    [SerializeField] Button activateSaveSlotsButton;

    void Update()
    {
        localUI.transform.rotation = Camera.main.transform.rotation;

        if (interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (!gameManager.CheckPartyIsReady(this.gameObject.transform))
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            Rest();
        }
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
        //パーティの全員を回復
        foreach (var pokkur in gameManager.Party)
        {
            var status = pokkur.GetComponentInChildren<CreatureStatus>();
            var heal = 100 - status.HealthPoint;
            status.HealthPoint = 100;
            pokkur.GetComponentInChildren<BattleManager>().UpdateBattleUI(heal, false);
        }

        //メインメニューを表示
        inGameMenu.ActivateMainMenu();
    }
}
