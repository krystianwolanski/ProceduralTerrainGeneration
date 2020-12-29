using Assets.OsmGenerator.Scripts.Serialization;
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
    }
}
