using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AspectRatioRectTransformAdapter))]
public class AspectRatioRectTransformAdapterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AspectRatioRectTransformAdapter adapter =
            (AspectRatioRectTransformAdapter)target;

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Apply Nearest Preset"))
        {
            adapter.ApplyNearestPreset();
            EditorUtility.SetDirty(adapter);
        }

        GUI.backgroundColor = Color.yellow;

        if (GUILayout.Button("Save Current Values To Nearest Ratio"))
        {
            adapter.SaveCurrentValuesToNearestRatio();
            EditorUtility.SetDirty(adapter);
        }

        GUI.backgroundColor = Color.white;
    }
}