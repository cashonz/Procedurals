using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorleyTest : MonoBehaviour
{
    public int worleyHeight = 100;
    public int worleyWidth = 100;
    [Range(1, 20)]
    public int numCellsHeight = 4;
    [Range(1, 20)]
    public int numCellsWidth = 4;
    [Range(0.0f, 1.0f)]
    public int nClosestPoint = 0;
    public bool autoUpdate;
    public Renderer textureRenderer;

    public void GenerateWorleyTexture()
    {
        float[,] worleyTest = WorleyNoise.GenerateWorleyNoiseMap(worleyHeight, worleyWidth, numCellsWidth, numCellsHeight, nClosestPoint);
        Texture2D tex = new Texture2D(worleyHeight, worleyWidth);
        Color[] colorMap = new Color[worleyHeight * worleyWidth];

        for (int y = 0; y < worleyHeight; y++) {
			for (int x = 0; x < worleyWidth; x++) {
				colorMap [y * worleyWidth + x] = Color.Lerp(Color.black, Color.white, worleyTest[x, y]);
			}
		}
        
        tex.SetPixels(colorMap);
        tex.Apply();

        textureRenderer.sharedMaterial.mainTexture = tex;
    }
}
