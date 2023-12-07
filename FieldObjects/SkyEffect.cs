using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 天候エフェクト
/// </summary>
public class SkyEffect : MonoBehaviour
{
    [SerializeField, Tooltip("天候エフェクトの高さ")] Vector3 offset;

    void Update()
    {
        //アクティブオブジェクトについていく
        if (GameManager.activeObject != null) transform.position = GameManager.activeObject.transform.position + offset;
    }

    /// <summary>
    /// 天候変化時に呼び出され、インスタンスを破棄する
    /// </summary>
    public async void OnWeatherChanged()
    {
        this.GetComponent<ParticleSystem>().Stop();
        //存在するパーティクルが全て消滅するのを待つ
        await UniTask.Delay(2000);
        Destroy(this.gameObject);
    }
}
