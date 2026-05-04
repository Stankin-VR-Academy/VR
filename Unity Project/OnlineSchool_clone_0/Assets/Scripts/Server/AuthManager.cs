using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public ApiClient api;

    // Панели
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject roomsPanel;

    // Поля логина
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;

    // Поля регистрации
    public TMP_InputField regEmailInput;
    public TMP_InputField regUsernameInput;
    public TMP_InputField regFullNameInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regConfirmPasswordInput;
    public Button registerButton;
    public Button toLoginButton;

    // Комнаты
    public Transform roomsContainer;
    public GameObject roomButtonPrefab;

    // Кнопка создания
    public Button createRoomButton;

    // Панель создания комнаты
    public GameObject createRoomPanel;
    public TMP_InputField newRoomNameInput;
    public Button confirmCreateRoomButton;
    public Button cancelCreateRoomButton;

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
        toLoginButton.onClick.AddListener(ShowLoginPanel);
        createRoomButton.onClick.AddListener(() => createRoomPanel.SetActive(true));
        confirmCreateRoomButton.onClick.AddListener(OnConfirmCreateRoom);
        cancelCreateRoomButton.onClick.AddListener(() => createRoomPanel.SetActive(false));
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        roomsPanel.SetActive(false);
        createRoomPanel.SetActive(false);
    }

    void OnLoginClick()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Введите email и пароль");
            return;
        }

        var request = new LoginRequest { email = email, password = password };
        StartCoroutine(api.Post("/auth/login", request, OnLoginResponse));
    }

    void OnLoginResponse(bool success, string response)
    {
        if (success)
        {
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);
            api.SetToken(loginResponse.access_token);
            Debug.Log("Успешный вход!");

            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            roomsPanel.SetActive(true);

            GetRooms();
        }
        else
        {
            Debug.LogError("Ошибка входа: " + response);
        }
    }

    void OnRegisterClick()
    {
        string email = regEmailInput.text;
        string username = regUsernameInput.text;
        string fullName = regFullNameInput.text;
        string password = regPasswordInput.text;
        string confirm = regConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Заполните все поля!");
            return;
        }

        if (password != confirm)
        {
            Debug.LogError("Пароли не совпадают!");
            return;
        }

        Register(email, username, fullName, password);
    }

    void Register(string email, string username, string fullName, string password)
    {
        var request = new RegisterRequest
        {
            email = email,
            username = username,
            full_name = fullName,
            password = password
        };

        StartCoroutine(api.Post("/auth/register", request, OnRegisterResponse));
    }

    void OnRegisterResponse(bool success, string response)
    {
        if (success)
        {
            Debug.Log("Регистрация успешна! Теперь войдите.");
            ShowLoginPanel();
        }
        else
        {
            Debug.LogError("Ошибка регистрации: " + response);
        }
    }

    void ShowLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        roomsPanel.SetActive(false);
    }

    void GetRooms()
    {
        StartCoroutine(api.Get("/rooms", OnGetRoomsResponse));
    }

    void OnGetRoomsResponse(bool success, string response)
    {
        if (success)
        {
            RoomsList rooms = JsonUtility.FromJson<RoomsList>("{\"rooms\":" + response + "}");
            Debug.Log($"Получено комнат: {rooms.rooms.Length}");

            DisplayRooms(rooms.rooms);
        }
        else
        {
            Debug.LogError("Ошибка получения комнат: " + response);
        }
    }

    void DisplayRooms(Room[] rooms)
    {
        foreach (Transform child in roomsContainer)
            Destroy(child.gameObject);

        foreach (var room in rooms)
        {
            GameObject btn = Instantiate(roomButtonPrefab, roomsContainer);
            btn.GetComponentInChildren<TMP_Text>().text = room.name;
            string roomId = room.id;
            btn.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomId));
        }
    }

    public void JoinRoom(string roomId)
    {
        Debug.Log($"Присоединяюсь к комнате {roomId}");

        // Сохраняем ID комнаты и токен для следующей сцены
        PlayerPrefs.SetString("SelectedRoomId", roomId);
        PlayerPrefs.SetString("AccessToken", api.GetToken());
        PlayerPrefs.Save();

        // Загружаем сцену с классом
        SceneManager.LoadScene("Classroom");
    }

    void OnConfirmCreateRoom()
    {
        string roomName = newRoomNameInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Введите название комнаты!");
            return;
        }

        CreateRoom(roomName);
    }

    void CreateRoom(string roomName)
    {
        var request = new CreateRoomRequest { name = roomName };

        StartCoroutine(api.Post("/rooms", request, (success, response) =>
        {
            if (success)
            {
                Debug.Log($"Комната '{roomName}' создана!");

                // Закрываем панель
                createRoomPanel.SetActive(false);
                newRoomNameInput.text = "";

                // Обновляем список комнат
                GetRooms();
            }
            else
            {
                Debug.LogError($"Ошибка создания комнаты: {response}");
            }
        }));
    }
}