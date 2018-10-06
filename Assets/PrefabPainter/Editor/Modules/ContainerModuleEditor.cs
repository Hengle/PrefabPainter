using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class ContainerModuleEditor
    {

        PrefabPainter gizmo;

        public ContainerModuleEditor(PrefabPainter gizmo)
        {
            this.gizmo = gizmo;
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
