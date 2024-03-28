using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �Z�[�u�E�񕜂��\�ȃI�u�W�F�N�g
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
        //�p�[�e�B�̑S����S��
        foreach (var pokkur in gameManager.Party)
        {
            var status = pokkur.GetComponentInChildren<CreatureStatus>();
            var heal = status.MaxHealthPoint - status.HealthPoint;
            status.HealthPoint = status.MaxHealthPoint;
            pokkur.GetComponentInChildren<BattleManager>().UpdateBattleUI(heal, BattleManager.HealDamage);
        }

        //���C�����j���[��\��
        inGameMenu.ActivateMainMenu();
    }
}
