using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{
    public class BrushModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty brushSize;
        SerializedProperty allowOverlap;
        SerializedProperty alignToTerrain;
        SerializedProperty distribution;
        SerializedProperty poissonDiscSize;

        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414
         
        PrefabPainter gizmo;




        private enum BrushMode
        {
            None,
            Add,
            Remove
        }

        public BrushModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            brushSize = editor.FindProperty( x => x.brushSettings.brushSize);
            alignToTerrain = editor.FindProperty(x => x.brushSettings.alignToTerrain);
            distribution = editor.FindProperty(x => x.brushSettings.distribution);
            poissonDiscSize = editor.FindProperty(x => x.brushSettings.poissonDiscSize);
            allowOverlap = editor.FindProperty(x => x.brushSettings.allowOverlap);

        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Brush settings", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(brushSize, new GUIContent("Brush Size"));
            EditorGUILayout.PropertyField(alignToTerrain, new GUIContent("Align To Terrain"));
            EditorGUILayout.PropertyField(allowOverlap, new GUIContent("Allow Overlap", "Center Mode: Check against brush size.\nPoisson Mode: Check against Poisson Disc size"));

            EditorGUILayout.PropertyField(distribution, new GUIContent("Distribution"));

            if( gizmo.brushSettings.distribution == BrushSettings.Distribution.Poisson)
            {
                //EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(poissonDiscSize, new GUIContent("Poisson Disc Size"));
                //EditorGUI.indentLevel--;
            }


            // consistency check
            float minDiscSize = 0.01f;
            if( poissonDiscSize.floatValue < minDiscSize)
            {
                Debug.LogError("Poisson Disc Size is too small. Setting it to " + minDiscSize);
                poissonDiscSize.floatValue = minDiscSize;
            }

            GUILayout.EndVertical();

        }



        public void OnSceneGUI()
        {
            bool mousePosValid = false;
            Vector3 mousePos = Vector3.zero;

            float radius = gizmo.brushSettings.brushSize / 2f;

            int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            // TODO: raycast hit against layer
            //       see https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                mousePos = hit.point;
                mousePosValid = true;

                ///
                /// process mouse events
                ///

                // control key pressed
                if (Event.current.control)
                {
                    // mouse wheel up/down changes the radius
                    if (Event.current.type == EventType.ScrollWheel)
                    {

                        if (Event.current.delta.y > 0)
                        {
                            gizmo.brushSettings.brushSize++;
                            Event.current.Use();
                        }
                        else if (Event.current.delta.y < 0)
                        {
                            gizmo.brushSettings.brushSize--;

                            // TODO: slider
                            if (gizmo.brushSettings.brushSize < 1)
                                gizmo.brushSettings.brushSize = 1;

                            Event.current.Use();
                        }
                    }

                }

                BrushMode brushMode = BrushMode.None;
                if (Event.current.shift)
                {
                    brushMode = BrushMode.Add;

                    if (Event.current.control)
                    {
                        brushMode = BrushMode.Remove;
                    }

                }

                // draw brush gizmo
                DrawBrush(mousePos, hit.normal, radius, brushMode);

                // paint prefabs on mouse drag
                if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
                {
                    // left button = 0; right = 1; middle = 2
                    if (Event.current.button == 0)
                    {
                        switch (brushMode)
                        {
                            case BrushMode.None:
                                break;
                            case BrushMode.Add:
                                AddPrefabs(hit);
                                break;
                            case BrushMode.Remove:
                                RemovePrefabs(hit.point);
                                break;
                        }
                        
                        Event.current.Use();
                    }
                }
            }
            else
            {
                mousePosValid = false;
            }

            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlId);
            }


            // examples about how to show ui info
            // note: Handles.BeginGUI and EndGUI are important, otherwise the default gizmos aren't drawn
            Handles.BeginGUI();


            if (mousePosValid) {
                ShowHandleInfo( mousePos);
            }

            string[] info = new string[] { "Add prefabs: shift + drag mouse\nRemove prefabs: shift + ctrl + drag mouse\nBrush size: ctrl + mousewheel" ,"Children: " + editor.getContainerChildren().Length };
            PrefabPainterEditor.ShowGuiInfo(info);

            Handles.EndGUI();
        }

        private void DrawBrush( Vector3 position, Vector3 normal, float radius, BrushMode brushMode)
        {
            // set default colors
            Color innerColor = GUIStyles.BrushNoneInnerColor;
            Color outerColor = GUIStyles.BrushNoneOuterColor;

            // set colors depending on brush mode
            switch (brushMode)
            {
                case BrushMode.None:
                    innerColor = GUIStyles.BrushNoneInnerColor;
                    outerColor = GUIStyles.BrushNoneOuterColor;
                    break;
                case BrushMode.Add:
                    innerColor = GUIStyles.BrushAddInnerColor;
                    outerColor = GUIStyles.BrushAddOuterColor;
                    break;
                case BrushMode.Remove:
                    innerColor = GUIStyles.BrushRemoveInnerColor;
                    outerColor = GUIStyles.BrushRemoveOuterColor;
                    break;
            }

            // inner disc
            Handles.color = innerColor;
            Handles.DrawSolidDisc(position, normal, radius);

            // outer circle
            Handles.color = outerColor;
            Handles.DrawWireDisc(position, normal, radius);

            // center line / normal
            float lineLength = radius * 0.5f;
            Vector3 lineStart = position;
            Vector3 lineEnd = position + normal * lineLength;
            Handles.DrawLine(lineStart, lineEnd);


        }

        private void ShowHandleInfo( Vector3 position)
        {
            // example about how to show info at the gizmo
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.blue;
            string text = "Mouse Postion: " + position;
            text += "\n";
            text += "Children: " + editor.getContainerChildren().Length;
            Handles.Label(position, text, style);
        }


        #region Paint Prefabs

        private void AddPrefabs(RaycastHit hit)
        {
            if (!editor.IsEditorSettingsValid())
                return;

            switch (gizmo.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    AddPrefabs_Center(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.Poisson:
                    AddPrefabs_Poisson(hit.point, hit.normal);
                    break;
            }

        }

        /// <summary>
        /// Add prefabs, mode Center
        /// </summary>
        private void AddPrefabs_Center( Vector3 position, Vector3 normal)
        {

            // check if a gameobject is already within the brush size
            // allow only 1 instance per bush size
            GameObject container = gizmo.container as GameObject;


            // check if a prefab already exists within the brush
            bool prefabExists = false;

            // check overlap
            if (!gizmo.brushSettings.allowOverlap)
            {
                float brushRadius = gizmo.brushSettings.brushSize / 2f;

                foreach (Transform child in container.transform)
                {
                    float dist = Vector3.Distance(position, child.transform.position);

                    // check against the brush
                    if (dist <= brushRadius)
                    {
                        prefabExists = true;
                        break;
                    }

                }
            }

            if (!prefabExists)
            {
                AddNewPrefab(position, normal);
            }
        }

        private void AddNewPrefab( Vector3 position, Vector3 normal)
        {

            GameObject container = gizmo.container as GameObject;

            PrefabSettings prefabSettings = this.gizmo.GetPrefabSettings();

            // new prefab
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabSettings.prefab) as GameObject;

            // size
            if (prefabSettings.changeScale)
            {
                instance.transform.localScale = Vector3.one * Random.Range(prefabSettings.scaleMin, prefabSettings.scaleMax);
            }

            // position
            instance.transform.position = position;

            // add offset
            instance.transform.position += prefabSettings.positionOffset;

            // rotation
            Quaternion rotation;
            if (prefabSettings.randomRotation)
            {
                rotation = Random.rotation;
            }
            else if (this.gizmo.brushSettings.alignToTerrain)
            {
                rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }
            else
            {
                rotation = Quaternion.Euler(prefabSettings.rotationOffset);
                //rotation = Quaternion.identity;
            }

            instance.transform.rotation = rotation;

            // attach as child of container
            instance.transform.parent = container.transform;

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
        }

        /// <summary>
        /// Add prefabs, mode Center
        /// </summary>
        private void AddPrefabs_Poisson(Vector3 position, Vector3 normal)
        {
            GameObject container = gizmo.container as GameObject;

            float brushSize = gizmo.brushSettings.brushSize;
            float brushRadius = brushSize / 2.0f;
            float discRadius = gizmo.brushSettings.poissonDiscSize / 2;

            PoissonDiscSampler sampler = new PoissonDiscSampler(brushSize, brushSize, discRadius);

            foreach (Vector2 sample in sampler.Samples()) {

                // brush is currenlty a disc => ensure the samples are within the disc
                if (Vector2.Distance(sample, new Vector2(brushRadius, brushRadius)) > brushRadius)
                    continue;

                // x/z come from the poisson sample 
                float x = position.x + sample.x - brushRadius;
                float z = position.z + sample.y - brushRadius;

                // y depends on the terrain height
                Vector3 terrainPosition = new Vector3(x, position.y, z);

                // get terrain y position
                float y = Terrain.activeTerrain.SampleHeight(terrainPosition);

                Vector3 prefabPosition = new Vector3( x, y, z);

                // check if a prefab already exists within the brush
                bool prefabExists = false;

                // check overlap
                if (!gizmo.brushSettings.allowOverlap)
                {
                    foreach (Transform child in container.transform)
                    {
                        float dist = Vector3.Distance(prefabPosition, child.transform.position);

                        // check against a single poisson disc
                        if (dist <= discRadius)
                        {
                            prefabExists = true;
                            break;
                        }

                    }
                }

                // add prefab
                if( !prefabExists)
                {
                    AddNewPrefab(prefabPosition, normal);
                }
                

            }

           
        }


        /// <summary>
        /// Remove prefabs
        /// </summary>
        private void RemovePrefabs( Vector3 position)
        {

            if (!editor.IsEditorSettingsValid())
                return;

            // check if a gameobject of the container is within the brush size and remove it
            GameObject container = gizmo.container as GameObject;

            float radius = gizmo.brushSettings.brushSize / 2f;

            List<Transform> removeList = new List<Transform>();

            foreach (Transform transform in container.transform)
            {
                float dist = Vector3.Distance(position, transform.transform.position);

                if (dist <= radius)
                {
                    removeList.Add(transform);
                }

            }

            // remove gameobjects
            foreach( Transform transform in removeList)
            {
                PrefabPainter.DestroyImmediate(transform.gameObject);
            }
           
        }

        #endregion Paint Prefabs
    }

}
