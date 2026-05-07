using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance { get; private set; }

    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private string[] objectPrefabNames;

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private GameObject currentlySelectedObject;

    // Словарь для быстрого поиска префаба по имени
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public System.Action OnObjectSpawnConfirmed;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Заполняем словарь префабов
        for (int i = 0; i < objectPrefabs.Length && i < objectPrefabNames.Length; i++)
        {
            if (!prefabDictionary.ContainsKey(objectPrefabNames[i]))
                prefabDictionary.Add(objectPrefabNames[i], objectPrefabs[i]);
        }
    }

    void Update()
    {
        // Выбор объекта кликом мыши
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject selected = hit.transform.root.gameObject;
                ObjectIdentifier id = selected.GetComponent<ObjectIdentifier>();
                if (id != null)
                {
                    SetSelectedObject(selected);
                    Debug.Log($"Выбран объект: {selected.name} (ID: {id.objectId})");
                }
            }
        }

        // Управление выбранным объектом
        if (currentlySelectedObject != null)
        {
            bool changed = false;
            float moveSpeed = 3f * Time.deltaTime;
            float rotateSpeed = 50f * Time.deltaTime;
            float scaleSpeed = 0.5f * Time.deltaTime;

            // ========== ПЕРЕМЕЩЕНИЕ ==========
            if (Input.GetKey(KeyCode.U))
            {
                currentlySelectedObject.transform.Translate(0, 0, moveSpeed);
                changed = true;
            }
            if (Input.GetKey(KeyCode.I))
            {
                currentlySelectedObject.transform.Translate(0, 0, -moveSpeed);
                changed = true;
            }
            if (Input.GetKey(KeyCode.J))
            {
                currentlySelectedObject.transform.Translate(-moveSpeed, 0, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.K))
            {
                currentlySelectedObject.transform.Translate(moveSpeed, 0, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.N))
            {
                currentlySelectedObject.transform.Translate(0, -moveSpeed, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.M))
            {
                currentlySelectedObject.transform.Translate(0, moveSpeed, 0);
                changed = true;
            }

            // ========== ВРАЩЕНИЕ ==========
            if (Input.GetKey(KeyCode.Q))
            {
                currentlySelectedObject.transform.Rotate(rotateSpeed, 0, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.E))
            {
                currentlySelectedObject.transform.Rotate(-rotateSpeed, 0, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.R))
            {
                currentlySelectedObject.transform.Rotate(0, rotateSpeed, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.F))
            {
                currentlySelectedObject.transform.Rotate(0, -rotateSpeed, 0);
                changed = true;
            }
            if (Input.GetKey(KeyCode.T))
            {
                currentlySelectedObject.transform.Rotate(0, 0, rotateSpeed);
                changed = true;
            }
            if (Input.GetKey(KeyCode.G))
            {
                currentlySelectedObject.transform.Rotate(0, 0, -rotateSpeed);
                changed = true;
            }

            // ========== МАСШТАБИРОВАНИЕ ==========
            if (Input.GetKey(KeyCode.V))
            {
                currentlySelectedObject.transform.localScale += Vector3.one * scaleSpeed;
                changed = true;
            }
            if (Input.GetKey(KeyCode.B))
            {
                currentlySelectedObject.transform.localScale -= Vector3.one * scaleSpeed;
                changed = true;
            }

            // ========== ОТПРАВКА НА СЕРВЕР ==========
            if (changed)
            {
                ObjectIdentifier id = currentlySelectedObject.GetComponent<ObjectIdentifier>();
                if (id != null)
                {
                    UpdateObjectTransform(
                        currentlySelectedObject,
                        id.objectId,
                        currentlySelectedObject.transform.position,
                        currentlySelectedObject.transform.eulerAngles,
                        currentlySelectedObject.transform.localScale
                    );
                }
            }

            // Удаление
            if (Input.GetKeyDown(KeyCode.Z))
            {
                DeleteSelectedObject();
            }
        }
    }


    public void OnWebSocketMessage(string message)
    {
        // Обработка создания объекта
        if (message.Contains("\"type\":\"object_spawned\""))
        {
            ObjectSpawnedMessage msg = JsonUtility.FromJson<ObjectSpawnedMessage>(message);
            HandleObjectSpawned(msg);
        }
        // Обработка изменения объекта
        else if (message.Contains("\"type\":\"object_transformed\""))
        {
            ObjectTransformedMessage msg = JsonUtility.FromJson<ObjectTransformedMessage>(message);
            HandleObjectTransformed(msg);
        }
        // Обработка удаления объекта
        else if (message.Contains("\"type\":\"object_destroyed\""))
        {
            ObjectDestroyedMessage msg = JsonUtility.FromJson<ObjectDestroyedMessage>(message);
            HandleObjectDestroyed(msg);
        }
    }

    // Создание объекта (вызывается из UI)
    public void SpawnObject(string prefabName)
    {
        if (!prefabDictionary.ContainsKey(prefabName))
        {
            Debug.LogError($"Префаб '{prefabName}' не найден");
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;

        SpawnObjectMessage spawnMessage = new SpawnObjectMessage
        {
            type = "spawn_object",
            prefab = prefabName,
            transform = new ObjectTransform
            {
                position = new Position { x = spawnPos.x, y = spawnPos.y, z = spawnPos.z },
                rotation = new Rotation { x = 0f, y = 0f, z = 0f },
                scale = new Scale { x = 1f, y = 1f, z = 1f }
            },
            client_request_id = System.Guid.NewGuid().ToString()
        };

        string json = JsonUtility.ToJson(spawnMessage);
        Debug.Log($"📤 ОТПРАВКА spawn_object: {json}");
        webSocketClient.Send(json);
    }

    // Обновление объекта (позиция, вращение, масштаб)
    public void UpdateObjectTransform(GameObject obj, string objectId, Vector3 newPos, Vector3 newRot, Vector3 newScale)
    {
        TransformObjectMessage transformMessage = new TransformObjectMessage
        {
            type = "transform_object",
            object_id = objectId,
            transform = new ObjectTransform  // ← создаём вложенный объект
            {
                position = new Position { x = newPos.x, y = newPos.y, z = newPos.z },
                rotation = new Rotation { x = newRot.x, y = newRot.y, z = newRot.z },
                scale = new Scale { x = newScale.x, y = newScale.y, z = newScale.z }
            },
            client_ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonUtility.ToJson(transformMessage);
        Debug.Log($"📤 Отправлен object_transformed: {json}");
        webSocketClient.Send(json);
    }

    void DeleteSelectedObject()
    {
        if (currentlySelectedObject != null)
        {
            ObjectIdentifier identifier = currentlySelectedObject.GetComponent<ObjectIdentifier>();
            if (identifier != null)
            {
                DestroyObject(currentlySelectedObject, identifier.objectId);
                currentlySelectedObject = null;
            }
        }
    }

    // Удаление объекта
    public void DestroyObject(GameObject obj, string objectId)
    {
        Debug.Log($"🗑️ Удаляем объект: {objectId}");

        DestroyObjectMessage destroyMessage = new DestroyObjectMessage
        {
            type = "destroy_object", 
            object_id = objectId,
            reason = "user_request"
        };

        string json = JsonUtility.ToJson(destroyMessage);
        Debug.Log($"📤 ОТПРАВКА destroy: {json}");
        webSocketClient.Send(json);

        if (spawnedObjects.ContainsKey(objectId))
            spawnedObjects.Remove(objectId);

        Destroy(obj);
    }

    // Обработка получения сообщения о создании объекта
    void HandleObjectSpawned(ObjectSpawnedMessage msg)
    {
        if (!prefabDictionary.ContainsKey(msg.prefab))
        {
            Debug.LogError($"Неизвестный префаб: {msg.prefab}");
            return;
        }

        Vector3 pos = new Vector3(msg.transform.position.x, msg.transform.position.y, msg.transform.position.z);
        Quaternion rot = Quaternion.Euler(msg.transform.rotation.x, msg.transform.rotation.y, msg.transform.rotation.z);
        Vector3 scale = new Vector3(msg.transform.scale.x, msg.transform.scale.y, msg.transform.scale.z);

        GameObject obj = Instantiate(prefabDictionary[msg.prefab], pos, rot);
        obj.transform.localScale = scale;

        // Сохраняем ID объекта (можно добавить компонент для хранения)
        ObjectIdentifier identifier = obj.AddComponent<ObjectIdentifier>();
        identifier.objectId = msg.object_id;

        spawnedObjects[msg.object_id] = obj;
        Debug.Log($"Объект создан: {msg.object_id} ({msg.prefab})");

        OnObjectSpawnConfirmed?.Invoke();
    }

    // Обработка изменения объекта
    void HandleObjectTransformed(ObjectTransformedMessage msg)
    {
        if (spawnedObjects.TryGetValue(msg.object_id, out GameObject obj))
        {
            Vector3 newPos = new Vector3(msg.transform.position.x, msg.transform.position.y, msg.transform.position.z);
            Quaternion newRot = Quaternion.Euler(msg.transform.rotation.x, msg.transform.rotation.y, msg.transform.rotation.z);
            Vector3 newScale = new Vector3(msg.transform.scale.x, msg.transform.scale.y, msg.transform.scale.z);

            obj.transform.position = newPos;
            obj.transform.rotation = newRot;
            obj.transform.localScale = newScale;
        }
    }

    // Обработка удаления объекта
    void HandleObjectDestroyed(ObjectDestroyedMessage msg)
    {
        if (spawnedObjects.TryGetValue(msg.object_id, out GameObject obj))
        {
            spawnedObjects.Remove(msg.object_id);
            Destroy(obj);
            Debug.Log($"Объект удалён: {msg.object_id}");
        }
    }

    public void SetSelectedObject(GameObject obj)
    {
        currentlySelectedObject = obj;
    }

    public GameObject GetSelectedObject()
    {
        return currentlySelectedObject;
    }

}

// Компонент для хранения ID объекта
public class ObjectIdentifier : MonoBehaviour
{
    public string objectId;
}

