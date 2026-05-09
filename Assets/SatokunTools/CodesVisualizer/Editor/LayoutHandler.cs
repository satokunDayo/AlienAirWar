using UnityEditor;
using UnityEngine;

public class LayoutHandler
{
    private NodePositionData _posData;
    private const string AssetPath = "Assets/NodeLayout.asset";

    public LayoutHandler()
    {
        _posData = AssetDatabase.LoadAssetAtPath<NodePositionData>(AssetPath);
        if (_posData == null)
        {
            _posData = ScriptableObject.CreateInstance<NodePositionData>();
            if (!System.IO.File.Exists(AssetPath))
            {
                AssetDatabase.CreateAsset(_posData, AssetPath);
                AssetDatabase.SaveAssets();
            }
        }
    }

    public void SaveNode(string key, Vector2 pos)
    {
        _posData.SavePosition(key, pos);
        EditorUtility.SetDirty(_posData);
    }

    public void SaveGroup(string key, Vector2 pos)
    {
        _posData.SaveFolderPosition(key, pos);
        EditorUtility.SetDirty(_posData);
    }

    public bool TryGetNodePos(string key, out Vector2 pos) => _posData.TryGetPosition(key, out pos);
    public bool TryGetGroupPos(string key, out Vector2 pos) => _posData.TryGetFolderPosition(key, out pos);

    public void SaveAsset() => AssetDatabase.SaveAssets();
}