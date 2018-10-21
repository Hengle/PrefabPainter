using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class CopyPasteExtension
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414

        PrefabPainter gizmo;

        public CopyPasteExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Copy/Paste", GUIStyles.BoxTitleStyle);

            // transform copy/paste

            // GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy Transforms"))
            {
                CopyTransforms();
            }
            else if (GUILayout.Button("Paste Transforms"))
            {
                PasteTransforms();
            }

            // GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Use in combination with Physics to revert to another state than the previous one.", MessageType.Info);

            GUILayout.EndVertical();
        }

        #region Copy/Paste Transforms

        private void CopyTransforms()
        {
            gizmo.copyPasteGeometryMap.Clear();

            GameObject container = gizmo.container as GameObject;

            foreach (Transform child in container.transform)
            {
                GameObject go = child.gameObject;

                if (go == null)
                    continue;

                gizmo.copyPasteGeometryMap.Add(go.GetInstanceID(), new Geometry(go.transform));

            }

            // logging
            Debug.Log("Copying transforms & rotations: " + gizmo.copyPasteGeometryMap.Keys.Count);
        }


        private void PasteTransforms()
        {
            // logging
            Debug.Log("Pasting transforms & rotations: " + gizmo.copyPasteGeometryMap.Keys.Count);

            GameObject container = gizmo.container as GameObject;

            foreach (Transform child in container.transform)
            {
                GameObject go = child.gameObject;

                if (go == null)
                    continue;

                Geometry geometry = null;

                if (gizmo.copyPasteGeometryMap.TryGetValue(go.GetInstanceID(), out geometry))
                {
                    go.transform.position = geometry.getPosition();
                    go.transform.rotation = geometry.getRotation();
                }

            }
        }

        #endregion Copy/Paste Transforms
    }
}
