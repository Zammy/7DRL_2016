using UnityEngine;
using UnityEditor;

namespace RogueLib
{
    [CustomEditor(typeof(TileBehavior))]
    public class TileBehaviorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add Light"))
            {
                var tb = (TileBehavior)target;
                tb.gameObject.AddComponent<LightSource>();
            }
        }
    }
}