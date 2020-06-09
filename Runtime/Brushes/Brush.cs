using System.Collections.Generic;
using UnityEngine;

namespace SceneBuilder
{
    public abstract class Brush : ScriptableObject 
    {
        public readonly List<Brush> subBrushes = new List<Brush>();
    }
}
