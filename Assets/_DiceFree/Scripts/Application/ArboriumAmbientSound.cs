using DiceFree.Characters;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Visuals
{
    // A restrained, original nonverbal city layer. Existing Master/Ambience controls apply.
    public sealed class ArboriumAmbientSound : MonoBehaviour
    {
        [SerializeField] private AudioClip bed, bell;
        private AudioSource atmosphere, details;
        private WorldFogOfWar fog;
        private PlayerAudioDirector audioSettings;
        private float nextBell = 17;
        public float Weight { get; private set; }
        public float AppliedVolume => atmosphere != null ? atmosphere.volume : 0;
        public bool Playing => atmosphere != null && atmosphere.isPlaying;
        public void Configure(AudioClip ambience, AudioClip chime) { bed = ambience; bell = chime; }
        private void Awake()
        {
            atmosphere = gameObject.AddComponent<AudioSource>(); atmosphere.playOnAwake = false;
            atmosphere.loop = true; atmosphere.clip = bed; atmosphere.spatialBlend = 0; atmosphere.dopplerLevel = 0;
            details = gameObject.AddComponent<AudioSource>(); details.playOnAwake = false; details.spatialBlend = 0; details.dopplerLevel = 0;
        }
        private void Update()
        {
            if (fog == null) fog = FindAnyObjectByType<WorldFogOfWar>();
            if (fog?.Revealer == null) return;
            if (audioSettings == null) audioSettings = fog.Revealer.GetComponent<PlayerAudioDirector>();
            var p = fog.Revealer.position - transform.position;
            float range = new Vector2(p.x, p.z).magnitude;
            Weight = Mathf.SmoothStep(0, 1, Mathf.Clamp01((84 - range) / 14)) * (Mathf.Abs(p.y) < 40 ? 1 : 0);
            float gain = audioSettings != null ? audioSettings.AmbientLevel : 0;
            atmosphere.volume = .11f * gain * Weight; details.volume = .10f * gain * Weight;
            if (Weight < .001f) { if (atmosphere.isPlaying) atmosphere.Stop(); details.Stop(); return; }
            if (!atmosphere.isPlaying && bed != null) atmosphere.Play();
            nextBell -= Time.deltaTime;
            if (nextBell <= 0) { if (bell != null) details.PlayOneShot(bell); nextBell = 31; }
        }
        private void OnDisable() { if (atmosphere != null) atmosphere.Stop(); if (details != null) details.Stop(); Weight = 0; }
    }
}
