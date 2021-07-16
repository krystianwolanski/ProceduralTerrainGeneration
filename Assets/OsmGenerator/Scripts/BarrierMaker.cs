using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Assets.OsmGenerator.Scripts.Serialization;
using Assets.Scripts.Models;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.OsmGenerator.Scripts
{
    [RequireComponent(typeof(MapReader))]
    public class BarrierMaker : InfrastructureBehaviour
    {
        [SerializeField]
        private Material metal;

        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }

            foreach (var way in map.Ways.FindAll(w => w.IsBarrier))
            {

                GameObject go = new GameObject();
                go.name = "Barrier";

                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = GetBarrierMaterial(way.BarrierTypeName);

                List<Vector3> vectors = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<int> indicies = new List<int>();

                bool first = true;

                Vector3 prevLeft = Vector3.zero;
                Vector3 prevRight = Vector3.zero;

                int prevNTopLeft = -1;

                int prevNEdgeLeft = -1;
                int prevNEdgeRight = -1;

                Vector3 prevPosition = Vector3.zero;

                float u = 0.0f;

                var roadHit = false;
                for (int i = 1; i < way.NodeIDs.Count; i++)
                {
                    OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                    OsmNode p2 = map.Nodes[way.NodeIDs[i]];


                    Vector3 v1 = p1 - map.Bounds.Centre;
                    Vector3 v2 = p2 - map.Bounds.Centre;
                    Vector3 v3 = v1 + new Vector3(0, 5, 0);
                    Vector3 v4 = v2 + new Vector3(0, 5, 0);

                    vectors.Add(v1);
                    vectors.Add(v2);
                    vectors.Add(v3);
                    vectors.Add(v4);

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));

                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);

                    int idx1, idx2, idx3, idx4;
                    idx4 = vectors.Count - 1;
                    idx3 = vectors.Count - 2;
                    idx2 = vectors.Count - 3;
                    idx1 = vectors.Count - 4;

                    // first triangle v1, v3, v2
                    indicies.Add(idx1);
                    indicies.Add(idx3);
                    indicies.Add(idx2);

                    // second         v3, v4, v2
                    indicies.Add(idx3);
                    indicies.Add(idx4);
                    indicies.Add(idx2);

                    // third          v2, v3, v1
                    indicies.Add(idx2);
                    indicies.Add(idx3);
                    indicies.Add(idx1);

                    // fourth        v2, v4, v3
                    indicies.Add(idx2);
                    indicies.Add(idx4);
                    indicies.Add(idx3);


                }
                mf.mesh.vertices = vectors.ToArray();
                mf.mesh.normals = normals.ToArray();
                mf.mesh.triangles = indicies.ToArray();
                mf.mesh.uv = uvs.ToArray();


                yield return null;
            }
        }


      
        public Material GetBarrierMaterial(string barrierName)
        {
            switch (barrierName)
            {
                case "metal":
                    return metal;
                default:
                    return metal;
            }
        }
    }
}
