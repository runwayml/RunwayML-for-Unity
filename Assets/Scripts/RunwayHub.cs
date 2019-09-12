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
    public string runType;
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
    public float step;
    public float min;
    public float max;
    public string[] labels;
    public Color[] colors;
    public string defaultLabel;
    public Color defaultColor;
    public int length = 0;
    public float samplingStd = 1;
    public float samplingMean = 0;
    [NonSerialized] public object defaultValue;

    [NonSerialized] public bool hasStep = false;
    [NonSerialized] public bool hasMin = false;
    [NonSerialized] public bool hasMax = false;

    public static Field FromDictionary(Dictionary<string, object> dictionary) {
        Field f = new Field();
        f.name = dictionary["name"] as string;
        f.type = dictionary["type"] as string;
        try {
            List<object> serializedChoices = dictionary["oneOf"] as List<object>;
            List<string> deserializedChoices = new List<string>();
            for (var i = 0; i < serializedChoices.Count; i++) {
                deserializedChoices.Add(serializedChoices[i] as string);
            }
            f.oneOf = deserializedChoices.ToArray();
        } catch {
            f.oneOf = new string[0];
        }
        if (dictionary.ContainsKey("step") && dictionary["step"] != null) {
            f.step = Convert.ToSingle(dictionary["step"]);
            f.hasStep = true;
        }
        if (dictionary.ContainsKey("min") && dictionary["min"] != null) {
            f.min = Convert.ToSingle(dictionary["min"]);
            f.hasMin = true;
        }
        if (dictionary.ContainsKey("max") && dictionary["max"] != null) {
            f.max = Convert.ToSingle(dictionary["max"]);
            f.hasMax = true;
        }
        if (dictionary.ContainsKey("length") && dictionary["length"] != null) {
            f.length = Convert.ToInt32(dictionary["length"]);
        }
        if (dictionary.ContainsKey("samplingMean") && dictionary["samplingMean"] != null) {
            f.samplingMean = Convert.ToSingle(dictionary["samplingMean"]);
        }
        if (dictionary.ContainsKey("samplingStd") && dictionary["samplingStd"] != null) {
            f.samplingStd = Convert.ToSingle(dictionary["samplingStd"]);
        }
        try {
            f.defaultValue = dictionary["default"];
        } catch {
            f.defaultValue = null;
        }
        try {
            List<object> serializedLabels = dictionary["labels"] as List<object>;
            List<string> deserializedLabels = new List<string>();
            for (var i = 0; i < serializedLabels.Count; i++) {
                deserializedLabels.Add(serializedLabels[i] as string);
            }
            f.labels = deserializedLabels.ToArray();
        } catch {
            f.labels = new string[0];
        }
        try {
            f.defaultLabel = dictionary["defaultLabel"] as string;
        } catch {
            f.defaultLabel = null;
        }
        try {
            Dictionary<string, object> serializedLabelToColor = dictionary["labelToColor"] as Dictionary<string, object>;
            List<Color> deserializedColors = new List<Color>();
            for (var i = 0; i < f.labels.Length; i++) {
                List<object> serializedColor = serializedLabelToColor[f.labels[i]] as List<object>;
                Color color = new Color(Convert.ToSingle(serializedColor[0])/255f, Convert.ToSingle(serializedColor[1])/255f, Convert.ToSingle(serializedColor[2])/255f);
                deserializedColors.Add(color);
                if (f.labels[i].Equals(f.defaultLabel)) {
                    f.defaultColor = color;
                }
            }
            f.colors = deserializedColors.ToArray();
        } catch {
            f.colors = new Color[0];
        }
        return f;
    }
}

