using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneBuilderEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomBrushEditorAttribute : Attribute
    {
        public string brushType;

        /// <summary>
        /// Mark class as brush editor
        /// </summary>
        /// <param name="t">Target brush type</param>
        public CustomBrushEditorAttribute(Type brushType)
        {
            this.brushType = brushType.Name;
        }
    }
}
