using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public ApiClient api;

    // Панели
    public GameObject mainMenuPanel;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject roomsPanel;

    // Кнопки главного меню
    public Button toAuthButton;
    public Button exitButton;

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

    // Кнопка создания комнаты
    public Button createRoomButton;

    // Панель создания комнаты
    public GameObject createRoomPanel;
    public TMP_InputField newRoomNameInput;
    public Button confirmCreateRoomButton;
    public Button cancelCreateRoomButton;
    public Button cancelLoginButton; // ← исправлено название

    // Панель сообщений
    public GameObject messagePanel;
    public TMP_Text messageText;

    void Start()
    {
        // Кнопки главного меню
        if (toAuthButton != null)
            toAuthButton.onClick.AddListener(ShowRegisterPanel);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitApplication);

        // Кнопки логина/регистрации
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
        toLoginButton.onClick.AddListener(ShowLoginPanel);

        // Комнаты
        createRoomButton.onClick.AddListener(() => createRoomPanel.SetActive(true));
        confirmCreateRoomButton.onClick.AddListener(OnConfirmCreateRoom);
        cancelCreateRoomButton.onClick.AddListener(() => createRoomPanel.SetActive(false));

        // Кнопка "Назад" на панели логина
        if (cancelLoginButton != null)
            cancelLoginButton.onClick.AddListener(() =>
            {
                loginPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
            });

        // Начальное состояние
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        roomsPanel.SetActive(false);
        createRoomPanel.SetActive(false);

        // Скрываем панель сообщений
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    void ShowRegisterPanel()
    {
        mainMenuPanel.SetActive(false);
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        roomsPanel.SetActive(false);
    }

    void ShowLoginPanel()
    {
        mainMenuPanel.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        roomsPanel.SetActive(false);
    }

    void ShowRoomsPanel()
    {
        mainMenuPanel.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
        roomsPanel.SetActive(true);
    }

    void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnLoginClick()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Введите email и пароль!", 2f);
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

            ShowRoomsPanel();
            GetRooms();
        }
        else
        {
            // Вместо Debug.LogError показываем сообщение пользователю
            ShowMessage("Ошибка входа: неверный email или пароль", 3f);
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
            ShowMessage("Заполните все поля!", 2f);
            return;
        }

        if (password != confirm)
        {
            ShowMessage("Пароли не совпадают!", 2f);
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
            ShowLoginPanel();
        }
        else
        {
            ShowMessage("Ошибка регистрации: проверьте введённые данные", 3f);
        }
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
            ShowMessage("Ошибка получения комнат", 2f);
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

        // Останавливаем музыку главного меню (если есть)
        if (mainMenuPanel != null)
        {
            AudioSource menuMusic = mainMenuPanel.GetComponent<AudioSource>();
            if (menuMusic != null)
                menuMusic.Stop();
        }

        PlayerPrefs.SetString("SelectedRoomId", roomId);
        PlayerPrefs.SetString("AccessToken", api.GetToken());
        PlayerPrefs.Save();

        SceneManager.LoadScene("Classroom");
    }

    void OnConfirmCreateRoom()
    {
        string roomName = newRoomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            ShowMessage("Введите название комнаты!", 2f);
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
                createRoomPanel.SetActive(false);
                newRoomNameInput.text = "";
                GetRooms();
            }
            else
            {
                ShowMessage("Ошибка создания комнаты!", 2f);
            }
        }));
    }


    public void ShowMessage(string message, float duration = 3f)
    {
        if (messagePanel == null || messageText == null) return;

        messageText.text = message;
        messagePanel.SetActive(true);

        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), duration);
    }

    private void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
}

