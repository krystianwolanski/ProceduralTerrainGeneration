using System.Collections;
using System.Collections.Generic;
using Assets.OsmGenerator.Scripts.Serialization;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts
{
    [RequireComponent(typeof(MapReader))]
    class BuildingMaker : InfrastructureBehaviour
    {

        [SerializeField]
        private Material glass;
        [SerializeField]
        private Material brick;
        [SerializeField]
        private Material concrete;
        [SerializeField]
        private List<Material> defaultMaterials;
        [SerializeField]
        private Material roof;
        [SerializeField]
        private Material window;

        [SerializeField]
        [Range(0,.5f)]
        private float xFraction;
        
        [SerializeField]
        [Range(0,.5f)]
        private float yFraction;

        [SerializeField]
        [Range(0,100)]
        private int changeToRenderWindow;

        private List<Vector3> vectors;
        private List<Vector3> normals;
        private List<Vector2> uvs;
        private List<int> indicies;
        private List<int> indiciesRoof;
        private List<int> indiciesWindow;

        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }

        }

        public void RenderBuildings()
        {
            foreach (var way in map.Ways.FindAll(w => w.IsBuilding && w.NodeIDs.Count > 1 && w.Deleted == false))
            {
                List<Vector3> vectors = new List<Vector3>();
                var noTerrain = false;

                var lowestY = 1000f;
                
                for (int i = 0; i < way.NodeIDs.Count; i++)
                {
                    var p1 = map.Nodes[way.NodeIDs[i]] - map.Bounds.Centre;

                    if(Physics.Raycast(new Ray(p1 + (Vector3.up * 20f) ,Vector3.down), out RaycastHit hit,30f,1 << LayerMask.NameToLayer("Ground")) == false)
                    {
                        noTerrain = true;
                        break;
                    }

                    if (hit.point.y < lowestY)
                    {

                        lowestY = hit.point.y;
                    }
                }

                if (noTerrain) continue;

                if (way.HasLevels) RenderBuildingWithoutRoof(way, lowestY);
                else RenderBuildingWithRoof(way, lowestY);

                way.Deleted = true;
            }
        }

        private void RenderBuildingWithoutRoof(OsmWay way, float minY)
        {
            GameObject go = new GameObject();
            go.transform.position += new Vector3(0, minY, 0);
            go.layer = LayerMask.NameToLayer("Building");

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indicies = new List<int>();

            mr.material = GetMaterial(way.BuildingMaterialName);

            for (int i = 1; i < way.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                OsmNode p2 = map.Nodes[way.NodeIDs[i]];


                Vector3 v1 = p1 - map.Bounds.Centre;
                Vector3 v2 = p2 - map.Bounds.Centre;

                Vector3 v3 = v1 + new Vector3(0, way.Height, 0);
                Vector3 v4 = v2 + new Vector3(0, way.Height, 0);

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
            mf.mesh.triangles = indicies.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.uv = uvs.ToArray();
            mc.sharedMesh = mf.mesh;
        }

        private void RenderWallWithWindow(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            vectors.Add(v1); //0

            vectors.Add(v3); //1

            var lerp = Vector3.Lerp(v1, v2, xFraction);
            lerp.y = Mathf.Lerp(v1.y, v3.y, yFraction);
            vectors.Add(lerp); //2


            lerp = Vector3.Lerp(v1, v2, 1 - xFraction);
            lerp.y = Mathf.Lerp(v1.y, v3.y, yFraction);
            vectors.Add(lerp); //3

            vectors.Add(v2); //4

            lerp = Vector3.Lerp(v1, v2, xFraction);
            lerp.y = Mathf.Lerp(v1.y, v3.y, 1 - yFraction);
            vectors.Add(lerp); //5

            vectors.Add(v4); //6

            lerp = Vector3.Lerp(v1, v2, 1 - xFraction);
            lerp.y = Mathf.Lerp(v1.y, v3.y, 1 - yFraction);
            vectors.Add(lerp); //7

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(xFraction, yFraction));
            uvs.Add(new Vector2(1 - xFraction, yFraction));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(xFraction, 1 - yFraction));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1 - xFraction, 1 - yFraction));

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            int idx0, idx1, idx2, idx3, idx4, idx5, idx6, idx7;
            int lastVectorsIndex = vectors.Count - 1;

            idx0 = lastVectorsIndex - 7;
            idx1 = lastVectorsIndex - 6;
            idx2 = lastVectorsIndex - 5;
            idx3 = lastVectorsIndex - 4;
            idx4 = lastVectorsIndex - 3;
            idx5 = lastVectorsIndex - 2;
            idx6 = lastVectorsIndex - 1;
            idx7 = lastVectorsIndex;

            indicies.Add(idx0); indicies.Add(idx2);
            indicies.Add(idx1); indicies.Add(idx1);
            indicies.Add(idx2); indicies.Add(idx0);

            indicies.Add(idx0); indicies.Add(idx3);
            indicies.Add(idx2); indicies.Add(idx2);
            indicies.Add(idx3); indicies.Add(idx0);

            indicies.Add(idx0); indicies.Add(idx4);
            indicies.Add(idx3); indicies.Add(idx3);
            indicies.Add(idx4); indicies.Add(idx0);

            indicies.Add(idx1); indicies.Add(idx2);
            indicies.Add(idx5); indicies.Add(idx5);
            indicies.Add(idx2); indicies.Add(idx1);

            indicies.Add(idx1); indicies.Add(idx5);
            indicies.Add(idx6); indicies.Add(idx6);
            indicies.Add(idx5); indicies.Add(idx1);

            indicies.Add(idx6); indicies.Add(idx7);
            indicies.Add(idx4); indicies.Add(idx4);
            indicies.Add(idx7); indicies.Add(idx6);

            indicies.Add(idx7); indicies.Add(idx3);
            indicies.Add(idx4); indicies.Add(idx4);
            indicies.Add(idx3); indicies.Add(idx7);

            indicies.Add(idx5); indicies.Add(idx7);
            indicies.Add(idx6); indicies.Add(idx6);
            indicies.Add(idx7); indicies.Add(idx5);


            // roof
            indiciesRoof.Add(0); indiciesRoof.Add(idx1);
            indiciesRoof.Add(idx6); indiciesRoof.Add(idx6);
            indiciesRoof.Add(idx1); indiciesRoof.Add(0);

            //window
            indiciesWindow.Add(idx2); indiciesWindow.Add(idx3);
            indiciesWindow.Add(idx5); indiciesWindow.Add(idx5);
            indiciesWindow.Add(idx3); indiciesWindow.Add(idx2);

            indiciesWindow.Add(idx5); indiciesWindow.Add(idx3);
            indiciesWindow.Add(idx7); indiciesWindow.Add(idx7);
            indiciesWindow.Add(idx3); indiciesWindow.Add(idx5);
        }
        
        private void RenderWallWithoutWindow(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
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

            // fourth         v2, v4, v3
            indicies.Add(idx2);
            indicies.Add(idx4);
            indicies.Add(idx3);

            // And now the roof triangles
            indiciesRoof.Add(0);
            indiciesRoof.Add(idx3);
            indiciesRoof.Add(idx4);

            // Don't forget the upside down one!
            indiciesRoof.Add(idx4);
            indiciesRoof.Add(idx3);
            indiciesRoof.Add(0);
        }
        
        private void RenderBuildingWithRoof(OsmWay way, float minY)
        {
            GameObject go = new GameObject();
            go.transform.position += new Vector3(0, minY, 0);
            go.layer = LayerMask.NameToLayer("Building");

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();

            vectors = new List<Vector3>();
            normals = new List<Vector3>();
            uvs = new List<Vector2>();
            indicies = new List<int>();
            indiciesRoof = new List<int>();
            indiciesWindow = new List<int>();

            mr.material = GetMaterial(way.BuildingMaterialName);
            mr.materials = new Material[]
            {
                GetMaterial(way.BuildingMaterialName),
                roof,
                window
            };
            var centerVector = GetAverageVector(way) - map.Bounds.Centre;
            centerVector.y = way.Height + 8;

            vectors.Add(centerVector);
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 1; i < way.NodeIDs.Count; i++)
            {

                OsmNode p1 = map.Nodes[way.NodeIDs[i - 1]];
                OsmNode p2 = map.Nodes[way.NodeIDs[i]];


                Vector3 v1 = p1 - map.Bounds.Centre;
                Vector3 v2 = p2 - map.Bounds.Centre;
                Vector3 v3 = v1 + new Vector3(0, way.Height, 0);
                Vector3 v4 = v2 + new Vector3(0, way.Height, 0);

               
                if (Random.Range(1, 100) <= changeToRenderWindow) RenderWallWithWindow(v1,v2,v3,v4);
                else RenderWallWithoutWindow(v1,v2,v3,v4);
            }
            

            mf.mesh.subMeshCount = 3;

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.SetTriangles(indicies.ToArray(),0);
            mf.mesh.SetTriangles(indiciesRoof.ToArray(),1);
            mf.mesh.SetTriangles(indiciesWindow.ToArray(),2);
            mf.mesh.normals = normals.ToArray();
            mf.mesh.uv = uvs.ToArray();
            

            mc.sharedMesh = mf.mesh;
        }

        private Vector3 GetAverageVector(OsmWay way)
        {
            Vector3 sumVectors = new Vector3();

            for (int i = 0; i < way.NodeIDs.Count; i++)
            {
                sumVectors += map.Nodes[way.NodeIDs[i]];
            }

            return (sumVectors / way.NodeIDs.Count);
        }
        private Material GetMaterial(string materialName)
        {
            switch (materialName)
            {
                case "glass":
                    return glass;
                case "brick":
                    return brick;
                case "concrete":
                    return concrete;
                default:
                    {
                        var random = Random.Range(0, defaultMaterials.Count);
                        return defaultMaterials[random];
                    }
            }
        }
    }
}
