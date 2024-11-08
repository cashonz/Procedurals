using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class GenerateNoiseMap : MonoBehaviour
{
    public const int chunkSize = 239;
    [Range(0,6)] public int LOD;
    public int seed;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]public float percistance;
    [Range(1, 10)]public float lacunarity;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    public int riverWidth = 50; //for testing!!

    MapData GenerateMapData(Vector2 centre, Vector2 chunkIndex)//Remove chunkIndex solution later, only there for testing before real river generation!!
    {
        float[,] noiseMap = PerlinNoise.GeneratePerlinNoiseMap(chunkSize + 2, chunkSize + 2, seed, noiseScale, octaves, percistance, lacunarity, centre + offset);

        //First iteration or river generation, just a straight path
        int riverMiddle = (chunkSize + 1) / 2;

        if(chunkIndex.y == 0)
        {
            for(int y = riverMiddle - riverWidth; y < riverMiddle + riverWidth; y++)
            {
                for(int x = 0; x < chunkSize + 2; x++)
                {
                    noiseMap[x, y] = -50;
                }
            }
        }
        //End first iteration river gen
        return new MapData(noiseMap);
    }

    void OnValidate()
    {
        if(octaves < 0)
        {
            octaves = 0;
        }
    }
    /*
    ------------------------------------------------
    Things for fetching MapData in EndlessTerrain.cs
    ------------------------------------------------
    */
    public void RequestMapData(Vector2 centre, Action<MapData> callback, Vector2 chunkIndex)//Remove chunkIndex solution later, only there for testing before real river generation!!
    {
        ThreadStart threadStart = delegate{
            MapDataThread(centre, callback, chunkIndex);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback, Vector2 chunkIndex)//Remove chunkIndex solution later, only there for testing before real river generation!!
    {
        MapData mapData = GenerateMapData(centre, chunkIndex);
        lock(mapDataThreadInfoQueue){mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));}

    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate{
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock(meshDataThreadInfoQueue){meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));}
    }

    void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> mapThread = mapDataThreadInfoQueue.Dequeue();
                mapThread.callback(mapThread.param);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> mapThread = meshDataThreadInfoQueue.Dequeue();
                mapThread.callback(mapThread.param);
            }
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T param;

        public MapThreadInfo(Action<T> callback, T param)
        {
            this.callback = callback;
            this.param = param;
        }
    }
}
public struct MapData
    {
        public readonly float[,] heightMap;

        public MapData(float[,] heightMap)
        {
            this.heightMap = heightMap;
        }
    }
