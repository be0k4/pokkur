using System;

/// <summary>
/// �S�A�C�e�����ʂ̃C���^�[�t�F�C�X
/// <para>��r�\</para>
/// </summary>
public interface ICollectable : IDataPersistence, IComparable<ICollectable>
{
    //ICollectable�^�ŃA�C�e�����܂Ƃ߂Ĉ������A�C���^�[�t�F�C�X�ł̓t�B�[���h�����ĂȂ��̂ŁA�p����̃t�B�[���h�ɃA�N�Z�X���邽�߂̃��\�b�h��p��
    ItemData GetItemData();
    //�擾���̏���
    //�����蔻��ŁA�v���C���[�����炱�̃��\�b�h���Ăяo��
    void Collect();
    //�C���x���g������O�ɏo�������̏���
    void Instatiate();
    //���|�b�v����ŊǗ�����ID�𐶐�����
    //�p����̃N���X�Ŏ�������[ContextMenu]�����āA�C���X�y�N�^����ID�𐶐��ł���悤�ɂ���
    void GenerateGuid();
}
