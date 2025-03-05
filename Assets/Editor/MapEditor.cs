using UnityEditor;
using UnityEngine;
// 这个脚本让 MapGenerator 在 Inspector 修改时自动更新，非常适合需要频繁调整参数的地图生成系统。。
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {


        MapGenerator map = target as MapGenerator;

        if (DrawDefaultInspector())
        {
            map.GenerateMap();
        }

        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }


    }


}




