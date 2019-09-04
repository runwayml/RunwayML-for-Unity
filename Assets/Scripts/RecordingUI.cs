using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;

[RequireComponent (typeof(ImageSynthesis))]
public class RecordingUI : MonoBehaviour {

    private bool isRecording = false;

	public int width = 320;
	public int height = 200;
    public float fps = 5f;
	private int imageCounter = 1;
    public string recordingName = "MyRecording";

    void OnGUI()
    {
        if (!isRecording && GUILayout.Button("Record"))
        {
            StartCoroutine(RecordLoop());
        }

        if (isRecording && GUILayout.Button("Stop"))
        {
            isRecording = false;
        }
    }

    private IEnumerator RecordLoop()
    {
        if (isRecording) yield return false;
        isRecording = true;
        while (isRecording) {
            GetComponent<ImageSynthesis>().Save((imageCounter++).ToString().PadLeft(10, '0'), width, height, recordingName);
            yield return new WaitForSeconds(1 / fps);
        }
        EditorUtility.RevealInFinder(recordingName);
        yield return true;
    }

}
