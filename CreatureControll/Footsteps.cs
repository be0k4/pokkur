using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    //DictionaryをInspectorで登録するためのリスト
    [SerializeField] List<SerializableDictionary<string, AudioClip>.Pair> pairs;
    SerializableDictionary<string, AudioClip> audioDic;
    //ディクショナリからクリップを取り出すためのキー
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
        //インスペクターで設定したキーバリューペアのリストをDictinaryに変換する
        audioDic = SerializableDictionary<string, AudioClip>.ToDictionary(pairs);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //テレインとそれ以外
        //水と橋
        if (audioDic.ContainsKey(hit.gameObject.tag))
        {
            keyName = hit.gameObject.tag;
            return;
        }

        if (hit.gameObject.layer is ICreature.layer_ground)
        {
            //テレイン上の地点をアルファマップ上の地点に変換
            var terrainPosition = this.transform.position - terrain.transform.position;
            var mapPosition = new Vector3(terrainPosition.x / tData.size.x, 0, terrainPosition.z / tData.size.z);
            int alphamapPositoionX = (int)(mapPosition.x * tData.alphamapWidth);
            int alphamapPositionY = (int)(mapPosition.z * tData.alphamapHeight);
            //ここで1×1でサンプリングしているため、地点(alphamapPositoionX, alphamapPositionY)に対するweightの実質1次元配列が返ってくる
            float[,,] alphamaps = terrain.terrainData.GetAlphamaps(alphamapPositoionX, alphamapPositionY, 1, 1);
            float[] weights = alphamaps.Cast<float>().ToArray();
            //最大のweightを調べる
            index = System.Array.IndexOf(weights, weights.Max());
            //weightが最大のテクスチャ名をキーにする
            keyName = tData.terrainLayers[index].name;
        }
    }

    //アニメーションイベントで呼び出す
    public void PlayFootsteps()
    {
        audioSource.pitch = 1 + Random.Range(-0.2f, 0.2f);
        audioSource.PlayOneShot(audioDic[keyName]);
    }
}
