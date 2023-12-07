using UnityEngine;
using TMPro;

/// <summary>
/// ダメージテキストオブジェクト
/// </summary>
public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    void Update()
    {
        //ダメージテキストは上にフェードアウトしていく
        transform.position += Vector3.up * 0.3f * Time.deltaTime;
        damageText.color = Color.Lerp(damageText.color, new Color(0,0,0,0), 1.5f * Time.deltaTime);
        if (damageText.color.a <= 0.1f) Destroy(gameObject);
    }

    public void SetDamageText(string text)
    {
        damageText.text = text;
    }
}
