using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    //Dictionary��Inspector�œo�^���邽�߂̃��X�g
    [SerializeField] List<SerializableDictionary<string, AudioClip>.Pair> pairs;
    SerializableDictionary<string, AudioClip> audioDic;
    //�f�B�N�V���i������N���b�v�����o�����߂̃L�[
    string keyName;
    AudioSource audioSource;
    Terrain terrain;
    TerrainData tData;
    int index;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        terrain = Terrain.activeTerrain;
        tData = terrain.terrainData;
        //�C���X�y�N�^�[�Őݒ肵���L�[�o�����[�y�A�̃��X�g��Dictinary�ɕϊ�����
        audioDic = SerializableDictionary<string, AudioClip>.ToDictionary(pairs);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //�e���C���Ƃ���ȊO
        //���Ƌ�
        if (audioDic.ContainsKey(hit.gameObject.tag))
        {
            keyName = hit.gameObject.tag;
            return;
        }

        if (hit.gameObject.layer is ICreature.layer_ground)
        {
            //�e���C����̒n�_���A���t�@�}�b�v��̒n�_�ɕϊ�
            var terrainPosition = this.transform.position - terrain.transform.position;
            var mapPosition = new Vector3(terrainPosition.x / tData.size.x, 0, terrainPosition.z / tData.size.z);
            int alphamapPositoionX = (int)(mapPosition.x * tData.alphamapWidth);
            int alphamapPositionY = (int)(mapPosition.z * tData.alphamapHeight);
            //������1�~1�ŃT���v�����O���Ă��邽�߁A�n�_(alphamapPositoionX, alphamapPositionY)�ɑ΂���weight�̎���1�����z�񂪕Ԃ��Ă���
            float[,,] alphamaps = terrain.terrainData.GetAlphamaps(alphamapPositoionX, alphamapPositionY, 1, 1);
            float[] weights = alphamaps.Cast<float>().ToArray();
            //�ő��weight�𒲂ׂ�
            index = System.Array.IndexOf(weights, weights.Max());
            //weight���ő�̃e�N�X�`�������L�[�ɂ���
            keyName = tData.terrainLayers[index].name;
        }
    }

    //�A�j���[�V�����C�x���g�ŌĂяo��
    public void PlayFootsteps()
    {
        audioSource.pitch = 1 + Random.Range(-0.2f, 0.2f);
        audioSource.PlayOneShot(audioDic[keyName]);
    }
}
