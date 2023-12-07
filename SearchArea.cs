using UnityEngine;

/// <summary>
/// õ“GƒRƒ‰ƒCƒ_
/// </summary>
public class SearchArea : MonoBehaviour
{

    [SerializeField] AbstractController controller;

    //HitBox‚ÆÚG”»’è
    private void OnTriggerEnter(Collider other)
    {
        GameObject enemy = other.gameObject;
        controller.SetEnemySlots(enemy);
    }
}
