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
        if (GUILayout.Button("Generate new puzzle"))
            myScript.GenerateNewPuzzle();
        if (GUILayout.Button("Reset puzzle"))
            myScript.ResetPuzzle();
        if (GUILayout.Button("Picked up part"))
            myScript.OnPickedUpPart();
        if (GUILayout.Button("Start game"))
            myScript.StartGame();
    }
}