using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorCoroutines;
using DefaultableDictionary;
using System.IO;

public class RunwayWindow : EditorWindow
{
  string[] runLocations = new string[] { "Remote", "Local" };
  private bool showAdvancedOptions = false;
  private bool isRunwayRunning = false;
  private Model[] availableModels;
  private ModelSession runningSession;
  private int selectedModelIndex = 0;
  private int runLocationIndex = 0;
  private bool isMakingRequest = false;
  private bool isProcessingInput = false;
  private bool isRetrievingModels = false;
  private IDictionary<int, int> optionSelectionIndices;
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
    optionSelectionIndices = new Dictionary<int, int>().WithDefaultValue(0);

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
    this.StartCoroutine(UpdateRunningSession());

    // this.titleContent = new GUIContent("Runway", Resources.Load("Icons/LogoDock") as Texture2D);
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
        if (!this.isRunwayRunning && newStatus)
        {
          DiscoverModels();
        }
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

  private void DiscoverModels()
  {
    isRetrievingModels = true;
    this.StartCoroutine(RunwayHub.listModels((models) =>
    {
      isRetrievingModels = false;
      availableModels = models;
    }));
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

  private Model[] getFilteredModels()
  {
    List<Model> ret = new List<Model>();
    foreach (Model m in availableModels)
    {
      foreach (Field output in m.commands[0].outputs)
      {
        if (output.type == "image")
        {
          ret.Add(m);
          break;
        }
      }
    }
    return ret.ToArray();
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

  private void RenderModelInfo(Model m)
  {
    GUILayout.BeginHorizontal("box");
    GUILayout.FlexibleSpace();
    GUILayout.BeginVertical();
    GUILayout.Label("MODEL INFORMATION", boldTextStyle);
    GUILayout.Space(5);
    GUILayout.Label(m.description, justifyCenterTextStyle);
    GUILayout.Space(5);
    GUILayout.EndVertical();
    GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
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

  private Dictionary<string, object> getOptions()
  {
    Model model = getSelectedModel();
    Dictionary<string, object> ret = new Dictionary<string, object>();
    for (var i = 0; i < model.options.Length; i++)
    {
      Field f = model.options[i];
      if (f.type == "file" || f.type == "category")
      {
        ret[f.name] = f.oneOf[optionSelectionIndices[i]];
      }
    }
    return ret;
  }

  private Model getSelectedModel()
  {
    return getFilteredModels().Length > 0 ? getFilteredModels()[selectedModelIndex] : null;
  }

  private void RenderModelSelection()
  {
    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.Label("Model");
    GUILayout.FlexibleSpace();
    string[] modelNames = new string[getFilteredModels().Length];
    for (var i = 0; i < modelNames.Length; i++) { modelNames[i] = getFilteredModels()[i].name; }
    if (selectedModelIndex >= modelNames.Length) { selectedModelIndex = 0; }
    selectedModelIndex = EditorGUILayout.Popup(selectedModelIndex, modelNames);
    GUILayout.EndHorizontal();

    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.EndVertical();

    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.EndVertical();
  }

  void RenderTextureInfo(Texture tex)
  {
    GUILayout.BeginVertical();
    GUILayout.Label(tex, justifyCenterTextStyle, GUILayout.MaxWidth(100), GUILayout.MaxHeight(100));
    GUILayout.Space(5);
    GUILayout.Label(System.String.Format("{0}x{1}", tex.width.ToString(), tex.height.ToString()), justifyCenterTextStyle);
    GUILayout.EndVertical();
  }

  void RenderInputsAndOutputs()
  {
    Field[] inputs = getFilteredModels()[selectedModelIndex].commands[0].inputs;
    Field[] outputs = getFilteredModels()[selectedModelIndex].commands[0].outputs;
    for (var i = 0; i < inputs.Length; i++)
    {
      Field input = inputs[i];

      GUILayout.BeginVertical();
      GUILayout.Space(5);
      GUILayout.EndVertical();

      GUILayout.BeginHorizontal("box");
      GUILayout.BeginVertical();

      GUILayout.Space(5);

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.FlexibleSpace();
      GUILayout.Label(System.String.Format("Input {0}: {1}", (i + 1).ToString(), RunwayUtils.FormatFieldName(input.name)), boldTextStyle);
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.Space(5);

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.FlexibleSpace();
      if (getSelectedTexture())
      {
        RenderTextureInfo(getSelectedTexture());
      }
      else
      {
        GUILayout.Label("N/A");
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      // string[] inputSources = new string[] { "Selected Texture", "Display" };
      // inputSourceSelectionIndices[i] = EditorGUILayout.Popup(inputSourceSelectionIndices[i], inputSources);

      // EditorGUIUtility.ShowObjectPicker<Object>(null, true, "t:Camera t:Texture", Random.Range(0, 100));

      GUILayout.Space(5);

      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

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

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.Space(5);

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();

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

    if (this.lastOutput != null)
    {
      GUILayout.BeginVertical();
      GUILayout.Space(5);
      GUILayout.EndVertical();

      GUILayout.BeginHorizontal("box");
      GUILayout.BeginVertical();

      GUILayout.Space(5);

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.FlexibleSpace();
      GUILayout.Label("Output", boldTextStyle);
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.Space(5);

      GUILayout.BeginHorizontal(horizontalStyle);
      GUILayout.FlexibleSpace();
      if (this.lastOutput)
      {
        RenderTextureInfo(this.lastOutput);
      }
      else
      {
        GUILayout.Label("N/A");
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      if (GUILayout.Button("Output Preview"))
      {
        outputWindow = GetWindow<RunwayOutputWindow>("Runway - Model Output");
      }

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.Space(5);

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();
    }

    if (lastOutput != null && outputWindow != null)
    {
      outputWindow.texture = lastOutput;
      outputWindow.Repaint();
    }
  }

  void RenderModelOptions()
  {
    GUILayout.BeginHorizontal("box");
    GUILayout.BeginVertical();
    GUILayout.Space(5);
    GUILayout.Label("SETUP OPTIONS", boldTextStyle);
    GUILayout.Space(5);

    for (var i = 0; i < getSelectedModel().options.Length; i++)
    {
      Field option = getSelectedModel().options[i];
      if ((option.type == "category" || option.type == "file") && option.oneOf.Length > 0)
      {
        GUILayout.BeginHorizontal(horizontalStyle);
        GUILayout.Label(RunwayUtils.FormatFieldName(option.name));
        GUILayout.FlexibleSpace();
        optionSelectionIndices[i] = EditorGUILayout.Popup(optionSelectionIndices[i], option.oneOf);
        GUILayout.EndHorizontal();
      }
      GUILayout.Space(5);
    }

    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.Label("Run Location");
    GUILayout.FlexibleSpace();
    runLocationIndex = EditorGUILayout.Popup(runLocationIndex, runLocations);
    GUILayout.EndHorizontal();
    GUILayout.Space(5);
    GUILayout.EndVertical();
    GUILayout.EndHorizontal();
  }

  void RenderRunModel()
  {
    GUILayout.BeginHorizontal(horizontalStyle);
    GUILayout.FlexibleSpace();

    if (modelIsRunning())
    {
      using (new EditorGUI.DisabledScope(this.isProcessingInput))
      {
        if (GUILayout.Button("Process"))
        {
          Field[] inputs = getSelectedModel().commands[0].inputs;
          Field[] outputs = getSelectedModel().commands[0].outputs;
          Dictionary<string, object> dataToSend = new Dictionary<string, object>();
          for (var i = 0; i < inputs.Length; i++)
          {
            Field input = inputs[i];
            object value = inputData[input.name];
            if (value is Texture2D)
            {
              dataToSend[input.name] = "data:image/png;base64," + RunwayUtils.TextureToBase64PNG(value as Texture2D);
            }
            else
            {
              dataToSend[input.name] = value;
            }
          }
          this.isProcessingInput = true;
          this.StartCoroutine(RunwayHub.runInference(runningSession.url, getFilteredModels()[selectedModelIndex].commands[0].name, dataToSend, (outputData) =>
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
                string stringValue = value as string;
                int dataStartIndex = stringValue.IndexOf("base64,") + 7;
                byte[] outputImg = System.Convert.FromBase64String(((string)value).Substring(dataStartIndex));
                Texture2D tex = new Texture2D(2, 2); // Once image is loaded, texture will auto-resize
              tex.LoadImage(outputImg);
                this.lastOutput = tex;
              }
            }
            Repaint();
          }));
        }
      }
    }

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
          this.StartCoroutine(RunwayHub.runModel(getFilteredModels()[selectedModelIndex].defaultVersionId, getOptions(), providerOptions, (session) =>
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

  void OnGUI()
  {
    if (isRunwayRunning && getFilteredModels().Length == 0 && !isRetrievingModels)
    {
      DiscoverModels();
    }
    RenderHeader();
    if (isRunwayRunning)
    {
      RenderModelSelection();
      if (getSelectedModel() != null)
      {
        // RenderModelInfo(getSelectedModel());
        RenderModelOptions();
        RenderInputsAndOutputs();
        RenderRunModel();
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
