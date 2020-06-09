using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneBuilder
{
    public class PreferencesObject : ScriptableObject
    {
        public string PathToBrushes = "Assets/SceneBuilder/Brushes";
        public Texture2D[] DefaultBrushIcons = new Texture2D[2];
        public Texture2D EditorPlayModePreviewIcon;
    }    
}