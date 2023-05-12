using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public enum AlgorithmType
{
    Dijkstra,
    AstarManhattan,
}

public class Algorithm : MonoBehaviour
{
    public AlgorithmType Type;

    // Tilemap related settings - used to dynamically determine start and goal positions
    [Header("Tiles")]
    [SerializeField] private TileBase[] m_ObstacleTiles;
    [SerializeField] private TileBase[] m_WalkableTiles;
    private Tilemap m_Tilemap;
    private TileBase m_StartTile;
    private TileBase m_GoalTile;

    [Header("UI")]
    public GameObject m_TileCostOverlay;
    public GameObject m_TileTextOverlayPrefab;
    public GameObject m_Character;
    public SpriteRenderer m_SpriteRenderer;
    public float m_tileOffset = 0.5f;
    public float m_moveSpeed = 1f;
    public float m_pauseBetweenTiles = 0.5f;

    // StartPos and GoalPos are found once a StartTile has been assigned
    private Vector3Int m_StartPos;
    private Vector3Int m_GoalPos;
    private Queue<Vector3Int> m_Path = new Queue<Vector3Int>();

    // These dictionaries will keep track of the calculated distances and the previously visited Tiles
    private Dictionary<Vector3Int, int> m_Distances = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, Vector3Int> m_Prev = new Dictionary<Vector3Int, Vector3Int>();
    // This dictionnary keeps track of where the overlay tiles are displayed
    private Dictionary<Vector3Int, GameObject> m_OverlaidTiles = new Dictionary<Vector3Int, GameObject>();



    // Start is called before the first frame after this script instance is created
    private void Start ()
    {
        // The cost overlay is set to inactive by default
        m_TileCostOverlay.SetActive(false);
    }



    // Detect if a position is a walkable tile
    private bool IsWalkable (Vector3Int tile)
    {
        TileBase tileBase = m_Tilemap.GetTile(tile);

        // Check in the list of walkable tiles
        foreach (TileBase walkableTile in m_WalkableTiles) {
            if (tileBase == walkableTile)
                return true;
        }

        return false;
    }



    // Fetch the movement cost assigned to a particular tile
    private int GetTileCost (Vector3Int tile)
    {
        // Same logic as before but with a switch case for each tile with extra movement
        // I started naming some in the tile set, like SLOW_1, SLOW_2...
        // Don't need to have loads, just one type will do

        TileBase tileBase = m_Tilemap.GetTile(tile);

        foreach (TileBase walkableTile in m_WalkableTiles) {
            if (tileBase == walkableTile) {
                // Return the cost of the tile based on its type
                switch (tileBase.name) {
                    case "GROUND":
                    //case "GOAL":
                        return GameManager.Instance.groundCost;
                    case "SLOW_1":
                    case "SLOW_2":
                    case "SLOW_3":
                    case "SLOW_4":
                        return GameManager.Instance.slowCost;
                    default:
                        // i.e., tile not walkable
                        return 0;
                }
            }
        }

        // i.e., tile not walkable
        return 0;
    }



    // Get the tiles which are adjacent to the current tile
    private List<Vector3Int> GetNeighbours (Vector3Int tile)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        // Check all four cardinal directions (up/down, left/right)
        Vector3Int[] directions =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (Vector3Int direction in directions) {
            Vector3Int neighbor = tile + direction;
            neighbors.Add(neighbor);
        }

