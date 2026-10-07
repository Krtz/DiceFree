using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    public enum WorldTransitionStyle
    {
        CubeRotate = 0,
        Fade = 1
    }

    [Serializable]
    public sealed class WorldEdgeLink
    {
        public string edgeId;
        public string destinationFaceId;
        public string destinationSpawnId;
        public WorldTransitionStyle transitionStyle = WorldTransitionStyle.CubeRotate;
    }

    [CreateAssetMenu(menuName = "DiceFree/World/World Face Definition")]
    public sealed class WorldFaceDefinition : ScriptableObject
    {
        public string stableId;
        public string sceneId;
        public WorldEdgeLink[] edges = Array.Empty<WorldEdgeLink>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(stableId))
                throw new InvalidOperationException(name + " has no stable world-face ID.");
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new InvalidOperationException(stableId + " has no scene ID.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in edges ?? Array.Empty<WorldEdgeLink>())
            {
                if (edge == null ||
                    string.IsNullOrWhiteSpace(edge.edgeId) ||
                    string.IsNullOrWhiteSpace(edge.destinationFaceId) ||
                    string.IsNullOrWhiteSpace(edge.destinationSpawnId))
                    throw new InvalidOperationException(stableId + " has an invalid world-edge link.");
                if (!ids.Add(edge.edgeId))
                    throw new InvalidOperationException(stableId + " repeats edge ID " + edge.edgeId + ".");
            }
        }

        public WorldEdgeLink ResolveEdge(string edgeId)
        {
            foreach (var edge in edges ?? Array.Empty<WorldEdgeLink>())
                if (edge != null && edge.edgeId == edgeId) return edge;
            return null;
        }
    }
}
