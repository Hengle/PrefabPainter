using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PrefabPainter
{
    public class PrefabModuleEditor : ModuleEditorI
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter gizmo;
        #pragma warning restore 0414

        public PrefabModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {

            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("Prefabs", GUIStyles.BoxTitleStyle);

                GUILayout.BeginHorizontal();
                {

                    GUILayout.BeginVertical();
                    {
                        // change background color in case there are no prefabs yet
                        if (gizmo.prefabSettingsList.Count == 0)
                        {
                            editor.SetErrorBackgroundColor();
                        }

                        // drop area
                        Rect prefabDropArea = GUILayoutUtility.GetRect(0.0f, 24.0f, GUIStyles.DropAreaStyle, GUILayout.ExpandWidth(true));
                        GUI.Box(prefabDropArea, "Drop prefabs here in order to use them", GUIStyles.DropAreaStyle);

                        editor.SetDefaultBackgroundColor();

                        Event evt = Event.current;
                        switch (evt.type)
                        {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:

                                if (prefabDropArea.Contains(evt.mousePosition))
                                {

                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                    if (evt.type == EventType.DragPerform)
                                    {
                                        DragAndDrop.AcceptDrag();

                                        // list of new prefabs that should be created via drag/drop
                                        // we can't do it in the drag/drop code itself, we'd get exceptions like
                                        //   ArgumentException: Getting control 12's position in a group with only 12 controls when doing dragPerform. Aborting
                                        // followed by
                                        //   Unexpected top level layout group! Missing GUILayout.EndScrollView/EndVertical/EndHorizontal? UnityEngine.GUIUtility:ProcessEvent(Int32, IntPtr)
                                        // they must be added when everything is done (currently at the end of this method)
                                        editor.newDraggedPrefabs = new List<PrefabSettings>();

                                        foreach (Object droppedObject in DragAndDrop.objectReferences)
                                        {

                                            // allow only prefabs
                                            if (PrefabUtility.GetPrefabType(droppedObject) == PrefabType.None)
                                            {
                                                Debug.Log("Not a gameobject: " + droppedObject);
                                                continue;
                                            }

                                            // new settings
                                            PrefabSettings prefabSettings = new PrefabSettings();

                                            // initialize with dropped prefab
                                            prefabSettings.prefab = droppedObject as GameObject;

                                            editor.newDraggedPrefabs.Add(prefabSettings);

                                        }
                                    }
                                }
                                break;
                        }

                    }


                    GUILayout.EndVertical();

                }

                GUILayout.EndHorizontal();


                for (int i = 0; i < gizmo.prefabSettingsList.Count; i++)
                {
                    if (i > 0)
                        editor.addGUISeparator();

                    PrefabSettings prefabSettings = this.gizmo.prefabSettingsList[i];

                    GUILayout.BeginHorizontal();
                    {
                        // preview
                        // try to get the asset preview
                        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabSettings.prefab);
                        // if no asset preview available, try to get the mini thumbnail
                        if (!previewTexture)
                        {
                            previewTexture = AssetPreview.GetMiniThumbnail(prefabSettings.prefab);
                        }
                        // if a preview is available, paint it
                        if (previewTexture)
                        {
                            //GUILayout.Label(previewTexture, EditorStyles.objectFieldThumb, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size
                            GUILayout.Label(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size

                            //GUILayout.Box(previewTexture); // with border
                            //GUILayout.Label(previewTexture); // no border
                            //GUILayout.Box(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // with border and size
                            //EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), previewTexture); // draws it in absolute coordinates

                        }

                        // right alin the buttons
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Insert(i + 1, new PrefabSettings());
                        }
                        if (GUILayout.Button("Duplicate", EditorStyles.miniButton))
                        {
                            PrefabSettings newPrefabSettings = prefabSettings.Clone();
                            this.gizmo.prefabSettingsList.Insert(i + 1, newPrefabSettings);
                        }
                        if (GUILayout.Button("Reset", EditorStyles.miniButton))
                        {
                            // remove existing
                            this.gizmo.prefabSettingsList.RemoveAt(i);

                            // add new
                            this.gizmo.prefabSettingsList.Insert(i, new PrefabSettings());

                        }
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Remove(prefabSettings);
                        }

                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    prefabSettings.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabSettings.prefab, typeof(GameObject), true);

                    prefabSettings.active = EditorGUILayout.Toggle("Active", prefabSettings.active);
                    prefabSettings.probability = EditorGUILayout.Slider("Probability", prefabSettings.probability, 0, 1);

                    // scale
                    prefabSettings.changeScale = EditorGUILayout.Toggle("Change Scale", prefabSettings.changeScale);

                    if (prefabSettings.changeScale)
                    {
                        prefabSettings.scaleMin = EditorGUILayout.FloatField("Scale Min", prefabSettings.scaleMin);
                        prefabSettings.scaleMax = EditorGUILayout.FloatField("Scale Max", prefabSettings.scaleMax);
                    }

                    // position
                    prefabSettings.positionOffset = EditorGUILayout.Vector3Field("Position Offset", prefabSettings.positionOffset);
                    
                    // rotation
                    prefabSettings.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", prefabSettings.rotationOffset);
                    prefabSettings.randomRotation = EditorGUILayout.Toggle("Random Rotation", prefabSettings.randomRotation);

                    // VS Pro Id
#if VEGETATION_STUDIO_PRO
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("Asset GUID", prefabSettings.assetGUID);
                    EditorGUILayout.TextField("VSPro Id", prefabSettings.vspro_VegetationItemID);
                    EditorGUI.EndDisabledGroup();
#endif
                }
            }

            GUILayout.EndVertical();

        }

        public void OnSceneGUI()
        {
        }

    }
}
