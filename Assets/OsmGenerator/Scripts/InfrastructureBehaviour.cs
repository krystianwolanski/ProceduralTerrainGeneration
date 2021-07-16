using Assets.OsmGenerator.Scripts.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts
{
    [RequireComponent(typeof(MapReader))]
    public abstract class InfrastructureBehaviour : MonoBehaviour
    {
        protected MapReader map;

        void Awake()
        {
            map = GetComponent<MapReader>();
        }
        
        protected Vector3 GetCentre(OsmWay way)
        {
            Vector3 total = Vector3.zero;

            foreach (var id in way.NodeIDs)
            {
                total += map.Nodes[id];
            }

            return total / way.NodeIDs.Count;
        }

        protected IEnumerable<Vector3> PointFeeder(Vector3 p1, Vector3 p2, float maxSpacing)
        {
            float distance = Vector3.Distance(p1, p2);
            int steps = (int)(distance / maxSpacing);

            for (int i = 0; i < steps; i++)
            {
                float fraction = (float)i / steps;

                Vector3 position = Vector3.Lerp(p1, p2, fraction);

                yield return position;
            }
        }

        protected Vector3 PointInFeeder(Vector3 p1, Vector3 p2, float maxSpacing, int i)
        {
            float distance = Vector3.Distance(p1, p2);
            int steps = (int)(distance / maxSpacing);
            float fraction = (float)i / steps;
            Vector3 position = Vector3.Lerp(p1, p2, fraction);

            return position;
            
        }
    }
}
