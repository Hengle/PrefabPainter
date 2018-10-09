using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PrefabPainter
{
    /// <summary>
    /// Prefab painter
    /// 
    /// Notes:
    /// [ExecuteInEditMode] has become necessary because of the usage of splineModule in OnDrawGizmos
    /// </summary>
    [ExecuteInEditMode]
    public class PrefabPainter : MonoBehaviour
    {

        public enum Mode { Paint, Spline, Container }

        /// <summary>
        /// The parent of the instantiated prefabs 
        /// </summary>
        [HideInInspector]
        public GameObject container;

        [HideInInspector]
        public Mode mode;

        /// <summary>
        /// The diameter of the brush
        /// </summary>
        [HideInInspector]
        public float brushSize = 2.0f;

        /// <summary>
        /// The prefab that will be instantiated
        /// </summary>
        [HideInInspector]
        public List<PrefabSettings> prefabSettingsList;

        /// <summary>
        /// Instance of PhysicsSimulation.
        /// Keeping the object here allows us to navigate away from the PrefabPainter gameobject
        /// and return to it and keep the phyics settings. Otherwise the physics settings would always be reset
        /// </summary>
        [HideInInspector]
        public PhysicsSimulation physicsSimulation;

        /// <summary>
        /// Container for copied positions and rotations
        /// </summary>
        [HideInInspector]
        public Dictionary<int, Geometry> copyPasteGeometryMap = new Dictionary<int, Geometry>();

        /// <summary>
        /// Settings of the spline curve
        /// </summary>
        [HideInInspector]
        public SplineSettings splineSettings = new SplineSettings();


        public SplineModule splineModule = null;

        void OnEnable()
        {
            // create initial prefab
            prefabSettingsList = new List<PrefabSettings>();

            // note: PrefabPainter.PhysicsSimulation must be instantiated using the ScriptableObject.CreateInstance method instead of new PhysicsSimulation.
            // see PhysicsExtension class. this won't work: physicsSimulation = new PhysicsSimulation();

            splineModule = new SplineModule(this);

        }

        void OnDrawGizmos()
        {
            splineModule.OnDrawGizmos();

        }

        /// <summary>
        /// Get a random active prefab setting from the prefab settings list, depending on the probability.
        /// </summary>
        /// <returns></returns>
        public PrefabSettings GetRandomWeightedPrefab()
        {

            if (prefabSettingsList.Count == 0)
                return null;

            float weight;
            float totalSum = 0;

            foreach (var item in prefabSettingsList)
            {
                if (!item.active)
                    continue;

                totalSum += item.probability;

            }

            float random = Random.value;
            float bound = 0f;

            foreach (var item in prefabSettingsList)
            {
                if (!item.active)
                    continue;

                weight = item.probability;

                if( weight <= 0f)
                    continue;

                bound += weight / totalSum;

                if (bound >= random)
                    return item;
            }

            return null;
        }

        public PrefabSettings GetPrefabSettings()
        {

            PrefabSettings selectedItem = GetRandomWeightedPrefab();

            if ( selectedItem == null)
            {
                Debug.LogError("No prefab is active! At least 1 prefab must be active. Using first one");
                selectedItem = prefabSettingsList[0];
            }

            return selectedItem;

        }
    }
}