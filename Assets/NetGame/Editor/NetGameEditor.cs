using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetGameBehaviour))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!EditorApplication.isPlaying)
            return;

        NetGameBehaviour myScript = (NetGameBehaviour)target;
        if(GUILayout.Button("Generate new puzzle"))
            myScript.GenerateNewPuzzle();
    }
}