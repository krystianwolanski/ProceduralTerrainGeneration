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
        [SerializeField]
        private int ExtraSpanWiseHeightSamples = 4;

        public Material roadMaterial;
        const float CastPullUp = 10f;
        const float CastRayDown = 500f;
        List<GameObject> listOfObjects = new List<GameObject>();
        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }
        }

        public void UpdateRoads()
        {
            listOfObjects.ForEach(obj =>
            {
                Destroy(obj);
            });

            foreach (var way in map.Ways.FindAll(w => w.IsRoad))
            {

                GameObject go = new GameObject();
                listOfObjects.Add(go);

                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();

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

                    Vector3 s1 = p1 - map.Bounds.Centre + (Vector3.up * 8f);
                    Vector3 s2 = p2 - map.Bounds.Centre + (Vector3.up * 8f);

                    
                    foreach (var pt in PointFeeder(s1, s2, 1.5f))
                    {

                        var diff2 = (s2 - pt).normalized;
                        var cross2 = Vector3.Cross(diff2, Vector3.up) * 2.0f;

                        Vector3 left = pt + cross2;
                        Vector3 right = pt - cross2;



                        Ray rayLeft = new Ray(left + Vector3.up * CastPullUp, Vector3.down);
                        Ray rayRight = new Ray(right + Vector3.up * CastPullUp, Vector3.down);
                        RaycastHit rchLeft, rchRight;
                        bool hitLeft = Physics.Raycast(rayLeft, out rchLeft, CastRayDown);
                        bool hitRight = Physics.Raycast(rayRight, out rchRight, CastRayDown);

                        if (hitLeft && hitRight)
                        {
                            left = rchLeft.point;

                            right = rchRight.point;

                            if (true)
                            {
                                float highestPoint = Mathf.Max(left.y, right.y);

                                float originalHighestPoint = highestPoint;

                                // To make sure we don't have ground poking through, we can
                                // check spanwise going across, and adjust upwards more.
                                for (int spanWiseCheckNo = 1; spanWiseCheckNo < ExtraSpanWiseHeightSamples; spanWiseCheckNo++)
                                {
                               
                                    float fraction = (float)spanWiseCheckNo / ExtraSpanWiseHeightSamples;

                                    Ray rayCenter = new Ray(Vector3.Lerp(left, right, fraction) + Vector3.up * CastPullUp, Vector3.down);
                                    RaycastHit rch;
                                    
                                    if (Physics.Raycast(rayCenter, out rch, CastRayDown))
                                    {
                                        if (rch.point.y > highestPoint)
                                        {
                                            highestPoint = rch.point.y;
                                        }
                                    }
                                }
                                

                                if(highestPoint > originalHighestPoint)
                                {
                                    left.y += highestPoint - originalHighestPoint;
                                    right.y += highestPoint - originalHighestPoint;
                                }

                            }
                            
                            left += Vector3.up * .15f;
                            right += Vector3.up * .15f;

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
                            float uAdvance = Vector3.Distance(pt, prevPosition) / 6f;

                            u -= uAdvance;

                            prevPosition = pt;

                            prevLeft = left;
                            prevRight = right;

                            first = false;
                            prevNTopLeft = nTopLeft;
                        }
                    }

                    mf.mesh.subMeshCount = 2;

                    mf.mesh.vertices = verts.ToArray();
                    mf.mesh.uv = uvs.ToArray();
                    mf.mesh.SetTriangles(tris0.ToArray(), 0);
                    mf.mesh.SetTriangles(tris1.ToArray(), 1);

                    mf.mesh.RecalculateBounds();
                    mf.mesh.RecalculateNormals();
                    mr.material = roadMaterial;
                }
            }
        }
    }
}
