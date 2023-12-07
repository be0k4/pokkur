using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// �_���W�����ɏo����A��֐i�ނ��߂̃I�u�W�F�N�g
/// </summary>
public class DungeonGate : AbstractInteractable
{
    [Header("�J�ڐ�V�|���̏��")]
    [SerializeField] string sceneName;
    [SerializeField, Tooltip("�_���W�����̓����A�������͏o����")] bool inOutDungeon;

    CancellationToken token;

    protected override void Start()
    {
        base.Start();
        token = this.GetCancellationTokenOnDestroy();
    }

    async void Update()
    {
        localUI.transform.rotation = Camera.main.transform.rotation;

        if(interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (!gameManager.CheckPartyIsReady(this.gameObject.transform))
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            var value = await gameManager.ConfirmWindow(token);

            if (value is 1) return;

            IntoGate(this.inOutDungeon);
        }

    }

    /// <summary>
    /// �Z�[�u���s���A�V�[����ǂ݂��ށB
    /// </summary>
    /// <param name="inOutDungeon">�_���W�����̓����������͏o���̏ꍇtrue</param>
    void IntoGate(bool inOutDungeon)
    {
        DataPersistenceManager.instance.SaveGame();
        //���ӁI�_���W���������O���ŃZ�[�u�̏������؂�ւ�邽�߁A�Z�[�u�̌�ɐ؂�ւ���B
        if(inOutDungeon) GameManager.isInDungeon = !GameManager.isInDungeon;
        SceneManager.LoadSceneAsync(this.sceneName);
    }
}
