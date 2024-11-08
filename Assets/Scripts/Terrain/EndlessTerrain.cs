using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float chunkUpdateMovedThreshold = 25f;
    const float squaredChunkUpdateMovedThreshold = chunkUpdateMovedThreshold * chunkUpdateMovedThreshold;
    public LODInfo[] detailLevels;
    public static float viewDist;
    public Transform player;
    public Material grassMaterial;
    public Material groundMaterial;
    public static Vector2 viewerPosition;
    private Vector2 oldViewerPos;
    static GenerateNoiseMap mapGenerator;
    public Transform parent;
    int chunkSize;
    int chunksVisible;
    Dictionary<Vector2, TerrainChunk> terrainDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        viewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        mapGenerator = FindObjectOfType<GenerateNoiseMap>();
        chunkSize = GenerateNoiseMap.chunkSize - 1;
        chunksVisible = Mathf.RoundToInt(viewDist / chunkSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(player.position.x, player.position.z);
        if((oldViewerPos - viewerPosition).sqrMagnitude > squaredChunkUpdateMovedThreshold)
        {
            oldViewerPos = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        /*
        Makes sure we clear terrain chunks when they are out of out viewing distance
        */
        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        /*
        Set all the chunk we actually see this update to be visible
        */
        int currChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = - chunksVisible; yOffset <= chunksVisible; yOffset++)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++)
            {
                Vector2 viewedChunkCoords = new Vector2(currChunkX + xOffset, currChunkY + yOffset);

                if(terrainDict.ContainsKey(viewedChunkCoords))
                {
                    terrainDict[viewedChunkCoords].UpdateChunk();
                    if(terrainDict[viewedChunkCoords].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainDict[viewedChunkCoords]);
                    }
                }
                else
                {
                    terrainDict.Add(viewedChunkCoords, new TerrainChunk(viewedChunkCoords, chunkSize, parent, grassMaterial, groundMaterial, detailLevels)); //add new chunk if we havent already created one
                }
            }
        }
    }

    //Class representing a chunk
    public class TerrainChunk
    {
        GameObject parentObj;
        GameObject grassObj;
        GameObject groundObj;
        Vector2 pos;
        Bounds bounds;
        MeshRenderer grassRenderer;
        MeshRenderer groundRenderer;
        MeshFilter grassFilter;
        MeshFilter groundFilter;
        LODInfo[] detailLevels;
        LODmesh[] lodMeshes;
        MapData mapData;
        bool mapDataRecieved;
        int previousLODIndex = -1;
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material grassMaterial, Material groundMaterial, LODInfo[] detailLevels)
        {
            this.detailLevels = detailLevels;
            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 position3D = new Vector3(pos.x, 0, pos.y);

            parentObj = new GameObject("parentObj");
            grassObj = new GameObject("grassChunk");
            groundObj = new GameObject("groundChunk");
            grassRenderer = grassObj.AddComponent<MeshRenderer>();
            grassFilter = grassObj.AddComponent<MeshFilter>();
            groundRenderer = groundObj.AddComponent<MeshRenderer>();
            groundFilter = groundObj.AddComponent<MeshFilter>();
            grassRenderer.material = grassMaterial;
            groundRenderer.material = groundMaterial;

            parentObj.transform.position = position3D;
            parentObj.transform.parent = parent;
            grassObj.transform.position = position3D;
            grassObj.transform.parent = parentObj.transform;
            groundObj.transform.position = position3D;
            groundObj.transform.parent = parentObj.transform;
            SetVisible(false);

            lodMeshes = new LODmesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODmesh(detailLevels[i].lod, UpdateChunk);
            }
            //shove coord it in here (aka index of chunk/key in dict)? can then manipulate heighmap after it generates the perlin noise.
            //Temporary thing just to test modifying the noise map.
            mapGenerator.RequestMapData(pos, OnMapDataRecieved, coord);
        }

        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true;

            UpdateChunk();
        }

        public void UpdateChunk()
        {
            if(mapDataRecieved)
            {
                float distFromViewer = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = distFromViewer <= viewDist;

                if(visible)
                {
                    int lodIndex = 0;
                    for(int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if(distFromViewer > detailLevels[i].visibleDistThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if(lodIndex != previousLODIndex)
                    {
                        LODmesh lodMesh = lodMeshes[lodIndex];
                        if(lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            grassFilter.mesh = lodMesh.mesh;
                            groundFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            parentObj.SetActive(visible);
        }

        public bool IsVisible()
        {
            return parentObj.activeSelf;
        }
    }

    class LODmesh //TODO
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallBack;

        public LODmesh(int lod, System.Action updateCallBack)
        {
            this.lod = lod;
            this.updateCallBack = updateCallBack;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallBack();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistThreshold;
    }
}
