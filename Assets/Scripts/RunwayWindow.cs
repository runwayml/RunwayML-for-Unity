using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorCoroutines;
using DefaultableDictionary;
using System.IO;

public class RunwayWindow : EditorWindow
{
  private bool showAdvancedOptions = false;
  private bool isRunwayRunning = false;
  private Model[] availableModels;
  private ModelSession runningSession;
  private int selectedModelIndex = 0;
  private int runLocationIndex = 0;
  private bool isMakingRequest = false;
  private bool isProcessingInput = false;
  private IDictionary<int, int> inputSourceSelectionIndices;

  private Dictionary<int, RunwayPreviewWindow> inputWindows;
  private RunwayPreviewWindow outputWindow;
  private Dictionary<string, object> inputData;
  private Texture2D lastOutput;

  private bool isWindowEnabled = false;

  private Texture2D logoTexture;
  private GUIStyle horizontalStyle;
  private GUIStyle justifyCenterTextStyle;
  private GUIStyle boldTextStyle;

  public void OnEnable()
  {
    availableModels = new Model[0];

    inputSourceSelectionIndices = new Dictionary<int, int>().WithDefaultValue(0);
    inputWindows = new Dictionary<int, RunwayPreviewWindow>();

    inputData = new Dictionary<string, object>();

    isWindowEnabled = true;

    logoTexture = Resources.Load("Icons/Logo") as Texture2D;

    horizontalStyle = new GUIStyle();
    horizontalStyle.margin = new RectOffset(10, 10, 0, 0);

    justifyCenterTextStyle = new GUIStyle();
    justifyCenterTextStyle.alignment = TextAnchor.MiddleCenter;

    boldTextStyle = new GUIStyle();
    boldTextStyle.alignment = TextAnchor.MiddleCenter;
    boldTextStyle.fontStyle = FontStyle.Bold;

    this.StartCoroutine(CheckIfRunwayRunning());
    this.StartCoroutine(DiscoverModels());
    this.StartCoroutine(UpdateRunningSession());
  }

  public void OnDisable()
  {
    isWindowEnabled = false;
  }

  [MenuItem("Window/Runway")]
  public static void ShowWindow()
  {
    GetWindow<RunwayWindow>("Runway");
  }

  private IEnumerator CheckIfRunwayRunning()
  {
    while (isWindowEnabled)
    {
      this.StartCoroutine(RunwayHub.isRunwayRunning((newStatus) =>
      {
        this.isRunwayRunning = newStatus;
      }));
      yield return new WaitForSeconds(1);
    }
  }

  private IEnumerator DiscoverRunningSessions()
  {
    while (isWindowEnabled)
    {
      if (isRunwayRunning)
      {
        this.StartCoroutine(RunwayHub.listSessions((sessions) =>
        {
          foreach (ModelSession s in sessions)
          {
            if (s.application.Equals("Unity"))
            {
              runningSession = s;
            }
          }
        }));
      }
      yield return new WaitForSeconds(1);
    }
  }

  private IEnumerator DiscoverModels()
  {
    while (isWindowEnabled)
    {
      if (isRunwayRunning)
      {
        this.StartCoroutine(RunwayHub.listModels((models) =>
        {
          availableModels = models;
        }));
      }
      yield return new WaitForSeconds(1);
    }
  }

  private IEnumerator UpdateRunningSession()
  {
    while (isWindowEnabled)
    {
      if (isRunwayRunning && runningSession != null)
      {
        this.StartCoroutine(RunwayHub.getSession(runningSession.id, (session) =>
        {
          runningSession = session;
        }));
      }
      yield return new WaitForSeconds(1);
    }
  }

  private bool modelIsRunning()
  {
    return runningSession != null && runningSession.runningStatus == "RUNNING";
  }

  private bool modelIsStarting()
  {
    return runningSession != null && runningSession.runningStatus == "STARTING";
  }

