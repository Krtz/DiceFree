using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public readonly struct WorldPointerTarget
    {
        public readonly CombatActor Actor;
        public readonly InteractionTarget Interaction;
        public readonly ResurrectionDiceVisual Dice;
        public Component Target => Actor != null ? Actor : Interaction != null ? Interaction : Dice;
        public WorldPointerTarget(CombatActor actor, InteractionTarget interaction, ResurrectionDiceVisual dice)
        { Actor=actor; Interaction=interaction; Dice=dice; }
    }
    public static class WorldTargetPicker
    {
        private static WorldFogOfWar fog;
        private static int cachedFrame=-1;
        private static CombatActor cachedViewer;
        private static Ray cachedRay;
        private static WorldPointerTarget cachedPick;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { fog=null; cachedFrame=-1; cachedViewer=null; cachedPick=default; }
        public static WorldPointerTarget Pick(CombatActor viewer, Camera camera, Vector2 screenPoint) =>
            camera == null ? default : Pick(viewer, camera.ScreenPointToRay(screenPoint));
        public static bool IsVisibleTo(CombatActor viewer, Component target)
        {
            if(viewer == null || target == null || !target.gameObject.activeInHierarchy) return false;
            if(fog == null) fog=Object.FindAnyObjectByType<WorldFogOfWar>();
            bool rootVisible=fog == null || fog.IsVisible(target.transform.position);
            // Actors retain authoritative fog visibility. Scenery interactions can
            // be rooted inside their own trunk/wall obstruction: test their visible
            // footprint as well, so an exposed entrance or counter remains usable.
            if(target is CombatActor && !rootVisible) return false;
            if(!WorldSelectionShape.For(target).TryBounds(out var bounds)) return false;
            if(rootVisible) return true;
            if(target is InteractionTarget interaction && fog.IsVisible(interaction.ApproachPosition)) return true;
            if(fog.IsVisible(bounds.ClosestPoint(viewer.transform.position))) return true;
            var p=bounds.center; var e=bounds.extents;
            return fog.IsVisible(p+new Vector3(e.x,0,0)) || fog.IsVisible(p-new Vector3(e.x,0,0)) ||
                fog.IsVisible(p+new Vector3(0,0,e.z)) || fog.IsVisible(p-new Vector3(0,0,e.z));
        }
        public static WorldPointerTarget Pick(CombatActor viewer, Ray ray)
        {
            if(viewer == null || !viewer.Alive) return default;
            if(cachedFrame==Time.frameCount && cachedViewer==viewer && cachedRay.origin==ray.origin && cachedRay.direction==ray.direction)
                return cachedPick;
            if(fog == null) fog = Object.FindAnyObjectByType<WorldFogOfWar>();
            var hits = Physics.RaycastAll(ray,1500,(1<<8)|(1<<9)|(1<<10)|(1<<11),QueryTriggerInteraction.Ignore);
            float obstruction = 1500;
            foreach(var hit in hits)
                if((hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 9) &&
                   !ArboriumInterior.IsTransparentToPointer(hit.collider)) obstruction = Mathf.Min(obstruction,hit.distance);
            WorldPointerTarget best = default; float distance = obstruction + .025f;
            void Consider(WorldPointerTarget target, float hit)
            {
                if(target.Target != null && hit >= 0 && hit <= distance) { best=target; distance=hit; }
            }
            foreach(var hit in hits)
            {
                if(hit.distance > distance) continue;
                var actor = hit.collider.GetComponentInParent<CombatActor>();
                if(actor != null && actor.Alive && (!actor.Concealed || viewer.IsFriendlyTo(actor)) && IsVisibleTo(viewer,actor))
                    Consider(new WorldPointerTarget(actor,null,null),hit.distance);
                else
                {
                    var interaction = hit.collider.GetComponentInParent<InteractionTarget>()?.WorldTarget;
                    if(interaction != null && interaction.isActiveAndEnabled && IsVisibleTo(viewer,interaction))
                        Consider(new WorldPointerTarget(null,interaction,null),hit.distance);
                    var dice = hit.collider.GetComponentInParent<ResurrectionDiceVisual>();
                    if(dice != null && IsVisibleTo(viewer,dice)) Consider(new WorldPointerTarget(null,null,dice),hit.distance);
                }
            }
            foreach(var actor in CombatActor.All)
            {
                if(actor == null || !actor.Alive || (actor.Concealed && !viewer.IsFriendlyTo(actor)) ||
                   !IsVisibleTo(viewer,actor)) continue;
                if(WorldSelectionShape.For(actor).Raycast(ray,distance,out var hit)) Consider(new WorldPointerTarget(actor,null,null),hit);
            }
            var registry = viewer.GetComponent<InteractionRegistry>();
            if(registry != null) foreach(var target in registry.Targets)
            {
                if(target == null || !target.isActiveAndEnabled || !IsVisibleTo(viewer,target)) continue;
                if(WorldSelectionShape.For(target).Raycast(ray,distance,out var hit)) Consider(new WorldPointerTarget(null,target.WorldTarget,null),hit);
            }
            cachedFrame=Time.frameCount; cachedViewer=viewer; cachedRay=ray; cachedPick=best;
            return best;
        }
    }
}
