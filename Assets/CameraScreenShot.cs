using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScreenShot : MonoBehaviour
{
    [Header("Screenshot settings")]
    public KeyCode key = KeyCode.I;
    public int width = 1920;
    public int height = 1080;

    private Camera myCamera;
    private bool takeScreenshot = false;

    // Start is called before the first frame update
    void Start()
    {
        myCamera = GetComponent<Camera>();
    }

    private void OnPostRender() {
        if (takeScreenshot) {
            takeScreenshot = false;

            RenderTexture renderTexture = myCamera.targetTexture;
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            System.IO.File.WriteAllBytes(Application.dataPath + "/cameraScreenshot.png", renderResult.EncodeToPNG());

            Debug.Log("Saved Screenshot");

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key)) {
            myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
            takeScreenshot = true;
        }
    }
}
