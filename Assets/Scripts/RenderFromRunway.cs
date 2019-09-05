using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;


[System.Serializable]
public class ModelData
{
    public string output;
}

public class RenderFromRunway : MonoBehaviour
{
    public RenderTexture rt; // render to this rt from another camera
                             // Start is called before the first frame update
    Camera cam;
    Rect cameraViewRect;
    void Start()
    {
        RenderTexture.active = rt;
        cam = GetComponent<Camera>();
        cameraViewRect = new Rect(cam.rect.xMin * Screen.width, Screen.height - cam.rect.yMax * Screen.height, cam.pixelWidth, cam.pixelHeight);

        StartCoroutine(RenderTexturesCoroutine());
    }

    IEnumerator RenderTexturesCoroutine()
    {
        Debug.Log("performing request");

        var jsonBinary = System.Text.Encoding.UTF8.GetBytes("{}");

        DownloadHandlerBuffer downloadHandlerBuffer = new DownloadHandlerBuffer();

        UploadHandlerRaw uploadHandlerRaw = new UploadHandlerRaw(jsonBinary);
        uploadHandlerRaw.contentType = "application/json";

        UnityWebRequest www =
            new UnityWebRequest("http://localhost:8000/data", "GET", downloadHandlerBuffer, uploadHandlerRaw);

        yield return www.SendWebRequest();

        string image = JsonUtility.FromJson<ModelData>(www.downloadHandler.text).output;
        byte[] img = Convert.FromBase64String(image.Substring(22));
        //File.WriteAllBytes("test_result.png", img);

        Texture2D tex = new Texture2D(640, 360);
        tex.LoadImage(img);
        Graphics.Blit(tex, rt);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator OnPostRender()
    {
        yield return new WaitForEndOfFrame();
        Graphics.DrawTexture(cameraViewRect, rt);
    }
}
