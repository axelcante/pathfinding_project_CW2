using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    // SINGLETON DECLARATION
    private static GameManager m_instance;
    public static GameManager Instance
    {
        get { return m_instance; }
    }

    [Header("Tilemap & Tiles")]
    [SerializeField] private Tilemap m_Tilemap;
    [SerializeField] private TileBase m_P1StartTile;
    [SerializeField] private TileBase m_P2StartTile;
    [SerializeField] private TileBase m_GoalTile;

    [Header("Player Settings")]
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private Sprite m_P1Sprite;
    [SerializeField] private Sprite m_P2Sprite;

    [Header("Game Settings")]
    public int groundCost;
    public int slowCost;
    private Algorithm m_Player1;
    private Algorithm m_Player2;



    // Awake is called when this script instance is being loaded
    private void Awake ()
    {
        // SINGLETON DECLARATION
        if (m_instance != null && m_instance != this)
            Destroy(gameObject);
        else
            m_instance = this;
    }


    // Instantiate a new player and return the reference to its Algorithm script
    private Algorithm InstantiatePlayer ()
    {
        GameObject player = Instantiate(m_PlayerPrefab);

        if (player.GetComponent<Algorithm>())
            return player.GetComponent<Algorithm>();

        // ERROR CASE
        Debug.LogWarning("Could not find Algorithm component on new player");
        return null;
    }



    // Initialize a new player's (algorithm) parameters
    private void InitializePlayer (Algorithm player)
    {
        player.SetTileMap(m_Tilemap);
        player.SetSprite(player == m_Player1 ? m_P1Sprite : m_P2Sprite);
        player.SetStartTile(player == m_Player1 ? m_P1StartTile : m_P2StartTile);
        player.SetGoalTile(m_GoalTile);
        player.SetName(player == m_Player1 ? "player1" : "player2");
        player.InitTilePositions();
        player.InitCharPosition();
    }

    public void RunAlgorithms(){
        // TODO: Move to a UI -- Run all instantiated algorithms (Dijkstra and versions of A*/Astar)
        if (m_Player1)
            m_Player1.RunAlgorithm();

        if (m_Player2)
            m_Player2.RunAlgorithm();      
    }

    public void InstantiatePlayer(int i){
        if (i == 1){
            if (m_Player1)
                Destroy(m_Player1.gameObject);
            m_Player1 = InstantiatePlayer();
            InitializePlayer(m_Player1);
        }
        else if(i == 2){
             if (m_Player2)
                Destroy(m_Player2.gameObject);
            m_Player2 = InstantiatePlayer();
            InitializePlayer(m_Player2);
            m_Player2.Type = AlgorithmType.AstarManhattan;
        }
    }

    public void DisplayOverlays(int i){
        if (m_Player1 && i == 1)
            m_Player1.m_TileCostOverlay.SetActive(!m_Player1.m_TileCostOverlay.activeSelf);

        if (m_Player2 && i == 2)
            m_Player2.m_TileCostOverlay.SetActive(!m_Player2.m_TileCostOverlay.activeSelf);
    }

    public void StartSimulation(){
        if (m_Player1)
                StartCoroutine(m_Player1.StartPathing());

        if (m_Player2)
                StartCoroutine(m_Player2.StartPathing());
    }
}
