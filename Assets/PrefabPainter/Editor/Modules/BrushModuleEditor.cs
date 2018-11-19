using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationStudio;
#endif

namespace PrefabPainter
{
    public class BrushModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty brushSize;
        SerializedProperty brushRotation;
        SerializedProperty allowOverlap;
        SerializedProperty alignToTerrain;
        SerializedProperty distribution;
        SerializedProperty poissonDiscSize;
        SerializedProperty fallOffCurve;
        SerializedProperty fallOff2dCurveX;
        SerializedProperty fallOff2dCurveZ;
        SerializedProperty curveSamplePoints;
        SerializedProperty spawnToVSPro;

        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414
         
        PrefabPainter gizmo;


        private bool debug = false;

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
            brushRotation = editor.FindProperty(x => x.brushSettings.brushRotation);

            alignToTerrain = editor.FindProperty(x => x.brushSettings.alignToTerrain);
            distribution = editor.FindProperty(x => x.brushSettings.distribution);
            poissonDiscSize = editor.FindProperty(x => x.brushSettings.poissonDiscSize);
            fallOffCurve = editor.FindProperty(x => x.brushSettings.fallOffCurve);
            fallOff2dCurveX = editor.FindProperty(x => x.brushSettings.fallOff2dCurveX);
            fallOff2dCurveZ = editor.FindProperty(x => x.brushSettings.fallOff2dCurveZ);
            curveSamplePoints = editor.FindProperty(x => x.brushSettings.curveSamplePoints);
            allowOverlap = editor.FindProperty(x => x.brushSettings.allowOverlap);

            spawnToVSPro = editor.FindProperty(x => x.brushSettings.spawnToVSPro);
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Brush settings", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(brushSize, new GUIContent("Brush Size"));
            EditorGUILayout.PropertyField(brushRotation, new GUIContent("Brush Rotation"));

            EditorGUILayout.PropertyField(alignToTerrain, new GUIContent("Align To Terrain"));
            EditorGUILayout.PropertyField(allowOverlap, new GUIContent("Allow Overlap", "Center Mode: Check against brush size.\nPoisson Mode: Check against Poisson Disc size"));

            EditorGUILayout.PropertyField(distribution, new GUIContent("Distribution"));

            switch (gizmo.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    break;
                case BrushSettings.Distribution.Poisson:
                    //EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(poissonDiscSize, new GUIContent("Poisson Disc Size"));
                    //EditorGUI.indentLevel--;
                    break;
                case BrushSettings.Distribution.FallOff:
                    EditorGUILayout.PropertyField(curveSamplePoints, new GUIContent("Curve Sample Points"));
                    EditorGUILayout.PropertyField(fallOffCurve, new GUIContent("FallOff"));
                    break;
                case BrushSettings.Distribution.FallOff2d:
                    EditorGUILayout.PropertyField(curveSamplePoints, new GUIContent("Curve Sample Points"));
                    EditorGUILayout.PropertyField(fallOff2dCurveX, new GUIContent("FallOff X"));
                    EditorGUILayout.PropertyField(fallOff2dCurveZ, new GUIContent("FallOff Z"));
                    break;
            }

            // TODO: how to create a minmaxslider with propertyfield?
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Slope");
            EditorGUILayout.MinMaxSlider(ref gizmo.brushSettings.slopeMin, ref gizmo.brushSettings.slopeMax, gizmo.brushSettings.slopeMinLimit, gizmo.brushSettings.slopeMaxLimit);
            EditorGUILayout.EndHorizontal();

