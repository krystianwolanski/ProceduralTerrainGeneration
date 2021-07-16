using System;
using System.Collections;
using System.Collections.Generic;
using Assets.OsmGenerator.Scripts.Serialization;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.OsmGenerator.Scripts
{
    [RequireComponent(typeof(MapReader))]
    class ForestMaker : InfrastructureBehaviour
    {
        [SerializeField]
        private List<GameObject> treeModels;
        IEnumerator Start()
        {
            map = GetComponent<MapReader>();
            while (!map.IsReady)
            {
                yield return null;
            }

            GenerateForestMesh();
        }

        public void GenerateForest(GameTileExtremePoints gameTileExtremePoints)
        {
            var randomX = UnityEngine.Random.Range(gameTileExtremePoints.DownLeftPoint.x, gameTileExtremePoints.UpRightPoint.x);
            var randomZ = UnityEngine.Random.Range(gameTileExtremePoints.DownLeftPoint.z, gameTileExtremePoints.UpRightPoint.z);

            var ray = new Ray(new Vector3(randomX, 20, randomZ), Vector3.down);
     
            if(Physics.Raycast(ray, out RaycastHit hitInfo, 40, 1 << LayerMask.NameToLayer("Forest")))
            {
                var randomTreeIndex = UnityEngine.Random.Range(0, treeModels.Count); 
                var tree = Instantiate(treeModels[randomTreeIndex], hitInfo.point, Quaternion.identity);
                tree.transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
            }
        }

        public void GenerateForestMesh()
        {

            foreach (var way in map.Ways.FindAll(w => w.IsNatural && w.NodeIDs.Count > 1))
            {
               
                GameObject go = new GameObject();
                go.layer = LayerMask.NameToLayer("Forest");

                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshCollider mc = go.AddComponent<MeshCollider>();

                
                var vectors = new List<Vector3>();

                for (int i = 0; i < way.NodeIDs.Count; i++)
                {
                    var n = map.Nodes[way.NodeIDs[i]] - map.Bounds.Centre;
                    vectors.Add(n);
                }

                var vectors2D = new List<Vector2>();

                foreach(var ve in vectors)
                {
                    vectors2D.Add(new Vector2(ve.x, ve.z));
                }
                
                var triangulator = new Triangulator(vectors2D.ToArray());
                var indicies = triangulator.Triangulate();

                mf.mesh.vertices = vectors.ToArray();
                mf.mesh.triangles = indicies;
                mc.sharedMesh = mf.mesh;
            }
        }
    }
}
