using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    private string currentRoomId;
    private string accessToken;
    private GameObject localPlayer;
    private Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();
    private string myUserId;

    void Start()
    {
        currentRoomId = PlayerPrefs.GetString("SelectedRoomId", "");
        accessToken = PlayerPrefs.GetString("AccessToken", "");

        if (string.IsNullOrEmpty(currentRoomId) || string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Нет данных о комнате!");
            SceneManager.LoadScene("AuthScene");
            return;
        }

        Debug.Log($"Вход в комнату: {currentRoomId}");

        // Создаём локального игрока
        Vector3 startPosition = new Vector3(0, 1, 0);
        localPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        localPlayer.GetComponent<PlayerMovement>().enabled = true;

        // Подписываемся на события
        webSocketClient.OnConnected += OnWebSocketConnected;
        webSocketClient.OnMessage += OnWebSocketMessage;
        webSocketClient.OnError += OnWebSocketError;

        // Подключаемся
        webSocketClient.Connect(currentRoomId, accessToken);
    }

    void OnWebSocketConnected()
    {
        Debug.Log("WebSocket готов, начинаем отправку позиции");
        InvokeRepeating(nameof(SendMyPosition), 0f, 0.05f);
    }

    void OnWebSocketError(string error)
    {
        Debug.LogError($"Ошибка WebSocket: {error}");
    }

    void SendMyPosition()
    {
        if (localPlayer == null) return;

        Vector3 pos = localPlayer.transform.position;
        Vector3 rot = localPlayer.transform.eulerAngles;
        webSocketClient.SendTransform(pos, rot);
    }

    void OnWebSocketMessage(string message)
    {
        Debug.Log($"Получено: {message}");

        // Обработка init сообщения (содержит массив participants)
        if (message.Contains("\"type\":\"init\""))
        {
            HandleInit(message);
            return;
        }

        // Обработка остальных сообщений
        WebSocketMessage msg = JsonUtility.FromJson<WebSocketMessage>(message);
        if (msg == null)
        {
            Debug.LogError("Не удалось распарсить сообщение");
            return;
        }

        switch (msg.type)
        {
            case "join":
                HandlePlayerJoin(msg);
                break;
            case "transform":
                HandlePlayerTransform(msg);
                break;
            case "leave":
                HandlePlayerLeave(msg);
                break;
            default:
                Debug.Log($"Неизвестный тип: {msg.type}");
                break;
        }
    }

    void HandleInit(string jsonMessage)
    {
        InitMessage init = JsonUtility.FromJson<InitMessage>(jsonMessage);

        myUserId = init.self_user_id;
        Debug.Log($"Мой ID: {myUserId}");

        if (init.participants != null)
        {
            foreach (var p in init.participants)
            {
                if (p.user_id == myUserId) continue; // себя не создаём

                Debug.Log($"Создаём существующего игрока: {p.username} ({p.user_id})");

                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                if (p.transform?.position != null)
                    pos = new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z);
                if (p.transform?.rotation != null)
                    rot = Quaternion.Euler(p.transform.rotation.x, p.transform.rotation.y, p.transform.rotation.z);

                GameObject newPlayer = Instantiate(playerPrefab, pos, rot);
                newPlayer.GetComponent<PlayerMovement>().enabled = false;

                // Отключаем камеру
                Camera remoteCamera = newPlayer.GetComponentInChildren<Camera>();
                if (remoteCamera != null)
                    remoteCamera.enabled = false;

                AudioListener listener = newPlayer.GetComponentInChildren<AudioListener>();
                if (listener != null)
                    listener.enabled = false;

                remotePlayers[p.user_id] = newPlayer;
            }
        }
    }

    void HandlePlayerJoin(WebSocketMessage msg)
    {
        if (msg.user_id == myUserId) return;

        if (!remotePlayers.ContainsKey(msg.user_id))
        {
            Debug.Log($"Создаём игрока: {msg.user_name ?? msg.username ?? "без имени"} ({msg.user_id})");

            GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            newPlayer.GetComponent<PlayerMovement>().enabled = false;

            remotePlayers[msg.user_id] = newPlayer;

            // ОТКЛЮЧАЕМ КАМЕРУ у удалённого игрока
            Camera remoteCamera = newPlayer.GetComponentInChildren<Camera>();
            if (remoteCamera != null)
            {
                remoteCamera.enabled = false;
                Debug.Log("Камера удалённого игрока отключена");
            }

            // Отключаем Audio Listener у удалённого
            AudioListener listener = newPlayer.GetComponentInChildren<AudioListener>();
            if (listener != null)
                listener.enabled = false;

            remotePlayers[msg.user_id] = newPlayer;
        }
    }

    void HandlePlayerTransform(WebSocketMessage msg)
    {
        if (msg.user_id == myUserId) return;

        if (!remotePlayers.ContainsKey(msg.user_id))
        {
            // Если игрока ещё нет, создаём (на случай, если join не пришёл)
            HandlePlayerJoin(msg);
            return;
        }

        if (remotePlayers.TryGetValue(msg.user_id, out GameObject player))
        {
            if (msg.position != null)
                player.transform.position = new Vector3(msg.position.x, msg.position.y, msg.position.z);
            if (msg.rotation != null)
                player.transform.rotation = Quaternion.Euler(msg.rotation.x, msg.rotation.y, msg.rotation.z);
        }
    }

    void HandlePlayerLeave(WebSocketMessage msg)
    {
        if (remotePlayers.TryGetValue(msg.user_id, out GameObject player))
        {
            Debug.Log($"Удаляем игрока: {msg.user_id}");
            Destroy(player);
            remotePlayers.Remove(msg.user_id);
        }
    }

    private void OnDestroy()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnConnected -= OnWebSocketConnected;
            webSocketClient.OnMessage -= OnWebSocketMessage;
            webSocketClient.OnError -= OnWebSocketError;
        }
    }
}


[System.Serializable]
public class WebSocketMessage
{
    public string type;
    public string user_id;
    public string user_name;
    public string username;
    public Position position;
    public Rotation rotation;
    public long client_ts;
}

[System.Serializable]
public class InitMessage
{
    public string type;
    public string room_id;
    public string self_user_id;
    public ParticipantInfo[] participants;
    public float server_ts;
}

[System.Serializable]
public class ParticipantInfo
{
    public string user_id;
    public string username;
    public TransformInfo transform;
    public float server_ts;
}

[System.Serializable]
public class TransformInfo
{
    public Position position;
    public Rotation rotation;
    public long? client_ts;
}