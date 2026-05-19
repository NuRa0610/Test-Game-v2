using System.Collections.Generic;
using UnityEngine;

public class CameraWallFade : MonoBehaviour
{
    private const string StandardTransparentVariantResource = "WallFadeTransparent";
    private static Material s_standardTransparentVariantAnchor;

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
        public MaterialRenderState[] RenderStates;
        public float[] OriginalAlphas;
        public float CurrentAlpha = 1f;
        public float TargetAlpha = 1f;
    }

    private class MaterialRenderState
    {
        public bool HasMode;
        public float Mode;
        public bool HasSurface;
        public float Surface;
        public bool HasBlend;
        public float Blend;
        public bool HasSrcBlend;
        public int SrcBlend;
        public bool HasDstBlend;
        public int DstBlend;
        public bool HasZWrite;
        public int ZWrite;
        public int RenderQueue;
        public string RenderType;
        public bool AlphaTestEnabled;
        public bool AlphaBlendEnabled;
        public bool AlphaPremultiplyEnabled;
    }

    private void Awake()
    {
        CacheTransparentVariant();

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
            else
            {
                SetupMaterialsTransparency(state);
            }

            state.CurrentAlpha = Mathf.MoveTowards(
                state.CurrentAlpha,
                state.TargetAlpha,
                _fadeSpeed * Time.deltaTime
            );

            ApplyAlpha(state, state.CurrentAlpha);

            if (state.TargetAlpha >= 1f && Mathf.Approximately(state.CurrentAlpha, 1f))
            {
                RestoreMaterialState(state);

                if (toRemove == null)
                {
                    toRemove = new List<Renderer>();
                }

                toRemove.Add(renderer);
            }
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
            RenderStates = new MaterialRenderState[materials.Length],
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

            state.RenderStates[i] = CaptureMaterialState(material);

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

    private static MaterialRenderState CaptureMaterialState(Material material)
    {
        MaterialRenderState state = new MaterialRenderState
        {
            RenderQueue = material.renderQueue,
            RenderType = material.GetTag("RenderType", false, string.Empty),
            AlphaTestEnabled = material.IsKeywordEnabled("_ALPHATEST_ON"),
            AlphaBlendEnabled = material.IsKeywordEnabled("_ALPHABLEND_ON"),
            AlphaPremultiplyEnabled = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON")
        };

        if (material.HasProperty("_Mode"))
        {
            state.HasMode = true;
            state.Mode = material.GetFloat("_Mode");
        }

        if (material.HasProperty("_Surface"))
        {
            state.HasSurface = true;
            state.Surface = material.GetFloat("_Surface");
        }

        if (material.HasProperty("_Blend"))
        {
            state.HasBlend = true;
            state.Blend = material.GetFloat("_Blend");
        }

        if (material.HasProperty("_SrcBlend"))
        {
            state.HasSrcBlend = true;
            state.SrcBlend = material.GetInt("_SrcBlend");
        }

        if (material.HasProperty("_DstBlend"))
        {
            state.HasDstBlend = true;
            state.DstBlend = material.GetInt("_DstBlend");
        }

        if (material.HasProperty("_ZWrite"))
        {
            state.HasZWrite = true;
            state.ZWrite = material.GetInt("_ZWrite");
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

    private static void SetupMaterialsTransparency(FadeState state)
    {
        for (int i = 0; i < state.Materials.Length; i++)
        {
            Material material = state.Materials[i];
            if (material != null)
            {
                SetupMaterialTransparency(material);
            }
        }
    }

    private static void RestoreMaterialState(FadeState state)
    {
        for (int i = 0; i < state.Materials.Length; i++)
        {
            Material material = state.Materials[i];
            MaterialRenderState renderState = state.RenderStates[i];

            if (material == null || renderState == null)
            {
                continue;
            }

            if (TryGetColorProperty(material, out string colorProperty))
            {
                Color color = material.GetColor(colorProperty);
                color.a = state.OriginalAlphas[i];
                material.SetColor(colorProperty, color);
            }

            if (renderState.HasMode)
            {
                material.SetFloat("_Mode", renderState.Mode);
            }

            if (renderState.HasSurface)
            {
                material.SetFloat("_Surface", renderState.Surface);
            }

            if (renderState.HasBlend)
            {
                material.SetFloat("_Blend", renderState.Blend);
            }

            if (renderState.HasSrcBlend)
            {
                material.SetInt("_SrcBlend", renderState.SrcBlend);
            }

            if (renderState.HasDstBlend)
            {
                material.SetInt("_DstBlend", renderState.DstBlend);
            }

            if (renderState.HasZWrite)
            {
                material.SetInt("_ZWrite", renderState.ZWrite);
            }

            SetKeyword(material, "_ALPHATEST_ON", renderState.AlphaTestEnabled);
            SetKeyword(material, "_ALPHABLEND_ON", renderState.AlphaBlendEnabled);
            SetKeyword(material, "_ALPHAPREMULTIPLY_ON", renderState.AlphaPremultiplyEnabled);
            material.SetOverrideTag("RenderType", renderState.RenderType);
            material.renderQueue = renderState.RenderQueue;
        }
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
        {
            material.EnableKeyword(keyword);
        }
        else
        {
            material.DisableKeyword(keyword);
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
        CacheTransparentVariant();

        // Standard shader setup
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 2f);
            material.SetOverrideTag("RenderType", "Transparent");
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

    private static void CacheTransparentVariant()
    {
        if (s_standardTransparentVariantAnchor != null)
        {
            return;
        }

        s_standardTransparentVariantAnchor = Resources.Load<Material>(StandardTransparentVariantResource);
    }
}
