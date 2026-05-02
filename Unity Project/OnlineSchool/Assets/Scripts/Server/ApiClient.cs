using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://213.165.219.60:8000/api/v1";
    private string accessToken = "";

    // POST запрос с JSON
    public IEnumerator Post(string endpoint, object data, System.Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"POST {endpoint} успешно: {request.downloadHandler.text}");
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"POST {endpoint} ошибка: {request.error}");
                callback?.Invoke(false, request.error);
            }
        }
    }

    // GET запрос
    public IEnumerator Get(string endpoint, System.Action<bool, string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint))
        {
            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"GET {endpoint} успешно: {request.downloadHandler.text}");
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"GET {endpoint} ошибка: {request.error}");
                callback?.Invoke(false, request.error);
            }
        }
    }

    // PATCH запрос
    public IEnumerator Patch(string endpoint, object data, System.Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"PATCH {endpoint} успешно: {request.downloadHandler.text}");
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"PATCH {endpoint} ошибка: {request.error}");
                callback?.Invoke(false, request.error);
            }
        }
    }

    // DELETE запрос
    public IEnumerator Delete(string endpoint, System.Action<bool, string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(baseUrl + endpoint))
        {
            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"DELETE {endpoint} успешно");
                callback?.Invoke(true, "Удалено успешно");
            }
            else
            {
                Debug.LogError($"DELETE {endpoint} ошибка: {request.error}");
                callback?.Invoke(false, request.error);
            }
        }
    }

    public void SetToken(string token)
    {
        accessToken = token;
        Debug.Log("Токен сохранён: " + token.Substring(0, Mathf.Min(20, token.Length)) + "...");
    }

    public string GetToken()
    {
        return accessToken;
    }

    public void SetBaseUrl(string url)
    {
        baseUrl = url;
        Debug.Log($"Base URL изменён на: {baseUrl}");
    }
}