using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine;

// CORE RUNWAY TYPES

[Serializable]
public class ProviderOptions
{
    string runLocation;
    string runType;
    int gpuIndex;
    string gpuType;
}

[Serializable]
public class ModelSession
{
    public string createdAt;
    public string startedRunningAt;
    public string endedAt;
    public string runningStatus;
    public string url;
    public bool persistent;
    public string application;
    public int modelVersionId;
    public ProviderOptions providerOptions;
}

[Serializable]
public class Model
{
    public string name;
    public int defaultVersionId;
}

// API RESPONSE TYPES

[Serializable]
public class HealthcheckResponse
{
    public bool success;
}

[Serializable]
public class GetModelsResponse
{
    public Model[] models;
}

[Serializable]
public class GetSessionsResponse
{
    public ModelSession[] modelSessions;
}

[Serializable]
public class RunSessionRequest
{
    public int modelVersionId;
    public object modelOptions;
    public string application;
    public ProviderOptions providerOptions;
}

[Serializable]
public class RunSessionResponse
{
    public ModelSession modelSession;
}


public class RunwayHub
{
    static public IEnumerator GET(string host, string endpoint, Action<string, string> callback)
    {
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

    static public IEnumerator POST(string host, string endpoint, string postData, Action<string, string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Post(host + endpoint, postData);
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
            if (error != null)
            {
                callback(false);
            }
            else
            {
                callback(JsonUtility.FromJson<HealthcheckResponse>(result).success);
            }
        });
    }

    static public IEnumerator listModels(Action<Model[]> callback)
    {
        return GET("http://localhost:5142", "/v1/models", (string error, string result) =>
        {
            if (error != null)
            {
                callback(new Model[0]);
            }
            else
            {
                callback(JsonUtility.FromJson<GetModelsResponse>(result).models);
            }
        });
    }

    static public IEnumerator listSessions(Action<ModelSession[]> callback)
    {
        return GET("http://localhost:5142", "/v1/model_sessions", (string error, string result) =>
        {
            if (error != null)
            {
                callback(new ModelSession[0]);
            }
            else
            {
                callback(JsonUtility.FromJson<GetSessionsResponse>(result).modelSessions);
            }
        });
    }

    static public IEnumerator runModel(int modelVersionId, object modelOptions, ProviderOptions providerOptions, Action<ModelSession> callback)
    {
        RunSessionRequest req = new RunSessionRequest();
        req.modelVersionId = modelVersionId;
        req.modelOptions = modelOptions;
        req.providerOptions = providerOptions;
        req.application = "Unity";
        return POST("http://localhost:5142", "/v1/model_sessions", JsonUtility.ToJson(req), (string error, string result) =>
        {
            if (error != null)
            {
                callback(null);
            }
            else
            {
                callback(JsonUtility.FromJson<RunSessionResponse>(result).modelSession);
            }
        });

    }
}
