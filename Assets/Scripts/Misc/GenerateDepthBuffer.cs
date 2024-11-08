using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDepthBuffer : MonoBehaviour
{
    [SerializeField]
    Shader waterShader;
    private new Camera camera;
    private RenderTexture renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        Camera thisCamera = GetComponent<Camera>();
        renderTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);
        thisCamera.depthTextureMode = DepthTextureMode.Depth;
        Shader.SetGlobalTexture("_CameraDepthTexture", renderTexture);
    }

}
