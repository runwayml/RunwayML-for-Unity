using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class ImageSynthesis : MonoBehaviour
{
  // pass configuration
	[HideInInspector]
  public CapturePass[] capturePasses = new CapturePass[] {
    new CapturePass() { name = "_img" },
    new CapturePass() { name = "_id", supportsAntialiasing = false },
    new CapturePass() { name = "_layer", supportsAntialiasing = false },
    new CapturePass() { name = "_depth" },
    new CapturePass() { name = "_normals" },
    new CapturePass() { name = "_output"}
  };

public struct CapturePass
  {
    // configuration
    public string name;
    public bool supportsAntialiasing;
    public bool needsRescale;
    public CapturePass(string name_) { name = name_; supportsAntialiasing = true; needsRescale = false; camera = null; }

    // impl
    public Camera camera;
  };

  public Shader uberReplacementShader;

  public string[] labels;
  public Color[] colors;
	public Color defaultColor;

  void Update()
  {
    if (!uberReplacementShader)
      uberReplacementShader = Shader.Find("Hidden/UberReplacement");

    // use real camera to capture final image
    capturePasses[0].camera = GetComponent<Camera>();
    for (int q = 1; q < capturePasses.Length; q++)
    {
      if (capturePasses[q].camera == null)
      {
        capturePasses[q].camera = CreateHiddenCamera(capturePasses[q].name);
      }
    }

    OnSceneChange();
    OnCameraChange();
  }

  private Camera CreateHiddenCamera(string name)
  {
    var go = new GameObject(name, typeof(Camera));
    go.hideFlags = HideFlags.HideAndDontSave;
    go.transform.parent = transform;

    var newCamera = go.GetComponent<Camera>();
    return newCamera;
  }

  static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacelementModes mode)
  {
    SetupCameraWithReplacementShader(cam, shader, mode, Color.black);
  }

  static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacelementModes mode, Color clearColor)
  {
    var cb = new CommandBuffer();
    cb.SetGlobalFloat("_OutputMode", (int)mode); // @TODO: CommandBuffer is missing SetGlobalInt() method
    cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
    cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
    cam.SetReplacementShader(shader, "");
    cam.backgroundColor = clearColor;
    cam.clearFlags = CameraClearFlags.SolidColor;
  }

  static private void SetupCameraWithPostShader(Camera cam, Material material, DepthTextureMode depthTextureMode = DepthTextureMode.None)
  {
    var cb = new CommandBuffer();
    cb.Blit(null, BuiltinRenderTextureType.CurrentActive, material);
    cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);
    cam.depthTextureMode = depthTextureMode;
  }

  enum ReplacelementModes
  {
    ObjectId = 0,
    CategoryId = 1,
    DepthCompressed = 2,
    DepthMultichannel = 3,
    Normals = 4
  };

  public void OnCameraChange()
  {
    int targetDisplay = 1;
    var mainCamera = GetComponent<Camera>();
    for (var i = 0; i < capturePasses.Length; i++)
    {
      CapturePass pass = capturePasses[i];

      if (pass.camera == mainCamera)
        continue;

      // cleanup capturing camera
      pass.camera.RemoveAllCommandBuffers();

      // copy all "main" camera parameters into capturing camera
      pass.camera.CopyFrom(mainCamera);

      // set targetDisplay here since it gets overriden by CopyFrom()
      pass.camera.targetDisplay = targetDisplay++;
    }

    // setup command buffers and replacement shaders
    SetupCameraWithReplacementShader(capturePasses[1].camera, uberReplacementShader, ReplacelementModes.ObjectId);
    SetupCameraWithReplacementShader(capturePasses[2].camera, uberReplacementShader, ReplacelementModes.CategoryId);
    SetupCameraWithReplacementShader(capturePasses[3].camera, uberReplacementShader, ReplacelementModes.DepthCompressed, Color.white);
    SetupCameraWithReplacementShader(capturePasses[4].camera, uberReplacementShader, ReplacelementModes.Normals);
  }

  private Color lookupColor(string label)
  {
    for (int i = 0; i < labels.Length; i++)
    {
      if (labels[i].Equals(label))
      {
        return colors[i];
      }
    }
    return this.defaultColor;
  }

  public void OnSceneChange()
  {
    var renderers = Object.FindObjectsOfType<Renderer>();
    var mpb = new MaterialPropertyBlock();
    foreach (var r in renderers)
    {
      var id = r.gameObject.GetInstanceID();
      var layer = r.gameObject.layer;
      var tag = r.gameObject.tag;

      mpb.SetColor("_ObjectColor", ColorEncoding.EncodeIDAsColor(id));
      mpb.SetColor("_CategoryColor", lookupColor(tag));
      r.SetPropertyBlock(mpb);
    }
  }

#if UNITY_EDITOR
  private GameObject lastSelectedGO;
  private int lastSelectedGOLayer = -1;
  private string lastSelectedGOTag = "unknown";
  private bool DetectPotentialSceneChangeInEditor()
  {
    bool change = false;
    // there is no callback in Unity Editor to automatically detect changes in scene objects
    // as a workaround lets track selected objects and check, if properties that are 
    // interesting for us (layer or tag) did not change since the last frame
    if (UnityEditor.Selection.transforms.Length > 1)
    {
      // multiple objects are selected, all bets are off!
      // we have to assume these objects are being edited
      change = true;
      lastSelectedGO = null;
    }
    else if (UnityEditor.Selection.activeGameObject)
    {
      var go = UnityEditor.Selection.activeGameObject;
      // check if layer or tag of a selected object have changed since the last frame
      var potentialChangeHappened = lastSelectedGOLayer != go.layer || lastSelectedGOTag != go.tag;
      if (go == lastSelectedGO && potentialChangeHappened)
        change = true;

      lastSelectedGO = go;
      lastSelectedGOLayer = go.layer;
      lastSelectedGOTag = go.tag;
    }

    return change;
  }
#endif // UNITY_EDITOR
}
