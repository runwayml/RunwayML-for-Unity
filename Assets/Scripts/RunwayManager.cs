using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RunwayManager : MonoBehaviour
{
    private static RunwayManager _instance;

    public static RunwayManager Instance { get { return _instance; } }

    private void Awake()
    {
        Debug.Log("AWAKE!");
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            InvokeRepeating("checkIfRunwayIsRunning", 0f, 1f);
        }
    }

    [HideInInspector]
    public bool isRunning;

    public void checkIfRunwayIsRunning()
    {
        Debug.Log("Checking");
        StartCoroutine(RunwayHub.isRunwayRunning((bool running) => {
            this.isRunning = running;
        }));
    }
}
