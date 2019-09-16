using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor;
using System;

public class RunwayUtils
{
  public static string UppercaseFirstEach(string s)
  {
    char[] a = s.ToLower().ToCharArray();

    for (int i = 0; i < a.Length; i++)
    {
      a[i] = i == 0 || a[i - 1] == ' ' ? char.ToUpper(a[i]) : a[i];

    }

    return new string(a);
  }
  public static string SplitCamelCase(string str)
  {
    return Regex.Replace(
        Regex.Replace(
            str,
            @"(\P{Ll})(\P{Ll}\p{Ll})",
            "$1 $2"
        ),
        @"(\p{Ll})(\P{Ll})",
        "$1 $2"
    );
  }

  public static string FormatFieldName(string fieldName)
  {
    return UppercaseFirstEach(SplitCamelCase(fieldName).Replace("_", ""));
  }

  // Convert texture to PNG bytes.
  //
  // Note: Encoding the texture to PNG is a bit more complicated than just calling the .EncodeToPNG() method,
  // because we need to handle the cases in which the texture has not been marked as readable.
  // So we first render the texture on a RenderTexture, then copy the pixels of the RenderTexture 
  // to a new Texture2D, then encode to PNG. 
  public static byte[] TextureToPNG(Texture2D tex, int width, int height)
  {
    RenderTexture tempRT = RenderTexture.GetTemporary(
                    tex.width,
                    tex.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);
    Graphics.Blit(tex, tempRT);
    RenderTexture previous = RenderTexture.active;
    RenderTexture.active = tempRT;
    Texture2D tempTexture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB24, false);
    tempTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
    tempTexture.Apply();
    if (tempTexture.width != width || tempTexture.height != height) ScaleTexture(tempTexture, width, height);
    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(tempRT);
    return tempTexture.EncodeToPNG();
  }

  public static Texture2D CameraToTexture(Camera cam, int width, int height)
  {
    RenderTexture prevActiveRT = RenderTexture.active;
    RenderTexture prevCameraRT = cam.targetTexture;
    RenderTexture renderRT = RenderTexture.GetTemporary(
                    width,
                    height,
                    24,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);
    RenderTexture.active = renderRT;
    cam.targetTexture = renderRT;
    cam.Render();
    Texture2D tempTexture = new Texture2D(renderRT.width, renderRT.height, TextureFormat.RGB24, false);
    tempTexture.ReadPixels(new Rect(0, 0, renderRT.width, renderRT.height), 0, 0);
    tempTexture.Apply();
    RenderTexture.ReleaseTemporary(renderRT);
    RenderTexture.active = prevActiveRT;
    cam.targetTexture = prevCameraRT;
    return tempTexture;
  }

  // Taken from: https://pastebin.com/qkkhWs2J
  public static void ScaleTexture(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
  {
    Rect texR = new Rect(0, 0, width, height);
    _gpu_scale(tex, width, height, mode);
    tex.Resize(width, height);
    tex.ReadPixels(texR, 0, 0, true);
    tex.Apply(true);        //Remove this if you hate us applying textures for you :)
  }

  static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
  {
    //We need the source texture in VRAM because we render with it
    src.filterMode = fmode;
    src.Apply(true);

    //Using RTT for best quality and performance. Thanks, Unity 5
    RenderTexture rtt = new RenderTexture(width, height, 32);

    //Set the RTT in order to render to it
    Graphics.SetRenderTarget(rtt);

    //Setup 2D matrix in range 0..1, so nobody needs to care about sized
    GL.LoadPixelMatrix(0, 1, 1, 0);

    //Then clear & draw the texture to fill the entire RTT.
    GL.Clear(true, true, new Color(0, 0, 0, 0));
    Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
  }

  public static string Dropdown(string currentOption, string[] options)
  {
    int selectedIndex = 0;
    for (var i = 0; i < options.Length; i++)
    {
      if (options[i].Equals(currentOption))
      {
        selectedIndex = i;
      }
    }
    int newIndex = EditorGUILayout.Popup(selectedIndex, options);
    return options[newIndex];
  }

  public static bool IsAnInteger(float val)
  {
    return Mathf.Approximately(val - Mathf.Round(val), 0);
  }

  // from: https://answers.unity.com/questions/33597/is-it-possible-to-create-a-tag-programmatically.html
  public static void AddTag(string tag)
  {
    UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
    if ((asset != null) && (asset.Length > 0))
    {
      SerializedObject so = new SerializedObject(asset[0]);
      SerializedProperty tags = so.FindProperty("tags");

      for (int i = 0; i < tags.arraySize; ++i)
      {
        if (tags.GetArrayElementAtIndex(i).stringValue == tag)
        {
          return;
        }
      }

      Debug.Log(tags.arraySize);
      Debug.Log(tags.GetArrayElementAtIndex(0).stringValue);
      tags.InsertArrayElementAtIndex(tags.arraySize - 1);
      tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
      so.ApplyModifiedProperties();
      so.Update();
    }
  }
  public static IEnumerator LoadTexture(string url, Action<Texture2D> callback)
  {
    using (WWW www = new WWW(url))
    {
      // Wait for download to complete
      yield return www;
      callback(www.texture);
    }
  }

  public static float GenerateNormalRandom(float mean, float sigma)
  {
    System.Random rand = new System.Random();
    double u1 = 1.0 - rand.NextDouble();
    double u2 = 1.0 - rand.NextDouble();
    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    return (float) (mean + sigma * randStdNormal);
  }

  public static float[] RandomVector(int length, float mean, float sigma)
  {
    float[] vec = new float[length];
    for (int i = 0; i < length; i++)
    {
      vec[i] = RunwayUtils.GenerateNormalRandom(mean, sigma);
    }
    return vec;
  }

  public static Texture MakeTexture(int width, int height, Color col)
  {
    Color[] pix = new Color[width * height];
    for (int i = 0; i < pix.Length; ++i)
    {
      pix[i] = col;
    }
    Texture2D result = new Texture2D(width, height);
    result.SetPixels(pix);
    result.Apply();
    return result;
  }
}
