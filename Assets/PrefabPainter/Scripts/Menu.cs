using UnityEditor;
using UnityEngine;

namespace Yapp
{
    /// <summary>
    /// Add custom menu entry:
    /// 
    /// Create -> Yapp -> New PrefabPainter
    /// 
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [MenuItem("GameObject/Yapp/Create PrefabPainter", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // create gameobject
            GameObject go = new GameObject("New PrefabPainter");

            // add the prefabpainter script
            go.AddComponent<PrefabPainter>();

            // ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            // select the new object
            Selection.activeObject = go;
        }
    }

}