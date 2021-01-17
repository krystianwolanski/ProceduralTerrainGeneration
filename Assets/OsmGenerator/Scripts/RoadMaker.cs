using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Assets.OsmGenerator.Scripts.Serialization;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.OsmGenerator.Scripts
{
    [RequireComponent(typeof(MapReader))]
    public class RoadMaker : InfrastructureBehaviour
    {
        IEnumerable<Vector3> PointFeeder(Vector3 p1, Vector3 p2, float maxSpacing)
        {
            
                float distance = Vector3.Distance(p1, p2);

                //float heading1 = p1.rotation.eulerAngles.y;
                //float heading2 = p2..eulerAngles.y;

                int steps = 1 + (int)(distance / maxSpacing);

                int limit = steps;
                //if (point == children.Length - 1) limit++;

                for (int i = 0; i < limit; i++)
                {
                    float fraction = (float)i / steps;

                    Vector3 position = Vector3.Lerp(p1, p2, fraction);
                    //float heading = Mathf.Lerp(heading1, heading2, fraction);

                    yield return position;
                }
        }

        public Material roadMaterial;
        const float CastPullUp = 25.0f;
        const float CastRayDown = 50.0f;
        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }


            foreach (var way in map.Ways.FindAll(w => w.IsRoad))
            {
                GameObject go = new GameObject();
                Vector3 localOrigin = GetCentre(way);
                go.transform.position = localOrigin - map.Bounds.Centre;

                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();

                mr.material = roadMaterial;

                //List<Vector3> vectors = new List<Vector3>();
                //List<Vector3> normals = new List<Vector3>();
                //List<Vector2> uvs = new List<Vector2>();
                //List<int> indicies = new List<int>();

                List<Vector3> verts = new List<Vector3>();
                List<int> tris0 = new List<int>();
                List<int> tris1 = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                bool first = true;

                Vector3 prevLeft = Vector3.zero;
                Vector3 prevRight = Vector3.zero;

                int prevNTopLeft = -1;

                int prevNEdgeLeft = -1;
                int prevNEdgeRight = -1;

                Vector3 prevPosition = Vector3.zero;

                float u = 0.0f;

                for (int i = 1; i < way.NodeIDs.Count; i++)
                {
                    OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                    OsmNode p2 = map.Nodes[way.NodeIDs[i]];

                    Vector3 s1 = p1 - localOrigin;
                    Vector3 s2 = p2 - localOrigin;

                    foreach (var pt in PointFeeder(s1,s2,1))
                    {
                        var diff2 = (s2 - pt).normalized;
                        var cross2 = Vector3.Cross(diff2, Vector3.up) * 2.0f;

                        Vector3 left = pt + cross2;
                        Vector3 right = pt - cross2;
                        if (true)
                        {
                            Ray rayLeft = new Ray(left + Vector3.up * CastPullUp, Vector3.down);
                            Ray rayRight = new Ray(right + Vector3.up * CastPullUp, Vector3.down);
                            RaycastHit rchLeft, rchRight;
                            bool hitLeft = Physics.Raycast(rayLeft, out rchLeft, CastRayDown);
                            bool hitRight = Physics.Raycast(rayRight, out rchRight, CastRayDown);

                            if (hitLeft && hitRight)
                            {
                                left = rchLeft.point + new Vector3(0, .5f, 0);
                                right = rchRight.point + new Vector3(0, .5f, 0);
                            }
                        }

                        int nTopLeft = verts.Count;

                        verts.Add(left);
                        verts.Add(right);

                        uvs.Add(new Vector2(u, 0));
                        uvs.Add(new Vector2(u, 1));

                        if (!first)
                        {
                            tris0.Add(prevNTopLeft);
                            tris0.Add(nTopLeft);
                            tris0.Add(prevNTopLeft + 1);

                            tris0.Add(nTopLeft);
                            tris0.Add(nTopLeft + 1);
                            tris0.Add(prevNTopLeft + 1);
                        }

                        // we need this early so we can scale the edge UVs properly and keep them nearly square
                        float uAdvance = Vector3.Distance(pt, prevPosition) / 6;

                        u -= uAdvance;

                        prevPosition = pt;

                        prevLeft = left;
                        prevRight = right;

                        first = false;
                        prevNTopLeft = nTopLeft;
                    }




                    //Vector3 s1 = p1 - localOrigin;
                    //Vector3 s2 = p2 - localOrigin;

                    //Vector3 diff = (s2 - s1).normalized;
                    //var cross = Vector3.Cross(diff, Vector3.up) * 2.0f; // 2 metres = width of lane


                    //Vector3 v1 = s1 + cross;
                    //Vector3 v2 = s1 - cross;
                    //Vector3 v3 = s2 + cross;
                    //Vector3 v4 = s2 - cross;

                    //vectors.Add(v1);
                    //vectors.Add(v2);
                    //vectors.Add(v3);
                    //vectors.Add(v4);

                    //// Nie wiem do końca co to jest. Muszę sprawdzić
                    //var CastPullUp = 25f;
                    //var CastRayDown = 50f;

                    //Ray rayLeft = new Ray(v1 + Vector3.up * CastPullUp, Vector3.down);
                    //Ray rayRight = new Ray(v2 + Vector3.up * CastPullUp, Vector3.down);
                    //RaycastHit rchLeft, rchRight;
                    //bool hitLeft = Physics.Raycast(rayLeft, out rchLeft, CastRayDown);
                    //bool hitRight = Physics.Raycast(rayRight, out rchRight, CastRayDown);

                    //if (hitLeft && hitRight)
                    //{

                    //}

                    //uvs.Add(new Vector2(0,0));
                    //uvs.Add(new Vector2(1,0));
                    //uvs.Add(new Vector3(0,1));
                    //uvs.Add(new Vector3(1,1));

                    //normals.Add(Vector3.up);
                    //normals.Add(Vector3.up);
                    //normals.Add(Vector3.up);
                    //normals.Add(Vector3.up);

                    //int idx1, idx2, idx3, idx4;
                    //idx4 = vectors.Count - 1;
                    //idx3 = vectors.Count - 2;
                    //idx2 = vectors.Count - 3;
                    //idx1 = vectors.Count - 4;

                    //// first triangle v1, v3, v2
                    //indicies.Add(idx1);
                    //indicies.Add(idx3);
                    //indicies.Add(idx2);

                    //// second         v3, v4, v2
                    //indicies.Add(idx3);
                    //indicies.Add(idx4);
                    //indicies.Add(idx2);
                }

                mf.mesh.subMeshCount = 2;

                mf.mesh.vertices = verts.ToArray();
                mf.mesh.uv = uvs.ToArray();

                mf.mesh.SetTriangles(tris0.ToArray(), 0);
                mf.mesh.SetTriangles(tris1.ToArray(), 1);

                mf.mesh.RecalculateBounds();
                mf.mesh.RecalculateNormals();

                //mf.mesh.vertices = vectors.ToArray();
                //mf.mesh.normals = normals.ToArray();
                //mf.mesh.triangles = indicies.ToArray();
                //mf.mesh.uv = uvs.ToArray();

                //mf.mesh.RecalculateBounds();
                //mf.mesh.RecalculateNormals();

                yield return null;
            }
        }
    }
}
