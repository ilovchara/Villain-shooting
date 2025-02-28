using UnityEditor;
using UnityEngine;
// 在 Unity 编辑器中显示 MapGenerator 组件的界面时被调用。
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator map = target as MapGenerator;
        map.GenerateMap();
    }
}