        return neighbors;
    }



    // This is where algorithms set the priority of the PriorityQueue list. For Astar, this is also where heuristics come into play
    private int CalculatePriority (int cost, Vector3Int neighbour = default(Vector3Int))
    {
        switch (Type) {
            case AlgorithmType.Dijkstra:
                return cost;
            case AlgorithmType.AstarManhattan:
                return cost + ManhattanDistance(neighbour, m_GoalPos);
            default:
                return cost;
        }
    }



    // HEURISTIC: Calculate the Manhattan distance between current tile and goal tile (ASTAR)
    private int ManhattanDistance (Vector3Int p1,  Vector3Int p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }



    // Find the tiles where this algorithm will start from as well as the Goal tile position
    public void InitTilePositions ()
    {
        if (m_StartTile && m_GoalTile) {
            bool sFound = false;
            bool gFound = false;

            // Find the position of the start tile in the tilemap
            foreach (Vector3Int position in m_Tilemap.cellBounds.allPositionsWithin) {
                if (m_Tilemap.GetTile(position) == m_StartTile) {
                    m_StartPos = position;
                    sFound = true;
                } else if (m_Tilemap.GetTile(position) == m_GoalTile) {
                    m_GoalPos = position;
                    gFound = true;
                }

                if (sFound && gFound)
                    return;
            }
        }

        Debug.LogWarning("Could not find either the start position or the goal position!");
    }



    // Run the algorithm -- THIS IS WHERE DIJKSRTA/A* ARE IMPLEMENTED
    public void RunAlgorithm ()
    {
        // Set the distance to the start tile to 0 and all other tiles to infinity
        foreach (Vector3Int pos in m_Tilemap.cellBounds.allPositionsWithin)
            m_Distances[pos] = int.MaxValue;
        m_Distances[m_StartPos] = 0;

        // Create a queue (i.e., First-In-First-Out) to store tiles to visit, and add the first one
        PriorityQueue<Vector3Int> queue = new PriorityQueue<Vector3Int>();
        queue.Enqueue(m_StartPos, 0);

        // As long as there are still tiles to visit, dequeue the one with the highest priority
        while (queue.Count > 0) {
            Vector3Int currentTile = queue.Dequeue();

            // Check if the current tile visited is the end tile, in which case we end
            if (currentTile == m_GoalPos)
                break;

            foreach (Vector3Int neighbour in GetNeighbours(currentTile)) {
                // Check if the neighbour is not walkable, in which case skip this neighbour tile
                if (!IsWalkable(neighbour))
                    continue;

                // Calculate the new distance to the neighbour
                int distanceToNeighbour = m_Distances[currentTile] + GetTileCost(neighbour);

                // If the new distance is less than the current distance, update it
                if (distanceToNeighbour < m_Distances[neighbour]) {
                    m_Distances[neighbour] = distanceToNeighbour;
                    m_Prev[neighbour] = currentTile;

                    // Add the neighbour to the queue with a priority
                    // This is where Dijkstra and Astar diverge; for Dijkstra, priority is simply the cost
                    // But for Astar, it is based on a heuristic
                    int priority = CalculatePriority(distanceToNeighbour, neighbour);
                    queue.Enqueue(neighbour, priority);

                    // Add a text field with the cost on top of the tile to show what calculations have been done
                    GameObject overlayTile = AddOverlayTile(neighbour);
                    TileText overlayText = overlayTile.GetComponent<TileText>();
                    if (overlayText) {
                        overlayText.SetText(distanceToNeighbour.ToString());
                        overlayText.SetPosition(neighbour);
                    }
                }
            }
        }
        // End of while (queue.Count > 0)

        // Once we're done, we can reconstruct the path
        Vector3Int current = m_GoalPos;
        List<Vector3Int> path = new List<Vector3Int>();
        while (current != m_StartPos) {
            path.Add(current);
            current = m_Prev[current];
        }

        path.Reverse();
        m_Path = new Queue<Vector3Int>(path);
    }



    // Display the character sprite on start position
    public void InitCharPosition () => MoveCharacter(m_StartPos);



    // Move character to specific position with offset
    public void MoveCharacter (Vector3Int newPos) => m_Character.transform.position = new Vector3(
            newPos.x + m_tileOffset,
            newPos.y + m_tileOffset,
            newPos.z
        );



    // Get the new position offset to be centered on a tile (for smooth movement in IEnumerator)
    public Vector3 GetNewOffsetPosition (Vector3Int newPos) => new Vector3(
            newPos.x + m_tileOffset,
            newPos.y + m_tileOffset,
            newPos.z
        );



    // Start pathing along the ideal path
    public IEnumerator StartPathing ()
    {
        Vector3 lastPosition = m_Character.transform.position;

        while (m_Path.Count > 0) {
            Vector3Int nextPos = m_Path.Dequeue();
            Vector3 desiredPosition = GetNewOffsetPosition(nextPos);
            float lerpVal = 0;
            while (lerpVal < 1) {
                lerpVal += Time.deltaTime * m_moveSpeed;
                m_Character.transform.position = Vector3.Lerp(lastPosition, desiredPosition, lerpVal);
                yield return new WaitForEndOfFrame();
            }
            m_Character.transform.position = desiredPosition;
            lastPosition = desiredPosition;
            yield return new WaitForSeconds(m_pauseBetweenTiles);
        }
    }



    // Check if an overlay tile (with text) is currently displayed at this position
    public bool IsOverlayTileAtPosition (Vector3Int pos)
    {
        return m_OverlaidTiles.ContainsKey(pos);
    }



    // Add an overlay tile if there are none; otherwise replace the previous one
    public GameObject AddOverlayTile (Vector3Int position)
    {
        if (m_OverlaidTiles.ContainsKey(position))
            RemoveOverlayTile(position);

        GameObject overlayTile = Instantiate(m_TileTextOverlayPrefab, m_TileCostOverlay.transform);
        m_OverlaidTiles.Add(position, overlayTile);
        return overlayTile;
    }



    // Remove an overlay tile from a given position
    public void RemoveOverlayTile (Vector3Int position)
    {
        if (m_OverlaidTiles.TryGetValue(position, out GameObject overlayTile)) {
            Destroy(overlayTile);
            m_OverlaidTiles.Remove(position);
        }
    }



    // GETTERS & SETTERS
    public void SetTileMap (Tilemap tilemap) => m_Tilemap = tilemap;
    public void SetStartTile (TileBase tile) => m_StartTile = tile;
    public void SetGoalTile (TileBase tile) => m_GoalTile = tile;
    public void SetSprite (Sprite sprite) => m_SpriteRenderer.sprite = sprite;
}
