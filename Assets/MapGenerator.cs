using UnityEngine;
using UnityEngine.Rendering;
// 一个代码一个职责
public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;

    [System.Obsolete]
    void Start()
    {
        GenerateMap();
    }

    // 使用for循环 - 构造地图
    [System.Obsolete]
    public void GenerateMap()
    {
        // 定义一个字符串，表示要查找或创建的空物体的名字
        string holderName = "Generated Map";

        // 查找当前对象下是否已经存在一个名为 "Generated Map" 的子物体
        if (transform.FindChild(holderName))
        {
            // 如果找到了名为 "Generated Map" 的子物体，则销毁它（直接在编辑模式中销毁）
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        // 创建一个新的空物体，并命名为 "Generated Map"
        Transform mapHolder = new GameObject(holderName).transform;

        // 将新创建的空物体作为当前脚本物体的子物体
        mapHolder.parent = transform;
        
        // 遍历地图的每一行（x轴）
        for (int x = 0; x < mapSize.x; x++)
        {
            // 遍历地图的每一列（y轴）
            for (int y = 0; y < mapSize.y; y++)
            {
                // 计算每个瓷砖的位置：
                // 将地图中心设置为(0,0)，并根据行列计算每个瓷砖的世界坐标
                // -mapSize.x / 2 + 0.5f 是让地图的中心对齐坐标系的原点
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);

                // 实例化一个新的瓷砖对象（tilePrefab）并设置位置和旋转
                // Quaternion.Euler(Vector3.right * 90) 让瓷砖绕X轴旋转90度
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                // 将新创建的瓷砖设置为当前对象的子物体（在层级中组织结构）
                // 赋予整体地图间隔 - 自定义比例
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;

            }
        }
    }



}
