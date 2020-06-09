using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SceneBuilder
{
    public static class SceneBuilderUtility
    {
        public static string CreatePath(string path)
        {
            string[] splittedPath = path.Split('/');
            string createdPath = splittedPath[0];
            string guid = null;

            for (int i = 1; i < splittedPath.Length; i++)
            {
                if (!string.IsNullOrEmpty(splittedPath[i]) && 
                    !AssetDatabase.IsValidFolder($"{createdPath}/{splittedPath[i]}"))
                {
                    guid = AssetDatabase.CreateFolder(createdPath, splittedPath[i]);
                    createdPath = AssetDatabase.GUIDToAssetPath(guid);
                }
            }
            return guid;
        }
    }
}
