using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private static MapGenerator instance;
    public static MapGenerator Instance {
        get { return instance; }
    }

    public enum GameMode { Tutorial, Infinite, Scenario };
    public GameMode currentMode;

    public Map[] maps;
    public int mapIndex;


    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navMeshFloor;
    public Transform mapFloor;
    public Transform navMeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    Map currentMap;
    System.Random mapRand;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
        mapIndex = 0;
    }

    void OnNewWave(int waveNumber) {
        if (waveNumber <= maps.Length) {
            mapIndex = waveNumber - 1;
            GenerateMap(maps[mapIndex]);
        }
        else if (currentMode == GameMode.Infinite) {
            mapIndex++;
            mapRand = new System.Random((int)Time.time + mapIndex);
            Coord mapSize = new Coord(mapRand.Next(15, (int)maxMapSize.x - 1), mapRand.Next(15, (int)maxMapSize.y - 1));
            Debug.Log($"Mapsize: {mapSize.x}, {mapSize.y}");
            float obstaclePercent = (float)mapRand.NextDouble() * 0.25f + 0.05f;
            
            Color randomBackColor, randomForeColor;
            switch (Random.Range(0,6)) {
                case 0:
                    randomBackColor = Color.red;
                    randomForeColor = Color.magenta;
                    break;
                case 1:
                    randomBackColor = Color.green;
                    randomForeColor = Color.yellow;
                    break;
                case 2:
                    randomBackColor = Color.blue;
                    randomForeColor = Color.cyan;
                    break;
                case 3:
                    randomBackColor = Color.magenta;
                    randomForeColor = Color.blue;
                    break;
                case 4:
                    randomBackColor = Color.cyan;
                    randomForeColor = Color.green;
                    break;
                case 5:
                    randomBackColor = Color.yellow;
                    randomForeColor = Color.red;
                    break;
                default:
                    randomBackColor = Color.black;
                    randomForeColor = Color.white;
                    break;
            }

            Map newMap = new Map(mapSize, obstaclePercent, mapRand.Next(), randomBackColor, randomForeColor);
            GenerateMap(newMap);
        } // Infinite mode 정해진 맵 목록 이후 랜덤 맵 출현
        else if (currentMode == GameMode.Tutorial) {
            //You Finished Tutorial!
        }

    }

    public void GenerateMap(Map _map) {

        currentMap = _map;
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);

        //Generate Coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                allTileCoords.Add(new Coord(x,y));
            }
        }
        shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //Generate Map Holder
        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            Destroy(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //print($"current map size: {currentMap.mapSize.x}, {currentMap.mapSize.y}");

        //* Tile Generation
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x,y] = newTile;
            }
        }
       
       //* Obstacle Generation

        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {

                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                newObstacle.parent = mapHolder;

                //color control
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = (float)randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //navmeshmask generation
        Transform maskLeft = Instantiate(navMeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navMeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navMeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navMeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 1) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x, currentMap.mapSize.y, 1) * tileSize;
    }

    //Flood Fill Algorithm
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    if (x == 0 || y == 0) {
                        if ((neighborX >= 0) && (neighborX < obstacleMap.GetLength(0))
                        && (neighborY >= 0) && (neighborY < obstacleMap.GetLength(1))) {
                            if (!mapFlags[neighborX, neighborY] && !obstacleMap[neighborX,neighborY]) {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                                accessibleTileCount++;
                            }
                        } 
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return (targetAccessibleTileCount == accessibleTileCount);
    }

    public Vector3 CoordToPosition(int x, int y) {
        return new Vector3 (-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position) {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile() {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    public Coord GetCurrentMapCenter()
    {
        return currentMap.mapCenter;
    }

    [System.Serializable]
    public struct Coord {
        public int x, y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator == (Coord c1, Coord c2) {
            return ((c1.x ==  c2.x) && (c1.y == c2.y));
        }

        public static bool operator != (Coord c1, Coord c2) {
            return ((c1.x != c2.x) || (c1.y != c2.y));
        }

        public override bool Equals(object obj) {
            if (!(obj is Coord)) return false;

            Coord c = (Coord)obj;
            return ((x == c.x) && (y == c.y));
        }

        public override int GetHashCode() {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                return hash;
            }
        }
    }

    [System.Serializable]
    public class Map {
        public Coord mapSize;

        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight, maxObstacleHeight;
        public Color foregroundColor, backgroundColor;

        public Coord mapCenter {
            get {
                return new Coord(mapSize.x/2, mapSize.y/2);
            }
        }

        public Map() {
            minObstacleHeight = 1;
            maxObstacleHeight = 3;
        }

        public Map(Coord _mapSize, float _obstaclePercent, int _seed, Color _back, Color _fore) { //Generating Random Map
            mapSize = _mapSize;
            obstaclePercent = _obstaclePercent;
            seed = _seed;
            minObstacleHeight = 1;
            maxObstacleHeight = 3;        
            backgroundColor = _back;
            foregroundColor = _fore;
        }
    }
}
