using Assets.OsmGenerator.Scripts;
using Assets.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RoadMaker))]
public class TerrainControllerSimple : MonoBehaviour {

    [SerializeField]
    private GameObject terrainTilePrefab = null;
    [SerializeField]
    private Vector3 terrainSize = new Vector3(20, 1, 20);
    [SerializeField]
    private Gradient gradient;
    [SerializeField]
    private float noiseScale = 3, cellSize = 1;
    [SerializeField]
    private int radiusToRender = 5;
    [SerializeField]
    private Transform[] gameTransforms;
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private bool deactiveOldTiles = false;

    private Vector2 startOffset;
    private Dictionary<Vector2, GameObject> terrainTiles = new Dictionary<Vector2, GameObject>();
    
    private int terrainTreeIndex = 0;

    private Vector2 previousCenterTiles;
    private List<GameObject> previousTileObjects = new List<GameObject>();

    [SerializeField]
    private GameObject mapRenderPrefab = null;

    private RoadMaker roadMaker;
    private ForestMaker forestMaker;
    private BarrierMaker barrierMaker;
    private FloraGenerator floraGenerator;
    private BuildingMaker buildingMaker;

    private void Start() {
        InitialLoad();

        mapRenderPrefab = Instantiate(mapRenderPrefab);
        roadMaker = mapRenderPrefab.GetComponent<RoadMaker>();
        forestMaker = mapRenderPrefab.GetComponent<ForestMaker>();
        barrierMaker = mapRenderPrefab.GetComponent<BarrierMaker>();
        floraGenerator = mapRenderPrefab.GetComponent<FloraGenerator>();
        buildingMaker = mapRenderPrefab.GetComponent<BuildingMaker>();
    }

    public void InitialLoad() {
        DestroyTerrain();

        startOffset = new Vector2(Random.Range(0f, 256f), Random.Range(0f, 256f));
    }

    private void Update() {
        Vector2 playerTile = TileFromPosition(playerTransform.position);
        List<Vector2> centerTiles = new List<Vector2>();
        centerTiles.Add(playerTile);

        if (previousCenterTiles == null || !previousCenterTiles.Equals(playerTile)) 
        {    
            int radius = radiusToRender;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                    if (!terrainTiles.ContainsKey(new Vector2(playerTile.x + i, (int)playerTile.y + j)))
                        CreateTile((int)playerTile.x + i, (int)playerTile.y + j);

                roadMaker.UpdateRoads();
                buildingMaker.RenderBuildings();

                for (;terrainTreeIndex < terrainTiles.Count; terrainTreeIndex++)
                {
                    floraGenerator.GenerateFlora(terrainTiles.ElementAt(terrainTreeIndex).Value);
                }


            if (deactiveOldTiles)
            {
           
            }
        }

        previousCenterTiles = new Vector2(playerTile.x, playerTile.y);
    }

    //Helper methods below

    private GameTileExtremePoints GetTileExtremePoints(GameObject gameObject)
    {
        var generateMeshSimple = gameObject.GetComponent<GenerateMeshSimple>();
        var downLeftPoint = gameObject.transform.position - new Vector3(generateMeshSimple.TerrainSize.x / 2, 0 , generateMeshSimple.TerrainSize.z / 2);
        var upRightPoint = gameObject.transform.position + new Vector3(generateMeshSimple.TerrainSize.x / 2, 0, generateMeshSimple.TerrainSize.z / 2);

        return new GameTileExtremePoints() { DownLeftPoint = downLeftPoint, UpRightPoint = upRightPoint };

    }

    private void CreateTile(int xIndex, int yIndex) {
       
            GameObject terrain = Instantiate(
            terrainTilePrefab,
            new Vector3(terrainSize.x * xIndex, terrainSize.y, terrainSize.z * yIndex),
            Quaternion.identity
            );

            terrain.name = TrimEnd(terrain.name, "(Clone)") + " [" + xIndex + " , " + yIndex + "]";
            terrain.layer = LayerMask.NameToLayer("Ground");

            terrainTiles.Add(new Vector2(xIndex, yIndex), terrain);

            GenerateMeshSimple gm = terrain.GetComponent<GenerateMeshSimple>();
            gm.TerrainSize = terrainSize;
            gm.Gradient = gradient;
            gm.NoiseScale = noiseScale;
            gm.CellSize = cellSize;
            gm.NoiseOffset = NoiseOffset(xIndex, yIndex);
            gm.Generate();

            var extremePoints = GetTileExtremePoints(terrain);
            forestMaker.GenerateForest(extremePoints);
    }

    private Vector2 NoiseOffset(int xIndex, int yIndex) {
        Vector2 noiseOffset = new Vector2(
            (xIndex * noiseScale + startOffset.x) % 256,
            (yIndex * noiseScale + startOffset.y) % 256
        );

        if (noiseOffset.x < 0)
            noiseOffset = new Vector2(noiseOffset.x + 256, noiseOffset.y);
        if (noiseOffset.y < 0)
            noiseOffset = new Vector2(noiseOffset.x, noiseOffset.y + 256);
        return noiseOffset;
    }

    private Vector2 TileFromPosition(Vector3 position) {
        return new Vector2(Mathf.FloorToInt(position.x / terrainSize.x + .5f), Mathf.FloorToInt(position.z / terrainSize.z + .5f));
    }

    public void DestroyTerrain() {
        foreach (KeyValuePair<Vector2, GameObject> kv in terrainTiles)
            Destroy(kv.Value);
        terrainTiles.Clear();
    }

    private static string TrimEnd(string str, string end) {
        if (str.EndsWith(end))
            return str.Substring(0, str.LastIndexOf(end));
        return str;
    }

}