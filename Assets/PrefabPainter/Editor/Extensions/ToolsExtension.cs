using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class ToolsExtension
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414

        PrefabPainter gizmo;

        public ToolsExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Tools", GUIStyles.BoxTitleStyle);

            // draw custom components
            if (GUILayout.Button("Remove Container Children"))
            {
                RemoveContainerChildren();
            }

            GUILayout.EndVertical();
        }

        #region Remove Container Children

        public void RemoveContainerChildren()
        {
            GameObject container = gizmo.container as GameObject;

            List<Transform> list = new List<Transform>();
            foreach (Transform child in container.transform)
            {
                list.Add(child);
            }

            foreach (Transform child in list)
            {
                GameObject go = child.gameObject;

                PrefabPainterEditor.DestroyImmediate(go);

            }
        }

        #endregion Remove Container Children

    }
}
