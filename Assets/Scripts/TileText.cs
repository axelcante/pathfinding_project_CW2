using TMPro;
using UnityEngine;

public class TileText : MonoBehaviour
{
    public TMP_Text m_TextComponent;
    public float m_tileOffset = 0.5f;

    public void SetText (string text) => m_TextComponent.text = text;
    public void SetPosition (Vector3 pos)
        => gameObject.transform.position = new Vector3(
            pos.x + m_tileOffset,
            pos.y + m_tileOffset,
            pos.z
        );
}
