using UnityEngine;

/// <summary>
/// 索敵コライダ
/// </summary>
public class SearchArea : MonoBehaviour
{

    [SerializeField] AbstractController controller;

    //HitBoxと接触判定
    private void OnTriggerEnter(Collider other)
    {
        GameObject enemy = other.gameObject;
        controller.SetEnemySlots(enemy);
        Debug.Log("エネミーに追加");
    }
}