            #if VEGETATION_STUDIO_PRO
                EditorGUILayout.PropertyField(spawnToVSPro, new GUIContent("Spawn to VS Pro"));
            #endif

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
                        // ctrl + shift + scroll = brush rotation
                        if( Event.current.shift)
                        {
                            int rotationStepSize = 10;
                            int rotationMin = 0; // TODO: find out of to get that from Range
                            int rotationMax = 360; // TODO: find out of to get that from Range

                            // scroll up
                            if (Event.current.delta.y > 0)
                            {
                                gizmo.brushSettings.brushRotation+= rotationStepSize;
                                if (gizmo.brushSettings.brushRotation > rotationMax)
                                {
                                    gizmo.brushSettings.brushRotation = rotationMin + rotationStepSize;
                                }
                                Event.current.Use();
                            }
                            // scroll down
                            else if (Event.current.delta.y < 0)
                            {
                                gizmo.brushSettings.brushRotation -= rotationStepSize;
                                if (gizmo.brushSettings.brushRotation < rotationMin) { 
                                    gizmo.brushSettings.brushRotation = rotationMax - rotationStepSize;
                                }
                                Event.current.Use();
                            }
                        }
                        // ctrl + scroll = brush size
                        else
                        {
                            // scroll up
                            if (Event.current.delta.y > 0)
                            {
                                gizmo.brushSettings.brushSize++;
                                Event.current.Use();
                            }
                            // scroll down
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

            string[] info = new string[] { "Add prefabs: shift + drag mouse\nRemove prefabs: shift + ctrl + drag mouse\nBrush size: ctrl + mousewheel, Brush rotation: ctrl + shift + mousewheel" ,"Children: " + editor.getContainerChildren().Length };
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



            // consider distribution
            switch (gizmo.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center: // fallthrough
                case BrushSettings.Distribution.Poisson:
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

                    break;

                case BrushSettings.Distribution.FallOff:

                    // use same curve for x and z
                    AnimationCurve fallOffCurve = gizmo.brushSettings.fallOffCurve;
                    DrawCurveBrushSamplePoints(position, normal, innerColor, outerColor, fallOffCurve, fallOffCurve);

                    // alternate version: draw rings
                    // DrawCurveBrushSampleRings(position, normal, radius, innerColor, outerColor);

                    break;

                case BrushSettings.Distribution.FallOff2d:
                    AnimationCurve fallOff2dCurveX = gizmo.brushSettings.fallOff2dCurveX;
                    AnimationCurve fallOff2dCurveZ = gizmo.brushSettings.fallOff2dCurveZ;
                    //DrawCurveBrushSamplePoints( position, normal, innerColor, outerColor, fallOff2dCurveX, fallOff2dCurveZ);
                    DrawCurveBrushSamplePointsAsGrid(position, normal, innerColor, outerColor, fallOff2dCurveX, fallOff2dCurveZ);
                    break;
            }


        }

        /// <summary>
        /// Draw rings with alpha value set to the curve value
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="radius"></param>
        /// <param name="innerColor"></param>
        /// <param name="outerColor"></param>
        private void DrawCurveBrushSampleRings(Vector3 position, Vector3 normal, float radius, Color innerColor, Color outerColor)
        {
            // number of sample points in 1 direction, i. e. there will be n * n sample points
            int samplePointsPerRow = gizmo.brushSettings.curveSamplePoints;

            // the sample point distance on a [0,1] range, i. e. for 10 the distance will be 0.1
            float samplePointDistanceNormalized = 1f / samplePointsPerRow;

            AnimationCurve curve = gizmo.brushSettings.fallOffCurve;
            for (var t = 0f; t <= 1f; t += samplePointDistanceNormalized)
            {
                float curvePoint = curve.Evaluate(t);

                // ensure value is [0,1]
                curvePoint = Mathf.Clamp01(curvePoint);

                Handles.color = new Color(innerColor.r, innerColor.g, innerColor.b, curvePoint);

                Handles.DrawWireDisc(position, normal, radius * t);

            }
        }

        // TODO: just a testing function with discs
        private void DrawCurveBrushSamplePoints(Vector3 position, Vector3 normal, Color innerColor, Color outerColor, AnimationCurve curveX, AnimationCurve curveZ)
        {
            // number of sample points in 1 direction, i. e. there will be n * n sample points
            int samplePointsPerRow = gizmo.brushSettings.curveSamplePoints;

            // the sample point distance on a [0,1] range, i. e. for 10 the distance will be 0.1
            float samplePointDistanceNormalized = 1f / samplePointsPerRow;

            for (var x = 0f; x <= 1f; x += samplePointDistanceNormalized)
            {
                for (var z = 0f; z <= 1f; z += samplePointDistanceNormalized)
                {
                    float curvePointX = curveX.Evaluate(x);
                    float curvePointZ = curveZ.Evaluate(z);

                    // ensure value is [0,1]
                    curvePointX = Mathf.Clamp01(curvePointX);
                    curvePointZ = Mathf.Clamp01(curvePointZ);

                    float discSize = gizmo.brushSettings.brushSize * x; // is same as y

                    Handles.color = new Color(innerColor.r, innerColor.g, innerColor.b, curvePointX * curvePointZ);

                    float radius = gizmo.brushSettings.brushSize * samplePointDistanceNormalized * 0.5f;

                    // TODO: align depending on brush size
                    float xPosition = position.x - gizmo.brushSettings.brushSize * (x - 0.5f) - radius;
                    float zPosition = position.z - gizmo.brushSettings.brushSize * (z - 0.5f) - radius;

                    // high enough offset for y, in case the terrain below the brush aligned in it's normal direction isn't flat
                    // otherwise parts might be above terrain while others might be below it; another way would be to do an additional up raycast
                    float yRaystOffset = 3000f;
                    float yPosition = position.y + yRaystOffset;

                    // individual disc position, but with y offset
                    Vector3 discPosition = new Vector3(xPosition, yPosition, zPosition);

                    // y via raycast down
                    // TODO: raycast hit against layer
                    //       see https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
                    RaycastHit hit;
                    if (Physics.Raycast(discPosition, Vector3.down, out hit, Mathf.Infinity))
                    {
                        // set y position depending on the terrain
                        discPosition.y = hit.point.y;

                        // set the normal depending on the terrain
                        normal = hit.normal;

                    }

                    // y via height sampling
                    // discPosition.y = Terrain.activeTerrain.SampleHeight(discPosition);

                    Handles.DrawSolidDisc(discPosition, normal, radius);

                }

            }
        }

