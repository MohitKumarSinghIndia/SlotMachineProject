using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AspectRatioRectTransformAdapter : MonoBehaviour
{
    [Serializable]
    public class AspectPreset
    {
        public bool useThisRatio = true;
        public string presetName;
        public float aspectRatio;

        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale = Vector3.one;
    }

    [Header("Target")]
    [SerializeField] private RectTransform target;

    [Header("Settings")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool autoUpdateOnResolutionChange = true;
    [SerializeField] private bool debugAspect;

    [Header("Aspect Presets")]
    [SerializeField]
    private List<AspectPreset> presets = new List<AspectPreset>
    {
        new AspectPreset { presetName = "16:10", aspectRatio = 16f / 10f },
        new AspectPreset { presetName = "16:9", aspectRatio = 16f / 9f },
        new AspectPreset { presetName = "18:9", aspectRatio = 18f / 9f },
        new AspectPreset { presetName = "19.5:9", aspectRatio = 19.5f / 9f },
        new AspectPreset { presetName = "20:9", aspectRatio = 20f / 9f },
        new AspectPreset { presetName = "21:9", aspectRatio = 21f / 9f }
    };

    private Vector2 lastResolution;

    private void Reset()
    {
        target = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyNearestPreset();

        lastResolution = new Vector2(Screen.width, Screen.height);
    }

    private void Update()
    {
        if (!autoUpdateOnResolutionChange)
            return;

        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        if (currentResolution != lastResolution)
        {
            lastResolution = currentResolution;
            ApplyNearestPreset();
        }
    }

    [ContextMenu("Apply Nearest Preset")]
    public void ApplyNearestPreset()
    {
        AspectPreset preset = GetNearestEnabledPreset();

        if (preset == null || target == null)
            return;

        target.anchoredPosition = preset.anchoredPosition;
        target.sizeDelta = preset.sizeDelta;
        target.localScale = preset.localScale;
    }

    [ContextMenu("Save Current Values To Nearest Ratio")]
    public void SaveCurrentValuesToNearestRatio()
    {
        AspectPreset preset = GetNearestEnabledPreset();

        if (preset == null || target == null)
            return;

        preset.anchoredPosition = target.anchoredPosition;
        preset.sizeDelta = target.sizeDelta;
        preset.localScale = target.localScale;
    }

    private AspectPreset GetNearestEnabledPreset()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (target == null || presets == null || presets.Count == 0)
            return null;

        float width = Mathf.Max(Screen.width, Screen.height);
        float height = Mathf.Min(Screen.width, Screen.height);
        float currentAspect = width / height;

        AspectPreset nearestPreset = null;
        float nearestDifference = float.MaxValue;

        for (int i = 0; i < presets.Count; i++)
        {
            AspectPreset preset = presets[i];

            if (preset == null || !preset.useThisRatio)
                continue;

            float difference = Mathf.Abs(currentAspect - preset.aspectRatio);

            if (difference < nearestDifference)
            {
                nearestPreset = preset;
                nearestDifference = difference;
            }
        }

        if (debugAspect && nearestPreset != null)
        {
            Debug.Log(
                $"[{name}] Screen {Screen.width}x{Screen.height} | Aspect {currentAspect:0.000} | Selected {nearestPreset.presetName}"
            );
        }

        return nearestPreset;
    }
}