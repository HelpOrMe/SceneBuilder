using System;
using System.Collections.Generic;
using UnityEngine;

namespace SceneBuilder
{
    public class ObjectBrush : Brush
    {
        public GameObject selectedObject;
        public bool unpuckObject = false;
        public Vector3 originRotation = Vector3.right * 90f;
        public List<ModelSettings> modelSettings = new List<ModelSettings>();

        public GameObject[] drawObjects;
        public int[] drawObjectsSettings;
        public bool[] drawObjectsUsage;
    }

#pragma warning disable CA2235 // Mark all non-serializable fields
    [Serializable]
    public class ModelSettings
    {
        public string name = "Settings";
        public bool showed = false;

        public bool MoveToMesh = true;
        public bool RotateByMesh = true;

        public RandomOffsetSettings randomPositionOffset = new RandomOffsetSettings();
        public RandomOffsetSettings randomRotationOffset = new RandomOffsetSettings();
        public RandomOffsetSettings randomScaleOffset = new RandomOffsetSettings();

        public OffsetSettings positionOffset = new OffsetSettings();
        public OffsetSettings rotationOffset = new OffsetSettings();
        public OffsetSettings scaleOffset = new OffsetSettings();

        public bool RandomSet = false;
        public float RandomSetPercent = 1f;
        public bool useEnemyColliders = false;
        public string[] enemyColliderNames = Array.Empty<string>();
    }

    public class RandomOffsetSettings
    {
        public bool useOffset = false;
        public RandomType randomType = RandomType.Range;
        public Vector3 randomRangesMin = -Vector3.one;
        public Vector3 randomRangesMax = Vector3.one;
        public bool randomArrayShowed = false;
        public Vector3[] randomArray = Array.Empty<Vector3>();
    }

    public class OffsetSettings
    {
        public bool useOffset = false;
        public Vector3 offset;
    }
#pragma warning restore CA2235 // Mark all non-serializable fields

    public enum RandomType
    {
        Range,
        Between
    }
}
