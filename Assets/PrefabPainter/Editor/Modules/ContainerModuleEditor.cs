using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class ContainerModuleEditor: ModuleEditorI
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter gizmo;
        #pragma warning restore 0414
        
        public ContainerModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Container", GUIStyles.BoxTitleStyle);

            EditorGUILayout.HelpBox("Perform operations on the container children", MessageType.Info);

            GUILayout.EndVertical();

        }

        public void OnSceneGUI()
        {
        }
    }
}
