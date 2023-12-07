using UnityEngine;
using TMPro;

/// <summary>
/// �_���[�W�e�L�X�g�I�u�W�F�N�g
/// </summary>
public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    void Update()
    {
        //�_���[�W�e�L�X�g�͏�Ƀt�F�[�h�A�E�g���Ă���
        transform.position += Vector3.up * 0.3f * Time.deltaTime;
        damageText.color = Color.Lerp(damageText.color, new Color(0,0,0,0), 1.5f * Time.deltaTime);
        if (damageText.color.a <= 0.1f) Destroy(gameObject);
    }

    public void SetDamageText(string text)
    {
        damageText.text = text;
    }
}
