using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class RunwayUtils
{
  public static string UppercaseFirst(string s)
  {
    if (string.IsNullOrEmpty(s))
    {
      return string.Empty;
    }
    char[] a = s.ToCharArray();
    a[0] = char.ToUpper(a[0]);
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
    return UppercaseFirst(SplitCamelCase(fieldName));
  }

  // Convert texture to base64-encoded PNG.
  //
  // Note: Encoding the texture to PNG is a bit more complicated than just calling the .EncodeToPNG() method,
  // because we need to handle the cases in which the texture has not been marked as readable.
  // So we first render the texture on a RenderTexture, then copy the pixels of the RenderTexture 
  // to a new Texture2D, then encode to PNG. 
  public static string TextureToBase64PNG(Texture2D tex)
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
    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(tempRT);
    byte[] bytes = tempTexture.EncodeToPNG();
    return System.Convert.ToBase64String(bytes);
  }

}
