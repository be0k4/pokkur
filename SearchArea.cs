using UnityEngine;

/// <summary>
/// ���G�R���C�_
/// </summary>
public class SearchArea : MonoBehaviour
{

    [SerializeField] AbstractController controller;

    //HitBox�ƐڐG����
    private void OnTriggerEnter(Collider other)
    {
        GameObject enemy = other.gameObject;
        controller.SetEnemySlots(enemy);
        Debug.Log("�G�l�~�[�ɒǉ�");
    }
}
