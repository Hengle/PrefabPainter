using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    /// <summary>
    /// Interface for the editor modules
    /// </summary>
    public interface ModuleEditorI
    {

        void OnInspectorGUI();

        void OnSceneGUI();

    }
}
