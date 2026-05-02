using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private GameObject playerPrefab;

    private string currentRoomId;
    private string accessToken;
    private GameObject localPlayer;

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
        localPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
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
        InvokeRepeating(nameof(SendMyPosition), 0f, 0.1f);
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

        string json = $"{{\"type\":\"move\",\"x\":{pos.x},\"y\":{pos.y},\"z\":{pos.z},\"rx\":{rot.x},\"ry\":{rot.y},\"rz\":{rot.z}}}";
        webSocketClient.Send(json);
    }

    void OnWebSocketMessage(string message)
    {
        Debug.Log($"Получено от сервера: {message}");
        // TODO: обработка других игроков
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