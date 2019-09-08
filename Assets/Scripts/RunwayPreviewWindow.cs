using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RunwayPreviewWindow : EditorWindow
{
  public Texture texture;
  public void OnEnable()
  {
    Debug.Log("window enabled");
  }

  public void OnGUI()
  {
    GUILayout.Label(texture);
  }

  public void OnInspectorUpdate()
  {
    Repaint();
  }
}

public class RunwayInput1Window : RunwayPreviewWindow { }
public class RunwayInput2Window : RunwayPreviewWindow { }
public class RunwayOutputWindow : RunwayPreviewWindow { }