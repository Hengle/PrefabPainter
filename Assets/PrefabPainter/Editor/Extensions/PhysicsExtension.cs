using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{

    public class PhysicsExtension 
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414

        PrefabPainter gizmo;

        public PhysicsExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            if (this.gizmo.physicsSimulation == null)
            {
                this.gizmo.physicsSimulation = ScriptableObject.CreateInstance<PhysicsSimulation>();
            }
        }

        public void OnInspectorGUI()
        {
            // separator
            GUILayout.BeginVertical("box");
            //addGUISeparator();

            EditorGUILayout.LabelField("Physics Settings", GUIStyles.BoxTitleStyle);

            this.gizmo.physicsSimulation.maxIterations = EditorGUILayout.IntField("Max Iterations", this.gizmo.physicsSimulation.maxIterations);
            this.gizmo.physicsSimulation.forceMinMax = EditorGUILayout.Vector2Field("Force Min/Max", this.gizmo.physicsSimulation.forceMinMax);
            this.gizmo.physicsSimulation.forceAngleInDegrees = EditorGUILayout.FloatField("Force Angle (Degrees)", this.gizmo.physicsSimulation.forceAngleInDegrees);
            this.gizmo.physicsSimulation.randomizeForceAngle = EditorGUILayout.Toggle("Randomize Force Angle", this.gizmo.physicsSimulation.randomizeForceAngle);

            // GUILayout.BeginHorizontal();

            if (GUILayout.Button("Run Simulation"))
            {
                RunSimulation();
            }

            if (GUILayout.Button("Undo Last Simulation"))
            {
                ResetAllBodies();
            }

            // GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #region Physics Simulation

        private void RunSimulation()
        {

            this.gizmo.physicsSimulation.RunSimulation(getContainerChildren());

        }

        private void ResetAllBodies()
        {
            this.gizmo.physicsSimulation.UndoSimulation();
        }

        #endregion Physics Simulation

        // TODO: create common class
        private Transform[] getContainerChildren()
        {
            if (gizmo.container == null)
                return new Transform[0];

            Transform[] children = gizmo.container.transform.Cast<Transform>().ToArray();

            return children;
        }
    }
}
