using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine;

// CORE RUNWAY TYPES

[Serializable]
public class RunwayProviderOptions {
    string runLocation;
    string runType;
    int gpuIndex;
    string gpuType;
}

[Serializable]
public class RunwayModelSession {
    public string createdAt;
    public string startedRunningAt;
    public string endedAt;
    public string runningStatus;
    public string url;
    public bool persistent;
    public string application;
    public int modelVersionId;
    public RunwayProviderOptions providerOptions;
}

[Serializable]
public class RunwayModel
{
    public string name;
    public int defaultVersionId;
}

// API RESPONSE TYPES

[Serializable]
public class HealthcheckResult
{
    public bool success;
}

[Serializable]
public class ModelsResult
{
    public RunwayModel[] models;
}

[Serializable]
public class SessionsResult
{
    public RunwayModelSession[] modelSessions;
}


public class RunwayHub
{
    static public IEnumerator GET (string host, string endpoint, Action<string, string> callback) {
        UnityWebRequest www = UnityWebRequest.Get(host + endpoint);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            callback(www.error, null);
        }
        else
        {
            callback(null, www.downloadHandler.text);
        }
    }

    static public IEnumerator isRunwayRunning(Action<bool> callback)
    {
        return GET("http://localhost:5142", "/v1/healthcheck", (string error, string result) =>
        {
            if (error != null) {
                callback(false);
            } else {
                callback(JsonUtility.FromJson<HealthcheckResult>(result).success);
            }
        });
    }

    static public IEnumerator listModels(Action<RunwayModel[]> callback) {
        return GET("http://localhost:5142", "/v1/models", (string error, string result) =>
        {
            if (error != null)
            {
                callback(new RunwayModel[0]);
            }
            else
            {
                callback(JsonUtility.FromJson<ModelsResult>(result).models);
            }
        });

    }

    static public IEnumerator listSessions(Action<RunwayModelSession[]> callback) {
        return GET("http://localhost:5142", "/v1/model_sessions", (string error, string result) =>
        {
            if (error != null)
            {
                callback(new RunwayModelSession[0]);
            }
            else
            {
                callback(JsonUtility.FromJson<SessionsResult>(result).modelSessions);
            }
        });

    }

    //static public RunwayModelSession runModel(int modelVersionId, RunwayProviderOptions providerOptions) {

    //}
}
