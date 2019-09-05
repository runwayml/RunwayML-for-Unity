using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorCoroutines;

public class RunwayWindow : EditorWindow
{
  private bool showAdvancedOptions = false;
  private bool isRunwayRunning = false;

  private bool isWindowEnabled = false;

  private Texture logoTexture;
  private GUIStyle horizontalStyle;
  private GUIStyle justifyCenterTextStyle;
  private GUIStyle boldTextStyle;

  public void OnEnable() {
    isWindowEnabled = true;

    logoTexture = Resources.Load("Icons/Logo") as Texture;

    horizontalStyle = new GUIStyle();
    horizontalStyle.margin = new RectOffset(10, 10, 0, 0);

    justifyCenterTextStyle = new GUIStyle();
    justifyCenterTextStyle.alignment = TextAnchor.MiddleCenter;

    boldTextStyle = new GUIStyle();
    boldTextStyle.alignment = TextAnchor.MiddleCenter;
    boldTextStyle.fontStyle = FontStyle.Bold;

    this.StartCoroutine(CheckIfRunwayRunning());
  }

  public void OnDisable() {
      isWindowEnabled = false;
  }
  
  [MenuItem("Window/Runway")]
  public static void ShowWindow()
  {
    GetWindow<RunwayWindow>("Runway");
  }

  private IEnumerator CheckIfRunwayRunning() {
      while (isWindowEnabled) {
          this.StartCoroutine(RunwayHub.isRunwayRunning((isRunning) => {
              this.isRunwayRunning = isRunning;
          }));
          yield return new WaitForSeconds(1); 
      }
  }

  private void RenderHeader() {
    GUILayout.BeginVertical();
    GUILayout.Space(15);
    GUILayout.EndVertical();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();
    GUILayout.Label(logoTexture, GUILayout.Width(50), GUILayout.Height(50));
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();
    GUIStyle titleStyle = new GUIStyle();
    titleStyle.fontSize = 20;
    GUILayout.Label("Runway", titleStyle);
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();

    GUILayout.BeginVertical();
    GUILayout.Space(15);
    GUILayout.EndVertical();
  }

  private void RenderRunwayNotFound() {
    GUILayout.BeginHorizontal("box");
    GUILayout.FlexibleSpace();
    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.Label("RUNWAY NOT FOUND", boldTextStyle);
    GUILayout.Space(5);
    GUILayout.Label("Make sure that the Runway app is running\n and that you are signed in.", justifyCenterTextStyle);
    GUILayout.Space(5);
    if (GUILayout.Button("Download Runway")) {
        Application.OpenURL("https://runwayml.com/download");
    }
    GUILayout.Space(5);
    GUILayout.EndVertical();
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
  }

  private void RenderModelSelection() {
    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.Label("Model");
    GUILayout.FlexibleSpace();
    EditorGUILayout.Popup(0, new string[] { "StyleGAN", "ProGAN" });
    GUILayout.EndHorizontal();

    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.EndVertical();

    showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
    if (showAdvancedOptions)
    {
      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.Label("Checkpoint");
      GUILayout.FlexibleSpace();
      EditorGUILayout.Popup(0, new string[] { "COCO" });
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.Label("Run Location");
      GUILayout.FlexibleSpace();
      EditorGUILayout.Popup(0, new string[] { "Local", "Remote" });
      GUILayout.EndHorizontal();
    }

    GUILayout.BeginVertical();
    GUILayout.Space(15);
    GUILayout.EndVertical();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();
    using (new EditorGUI.DisabledScope(true))
    {
      if (GUILayout.Button("Start Model"))
      {
        Repaint();
      }
    }
    GUILayout.EndHorizontal();

  }

  void OnGUI()
  {   
      RenderHeader();
      if (isRunwayRunning) {
          RenderModelSelection();
      } else {
          RenderRunwayNotFound();
      }
  }
}
