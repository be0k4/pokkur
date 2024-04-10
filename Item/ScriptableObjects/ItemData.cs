using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

/// <summary>
/// スクリプタブルオブジェクト
/// <para>アイテムに共通するデータを含む</para>
/// </summary>
[CreateAssetMenu(menuName = "ItemData")]
public class ItemData : ScriptableObject
{
    public float data;
    public Sprite icon;
    public GameObject prefab;
    [TextArea] public string itemText;
    public string address;
}

/// <summary>
/// アドレスを選択できるよう拡張
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(ItemData))]
public class CustomInspectorItemData : Editor
{
    string[] options;
    int index;
    //一度だけ処理を呼び出すためのフラグ
    bool isCalled;
    public override void OnInspectorGUI()
    {
        if (isCalled is false)
        {
            //アイテムのアドレス名を一括取得
            List<string> optionList = new();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.FindGroup("ICollectable");
            foreach (var entry in group.entries)
            {
                optionList.Add(entry.address);
            }
            //ユニークウェポン用
            optionList.Add(ICreature.uniqueWeapon);
            this.options = optionList.ToArray();

            //ドロップダウンの初期値を設定 
            for (var i = 0; i < this.options.Length; i++)
            {
                if (options[i] == serializedObject.FindProperty("address").stringValue) index = i;
            }
            isCalled = true;
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("data"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemText"));
        index = EditorGUILayout.Popup("アドレス名を選択", index, options);
        serializedObject.FindProperty("address").stringValue = options[index];
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