        // TODO: just a testing function with rectangles
        private void DrawCurveBrushSamplePointsAsGrid(Vector3 position, Vector3 normal, Color innerColor, Color outerColor, AnimationCurve curveX, AnimationCurve curveZ)
        {
            // number of sample points in 1 direction, i. e. there will be n * n sample points
            int samplePointsPerRow = gizmo.brushSettings.curveSamplePoints;

            // the sample point distance on a [0,1] range, i. e. for 10 the distance will be 0.1
            float samplePointDistanceNormalized = 1f / samplePointsPerRow;

            Vector3[,] v = new Vector3[samplePointsPerRow, samplePointsPerRow];
            Color[,] c = new Color[samplePointsPerRow, samplePointsPerRow];

            int i;
            int j;
            for ( i = 0; i < samplePointsPerRow; i++)
            {
                for ( j = 0; j < samplePointsPerRow; j++)
                {

                    float x = i * samplePointDistanceNormalized;
                    float z = j * samplePointDistanceNormalized;

                    float curvePointX = curveX.Evaluate(x);
                    float curvePointZ = curveZ.Evaluate(z);

                    // ensure value is [0,1]
                    curvePointX = Mathf.Clamp01(curvePointX);
                    curvePointZ = Mathf.Clamp01(curvePointZ);

                    float discSize = gizmo.brushSettings.brushSize * x; // is same as y

                    Handles.color = new Color(innerColor.r, innerColor.g, innerColor.b, curvePointX * curvePointZ);

                    float radius = gizmo.brushSettings.brushSize * samplePointDistanceNormalized * 0.5f;

                    // TODO: align depending on brush size
                    float xPosition = position.x - gizmo.brushSettings.brushSize * (x - 0.5f) - radius;
                    float zPosition = position.z - gizmo.brushSettings.brushSize * (z - 0.5f) - radius;

                    // high enough offset for y, in case the terrain below the brush aligned in it's normal direction isn't flat
                    // otherwise parts might be above terrain while others might be below it; another way would be to do an additional up raycast
                    float yRaystOffset = 3000f;
                    float yPosition = position.y + yRaystOffset;

                    // individual disc position, but with y offset
                    Vector3 discPosition = new Vector3(xPosition, yPosition, zPosition);

                    // rotate around y world axis
                    float angle = gizmo.brushSettings.brushRotation;
                    discPosition -= position; // move to origin
                    discPosition = Quaternion.Euler(0, angle, 0) * discPosition; // rotate around world axis
                    discPosition += position; // move back to position

                    // y via raycast down
                    // TODO: raycast hit against layer
                    //       see https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
                    RaycastHit hit;
                    if (Physics.Raycast(discPosition, Vector3.down, out hit, Mathf.Infinity))
                    {
                        // set y position depending on the terrain
                        discPosition.y = hit.point.y;

                        // set the normal depending on the terrain
                        normal = hit.normal;

                    }

                    // y via height sampling
                    // discPosition.y = Terrain.activeTerrain.SampleHeight(discPosition);

                    v[i, j] = discPosition;
                    c[i, j] = Handles.color;

                    // slope
                    float slopeAngle = Vector3.Angle(normal.normalized, new Vector3(0, 1, 0));
                    //Handles.Label(discPosition, new GUIContent("angle: " + slopeAngle));

                    // if brush area isn't inside the slope range, make the color almost transparent
                    if( slopeAngle < gizmo.brushSettings.slopeMin || slopeAngle > gizmo.brushSettings.slopeMax)
                    {
                        c[i, j].a = 0.05f;
                    }
                }
            }


            for ( i = 0; i < v.GetLength(0) - 1; i++)
            {
                for ( j = 0; j < v.GetLength(1) - 1; j++)
                {

                    Vector3[] verts = new Vector3[]
                    {
                                    v[i,j],
                                    v[i,j+1],
                                    v[i+1,j+1],
                                    v[i+1,j],
                    };

                    Handles.DrawSolidRectangleWithOutline(verts, c[i,j], new Color(0, 0, 0, c[i,j].a));
                }
            }
        }

