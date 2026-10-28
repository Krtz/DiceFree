using UnityEngine;

namespace DiceFree.World
{
    /// <summary>World pip presentation for the local player's selected resurrection anchor.</summary>
    public sealed class ResurrectionDiceVisual : MonoBehaviour
    {
        [SerializeField] private ResurrectionAnchor anchor;
        [SerializeField] private Renderer worldPip;
        [SerializeField] private Light pipLight;
        private MaterialPropertyBlock properties;
        private float nextCheck;
        public bool Glowing { get; private set; }
        public ResurrectionAnchor Anchor => anchor;
        public Renderer WorldPip => worldPip;
        public void Configure(ResurrectionAnchor value, Renderer pip, Light glow)
        { anchor=value; worldPip=pip; pipLight=glow; }
        private void OnEnable() { nextCheck=0; Refresh(); }
        private void Update()
        {
            if(Time.unscaledTime<nextCheck)return;
            nextCheck=Time.unscaledTime+.2f;
            Refresh();
        }
        public void Refresh()
        {
            bool selected=false;
            if(Application.isPlaying && anchor!=null)
                foreach(var respawn in FindObjectsByType<RespawnAtAnchor>())
                    if(respawn.isActiveAndEnabled && respawn.AnchorId==anchor.StableId)
                    { selected=true; break; }
            Glowing=selected;
            if(worldPip!=null)
            {
                properties ??= new MaterialPropertyBlock();
                worldPip.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor",selected ? new Color(.35f,1f,.85f) : new Color(.08f,.18f,.17f));
                properties.SetColor("_EmissionColor",selected ? new Color(.25f,1f,.75f)*4f : Color.black);
                worldPip.SetPropertyBlock(properties);
            }
            if(pipLight!=null)pipLight.enabled=selected;
        }
    }
}
