using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OwnGrid : MonoBehaviour
{
    public int xSize, zSize;
    public int Seed = 0;
    private Vector3[] vertices;
    private int[] triangles;
    private Mesh mesh;

  
    private void Awake()
    {       
        Generate();
        //StartCoroutine(GenerateWaiting());
        
    }
    //private void Update()
    //{
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    mesh.RecalculateNormals();
    //}

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                
                float y1 = Mathf.PerlinNoise((x + Seed) * 0.01f, (z+ Seed) * 0.01f) * 100f;
                float y2 = Mathf.PerlinNoise((x + Seed) * 0.07f, (z+ Seed) * 0.07f) * 3f;
                float y3 = Mathf.PerlinNoise((x + Seed) * 0.03f, (z + Seed) * 0.03f) * 3f;

                //float finalY = Mathf.Lerp(y1, y2, y3);
                //float finalY = Mathf.Clamp(y1 + y2 + y3, 0, 1); 
                float finalY = (y1 + y2 + y3) / 3;
                //Debug.Log(finalY);
                vertices[i] = new Vector3(x,finalY,z);
                
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];
        for(int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
       
                tris += 6;

           
            }
            vert++;   
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    private IEnumerator GenerateWaiting()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {

                float y1 = Mathf.PerlinNoise((x + Seed) * 0.01f, (z + Seed) * 0.01f) * 100f;
                float y2 = Mathf.PerlinNoise((x + Seed) * 0.07f, (z + Seed) * 0.07f) * 3f;
                float y3 = Mathf.PerlinNoise((x + Seed) * 0.03f, (z + Seed) * 0.03f) * 3f;

                //float finalY = Mathf.Lerp(y1, y2, y3);
                //float finalY = Mathf.Clamp(y1 + y2 + y3, 0, 1); 
                float finalY = (y1 + y2 + y3) / 3;
                //Debug.Log(finalY);
                vertices[i] = new Vector3(x, finalY, z);

            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;

                tris += 6;

                yield return new WaitForSeconds(.001f);
            }
            vert++;
        }

    }
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }

}
