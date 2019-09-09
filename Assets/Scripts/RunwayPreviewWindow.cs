using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RunwayPreviewWindow : EditorWindow
{
  public Texture texture;
  private GUIStyle style;

  public void OnEnable()
  {
    style = new GUIStyle();
    style.alignment = TextAnchor.MiddleCenter;
  }
  public void OnGUI()
  {
    if (texture != null)
    {
      GUILayout.Label(texture, style, GUILayout.Width(position.width), GUILayout.Height(position.height));
    }
  }

  public void OnInspectorUpdate()
  {
    Repaint();
  }
}

public class RunwayInput1Window : RunwayPreviewWindow { }
public class RunwayInput2Window : RunwayPreviewWindow { }
public class RunwayOutputWindow : RunwayPreviewWindow { }