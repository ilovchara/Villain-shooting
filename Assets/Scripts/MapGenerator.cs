using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

	public Map[] maps;
	public int mapIndex;

	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	public Vector2 maxMapSize;

	[Range(0, 1)]
	public float outlinePercent;

	public float tileSize;
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	Queue<Coord> shuffledOpenTileCoords;
	Transform[,] tileMap;

	Map currentMap;

	void Awake()
	{
		FindAnyObjectByType<Spawner>().OnNewWave += OnNewWave;
	}

	void OnNewWave(int waveNumber)
	{
		mapIndex = waveNumber - 1;
		GenerateMap();
	}


	public void GenerateMap()
	{
		currentMap = maps[mapIndex];
		tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
		// 1. 伪随机数生成器 调用.next生成一个随机数
		System.Random prng = new System.Random(currentMap.seed);
		// 改变地图碰撞体大小 - 符合地图大小
		GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f, currentMap.mapSize.y * tileSize);

		// Generating coords - 存储所有的地图Tile - add进入list
		allTileCoords = new List<Coord>();
		for (int x = 0; x < currentMap.mapSize.x; x++)
		{
			for (int y = 0; y < currentMap.mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
		// 使用洗牌算法
		shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

		// Create map holder object
		// 创建一个新的 "Generated Map"，作为新地图的父对象，便于管理地图元素
		string holderName = "Generated Map";
		if (transform.Find(holderName))
		{
			DestroyImmediate(transform.Find(holderName).gameObject);
		}

		Transform mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = transform;

		// Spawning tiles
		// 创建Tile 并且交由mapHoldr管理
		for (int x = 0; x < currentMap.mapSize.x; x++)
		{
			for (int y = 0; y < currentMap.mapSize.y; y++)
			{
				Vector3 tilePosition = CoordToPosition(x, y);
				Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
				// outlinePercent是轮廓 - 值在1-0之间
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x, y] = newTile;
			}
		}

		// Spawning obstacles
		// 障碍物数组
		bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
		// 使用百分比控制的障碍物数量
		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		// 当前数量 - 用于数组比对地图生成是否正确
		int currentObstacleCount = 0;
		// 可供敌方生成的位置
		List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
		// 在可用地图中生成一个随机障碍物
		for (int i = 0; i < obstacleCount; i++)
		{
			Coord randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				// 障碍物高度 - 采用插值的方式让高度保持在这两个变量之间
				float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
				Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
				// Quaternion.identity 通常用来表示对象的初始旋转状态
				Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
				newObstacle.parent = mapHolder;
				newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
				// 为障碍物添加 渲染器和材质
				Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
				Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
				float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
				obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
				obstacleRenderer.sharedMaterial = obstacleMaterial;
				// 生成之后标注这个位置被占用
				allOpenCoords.Remove(randomCoord);
			}
			else
			{
				// 生成位置不合法 - 当前障碍物数量减少
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		// 这行代码的作用是将 allOpenCoords 列表中的坐标打乱顺序，并将打乱后的坐标队列存入 shuffledOpenTileCoords 中
		shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

		// Creating navmesh mask
		// 生成障碍物 - 通过增加遮罩的大小可以间接地控制四个方向上障碍物的大小或范围
		Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

		Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

		Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

		Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

		navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

	}
	// 使用BFS算法来显示哪一个地图可用
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
	{
		// 镖旗是否访问过 数组内的元素和map元素大小是一一对应的
		bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		Queue<Coord> queue = new Queue<Coord>();
		// 入队的第一个坐标 - 这里设定为玩家生成点
		queue.Enqueue(currentMap.mapCentre);
		mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

		int accessibleTileCount = 1;

		while (queue.Count > 0)
		{
			// 出队
			Coord tile = queue.Dequeue();
			// 判断当前坐标的上下左右是否合法
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
								// 合法Tile的数量
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}
		// 判断合法tile数量，符合地图构造
		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}
	// 将网格坐标转换为世界坐标
	Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
	}

	// 根据传入的世界坐标位置，获取对应的网格位置上的 Tile 对象。
	// position: 世界坐标系中的位置 (Vector3)，通常是物体在游戏场景中的位置。
	// 返回值: 返回位于与给定世界位置相对应的瓦片对象 (Transform)，
	//         如果坐标位置超出边界，会限制到有效范围内的瓦片。
	public Transform GetTileFromPosition(Vector3 position)
	{
		int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
		y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
		return tileMap[x, y];
	}

	// 从打乱过的瓦片坐标队列中随机获取一个坐标。
	// 返回值: 返回一个 `Coord` 类型的坐标 (x, y)，表示一个地图上随机位置。
	//         该坐标在每次调用时会在队列中循环，以保证每个位置都有机会被选中。
	public Coord GetRandomCoord()
	{
		Coord randomCoord = shuffledTileCoords.Dequeue();
		shuffledTileCoords.Enqueue(randomCoord);
		return randomCoord;
	}

	// 从打乱过的开放格子坐标队列中随机获取一个开放的瓦片。
	// 返回值: 返回一个 Transform 类型的瓦片对象，这个瓦片位于一个可用的开放位置。
	//         该位置是在 `shuffledOpenTileCoords` 队列中随机选中的，并且该瓦片位置为未被障碍物占据。
	public Transform GetRandomOpenTile()
	{
		Coord randomCoord = shuffledOpenTileCoords.Dequeue();
		shuffledOpenTileCoords.Enqueue(randomCoord);
		return tileMap[randomCoord.x, randomCoord.y];
	}

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

		// 重载 == 运算符
		public static bool operator ==(Coord c1, Coord c2)
		{
			return c1.x == c2.x && c1.y == c2.y;
		}

		// 重载 != 运算符
		public static bool operator !=(Coord c1, Coord c2)
		{
			return !(c1 == c2);
		}

		// 重写 Equals 方法
		public override bool Equals(object obj)
		{
			if (obj is Coord)
			{
				Coord other = (Coord)obj;
				return this == other;
			}
			return false;
		}

		// 重写 GetHashCode 方法
		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}
	}


	[System.Serializable]
	public class Map
	{

		public Coord mapSize;
		[Range(0, 1)]
		public float obstaclePercent;
		public int seed;
		public float minObstacleHeight;
		public float maxObstacleHeight;
		public Color foregroundColour;
		public Color backgroundColour;

		public Coord mapCentre
		{
			get
			{
				return new Coord(mapSize.x / 2, mapSize.y / 2);
			}
		}

	}
}