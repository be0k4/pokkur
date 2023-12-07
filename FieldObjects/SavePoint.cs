using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �Z�[�u�E�񕜂��\�ȃI�u�W�F�N�g
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
        //�Z�[�u�\�ɂ���
        activateSaveSlotsButton.interactable = true;

    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        //�Z�[�u�s�\�ɂ���
        activateSaveSlotsButton.interactable = false;
    }

    void Rest()
    {
        //�p�[�e�B�̑S������
        foreach (var pokkur in gameManager.Party)
        {
            var status = pokkur.GetComponentInChildren<CreatureStatus>();
            var heal = 100 - status.HealthPoint;
            status.HealthPoint = 100;
            pokkur.GetComponentInChildren<BattleManager>().UpdateBattleUI(heal, false);
        }

        //���C�����j���[��\��
        inGameMenu.ActivateMainMenu();
    }
}