[Serializable]
public class Command {
    public string name;
    public Field[] inputs;
    public Field[] outputs;
    public static Command FromDictionary(Dictionary<string, object> dictionary) {
        Command cmd = new Command();
        cmd.name = dictionary["name"] as string;

        List<object> inputsListSerialized = dictionary["inputs"] as List<object>;
        List<Field> inputsListDeserialized = new List<Field>();
        for (var i = 0; i < inputsListSerialized.Count; i++) {
            inputsListDeserialized.Add(Field.FromDictionary(inputsListSerialized[i] as Dictionary<string, object>));
        }
        cmd.inputs = inputsListDeserialized.ToArray();

        List<object> outputsListSerialized = dictionary["outputs"] as List<object>;
        List<Field> outputsListDeserialized = new List<Field>();
        for (var i = 0; i < outputsListSerialized.Count; i++) {
            outputsListDeserialized.Add(Field.FromDictionary(outputsListSerialized[i] as Dictionary<string, object>));
        }
        cmd.outputs = outputsListDeserialized.ToArray();
        
        return cmd;
    }
}

[Serializable]
public class Model
{
    public string name;
    public string description;
    public int defaultVersionId;
    public Field[] options;
    public Command[] commands;

    public static Model FromDictionary(Dictionary<string, object> dictionary) {
        Model m = new Model();
        m.name = dictionary["name"] as string;
        m.description = dictionary["description"] as string;
        try {
            m.defaultVersionId = Convert.ToInt32(dictionary["defaultVersionId"]);
        } catch {
            m.defaultVersionId = -1;
        }
        
        List<object> optionsListSerialized = dictionary["options"] as List<object>;
        List<Field> optionsListDeserialized = new List<Field>();
        for (var i = 0; i < optionsListSerialized.Count; i++) {
            optionsListDeserialized.Add(Field.FromDictionary(optionsListSerialized[i] as Dictionary<string, object>));
        }
        m.options = optionsListDeserialized.ToArray();

        List<object> commandsListSerialized = dictionary["commands"] as List<object>;
        List<Command> commandsListDeserialized = new List<Command>();
        for (var i = 0; i < commandsListSerialized.Count; i++) {
            commandsListDeserialized.Add(Command.FromDictionary(commandsListSerialized[i] as Dictionary<string, object>));
        }
        m.commands = commandsListDeserialized.ToArray();

        return m;        
    }
}

// API RESPONSE TYPES

[Serializable]
public class SuccessResponse
{
    public bool success;
}

public class ErrorResponse
{
    public string error;
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

    public Dictionary<string, object> AsDictionary() {
        Dictionary<string, object> ret = new Dictionary<string, object>();
        ret["modelVersionId"] = modelVersionId;
        ret["modelOptions"] = modelOptions;
        ret["application"] = application;
        Dictionary<string, object> retProviderOptions = new Dictionary<string, object>();
        retProviderOptions["runLocation"] = providerOptions.runLocation;
        retProviderOptions["runType"] = providerOptions.runType;
        retProviderOptions["gpuIndex"] = providerOptions.gpuIndex;
        ret["providerOptions"] = retProviderOptions;
        return ret;
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
            try {
                callback(JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text).error, null);
            } catch {
                callback(www.error.ToString(), null);
            }
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
            try {
                callback(JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text).error, null);
            }
            catch {
                callback(www.error.ToString(), null);
            }
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
                Dictionary<string, object> deserialized = MiniJSON.Json.Deserialize(result) as Dictionary<string, object>;
                List<object> modelsList = deserialized["models"] as List<object>;
                List<Model> modelsDeserialized = new List<Model>();
                for (var i = 0; i < modelsList.Count; i++) {
                    modelsDeserialized.Add(Model.FromDictionary(modelsList[i] as Dictionary<string, object>));
                }
                callback(modelsDeserialized.ToArray());
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

    static public IEnumerator runModel(int modelVersionId, Dictionary<string, object> modelOptions, ProviderOptions providerOptions, Action<string, ModelSession> callback)
    {
        RunSessionRequest req = new RunSessionRequest();
        req.modelVersionId = modelVersionId;
        req.modelOptions = modelOptions;
        req.providerOptions = providerOptions;
        req.application = "Unity";
        return POST("http://localhost:5142", "/v1/model_sessions", MiniJSON.Json.Serialize(req.AsDictionary()), (string error, string result) =>
        {
            if (error != null)
            {
                callback(error, null);
            }
            else
            {
                callback(null, JsonUtility.FromJson<RunSessionResponse>(result).modelSession);
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
