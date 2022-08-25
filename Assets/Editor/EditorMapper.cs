using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorMapperScript))]
public class EditorMapper : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorMapperScript script = (EditorMapperScript) target;

        if(GUILayout.Button("Save map to file"))
        {
            script.SaveCurrentMap();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
