using UnityEngine;

public class ObjectSelectionUI : MonoBehaviour
{
    public GameObject objectSelectionPanel;
    private ObjectManager objectManager;

    private bool isPanelOpen = false;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisibility;

    void Start()
    {
        objectSelectionPanel.SetActive(false);
        objectManager = FindFirstObjectByType<ObjectManager>();
        objectManager.OnObjectSpawnConfirmed += ClosePanel;

        if (objectManager == null)
            Debug.LogError("ObjectManager не найден в сцене!");
    }

    void Update()
    {
        // Открыть/закрыть панель по кнопке Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TogglePanel();
        }

        // Закрыть по Escape (если панель открыта)
        if (Input.GetKeyDown(KeyCode.Escape) && isPanelOpen)
        {
            ClosePanel();
        }
    }

    void TogglePanel()
    {
        if (isPanelOpen)
            ClosePanel();
        else
            OpenPanel();
    }

    void OpenPanel()
    {
        isPanelOpen = true;
        objectSelectionPanel.SetActive(true);

        // Сохраняем текущие настройки курсора
        previousLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        // Показываем курсор и разблокируем
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Блокируем движение игрока
        BlockPlayerMovement(true);
    }

    void ClosePanel()
    {
        isPanelOpen = false;
        objectSelectionPanel.SetActive(false);

        // Возвращаем курсор в исходное состояние
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Разблокируем движение игрока
        BlockPlayerMovement(false);
    }

    void BlockPlayerMovement(bool block)
    {
        // Находим локального игрока
        GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
        if (localPlayer != null)
        {
            PlayerMovement movement = localPlayer.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = !block;
            }
        }
    }

    // Кнопки выбора объекта
    public void OnCubeButtonClick()
    {
        
        if (objectManager != null)
            objectManager.SpawnObject("Cube");
    }

    public void OnSphereButtonClick()
    {
        if (objectManager != null)
            objectManager.SpawnObject("Sphere");
    }

    public void OnCylinderButtonClick()
    {
        if (objectManager != null)
            objectManager.SpawnObject("Cylinder");
    }
}