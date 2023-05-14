using TMPro;
using UnityEngine;

public class TileText : MonoBehaviour
{
    public TMP_Text m_TextComponent;
    public float m_tileOffset = 0.5f;
    
    private void Awake() {
        m_TextComponent.fontSize = 3;
    }

    public void SetText (string text) => m_TextComponent.text = text;
    public void SetPosition (Vector3 pos, string name){
        if (name == "player1"){
        gameObject.transform.position = new Vector3(
            pos.x + m_tileOffset - 0.25f,
            pos.y + m_tileOffset - 0.25f,
            pos.z);
        }
        else if (name == "player2"){
            gameObject.transform.position = new Vector3(
            pos.x + m_tileOffset + 0.25f,
            pos.y + m_tileOffset + 0.25f,
            pos.z);
        }
    }
    public void SetColor (string name){
        if (name == "player1"){
            m_TextComponent.color = new Color32(0, 0, 255, 255);
        }
        else if (name == "player2"){
            m_TextComponent.color = new Color32(255, 0, 0, 255);
        }
    }
}
