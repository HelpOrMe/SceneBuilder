using SceneBuilder;
using UnityEngine;
using UnityEditor.EditorTools;

namespace SceneBuilder
{
    public class BrushTool : EditorTool
    {
        public static Brush brush;
        public virtual Texture2D BrushIcon { get; }

        public override GUIContent toolbarIcon
        {
            get => new GUIContent()
            {
                image = IsAvailable() && BrushIcon != null ? BrushIcon : PreferencesProvider.GetPreferences().DefaultBrushIcons[0],
                text = IsAvailable() ? brush.GetType().Name : "No selected brushes",
                tooltip = IsAvailable() ? "SceneBuilder tool" : "Select brush in Brush Window"
            };
        }

        public override bool IsAvailable()
        {
            return brush != null;
        }
    }
}
