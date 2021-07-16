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
    public class RoadMaker : InfrastructureBehaviour
    {
        [SerializeField]
        private int ExtraSpanWiseHeightSamples = 64;

        [SerializeField]
        private Material track;

        [SerializeField]
        private Material roadMaterial;

        [SerializeField]
        private GameObject streetLamp;
        
        const float CastPullUp = 10f;
        const float CastRayDown = 500f;
        List<GameObject> listOfObjects = new List<GameObject>();
        List<GameObject> listOfLamps = new List<GameObject>();

        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }
        }

        public void GenerateLamps()
        {
            foreach (var way in map.Ways.FindAll(w => w.IsRoad))
            {
                for (int i = 1; i < way.NodeIDs.Count; i++)
                {
                    OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                    OsmNode p2 = map.Nodes[way.NodeIDs[i]];

                    Vector3 s1 = p1 - map.Bounds.Centre + (Vector3.up * 8f);
                    Vector3 s2 = p2 - map.Bounds.Centre + (Vector3.up * 8f);

                    var highWayWidth = GetHighwayWidth(way.RoadTypeName);

                    foreach (var pt in PointFeeder(s1, s2, 20f))
                    {
                        var diff2 = (s2 - pt).normalized;
                        var cross2 = Vector3.Cross(diff2, Vector3.up) * (highWayWidth + 3.5f);

                        Vector3 left = pt + cross2;
                        left.z += 5;
                        RaycastHit raycastHit;

                        var raycastsHits = Physics.RaycastAll(new Ray(left, Vector3.down), 100);

                        if(raycastsHits.Length != 1 || raycastsHits[0].collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                        {
                            continue;
                        }
                       
                        left = raycastsHits[0].point;
                        var newLamp = Instantiate(streetLamp, left, UnityEngine.Quaternion.identity);
                        listOfLamps.Add(newLamp);             
                    }
                }
            }
        }

        public void UpdateRoads()
        {
            listOfLamps.ForEach(obj =>
            {
                Destroy(obj);
            });
            listOfLamps = new List<GameObject>();


            listOfObjects.ForEach(obj =>
            {
                obj.GetComponent<MeshCollider>().enabled = false;
                Destroy(obj);
            });

            listOfObjects = new List<GameObject>();

            foreach (var way in map.Ways.FindAll(w => w.IsRoad && w.Deleted == false))
            {
                GameObject go = new GameObject();
                go.name = "road";
                go.layer = LayerMask.NameToLayer("Road");

                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.mesh = new Mesh();
                
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                MeshCollider mc = go.AddComponent<MeshCollider>();
                mr.material = GetHighWayMaterial(way.RoadTypeName);

                List<Vector3> verts = new List<Vector3>();
                List<int> tris0 = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                bool first = true;

                Vector3 prevLeft = Vector3.zero;
                Vector3 prevRight = Vector3.zero;

                int prevNTopLeft = -1;

                Vector3 prevPosition = Vector3.zero;
                var uvStep = 0f;
                
                bool wayGenerated = true;

                var innerListOfLamps = new List<GameObject>();
                for (int i = 1; i < way.NodeIDs.Count; i++)
                {
                    OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                    OsmNode p2 = map.Nodes[way.NodeIDs[i]];

                    Vector3 s1 = p1 - map.Bounds.Centre + (Vector3.up * 8f);
                    Vector3 s2 = p2 - map.Bounds.Centre + (Vector3.up * 8f);

                    var highWayWidth = GetHighwayWidth(way.RoadTypeName);

                    var index = 1;
                    foreach (var pt in PointFeeder(s1, s2, 1.5f))
                    {
                        
                        var diff = (s2 - pt).normalized;
                        var cross = Vector3.Cross(diff, Vector3.up) * highWayWidth;

                        Vector3 left = pt + cross;
                        Vector3 right = pt - cross;

                        Ray rayLeft = new Ray(left + Vector3.up * CastPullUp, Vector3.down);
                        Ray rayRight = new Ray(right + Vector3.up * CastPullUp, Vector3.down);
                        RaycastHit rchLeft, rchRight;

                        

                        bool hitLeft = Physics.Raycast(rayLeft, out rchLeft, CastRayDown, 1 << LayerMask.NameToLayer("Ground"));  
                        bool hitRight = Physics.Raycast(rayRight, out rchRight, CastRayDown, 1 << LayerMask.NameToLayer("Ground"));

                        if (hitLeft && hitRight)
                        {
                            left = rchLeft.point;
                            right = rchRight.point;

                            float highestPoint = Mathf.Max(left.y, right.y);
                            float originalHighestPoint = highestPoint;


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

                            left.y += highestPoint - originalHighestPoint;
                            right.y += highestPoint - originalHighestPoint;          
                            


                            if (index % 20 == 0)
                            {
                                var diff3 = (s2 - left).normalized;
                                var lampCross = Vector3.Cross(diff3, Vector3.up) * .4f;
                                var lamp = Instantiate(streetLamp, left + lampCross, UnityEngine.Quaternion.identity);
                                innerListOfLamps.Add(lamp);
                            }
                            index++;

                            left += Vector3.up * .15f;
                            right += Vector3.up * .15f;

                            int nTopLeft = verts.Count;

                            verts.Add(left);
                            verts.Add(right);

                            uvs.Add(new Vector2(uvStep, 0));
                            uvs.Add(new Vector2(uvStep, 1));

                            uvStep += .3f;
                            if (!first)
                            {
                                tris0.Add(prevNTopLeft);
                                tris0.Add(nTopLeft);
                                tris0.Add(prevNTopLeft + 1);

                                tris0.Add(nTopLeft);
                                tris0.Add(nTopLeft + 1);
                                tris0.Add(prevNTopLeft + 1);
                            }

                            prevPosition = pt;

                            prevLeft = left;
                            prevRight = right;

                            first = false;
                            prevNTopLeft = nTopLeft;
                        }
                        else
                        {
                            wayGenerated = false;
                            break;
                        }
                    }
                }

                if(wayGenerated == false)
                {
                    listOfObjects.Add(go);
                    listOfLamps.AddRange(innerListOfLamps);
                }
                else
                {
                    way.Deleted = true;
                }

                mf.mesh.vertices = verts.ToArray();
                mf.mesh.uv = uvs.ToArray();
                mf.mesh.triangles = tris0.ToArray();

                mf.mesh.RecalculateBounds();
                mf.mesh.RecalculateNormals();

                mc.sharedMesh = mf.mesh;

            }
        }

        public float GetHighwayWidth(string highwayName)
        {
            switch (highwayName)
            {
                case "track":
                    return 3;
                case "residential":
                    return 3;
                case "service":
                    return 2f;
                default:
                    return 3;
            }
        }

        public Material GetHighWayMaterial(string highwayName)
        {
            switch (highwayName)
            {
                case "track":
                    return track;
                case "residential":
                    return track;
                default:
                    return track;
            }
        }
    }
}