  private Texture2D getSelectedTexture()
  {
    Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Unfiltered);
    return textures.Length > 0 ? textures[0] : null;
  }

  private Display[] getAvailableDisplays()
  {
    Display[] all = Display.displays;
    List<Display> filteredDisplays = new List<Display>();
    foreach (Display d in Display.displays)
    {
      if (d.active)
      {
        filteredDisplays.Add(d);
      }
    }
    return filteredDisplays.ToArray();
  }

  // Convert texture to base64-encoded PNG.
  //
  // Note: Encoding the texture to PNG is a bit more complicated than just calling the .EncodeToPNG() method,
  // because we need to handle the cases in which the texture has not been marked as readable.
  // So we first render the texture on a RenderTexture, then copy the pixels of the RenderTexture 
  // to a new Texture2D, then encode to PNG. 
  public string textureToBase64PNG(Texture2D tex)
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

  private void RenderHeader()
  {
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

  private void RenderRunwayNotFound()
  {
    GUILayout.BeginHorizontal("box");
    GUILayout.FlexibleSpace();
    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.Label("RUNWAY NOT FOUND", boldTextStyle);
    GUILayout.Space(5);
    GUILayout.Label("Make sure that the Runway app is running\n and that you are signed in.", justifyCenterTextStyle);
    GUILayout.Space(5);
    if (GUILayout.Button("Download Runway"))
    {
      Application.OpenURL("https://runwayml.com");
    }
    GUILayout.Space(5);
    GUILayout.EndVertical();
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
  }

  private void RenderModelSelection()
  {
    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.Label("Model");
    GUILayout.FlexibleSpace();
    string[] modelNames = new string[availableModels.Length];
    for (var i = 0; i < modelNames.Length; i++) { modelNames[i] = availableModels[i].name; }
    if (selectedModelIndex >= modelNames.Length) { selectedModelIndex = 0; }
    selectedModelIndex = EditorGUILayout.Popup(selectedModelIndex, modelNames);
    GUILayout.EndHorizontal();

    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.EndVertical();

    Model selectedModel = availableModels.Length > 0 ? availableModels[selectedModelIndex] : null;

    if (selectedModel == null) return;

    showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
    string[] runLocations = new string[] { "Remote", "Local" };
    if (showAdvancedOptions)
    {
      // GUILayout.BeginHorizontal(horizontalStyle);
      // GUILayout.Label("Checkpoint");
      // GUILayout.FlexibleSpace();
      // EditorGUILayout.Popup(0, new string[] { "COCO" });
      // GUILayout.EndHorizontal();
      foreach (Field option in selectedModel.options)
      {
        GUILayout.Label(option.name);
      }

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.Label("Run Location");
      GUILayout.FlexibleSpace();
      runLocationIndex = EditorGUILayout.Popup(runLocationIndex, runLocations);
      GUILayout.EndHorizontal();
    }

    GUILayout.BeginVertical();
    GUILayout.Space(15);
    GUILayout.EndVertical();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();

    string buttonText;
    bool buttonDisabled;
    if (modelIsRunning())
    {
      buttonText = "Stop Model";
      buttonDisabled = false;
    }
    else if (modelIsStarting())
    {
      buttonText = "Starting Model...";
      buttonDisabled = true;
    }
    else
    {
      buttonText = "Start Model";
      buttonDisabled = false;
    }
    buttonDisabled = buttonDisabled || this.isMakingRequest;

    using (new EditorGUI.DisabledScope(buttonDisabled))
    {
      if (GUILayout.Button(buttonText))
      {
        if (modelIsRunning())
        {
          this.isMakingRequest = true;
          this.StartCoroutine(RunwayHub.stopModel(runningSession.id, (response) =>
          {
            this.runningSession = null;
            this.isMakingRequest = false;
            Repaint();
          }));
        }
        else
        {
          ProviderOptions providerOptions = new ProviderOptions();
          providerOptions.runLocation = runLocations[runLocationIndex];
          this.isMakingRequest = true;
          this.StartCoroutine(RunwayHub.runModel(availableModels[selectedModelIndex].defaultVersionId, null, providerOptions, (session) =>
          {
            this.isMakingRequest = false;
            this.runningSession = session;
            Repaint();
          }));
        }
      }
    }
    GUILayout.EndHorizontal();
  }

  void RenderModelInformation()
  {
    GUILayout.BeginVertical();
    GUILayout.Space(15);
    GUILayout.EndVertical();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();
    GUILayout.Label("");
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();
    GUIStyle titleStyle = new GUIStyle();
    titleStyle.fontSize = 20;
    GUILayout.Label("Runway", titleStyle);
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
  }

  void OnGUI()
  {
    RenderHeader();
    if (isRunwayRunning)
    {
      RenderModelSelection();


      Field[] inputs = availableModels[selectedModelIndex].commands[0].inputs;
      Field[] outputs = availableModels[selectedModelIndex].commands[0].outputs;
      for (var i = 0; i < inputs.Length; i++)
      {
        Field input = inputs[i];

        GUILayout.BeginHorizontal(horizontalStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(System.String.Format("Input {0}: {1}", (i + 1).ToString(), input.name));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(horizontalStyle);
        GUILayout.Label("Source");
        GUILayout.FlexibleSpace();
        string[] inputSources = new string[] { "Selected Texture", "Display" };
        inputSourceSelectionIndices[i] = EditorGUILayout.Popup(inputSourceSelectionIndices[i], inputSources);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Open Preview"))
        {
          if (i == 0)
          {
            inputWindows[i] = GetWindow<RunwayInput1Window>("Runway - Model Input 1", true);
          }
          else
          {
            inputWindows[i] = GetWindow<RunwayInput2Window>("Runway - Model Input 2", true);
          }
        }

        if (getSelectedTexture() != null)
        {
          if (inputWindows.ContainsKey(i))
          {
            inputWindows[i].texture = getSelectedTexture();
            inputWindows[i].Repaint();
          }
          inputData[input.name] = getSelectedTexture();
        }
      }

      if (GUILayout.Button("Output Preview"))
      {
        outputWindow = GetWindow<RunwayOutputWindow>("Runway - Model Output");
      }

      using (new EditorGUI.DisabledScope(this.isProcessingInput))
      {
        if (GUILayout.Button("Process"))
        {
          Dictionary<string, object> dataToSend = new Dictionary<string, object>();
          for (var i = 0; i < inputs.Length; i++)
          {
            Field input = inputs[i];
            object value = inputData[input.name];
            if (value is Texture2D)
            {
              dataToSend[input.name] = "data:image/png;base64," + textureToBase64PNG(value as Texture2D);
            }
            else
            {
              dataToSend[input.name] = value;
            }
          }
          this.isProcessingInput = true;
          this.StartCoroutine(RunwayHub.runInference(runningSession.url, availableModels[selectedModelIndex].commands[0].name, dataToSend, (outputData) =>
          {
            this.isProcessingInput = false;
            if (outputData == null)
            {
              EditorUtility.DisplayDialog("Inference Error", "There was an error processing this input", "OK");
              return;
            }
            for (var i = 0; i < outputs.Length; i++)
            {
              object value = outputData[outputs[i].name];
              if (outputs[i].type.Equals("image"))
              {
                // Note: Assumes PNG output
                byte[] outputImg = System.Convert.FromBase64String(((string)value).Substring(22));
                Texture2D tex = new Texture2D(2, 2); // Once image is loaded, texture will auto-resize
                tex.LoadImage(outputImg);
                this.lastOutput = tex;
                Debug.Log("setting last output");
              }
            }
            Repaint();
          }));
        }

        if (lastOutput != null && outputWindow != null)
        {
          outputWindow.texture = lastOutput;
          outputWindow.Repaint();
        }
      }
    }
    else
    {
      RenderRunwayNotFound();
    }
  }

  public void OnInspectorUpdate()
  {
    Repaint();
  }

}
