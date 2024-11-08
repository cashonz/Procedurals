using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
    public static float[,] GeneratePerlinNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossible = 0;
        float amplitude = 1;
        float frequency = 1;

        for(int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossible += amplitude;
            amplitude *= persistance;
        }

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float max = float.MinValue;
        float min = float.MaxValue;
        //"zoom" in on noisemap when we change the scale
        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0; //was 1 before?

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVal * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > max)
                {
                    max = noiseHeight;
                }
                else if(noiseHeight < min)
                {
                    min = noiseHeight;
                }
                
                noiseMap[x, y] = noiseHeight;
            }
        }

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                /* preferred if not generating endless terrain
                noiseMap[x,y] = Mathf.InverseLerp(min, max, noiseMap[x, y]); //normalizes noise map
                */
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossible / 1.5f);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        return noiseMap;
    }
}