        private void ShowHandleInfo( Vector3 position)
        {
            if (debug)
            {
                // example about how to show info at the gizmo
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.blue;
                string text = "Mouse Postion: " + position;
                text += "\n";
                text += "Children: " + editor.getContainerChildren().Length;
                Handles.Label(position, text, style);
            }
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
                case BrushSettings.Distribution.FallOff:
                    Debug.Log("Not implemented yet: " + gizmo.brushSettings.distribution);
                    break;
                case BrushSettings.Distribution.FallOff2d:
                    Debug.Log("Not implemented yet: " + gizmo.brushSettings.distribution);
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

        /// <summary>
        /// Ensure the prefab has a VegetationItemID
        /// </summary>
        /// <param name="prefabSettings"></param>
        private void updateVSProSettings(PrefabSettings prefabSettings, bool forceVegetationItemIDUpdate)
        {
#if VEGETATION_STUDIO_PRO

            GameObject prefab = prefabSettings.prefab;

            // check if we have a VegetationItemID, otherwise create it using the current prefab
            if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID) || forceVegetationItemIDUpdate)
            {
                // get the asset guid
                if (string.IsNullOrEmpty(prefabSettings.assetGUID))
                {
                    string assetPath = AssetDatabase.GetAssetPath(prefab);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                        prefabSettings.assetGUID = assetGUID;
                    }
                }

                // if we have a guid, get the vs pro id
                if (!string.IsNullOrEmpty(prefabSettings.assetGUID))
                {
                    // get the VegetationItemID
                    prefabSettings.vspro_VegetationItemID = VegetationStudioManager.GetVegetationItemID(prefabSettings.assetGUID);

                    // if the vegetation item id doesn't exist, create a new vegetation item
                    if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID))
                    {
                        VegetationType vegetationType = VegetationType.Objects;
                        bool enableRuntimeSpawn = false; // no runtime spawn, we want it spawned from persistent storage
                        BiomeType biomeType = BiomeType.Default;

                        prefabSettings.vspro_VegetationItemID = VegetationStudioManager.AddVegetationItem(prefab, vegetationType, enableRuntimeSpawn, biomeType);
                    }

                }
                else
                {
                    Debug.LogError("Can't get assetGUID for prefab " + prefab);
                }
            }

            if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID))
            {
                Debug.LogError("Can't get VegetationItemId for prefab " + prefab);
            }
#endif
        }

        private void AddNewPrefab( Vector3 position, Vector3 normal)
        {

            GameObject container = gizmo.container as GameObject;

            PrefabSettings prefabSettings = this.gizmo.GetPrefabSettings();

            GameObject prefab = prefabSettings.prefab;

            ///
            /// Calculate position / rotation / scale
            /// 

            // get new position
            Vector3 newPosition = position;

            // add offset
            newPosition += prefabSettings.positionOffset;

            Vector3 newLocalScale = prefabSettings.prefab.transform.localScale;

            // size
            if (prefabSettings.changeScale)
            {
                newLocalScale = Vector3.one * Random.Range(prefabSettings.scaleMin, prefabSettings.scaleMax);
            }

            // rotation
            Quaternion newRotation;
            if (prefabSettings.randomRotation)
            {
                newRotation = Random.rotation;
            }
            else if (this.gizmo.brushSettings.alignToTerrain)
            {
                newRotation = Quaternion.FromToRotation(Vector3.up, normal);
            }
            else
            {
                newRotation = Quaternion.Euler(prefabSettings.rotationOffset);
                //rotation = Quaternion.identity;
            }

            ///
            /// create instance and apply position / rotation / scale
            /// 

            // spawn item to vs pro
            if ( gizmo.brushSettings.spawnToVSPro)
            {
#if VEGETATION_STUDIO_PRO

                // ensure the prefab has a VegetationItemID
                updateVSProSettings( prefabSettings, true);

                if( !string.IsNullOrEmpty( prefabSettings.vspro_VegetationItemID))
                {
                    string vegetationItemID = prefabSettings.vspro_VegetationItemID;
                    Vector3 worldPosition = position;
                    Vector3 scale = newLocalScale; // TODO local or world?
                    Quaternion rotation = newRotation;
                    bool applyMeshRotation = true; // TODO ???
                    byte vegetationSourceID = 5; // TODO see PersistentVegetationStorageTools for constants. 5 = "Vegetation Studio - Painted"
                    float distanceFalloff = 1f; // TODO ???
                    bool clearCellCache = true; // TODO ???

                    VegetationStudioManager.AddVegetationItemInstance(vegetationItemID, worldPosition, scale, rotation, applyMeshRotation, vegetationSourceID, distanceFalloff, clearCellCache);

                }
#endif
            }
            // spawn item to scene
            else
            {

                // new prefab
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                instance.transform.position = newPosition;
                instance.transform.rotation = newRotation;
                instance.transform.localScale = newLocalScale;

                // attach as child of container
                instance.transform.parent = container.transform;

                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");

            }

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
