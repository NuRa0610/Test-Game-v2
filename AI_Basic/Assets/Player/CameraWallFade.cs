using System.Collections.Generic;
using UnityEngine;

public class CameraWallFade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _camera;

    [Header("Occlusion")]
    [SerializeField] private LayerMask _occluderLayers;
    [SerializeField, Range(0.05f, 1f)] private float _occludedAlpha = 0.35f;
    [SerializeField] private float _fadeSpeed = 10f;
    [SerializeField] private float _rayPadding = 0.15f;

    private readonly Dictionary<Renderer, FadeState> _fadeStates = new Dictionary<Renderer, FadeState>();
    private readonly HashSet<Renderer> _occludingThisFrame = new HashSet<Renderer>();

    private class FadeState
    {
        public Material[] Materials;
        public float[] OriginalAlphas;
        public float CurrentAlpha = 1f;
        public float TargetAlpha = 1f;
    }

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (_target == null || _camera == null)
        {
            return;
        }

        _occludingThisFrame.Clear();

        Vector3 cameraPosition = _camera.transform.position;
        Vector3 targetPosition = _target.position;
        Vector3 direction = cameraPosition - targetPosition;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            targetPosition,
            direction.normalized,
            distance + _rayPadding,
            _occluderLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Renderer hitRenderer = hits[i].collider.GetComponentInParent<Renderer>();

            if (hitRenderer == null)
            {
                continue;
            }

            _occludingThisFrame.Add(hitRenderer);

            if (!_fadeStates.TryGetValue(hitRenderer, out FadeState state))
            {
                state = CreateFadeState(hitRenderer);
                if (state == null)
                {
                    continue;
                }

                _fadeStates.Add(hitRenderer, state);
            }

            state.TargetAlpha = _occludedAlpha;
        }

        List<Renderer> toRemove = null;

        foreach (KeyValuePair<Renderer, FadeState> pair in _fadeStates)
        {
            Renderer renderer = pair.Key;
            FadeState state = pair.Value;

            if (renderer == null)
            {
                if (toRemove == null)
                {
                    toRemove = new List<Renderer>();
                }

                toRemove.Add(pair.Key);
                continue;
            }

            if (!_occludingThisFrame.Contains(renderer))
            {
                state.TargetAlpha = 1f;
            }

            state.CurrentAlpha = Mathf.MoveTowards(
                state.CurrentAlpha,
                state.TargetAlpha,
                _fadeSpeed * Time.deltaTime
            );

            ApplyAlpha(state, state.CurrentAlpha);
        }

        if (toRemove != null)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                _fadeStates.Remove(toRemove[i]);
            }
        }
    }

    private FadeState CreateFadeState(Renderer renderer)
    {
        Material[] materials = renderer.materials;
        if (materials == null || materials.Length == 0)
        {
            return null;
        }

        FadeState state = new FadeState
        {
            Materials = materials,
            OriginalAlphas = new float[materials.Length],
            CurrentAlpha = 1f,
            TargetAlpha = 1f
        };

        bool hasValidColorProperty = false;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material == null)
            {
                continue;
            }

            SetupMaterialTransparency(material);

            if (TryGetColorProperty(material, out string colorProperty))
            {
                Color color = material.GetColor(colorProperty);
                state.OriginalAlphas[i] = color.a;
                hasValidColorProperty = true;
            }
            else
            {
                state.OriginalAlphas[i] = 1f;
            }
        }

        if (!hasValidColorProperty)
        {
            return null;
        }

        return state;
    }

    private void ApplyAlpha(FadeState state, float normalizedAlpha)
    {
        for (int i = 0; i < state.Materials.Length; i++)
        {
            Material material = state.Materials[i];
            if (material == null)
            {
                continue;
            }

            if (!TryGetColorProperty(material, out string colorProperty))
            {
                continue;
            }

            Color color = material.GetColor(colorProperty);
            color.a = Mathf.Clamp01(state.OriginalAlphas[i] * normalizedAlpha);
            material.SetColor(colorProperty, color);
        }
    }

    private static bool TryGetColorProperty(Material material, out string propertyName)
    {
        if (material.HasProperty("_BaseColor"))
        {
            propertyName = "_BaseColor";
            return true;
        }

        if (material.HasProperty("_Color"))
        {
            propertyName = "_Color";
            return true;
        }

        propertyName = string.Empty;
        return false;
    }

    private static void SetupMaterialTransparency(Material material)
    {
        // Standard shader setup
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        // URP Lit setup (safe no-op if not URP)
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}
