using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Game Juice/Game Juice Manager")]
public sealed class GameJuiceManager : MonoSingleton<GameJuiceManager>
{
    [Header("Default Hit")]
    [SerializeField, Min(0f)] private float hitStopDuration = 0.07f;
    [SerializeField, Min(0f)] private float flashDuration = 0.10f;
    [SerializeField, Min(0f)] private float knockbackStrength = 4f;
    [SerializeField, Min(0f)] private float shakeDuration = 0.18f;
    [SerializeField, Min(0f)] private float shakeAmplitude = 0.14f;
    [SerializeField, Min(0f)] private float shockwaveDuration = 0.30f;
    [SerializeField, Min(0f)] private float shockwaveRadius = 1.5f;

    [Header("Scene Setup")]
    [Tooltip("A dedicated pivot between the camera follow rig and the Camera.")]
    [SerializeField] private Transform shakePivot;
    [SerializeField] private string shockwaveSortingLayer = "Default";
    [SerializeField] private int shockwaveSortingOrder = 1000;

    private int progressId;

    private struct FlashState
    {
        public SpriteRenderer Sprite;
        public Material OriginalMaterial;
        public Material FlashMaterial;
        public double EndTime;
    }

    private struct WaveState
    {
        public MeshRenderer Renderer;
        public double StartTime;
        public float Duration;
    }

    private readonly List<FlashState> flashes = new();
    private readonly List<WaveState> waves = new();
    private MaterialPropertyBlock waveProperties;
    private Material whiteFlashMaterial;
    private Material fovFlashMaterial;
    private Material shockwaveMaterial;
    private Mesh shockwaveMesh;

    private bool stoppingTime;
    private double stopEndTime;
    private float resumeTimeScale;

    private Transform activeShakePivot;
    private Vector3 shakeOrigin;
    private double shakeStartTime;
    private float activeShakeDuration;
    private float activeShakeAmplitude;

    public event Action<string> SoundRequested;

    protected override void Awake()
    {
        base.Awake();
        progressId = Shader.PropertyToID("_Progress");
        waveProperties = new MaterialPropertyBlock();
    }

    /// <summary>Plays the configured hit effects. Pass an impulse receiver for targets that can be knocked back.</summary>
    public void PlayHit(Vector3 point, SpriteRenderer sprite = null, string soundKey = null,
        Vector3 knockbackDirection = default, Action<Vector3> applyKnockback = null,
        Material flashMaterial = null)
    {
        PlayFlash(sprite, flashDuration, flashMaterial);
        PlayShake();
        PlayShockwave(point);
        PlayKnockback(knockbackDirection, knockbackStrength, applyKnockback);
        PlayHitStop(hitStopDuration);
        PlaySound(soundKey);
    }

    public void PlayHitStop(float duration)
    {
        if (!isActiveAndEnabled || !(duration > 0f) || float.IsInfinity(duration)) return;
        double endTime = Time.realtimeSinceStartupAsDouble + duration;
        if (stoppingTime && Time.timeScale == 0f)
        {
            stopEndTime = Math.Max(stopEndTime, endTime);
            return;
        }

        if (Time.timeScale == 0f) return;

        resumeTimeScale = Time.timeScale;
        stopEndTime = endTime;
        stoppingTime = true;
        Time.timeScale = 0f;
    }

    /// <summary>Use this for pause and slow motion when they can overlap a hit stop.</summary>
    public void SetGameplayTimeScale(float timeScale)
    {
        if (!(timeScale >= 0f) || float.IsInfinity(timeScale)) return;

        if (stoppingTime && Time.timeScale == 0f)
        {
            resumeTimeScale = timeScale;
            return;
        }

        stoppingTime = false;
        Time.timeScale = timeScale;
    }

