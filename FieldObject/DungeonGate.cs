using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
            if (gameManager.CheckPartyIsReady(this.gameObject.transform) is false)
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
    /// �Z�[�u���s���A�V�[�����O�̃t���O��؂�ւ��āA�V�[����ǂ݂��ށB
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

/// <summary>
/// �V�[������I���ł���悤�g��
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGate))]
public class CustomInspectorDungeonGate : Editor
{
    string[] options;
    int index;
    //��x�����������Ăяo�����߂̃t���O
    bool isCalled;
public override void OnInspectorGUI()
    {
        if(isCalled is false)
        {
            //�V�[�������ꊇ�擾
            List<string>optionList = new();

            foreach (var fullPath in EditorBuildSettings.scenes.Select(e => e.path))
            {
                optionList.Add(fullPath.Split("/")[^1].Split(".")[0]);
            }
            this.options = optionList.ToArray();

            //�h���b�v�_�E���̏����l��ݒ�
            for (var i = 0; i < this.options.Length; i++)
            {
                if (options[i] == serializedObject.FindProperty("sceneName").stringValue) index = i;
            }
            isCalled = true;
        }
        serializedObject.Update();
        EditorGUILayout.LabelField("�C���^���N�g�֘A");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("localUI"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hintText"));

        EditorGUILayout.LabelField("�J�ڐ�V�[���̏��");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("inOutDungeon"));
        index = EditorGUILayout.Popup("�V�[������I��", index, options);
        serializedObject.FindProperty("sceneName").stringValue = options[index];
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
