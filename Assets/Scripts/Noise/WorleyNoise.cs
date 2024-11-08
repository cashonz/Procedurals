using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorleyNoise
{
    public static float[,] GenerateWorleyNoiseMap(int gridHeight, int gridWidth, int numCellsWidth, int numCellsHeight, int nClosestPoint)
    {
        /*
        ------------------Arrays for storing our points and all pixel values------------------
        */
        Vector2[] points = new Vector2[numCellsWidth * numCellsHeight];
        float[,] worleyMap = new float[gridHeight, gridWidth];

        /*
        ------------------Width and Height of each cell------------------
        */
        int cellHeight = gridHeight / numCellsHeight;
        int cellWidth = gridWidth / numCellsWidth;

        float max = -float.MaxValue;

        /*
        ------------------Randomnize points in cells for the Worley noise------------------
        */
        int pointsIndex = 0;
        for(int x = 0; x < numCellsWidth; x++)
        {
            for(int y = 0; y < numCellsHeight; y++)
            {
                int localRandPosX = Random.Range(0, cellWidth);
                int localRandPosY = Random.Range(0, cellHeight);
                points[pointsIndex] = new Vector2(x * cellWidth + localRandPosX, y * cellHeight + localRandPosY);
                pointsIndex++;
            }
        }

        /*
        ------------------Get the distance of every pixel to the n-th closes point------------------
        */
        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                float[] distanceVals = new float[numCellsWidth * numCellsHeight];
                for(int i = 0; i < points.Length; i++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), points[i]);
                    distanceVals[i] = d;
                }

                Array.Sort(distanceVals);
                worleyMap[x, y] = distanceVals[nClosestPoint];

                if(worleyMap[x, y] > max)
                {
                    max = worleyMap[x, y];
                }
            }
        }

        /*
        ------------------Normalize the values to the range [0,1]------------------
        */
        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                worleyMap[x, y] = worleyMap[x, y] / max;
            }
        }

        return worleyMap;
    }
}
