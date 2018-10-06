using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PrefabPainter
{

    public class PhysicsExtension 
    {
        PrefabPainter gizmo;

        public PhysicsExtension(PrefabPainter gizmo)
        {
            this.gizmo = gizmo;

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

            if (GUILayout.Button("Run Simulation"))
            {
                RunSimulation();
            }

            if (GUILayout.Button("Undo Last Simulation"))
            {
                ResetAllBodies();
            }

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