    /// <summary>Temporarily replaces the sprite material with a white silhouette material.</summary>
    public void PlayFlash(SpriteRenderer sprite, float duration, Material flashMaterial = null)
    {
        if (!isActiveAndEnabled || sprite == null || !(duration > 0f) || float.IsInfinity(duration)) return;

        if (flashMaterial == null)
        {
            Material current = sprite.sharedMaterial;
            if (current != null && (current == fovFlashMaterial || current.shader.name == "Custom/FOVVisibleSprite"))
            {
                if (fovFlashMaterial == null)
                    fovFlashMaterial = Resources.Load<Material>("GameJuice/FOVWhiteFlash");
                flashMaterial = fovFlashMaterial;
            }
        }

        if (flashMaterial == null)
        {
            if (whiteFlashMaterial == null)
                whiteFlashMaterial = CreateMaterial("WhiteFlash");
            flashMaterial = whiteFlashMaterial;
        }
        if (flashMaterial == null) return;

        double endTime = Time.realtimeSinceStartupAsDouble + duration;
        for (int i = 0; i < flashes.Count; i++)
        {
            FlashState flash = flashes[i];
            if (flash.Sprite != sprite) continue;

            if (sprite.sharedMaterial != flash.FlashMaterial)
                flash.OriginalMaterial = sprite.sharedMaterial;
            flash.FlashMaterial = flashMaterial;
            flash.EndTime = Math.Max(flash.EndTime, endTime);
            flashes[i] = flash;
            sprite.sharedMaterial = flashMaterial;
            return;
        }

        flashes.Add(new FlashState
        {
            Sprite = sprite,
            OriginalMaterial = sprite.sharedMaterial,
            FlashMaterial = flashMaterial,
            EndTime = endTime
        });
        sprite.sharedMaterial = flashMaterial;
    }

    /// <summary>Passes a normalized direction times strength to the target's movement owner.</summary>
    public void PlayKnockback(Vector3 direction, float strength, Action<Vector3> applyImpulse)
    {
        if (!isActiveAndEnabled || applyImpulse == null || !(strength > 0f) || float.IsInfinity(strength)) return;

        Vector3 impulse = direction.normalized * strength;
        if (impulse.sqrMagnitude > 0f)
            applyImpulse(impulse);
    }

    public void PlaySound(string soundKey)
    {
        if (isActiveAndEnabled && !string.IsNullOrWhiteSpace(soundKey))
            SoundRequested?.Invoke(soundKey);
    }

    public void PlayShake()
    {
        PlayShake(shakeDuration, shakeAmplitude);
    }

    /// <summary>Shakes the assigned camera pivot in its parent's XY plane.</summary>
    public void PlayShake(float duration, float amplitude)
    {
        if (!isActiveAndEnabled || shakePivot == null || !(duration > 0f) || !(amplitude > 0f) ||
            float.IsInfinity(duration) || float.IsInfinity(amplitude)) return;

        if (activeShakePivot != shakePivot)
        {
            RestoreShake();
            activeShakePivot = shakePivot;
            shakeOrigin = shakePivot.localPosition;
        }

        shakeStartTime = Time.realtimeSinceStartupAsDouble;
        activeShakeDuration = duration;
        activeShakeAmplitude = amplitude;
    }

    public void PlayShockwave(Vector3 point)
    {
        PlayShockwave(point, shockwaveDuration, shockwaveRadius);
    }

    /// <summary>Spawns an expanding translucent ring in the world XY plane.</summary>
    public void PlayShockwave(Vector3 point, float duration, float radius)
    {
        if (!isActiveAndEnabled || !(duration > 0f) || !(radius > 0f) ||
            float.IsInfinity(duration) || float.IsInfinity(radius)) return;

        if (shockwaveMaterial == null)
            shockwaveMaterial = CreateMaterial("Shockwave");
        if (shockwaveMaterial == null) return;
        if (shockwaveMesh == null)
            shockwaveMesh = CreateWaveMesh();

        var wave = new GameObject("Game Juice Shockwave", typeof(MeshFilter), typeof(MeshRenderer));
        wave.layer = gameObject.layer;
        wave.transform.position = point;
        wave.transform.localScale = new Vector3(2f * radius, 2f * radius, 1f);
        wave.GetComponent<MeshFilter>().sharedMesh = shockwaveMesh;

        var renderer = wave.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = shockwaveMaterial;
        renderer.sortingLayerName = shockwaveSortingLayer;
        renderer.sortingOrder = shockwaveSortingOrder;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        waves.Add(new WaveState
        {
            Renderer = renderer,
            StartTime = Time.realtimeSinceStartupAsDouble,
            Duration = duration
        });
    }

