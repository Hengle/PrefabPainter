using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class ToolsExtension
    {
        PrefabPainter gizmo;

        public ToolsExtension(PrefabPainter gizmo)
        {
            this.gizmo = gizmo;
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

        private void RemoveContainerChildren()
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
