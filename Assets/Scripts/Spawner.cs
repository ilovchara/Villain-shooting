using UnityEngine;
using System.Collections;
using System;

public class Spawner : MonoBehaviour
{
    // Public variables for defining waves and enemy type
    public Wave[] waves;   // 存储不同波次的敌人数据
    public Enemy enemy;    // 用于生成敌人的预制体

    // Private variables to manage player and spawn mechanics
    LivingEntity playerEntity;    // 角色实体，用于获取玩家信息
    Transform playerT;            // 玩家的位置 (Transform)

    Wave currentWave;             // 当前波次信息
    int currentWaveNumber;        // 当前波数

    int enemiesRemainingToSpawn;  // 剩余要生成的敌人数量
    int enemiesRemainingAlive;    // 剩余存活的敌人数量
    float nextSpawnTime;          // 下一次生成敌人的时间

    MapGenerator map;             // 地图生成器，控制地图和障碍物

    // 露营检测机制相关变量
    float timeBetweenCampingChecks = 2;   // 露营检查的时间间隔
    float campThresholdDistance = 1.5f;   // 判断为露营的距离阈值
    float nextCampCheckTime;              // 下一次露营检测时间
    Vector3 campPositionOld;              // 之前的位置，用于比较是否露营
    bool isCamping;                       // 是否处于露营状态

    bool isDisabled;                     // 控制生成器是否禁用

    public event Action<int> OnNewWave;

    void Start()
    {
        // 初始化玩家实体和位置
        playerEntity = FindAnyObjectByType<Player>();  // 查找玩家
        playerT = playerEntity.transform;              // 获取玩家位置

        // 设置露营检查初始时间
        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;  // 保存玩家当前位置
        playerEntity.OnDeath += OnPlayerDeath;  // 注册玩家死亡事件

        // 获取地图生成器
        map = FindAnyObjectByType<MapGenerator>();
        NextWave();  // 开始下一波敌人的生成
    }

    void Update()
    {
        // 如果禁用生成器则不执行生成逻辑
        if (!isDisabled)
        {
            // 处理露营检测
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                // 判断玩家是否停留在原地 (露营)
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;  // 更新玩家位置
            }

            // 如果还有剩余敌人且到了生成时间，生成敌人
            if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine(SpawnEnemy());  // 启动协程生成敌人
            }
        }
    }

    // 协程用于生成敌人
    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;  // 生成延迟时间
        float tileFlashSpeed = 4;  // 瓦片闪烁速度

        // 获取一个随机的空地作为敌人生成位置
        Transform spawnTile = map.GetRandomOpenTile();

        // 如果玩家正在露营，就在玩家当前位置生成敌人
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        // 控制生成位置的闪烁效果
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColour = tileMat.color;
        Color flashColour = Color.red;
        float spawnTimer = 0;

        // 生成前的闪烁效果
        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;  // 计时
            yield return null;
        }

        // 生成敌人并设置位置
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;  // 注册敌人死亡事件
    }

    // 玩家死亡时禁用生成器
    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    // 敌人死亡时减少剩余存活敌人数
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        // 如果这一波敌人已全部消灭，开始下一波
        if (enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void ResetPlayerPosition()
    {
        // 让玩家从中心位置 掉落生成
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up *3;
    }


    // 开始下一波敌人生成
    void NextWave()
    {
        currentWaveNumber++;

        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];  // 获取当前波次的敌人数据

            enemiesRemainingToSpawn = currentWave.enemyCount;  // 设置当前波次敌人数量
            enemiesRemainingAlive = enemiesRemainingToSpawn;  // 设置当前波次剩余敌人数量
            // 执行当前波数？ - 不太理解
            if(OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
        }
        // 这里可以放到切换关卡处
        ResetPlayerPosition();
    }

    // 波次类，包含敌人数量和生成间隔时间
    [System.Serializable]
    public class Wave
    {
        public int enemyCount;           // 每波敌人数量
        public float timeBetweenSpawns;  // 每个敌人之间的生成时间间隔
    }
}
