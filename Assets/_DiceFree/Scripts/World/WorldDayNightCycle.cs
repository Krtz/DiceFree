using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.World
{
    // One session clock across outdoor scenes. SetHour is also the future host-clock seam.
    [DisallowMultipleComponent]
    public sealed class WorldDayNightCycle : MonoBehaviour
    {
        [SerializeField] Light celestialLight;
        [SerializeField] Transform arboriumLampDistrict;
        [SerializeField, Min(1)] float cycleMinutes = 48;
        [SerializeField, Range(0, 24)] float startingHour = 10;
        [SerializeField] bool running = true;
        static double epoch;
        static float sessionHour;
        static bool initialized;
        Material originalSky, ownedSky;
        Color originalSkyColor, originalEquator, originalGround, originalFog, originalLightColor;
        AmbientMode originalAmbientMode;
        Quaternion originalRotation;
        float originalIntensity;
        Light originalSun;
        bool interior;
        private GameObject arboriumLamps;
        public bool ArboriumLampsLit => arboriumLamps != null && arboriumLamps.activeSelf;
        public float Hour => Mathf.Repeat(sessionHour + (running ? (float)((Time.realtimeSinceStartupAsDouble - epoch) / (cycleMinutes * 60) * 24) : 0), 24);
        public float CycleMinutes => cycleMinutes;
        public bool IsInterior => interior;
        public Light CelestialLight => celestialLight;
        public bool MoonActive => !interior && SunElevation(Hour) < 0;
        public static float SunElevation(float hour) => Mathf.Sin((hour - 6) / 24 * Mathf.PI * 2);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetClock() { initialized = false; }

        public void Configure(Light light) => celestialLight = light;
        void Awake()
        {
            if (celestialLight == null) { enabled = false; return; }
            if (!initialized) { sessionHour = startingHour; epoch = Time.realtimeSinceStartupAsDouble; initialized = true; }
            celestialLight.cullingMask &= ~(1 << 31);
            originalSky = RenderSettings.skybox;
            originalSun = RenderSettings.sun;
            originalAmbientMode = RenderSettings.ambientMode;
            originalSkyColor = RenderSettings.ambientSkyColor;
            originalEquator = RenderSettings.ambientEquatorColor;
            originalGround = RenderSettings.ambientGroundColor;
            originalFog = RenderSettings.fogColor;
            originalRotation = celestialLight.transform.rotation;
            originalIntensity = celestialLight.intensity;
            originalLightColor = celestialLight.color;
            if (originalSky != null) { ownedSky = new Material(originalSky) { name = "World sky (session)" }; RenderSettings.skybox = ownedSky; }
            CreateArboriumLamps();
            Apply();
        }
        void Update() => Apply();
        public void RefreshLighting() => Apply();
        public void SetHour(float hour, bool advance = true)
        {
            sessionHour = Mathf.Repeat(hour, 24); epoch = Time.realtimeSinceStartupAsDouble;
            initialized = true; running = advance; Apply();
        }
        public void SetInterior(bool value) { interior = value; Apply(); }
        void Apply()
        {
            if (celestialLight == null) return;
            if (interior) { RestoreLighting(); return; }
            float elevation = SunElevation(Hour);
            if (arboriumLamps != null && arboriumLamps.activeSelf != (elevation < 0))
                arboriumLamps.SetActive(elevation < 0);
            float day = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-.10f, .30f, elevation));
            float warmth = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .40f, Mathf.Abs(elevation)));
            bool moon = elevation < 0;
            // URP main-light shadows: one directional light hands over from sun to moon.
            // Both rotate continuously on opposite sides of the same orbit.
            celestialLight.transform.rotation = Quaternion.Euler(Hour * 15 - 90 + (moon ? 180 : 0), -35, 0);
            celestialLight.color = moon ? new Color(.63f, .75f, 1f) : Color.Lerp(new Color(1f, .95f, .84f), new Color(1f, .52f, .28f), warmth);
            float altitude = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .20f, Mathf.Abs(elevation)));
            celestialLight.intensity = (moon ? .38f : 1.8f) * altitude;
            RenderSettings.sun = celestialLight;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.18f,.24f,.37f), new Color(.63f,.77f,.87f), day);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(.12f,.17f,.25f), new Color(.50f,.58f,.48f), day);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(.07f,.10f,.14f), new Color(.28f,.32f,.24f), day);
            RenderSettings.fogColor = Color.Lerp(new Color(.09f,.14f,.23f), new Color(.66f,.77f,.79f), day);
            if (ownedSky != null) {
                if (ownedSky.HasProperty("_Exposure")) ownedSky.SetFloat("_Exposure", Mathf.Lerp(.35f,1.1f,day));
                if (ownedSky.HasProperty("_SkyTint")) ownedSky.SetColor("_SkyTint", Color.Lerp(new Color(.14f,.22f,.38f),new Color(.48f,.57f,.67f),day));
                if (ownedSky.HasProperty("_GroundColor")) ownedSky.SetColor("_GroundColor", RenderSettings.ambientGroundColor);
            }
        }
        private void CreateArboriumLamps()
        {
            if (arboriumLampDistrict == null) return;
            arboriumLamps = new GameObject("Arborium night lamp cores");
            arboriumLamps.transform.SetParent(arboriumLampDistrict, false);
            var material = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            var glow = new MaterialPropertyBlock();
            glow.SetColor("_BaseColor", new Color(1f, .65f, .27f, .92f));
            // Centers match the three twelve-lamp rings in ArboriumArrivalAuthoring.
            for (int ring = 0; ring < 3; ring++)
                for (int i = 0; i < 12; i++)
                {
                    float radius = ring == 0 ? 28 : ring == 1 ? 48 : 70;
                    float angle = (i * 30 + 12) * Mathf.Deg2Rad;
                    var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    core.name = "Warm magical lamp";
                    core.layer = 2;
                    var collider = core.GetComponent<Collider>();
                    collider.enabled = false;
                    Destroy(collider);
                    core.transform.SetParent(arboriumLamps.transform, false);
                    core.transform.localPosition = new Vector3(Mathf.Sin(angle)*radius,5.18f,Mathf.Cos(angle)*radius);
                    core.transform.localScale = new Vector3(.57f,.60f,.57f);
                    var renderer = core.GetComponent<Renderer>();
                    renderer.sharedMaterial = material;
                    renderer.SetPropertyBlock(glow);
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    var lamp = core.AddComponent<Light>();
                    lamp.type = LightType.Point;
                    lamp.color = new Color(1f,.69f,.35f);
                    lamp.range = 11;
                    lamp.intensity = 7f;
                    lamp.shadows = LightShadows.None;
                    lamp.cullingMask = ~(1 << 31);
                }
            arboriumLamps.SetActive(false);
        }

        void RestoreLighting()
        {
            celestialLight.transform.rotation = originalRotation;
            celestialLight.intensity = originalIntensity; celestialLight.color = originalLightColor;
            RenderSettings.sun = originalSun;
            RenderSettings.ambientMode = originalAmbientMode;
            RenderSettings.ambientSkyColor = originalSkyColor;
            RenderSettings.ambientEquatorColor = originalEquator;
            RenderSettings.ambientGroundColor = originalGround;
            RenderSettings.fogColor = originalFog;
            RenderSettings.skybox = originalSky;
        }
        void OnDisable()
        {
            if (arboriumLamps != null) arboriumLamps.SetActive(false);
            if (celestialLight != null) RestoreLighting();
        }
        void OnDestroy()
        {
            if (celestialLight != null) RestoreLighting();
            if (ownedSky != null) Destroy(ownedSky);
            if (arboriumLamps != null) Destroy(arboriumLamps);
        }
        void OnEnable() { if (ownedSky != null && !interior) RenderSettings.skybox = ownedSky; }
        void LateUpdate() { if (!interior && ownedSky != null && RenderSettings.skybox != ownedSky) RenderSettings.skybox = ownedSky; }
    }
}
