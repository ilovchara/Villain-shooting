using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 随机生成地图
public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab; // 地板预制体
    public Transform obstaclePrefab; // 障碍物预制体
    public Transform navmeshFloor; // 用于寻路的地板
    public Vector2 mapSize; // 地图大小（宽和高）
    public Vector2 maxMapSize; // 地图的最大尺寸
    public float tileSize; // 单个地块的大小
    public Transform navmeshMaskPrefab;

    [Range(0, 1)]
    public float obstaclePercent; // 障碍物占比（0~1之间）
    [Range(0, 1)]
    public float outlinePercent; // 地块间隔比例（用于视觉上的分割）
    [SerializeField] public int seed = 10; // 随机种子，影响障碍物的排列

    List<Coord> allTileCoords; // 存储所有地块坐标
    Queue<Coord> shuffledTileCoords; // 存储随机打乱后的地块坐标
    Coord mapCentre; // 地图中心坐标

    void Start()
    {
        GenerateMap(); // 生成地图
    }

    public void GenerateMap()
    {
        allTileCoords = new List<Coord>();

        // 创建地图坐标列表
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // 生成随机化的地块队列
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2); // 计算地图中心坐标

        string holderName = "Generated Map";

        // 如果已经有地图对象，先销毁它
        if (transform.Find(holderName))
        {
            // ********** 特殊函数编辑器调用
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        // 创建一个新的地图容器对象
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // 生成地板
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
            }
        }

        // 生成障碍物
        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];
        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // 确保地图仍然是可通行的
            if (MapIsFullyAccessible(obstacleMap, currentObstacleCount) && randomCoord != mapCentre)
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.2f, Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
            }
        }

        // 左右边界
        Transform maskLeft = Instantiate(navmeshMaskPrefab,
            Vector3.left * (mapSize.x + maxMapSize.x) / 4 * tileSize,
            Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab,
            Vector3.right * (mapSize.x + maxMapSize.x) / 4 * tileSize,
            Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;

        // 上下边界
        Transform maskUp = Instantiate(navmeshMaskPrefab,
            Vector3.forward * (mapSize.y + maxMapSize.y) / 4 * tileSize,
            Quaternion.identity);
        maskUp.parent = mapHolder;
        maskUp.localScale = new Vector3(mapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        Transform maskDown = Instantiate(navmeshMaskPrefab,
            Vector3.back * (mapSize.y + maxMapSize.y) / 4 * tileSize,
            Quaternion.identity);
        maskDown.parent = mapHolder;
        maskDown.localScale = new Vector3(mapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;


        // 设置地图边界大小 - 这里不知道为什么 只能改变x和z轴，与教程不同
        navmeshFloor.localScale = new Vector3(maxMapSize.x, 0, maxMapSize.y) * tileSize;
    }

    // 检查地图是否仍然是可通行的（广度优先搜索）
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    // 坐标转换为世界坐标
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
    }

    // 获取一个随机坐标
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    // 坐标结构体
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) => c1.x == c2.x && c1.y == c2.y;
        public static bool operator !=(Coord c1, Coord c2) => !(c1 == c2);
    }
}
