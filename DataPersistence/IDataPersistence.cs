/// <summary>
/// �Z�[�u���s���I�u�W�F�N�g�̋��ʃC���^�[�t�F�C�X
/// </summary>
public interface IDataPersistence
{
    /// <summary>
    /// �Z�[�u�f�[�^�I�u�W�F�N�g����ǂݍ���
    /// </summary>
    void LoadData(SaveData data);
    /// <summary>
    /// �Z�[�u�f�[�^�I�u�W�F�N�g�ɏ�������
    /// </summary>
    void SaveData(SaveData data);
}
