using System;
using UnityEngine;
// 通过在unity设置波数和敌人的生成间隔时间，来控制敌人的生成
public class Spawner : MonoBehaviour
{

    public Wave[] waves;
    public Enemy enemy;
    
    Wave currentWave;
    // 波数
    int currentWaveNumber;
    // 每波敌人数量
    int enemiesRemainingToSpawn;
    // 剩余敌人数量
    int enemiesRemainingAlive;

    // 下一波到来时间
    float nextSpawnTime;

    void Start()
    {
        NextWave();
    }

    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
            // 生成敌人预制体
            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath;
        }
    }
    // 敌人死亡的行为触发了 - 当最后一个敌人死亡时，生成下一波敌人
    private void OnEnemyDeath()
    {
       // 
        enemiesRemainingAlive --;
         if (enemiesRemainingAlive == 0)
         {
              NextWave();
         }
    }

    void NextWave()
    {
        currentWaveNumber++;
        print("Wave: " + currentWaveNumber);
        // waves是自己定义的波数
        if (currentWaveNumber - 1 < waves.Length){
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }

    }

    // 生成敌人的波数 - 序列化类
    [System.Serializable]
    public class Wave
    {
        public int enemyCount; // 该波次敌人的总数
        public float timeBetweenSpawns; // 该波次敌人生成的间隔时间
    }

}

