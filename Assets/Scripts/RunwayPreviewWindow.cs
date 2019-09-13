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

public class RunwayInput1Window : RunwayPreviewWindow
{
  public static RunwayInput1Window Instance { get; private set; }
  public static bool IsOpen
  {
    get { return Instance != null; }
  }

  public new void OnEnable()
  {
    Instance = this;
    base.OnEnable();
  }
}
public class RunwayInput2Window : RunwayPreviewWindow
{
  public static RunwayInput2Window Instance { get; private set; }
  public static bool IsOpen
  {
    get { return Instance != null; }
  }

  public new void OnEnable()
  {
    Instance = this;
    base.OnEnable();
  }
}
public class RunwayOutputWindow : RunwayPreviewWindow
{
  public static RunwayOutputWindow Instance { get; private set; }
  public static bool IsOpen
  {
    get { return Instance != null; }
  }

  public new void OnEnable()
  {
    Instance = this;
    base.OnEnable();
  }
}
