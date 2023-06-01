using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoordinateSystem : MonoBehaviour
{
    private Dictionary<Vector2Int, bool> wallsTilemapCoordinates;
    private HashSet<Vector3> wallsLocations;
    private HashSet<Vector3> factoryOccupiedLocations;
    private HashSet<Vector3> wallFreeLocations;
    private HashSet<Vector3> playerSafeLocations;
    private LevelController levelController;
    private Transform monsterFactories;
    private bool factoryDestroyed;
    private float lastFactoryDestroyedTime;
    private int lastFactoriesCount;

    private void Awake()
    {
        Tilemap wallsTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        wallsTilemapCoordinates = GetTileCoordinates(wallsTilemap);
        wallsLocations = new HashSet<Vector3>();

        foreach (var (coordinates, is_wall) in wallsTilemapCoordinates)
        {
            if (is_wall) wallsLocations.Add(TilemapToWorldCoordinates(coordinates, wallsTilemap));
        }

        wallFreeLocations = GetObstacleFreeLocations(wallsLocations, wallsTilemap.cellBounds);

        PlayerController player = GameObject.Find("Player").GetComponent<PlayerController>();

        playerSafeLocations = GetObstacleFreeLocations(wallsLocations, GetPointCentredArea(player.gameObject.transform.position, player.PlayerSafeArea.x, player.PlayerSafeArea.y));

        monsterFactories = GameObject.Find("MonsterFactories").transform;

        factoryOccupiedLocations = new HashSet<Vector3>();

        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    private void Start()
    {
        lastFactoriesCount = monsterFactories.childCount;
        lastFactoryDestroyedTime = 0f;
        foreach (Transform factory in monsterFactories)
        {
            factoryOccupiedLocations = factoryOccupiedLocations.Union(factory.gameObject.GetComponent<MonsterFactory>().FactoryOccupiedLocations).ToHashSet<Vector3>();
        }

        GameObject.Find("Items").GetComponent<ItemsManager>().enabled = true;
        foreach (Transform factory in GameObject.Find("MonsterFactories").transform)
        {
            factory.gameObject.GetComponent<MonsterFactory>().enabled = true;
        }
    }

    private void Update()
    {
        if (levelController.IsGameActive)
        {
            if ((Time.time - lastFactoryDestroyedTime) > 2f && lastFactoryDestroyedTime != 0f && factoryDestroyed)
            {
                factoryDestroyed = false;
            }

            if (lastFactoriesCount != monsterFactories.childCount)
            {
                HashSet<Vector3> _factoryOccupiedLocations = new HashSet<Vector3>();
                foreach (Transform factory in monsterFactories)
                {
                    _factoryOccupiedLocations = _factoryOccupiedLocations.Union(factory.gameObject.GetComponent<MonsterFactory>().FactoryOccupiedLocations).ToHashSet<Vector3>();
                }
                factoryOccupiedLocations = _factoryOccupiedLocations;
                lastFactoriesCount = monsterFactories.childCount;
                lastFactoryDestroyedTime = Time.time;
                factoryDestroyed = true;
            }
        }
    }

    // Converts Tilemap Coordinates to World Coordinates
    public Vector3 TilemapToWorldCoordinates(Vector2Int tilemapCoordinates, Tilemap  tilemap)
    {
        tilemap.CompressBounds();
        float posx = tilemap.cellBounds.position.x;
        float posy = tilemap.cellBounds.position.y;
        
        return (new Vector3(tilemapCoordinates.x + posx + 0.5f, tilemapCoordinates.y + posy + 0.5f, 0f));
    }

    // Converts World Coordinates to Tilemap Coordinates
    public Vector2Int WorldToTilemapCoordinates(Vector3 worldCoordinates, Tilemap tilemap)
    {
        tilemap.CompressBounds();
        int posx = tilemap.cellBounds.position.x;
        int posy = tilemap.cellBounds.position.y;

        return (new Vector2Int(Mathf.FloorToInt(worldCoordinates.x) - posx, Mathf.FloorToInt(worldCoordinates.y) - posy));
    }

    public Dictionary<Vector2Int, bool> GetTileCoordinates(Tilemap tilemap)
    {
        tilemap.CompressBounds();

        BoundsInt tilemapBounds = tilemap.cellBounds;

        TileBase[] tiles = tilemap.GetTilesBlock(tilemapBounds);

        Dictionary<Vector2Int, bool> tileCoordinates = new Dictionary<Vector2Int, bool>();

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2Int coordinate = new Vector2Int(i % tilemapBounds.size.x, i / tilemapBounds.size.x);
            tileCoordinates.Add(coordinate, (tiles[i] != null));
        }

        return tileCoordinates;
    }

    public HashSet<Vector3> GetObstacleFreeLocations(HashSet<Vector3> obstaclesLocations, BoundsInt area, bool debug = false)
    {
        if (debug) Debug.Log("area : " + area);
        float min_x = area.position.x + 0.5f;
        float min_y = area.position.y + 0.5f;
        float max_x = area.size.x - 1f + min_x;
        float max_y = area.size.y - 1f + min_y;

        if (debug)
        {
            Debug.Log("GetObstacleFreeLocations : \nmin_x : " + min_x + "\nmin_y : " + min_y + "\nmax_x : " + max_x + "\nmax_y : " + max_y );
        }
        float z = 0f;

        HashSet<Vector3> obstacleFreeLocations = new HashSet<Vector3>();
        Vector3 location;

        for (float x = min_x; x <= max_x; x += 1f)
        {
            for (float y = min_y; y <= max_y; y += 1f)
            {
                location = new Vector3(x, y, z);
                if (!obstaclesLocations.Contains(location))
                {
                    if (debug) Debug.Log("GetObstacleFreeLocations : \nlocation : " + location);
                    obstacleFreeLocations.Add(location);
                }
            }
        }
        return obstacleFreeLocations;
    }

    public BoundsInt GetPointCentredArea(Vector3 centre, int width, int height, bool debug = false)
    {
        return GetPointCentredArea(new Vector3Int(Mathf.FloorToInt(centre.x), Mathf.FloorToInt(centre.y), Mathf.FloorToInt(centre.z)), width, height, debug);
    }

    public BoundsInt GetPointCentredArea(Vector3Int centre, int width, int height, bool debug = false)
    {
        BoundsInt bounds = GameObject.Find("Walls").GetComponent<Tilemap>().cellBounds;

        int min_x = bounds.position.x;
        int min_y = bounds.position.y;
        int max_x = bounds.size.x - 1 + min_x;
        int max_y = bounds.size.y - 1 + min_y;

        if (debug)
        {
            Debug.Log("GetPointCentredArea : \nmin_x : " + min_x + "\nmin_y : " + min_y + "\nmax_x : " + max_x + "\nmax_y : " + max_y);
        }

        Vector3Int position, size;

        position = new Vector3Int(Mathf.Max(centre.x - (width / 2), min_x), Mathf.Max(centre.y - (height / 2), min_y), 0);
        size = new Vector3Int(Mathf.Min(max_x - position.x, width), Mathf.Min(max_y - position.y, height), 1);

        if (debug)
        {
            Debug.Log("GetPointCentredArea : \nReturned area : \nposition : " + position + "\nsize : " + size);
        }

        return new BoundsInt(position, size);
    }

    public List<Vector2> GetPathTo(Vector2 start, Vector2 end, HashSet<Vector3> obstacles, BoundsInt bounds)
    {
        List<Vector2> path = new List<Vector2>();
        int height = bounds.size.y;
        int width = bounds.size.x;
        Vector2Int origin = new Vector2Int();
        origin.x = (int)bounds.position.x;
        origin.y = (int)bounds.position.y;
        Queue<Vector2> q = new Queue<Vector2>();
        Dictionary<Vector2, bool> Explored = new Dictionary<Vector2, bool>();
        Dictionary<Vector2, Vector2> nodeParents = new Dictionary<Vector2, Vector2>();
        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };


        for (int i = origin.x; i < width + origin.x; i++)
        {
            for (int j = origin.y; j < height + origin.y; j++)
            {
                Vector2 node = new Vector2();

                node.x = i + (float)0.5;
                node.y = j + (float)0.5;

                Vector2 nodeParent = new Vector2();
                nodeParents.Add(node, nodeParent);
                Explored.Add(node, false);
            }
        }

        q.Enqueue(start);
        Explored[start] = true;
        bool flag = false;
        while (q.Count > 0)
        {
            Vector2 node2 = q.Peek();
            q.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                Vector2 node;
                node.x = node2.x + dx[i];
                node.y = node2.y + dy[i];

                if ((node.x > origin.x) && (node.y > origin.y) && (node.x < width + origin.x) && (node.y < height + origin.y))
                {
                    if ((Explored[node] == false) && (!obstacles.Contains(new Vector3(node.x, node.y, 0f))))
                    {
                        q.Enqueue(node);
                        nodeParents[node] = node2;
                        Explored[node] = true;
                        if (node == end)
                        {   flag=true;
                            break;
                        }
                    }
                }
            }
            if(flag)
            {
                break;
            }
        }

        path.Add(end);
        Vector2 parent_node = nodeParents[end];

        if (parent_node == new Vector2())
        {
            return new List<Vector2>();
        }
        while (parent_node != start)
        {
            path.Add(parent_node);
            parent_node = nodeParents[parent_node];

            if (parent_node == new Vector2())
            {
                return new List<Vector2>();
            }
        }
        path.Reverse();

        return path;
    }

    public HashSet<Vector3> WallsLocations
    {
        get { return wallsLocations; }
    }

    public HashSet<Vector3> FactoryOccupiedLocations
    {
        get { return factoryOccupiedLocations; }
        set { factoryOccupiedLocations = value; }
    }

    public HashSet<Vector3> WallFreeLocations
    {
        get { return wallFreeLocations; }
    }

    public HashSet<Vector3> PlayerSafeLocations
    {
        get { return playerSafeLocations; }
    }

    public bool FactoryDestroyed
    {
        get { return factoryDestroyed; }
    }
}
