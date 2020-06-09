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

        public bool useRandomPositionOffset = false;
        public RandomType randomPositionType = RandomType.Range;
        public Vector3 randomPositionRangesMin = -Vector3.one;
        public Vector3 randomPositionRangesMax = Vector3.one;
        public bool randomPositionsShowed = false;
        public Vector3[] randomPositions = Array.Empty<Vector3>();

        public bool useRandomRotationOffset = false;
        public RandomType randomRotationType = RandomType.Range;
        public Vector3 randomRotationRangesMin = -Vector3.one * 360;
        public Vector3 randomRotationRangesMax = Vector3.one * 360;
        public bool randomRotationsShowed = false;
        public Vector3[] randomRotations = Array.Empty<Vector3>();

        public bool useRotationOffset = false;
        public Vector3 rotationOffset = Vector3.zero;

        public bool usePositionOffset = false;
        public Vector3 positionOffset = Vector3.zero;

        public bool RandomSet = false;
        public float RandomSetPercent = 1f;
        public bool useEnemyColliders = false;
        public string[] enemyColliderNames = Array.Empty<string>();
    }
#pragma warning restore CA2235 // Mark all non-serializable fields

    public enum RandomType
    {
        Range,
        Between
    }
}
