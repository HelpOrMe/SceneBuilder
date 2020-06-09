using SceneBuilder;
using SceneBuilder.Windows;

namespace SceneBuilderEditor
{
    public class BrushEditor
    {
        public Brush brush { get; private set; }
        public BrushWindow window { get; private set; }

        public virtual void Init(Brush brush, BrushWindow window)
        {
            this.brush = brush;
            this.window = window;
        }

        public virtual void OnGUI() { }

        public virtual void OnDisable() { }
    }
}
