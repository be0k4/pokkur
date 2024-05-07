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
/// ダンジョンに出入り、先へ進むためのオブジェクト
/// </summary>
public class DungeonGate : AbstractInteractable
{
    [Header("遷移先シ－ンの情報")]
    [SerializeField] string sceneName;
    [SerializeField, Tooltip("ダンジョンの入口、もしくは出口か")] bool inOutDungeon;

    CancellationToken token;

    protected override void Start()
    {
        base.Start();
        token = this.GetCancellationTokenOnDestroy();
    }


    public override async void Interact()
    {
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;

        var value = await gameManager.ConfirmWindow(token);

        if (value is 1) return;

        IntoGate();
    }

    /// <summary>
    /// 時間を進めてセーブを行い、シーン内外のフラグを切り替えて、シーンを読みこむ。
    /// </summary>
    void IntoGate()
    {
        DataPersistenceManager.instance.SaveGame();
        //注意！ダンジョン内か外かでセーブ処理が切り替わるため、セーブの後にフラグを切り替える。
        //この時点では切り替える前のフラグがデータに保存されている
        if (inOutDungeon) GameManager.IsInDungeon = !GameManager.IsInDungeon;
        SceneManager.LoadSceneAsync(this.sceneName);
    }
}

/// <summary>
/// シーン名を選択できるよう拡張
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGate))]
public class CustomInspectorDungeonGate : Editor
{
    string[] options;
    int index;
    //一度だけ処理を呼び出すためのフラグ
    bool isCalled;
    public override void OnInspectorGUI()
    {
        if (isCalled is false)
        {
            //シーン名を一括取得
            List<string> optionList = new();

            foreach (var fullPath in EditorBuildSettings.scenes.Select(e => e.path))
            {
                optionList.Add(fullPath.Split("/")[^1].Split(".")[0]);
            }
            this.options = optionList.ToArray();

            //ドロップダウンの初期値を設定
            for (var i = 0; i < this.options.Length; i++)
            {
                if (options[i] == serializedObject.FindProperty("sceneName").stringValue) index = i;
            }
            isCalled = true;
        }
        serializedObject.Update();
        EditorGUILayout.LabelField("インタラクト関連");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("localUI"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hintText"));

        EditorGUILayout.LabelField("遷移先シーンの情報");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("inOutDungeon"));
        index = EditorGUILayout.Popup("シーン名を選択", index, options);
        serializedObject.FindProperty("sceneName").stringValue = options[index];
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