    private void Update()
    {
        double now = Time.realtimeSinceStartupAsDouble;
        if (stoppingTime && (Time.timeScale != 0f || now >= stopEndTime))
            RestoreTime();

        for (int i = flashes.Count - 1; i >= 0; i--)
        {
            FlashState flash = flashes[i];
            if (flash.Sprite != null && flash.Sprite.sharedMaterial == flash.FlashMaterial && now < flash.EndTime)
                continue;

            RestoreFlash(flash);
            flashes.RemoveAt(i);
        }

        for (int i = waves.Count - 1; i >= 0; i--)
        {
            WaveState wave = waves[i];
            float progress = (float)((now - wave.StartTime) / wave.Duration);
            if (wave.Renderer == null || progress >= 1f)
            {
                RemoveWave(i);
                continue;
            }

            waveProperties.SetFloat(progressId, progress);
            wave.Renderer.SetPropertyBlock(waveProperties);
        }
    }

    private void LateUpdate()
    {
        if (activeShakePivot == null) return;

        float progress = (float)((Time.realtimeSinceStartupAsDouble - shakeStartTime) / activeShakeDuration);
        if (progress >= 1f)
        {
            RestoreShake();
            return;
        }

        float phase = progress * activeShakeDuration * 125f;
        var offset = new Vector3(Mathf.Cos(phase), Mathf.Sin(phase * 1.27f), 0f);
        activeShakePivot.localPosition = shakeOrigin + offset * (activeShakeAmplitude * (1f - progress));
    }

    private void OnDisable()
    {
        RestoreTime();
        RestoreShake();
        foreach (FlashState flash in flashes)
            RestoreFlash(flash);
        flashes.Clear();
        for (int i = waves.Count - 1; i >= 0; i--)
            RemoveWave(i);
    }

    private void OnDestroy()
    {
        Destroy(whiteFlashMaterial);
        Destroy(shockwaveMaterial);
        Destroy(shockwaveMesh);
    }

    private void RestoreTime()
    {
        if (!stoppingTime) return;

        stoppingTime = false;
        if (Time.timeScale == 0f)
            Time.timeScale = resumeTimeScale;
    }

    private void RestoreShake()
    {
        if (activeShakePivot != null)
            activeShakePivot.localPosition = shakeOrigin;
        activeShakePivot = null;
    }

    private void RestoreFlash(FlashState flash)
    {
        if (flash.Sprite != null && flash.Sprite.sharedMaterial == flash.FlashMaterial)
            flash.Sprite.sharedMaterial = flash.OriginalMaterial;
    }

    private void RemoveWave(int index)
    {
        MeshRenderer renderer = waves[index].Renderer;
        if (renderer != null)
        {
            renderer.enabled = false;
            Destroy(renderer.gameObject);
        }
        waves.RemoveAt(index);
    }

    private Material CreateMaterial(string shaderName)
    {
        Shader shader = Resources.Load<Shader>("GameJuice/" + shaderName);
        if (shader != null && shader.isSupported)
            return new Material(shader);

        Debug.LogWarning($"Game Juice shader '{shaderName}' is missing or unsupported.", this);
        return null;
    }

    private Mesh CreateWaveMesh()
    {
        var mesh = new Mesh
        {
            name = "Game Juice Shockwave Quad",
            vertices = new[]
            {
                new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
                new Vector3(0.5f, 0.5f), new Vector3(-0.5f, 0.5f)
            },
            uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up },
            triangles = new[] { 0, 2, 1, 0, 3, 2 }
        };
        mesh.RecalculateBounds();
        return mesh;
    }
}
