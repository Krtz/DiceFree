using DiceFree.Characters;using DiceFree.Core;using DiceFree.World;using UnityEngine;
namespace DiceFree.Briarback
{
    public sealed class OverworldBoundsPresentation:MonoBehaviour
    {
        public TraversalInput player;public ExplorationCamera exploration;public WorldFogOfWar fog;
        void Start(){
            var bounds=GetComponent<OverworldMapBounds>().Bounds;
            exploration.ConfigureMapBounds(bounds);
            fog.Configure(player.transform,exploration.GetComponent<Camera>(),new Vector2(bounds.xMin,bounds.yMin),new Vector2(bounds.xMax,bounds.yMax),fog.VisionRadius);
        }
    }
}
