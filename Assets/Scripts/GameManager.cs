using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Tilemap m_Tilemap;
    [SerializeField] private TileBase[] m_ObstacleTiles;
    [SerializeField] private TileBase[] m_WalkableTiles;
    [SerializeField] private TileBase m_GoalTile;
    [SerializeField] private TileBase m_StartTile;

    // These vectors represent the positions of the start and end tiles
    // TODO: MAKE PRIVATE
    public Vector3Int m_StartAstar;
    public Vector3Int m_StartDijk;
    public Vector3Int m_Goal;


    // These dictionaries will keep track of the calculated distances and the previously visited Tiles
    private Dictionary<Vector3Int, int> m_Distances = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, Vector3Int> m_Prev = new Dictionary<Vector3Int, Vector3Int>();



    // Start is called before the first frame update
    // Here we initialize the collections and run the algorithms
    void Start()
    {
        // Set the distance to the start tile to 0 and all other tiles to infinity
        m_Distances[m_StartAstar] = 0;
        foreach (Vector3Int pos in m_Tilemap.cellBounds.allPositionsWithin) {
            m_Distances[pos] = int.MaxValue;
            Debug.Log("Got one!");
        }

        // Create a queue (i.e., First-In-First-Out) to store tiles to visit, and add the first one
        PriorityQueue<Vector3Int> queue = new PriorityQueue<Vector3Int>();
        queue.Enqueue(m_StartDijk, 0);

        // As long as there are still tiles to visit, dequeue the one with the highest priority
        while (queue.Count > 0) {
            Vector3Int currentTile = queue.Dequeue();

            // Check if the current tile visited is the end tile, in which case we end
            if (currentTile == m_Goal)
                break;

            // Get the cost of the current tile
            // TODO: HEURISTICS
            int cost = GetTileCost(currentTile);

            foreach (Vector3Int neighbour in GetNeighbours(currentTile)) {
                // Check if the neighbour is walkable
                if (!IsWalkable(neighbour))
                    continue;

                // Calculate the new distance to the neighbour
                int distanceToNeighbour = m_Distances[currentTile] + cost;

                // If the new distance is less than the current distance, update it
                if (distanceToNeighbour < m_Distances[neighbour]) {
                    m_Distances[neighbour] = distanceToNeighbour;
                    m_Prev[neighbour] = currentTile;

                    // Add the neighbour to the queue
                    queue.Enqueue(neighbour, distanceToNeighbour);
                }
            }
        }
        // End of while (queue.Count > 0)

        // Once we're done, we can reconstruct the path
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = m_Goal;

        while (current != m_StartDijk) {
            path.Add(current);
            current = m_Prev[current];
        }

        path.Reverse();

        // TODO: MOVEMENT HERE
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

        // TODO: NOT NEEDED?
        foreach (TileBase obstacleTile in m_ObstacleTiles) {
            if (tileBase == obstacleTile)
                return false;
        }

        // TODO: DEFAULT SHOULD BE ENOUGH
        return false;
    }


    // Fetch the movement cost assigned to a particular tile
    // TODO
    private int GetTileCost (Vector3Int tile)
    {
        // Same logic as before but with a switch case for each tile with extra movement
        // I started naming some in the tile set, like SLOW_1, SLOW_2...
        // Don't need to have loads, just one type will do

        // CODE
        //TileBase tileBase = m_Tilemap.GetTile(tile);

        //foreach (TileBase walkableTile in m_WalkableTiles) {
        //    if (tileBase == walkableTile) {
        //        // Return the cost of the tile based on its type
        //        switch (tileBase.name) {
        //            default:
        //                return 1;
        //        }
        //    }
        //}

        return 1;
    }

    

    // Get the tiles which are adjacent to the current tile
    private List<Vector3Int> GetNeighbours (Vector3Int tile)
    {
        return new List<Vector3Int>();
    }
}
