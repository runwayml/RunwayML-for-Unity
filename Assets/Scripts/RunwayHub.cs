using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine;

// CORE RUNWAY TYPES

[Serializable]
public class ProviderOptions
{
    public string runLocation = "Remote";
    public string runType = "GPU";
    public int gpuIndex = -1;
}

[Serializable]
public class ModelSession
{
    public string id;
    public string createdAt;
    public string startedRunningAt;
    public string endedAt;
    public string runningStatus;
    public string url;
    public bool persistent;
    public string application;
    public int modelVersionId;
    public ProviderOptions providerOptions;
    public Model model;
}

[Serializable]
public class Field {
    public string name;
    public string type;
    public string[] oneOf;
}

[Serializable]
public class Command {
    public string name;
    public Field[] inputs;
    public Field[] outputs;
}

[Serializable]
public class Model
{
    public string name;
    public int defaultVersionId;
    public Field[] options;
    public Command[] commands;
}

// API RESPONSE TYPES

[Serializable]
public class SuccessResponse
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
public class GetSessionResponse
{
    public ModelSession modelSession;
}

[Serializable]
public class RunSessionRequest
{
    public int modelVersionId;
    public Dictionary<string, object> modelOptions;
    public string application;
    public ProviderOptions providerOptions;

    public string ToJSON() {
        Dictionary<string, object> ret = new Dictionary<string, object>();
        ret["modelVersionId"] = modelVersionId;
        ret["modelOptions"] = modelOptions;
        ret["application"] = application;
        ret["providerOptions"] = providerOptions;
        return MiniJSON.Json.Serialize(ret);
    }
}

[Serializable]
public class RunSessionResponse
{
    public ModelSession modelSession;
}


public class RunwayHub
{
    static private IEnumerator GET(string host, string endpoint, Action<string, string> callback)
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

    static private IEnumerator POST(string host, string endpoint, string postData, Action<string, string> callback)
    {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(postData);
        UnityWebRequest www = new UnityWebRequest(host + endpoint, "POST");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
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

    static private IEnumerator DELETE(string host, string endpoint, Action<string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Delete(host + endpoint);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            callback(www.error);
        }
        else
        {
            callback(null);
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
                callback(JsonUtility.FromJson<SuccessResponse>(result).success);
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

    static public IEnumerator getSession(string sessionId, Action<ModelSession> callback)
    {
        return GET("http://localhost:5142", "/v1/model_sessions/" + sessionId, (string error, string result) =>
        {
            if (error != null)
            {
                callback(null);
            }
            else
            {
                callback(JsonUtility.FromJson<GetSessionResponse>(result).modelSession);
            }
        });
    }

    static public IEnumerator runModel(int modelVersionId, Dictionary<string, object> modelOptions, ProviderOptions providerOptions, Action<ModelSession> callback)
    {
        RunSessionRequest req = new RunSessionRequest();
        req.modelVersionId = modelVersionId;
        req.modelOptions = modelOptions;
        req.providerOptions = providerOptions;
        req.application = "Unity";
        return POST("http://localhost:5142", "/v1/model_sessions", req.ToJSON(), (string error, string result) =>
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

    static public IEnumerator stopModel(string sessionId, Action<bool> callback)
    {
        return DELETE("http://localhost:5142", "/v1/model_sessions/" + sessionId, (string error) =>
        {
            if (error != null)
            {
                callback(false);
            }
            else
            {
                callback(true);
            }
        });
    }

    static public IEnumerator runInference(string url, string commandName, object data, Action<Dictionary<string, object>> callback) {
        string body = MiniJSON.Json.Serialize(data);
        return POST(url, "/" + commandName, body, (string error, string result) => {
            if (error != null)
            {
                callback(null);
            }
            else
            {
                callback(MiniJSON.Json.Deserialize(result) as Dictionary<string, object>);
            }
        });
    }
}
