using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket websocket;
    private string roomId;
    private string token;

    public event Action<string> OnMessage;
    public event Action OnConnected;
    public event Action<string> OnError;
    public event Action OnClosed;

    public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;

    public async void Connect(string roomId, string token)
    {
        this.roomId = roomId;
        this.token = token;

        string url = $"ws://213.165.219.60:8000/api/v1/ws/rooms/{roomId}?token={token}";
        Debug.Log($"Подключение к WebSocket: {url}");

        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket подключён!");
            OnConnected?.Invoke();
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket ошибка: {e}");
            OnError?.Invoke(e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log($"WebSocket закрыт: {e}");
            OnClosed?.Invoke();
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log($"Получено сообщение: {message}");
            OnMessage?.Invoke(message);
        };

        await websocket.Connect();
    }

    public async void SendTransform(Vector3 pos, Vector3 rot)
    {
        if (!IsConnected) return;

        var msg = new TransformMessage
        {
            type = "transform",
            position = new Position { x = pos.x, y = pos.y, z = pos.z },
            rotation = new Rotation { x = rot.x, y = rot.y, z = rot.z },
            client_ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonUtility.ToJson(msg);
        Debug.Log($"Отправка: {json}");  // Проверяем, что JSON не пустой
        await websocket.SendText(json);
    }

    public async void Send(string message)
    {
        if (IsConnected)
        {
            await websocket.SendText(message);
        }
    }

    private async void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }
}