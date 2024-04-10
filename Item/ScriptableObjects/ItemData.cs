using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

/// <summary>
/// �X�N���v�^�u���I�u�W�F�N�g
/// <para>�A�C�e���ɋ��ʂ���f�[�^���܂�</para>
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
/// �A�h���X��I���ł���悤�g��
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(ItemData))]
public class CustomInspectorItemData : Editor
{
    string[] options;
    int index;
    //��x�����������Ăяo�����߂̃t���O
    bool isCalled;
    public override void OnInspectorGUI()
    {
        if (isCalled is false)
        {
            //�A�C�e���̃A�h���X�����ꊇ�擾
            List<string> optionList = new();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.FindGroup("ICollectable");
            foreach (var entry in group.entries)
            {
                optionList.Add(entry.address);
            }
            //���j�[�N�E�F�|���p
            optionList.Add(ICreature.uniqueWeapon);
            this.options = optionList.ToArray();

            //�h���b�v�_�E���̏����l��ݒ� 
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
        index = EditorGUILayout.Popup("�A�h���X����I��", index, options);
        serializedObject.FindProperty("address").stringValue = options[index];
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
