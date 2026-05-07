[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string username;
    public string full_name;
    public string password;
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string access_token;
    public string refresh_token;
}

[System.Serializable]
public class Room
{
    public string id;
    public string name;
    public string owner_id;
    public bool is_active;
}

[System.Serializable]
public class RoomsList
{
    public Room[] rooms;
}

[System.Serializable]
public class CreateRoomRequest
{
    public string name;
}

[System.Serializable]
public class TransformMessage
{
    public string type;
    public Position position;
    public Rotation rotation;
    public long client_ts;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Rotation
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ApiErrorResponse
{
    public ErrorDetail[] detail;
}

[System.Serializable]
public class ErrorDetail
{
    public string type;
    public string[] loc;
    public string msg;
    public string input;
    public ErrorContext ctx;
}

[System.Serializable]
public class ErrorContext
{
    public int min_length;
    public string error;
}

[System.Serializable]
public class ObjectSpawnedMessage
{
    public string type;
    public string object_id;
    public string owner_user_id;
    public string prefab;
    public ObjectTransform transform;
    public ObjectData data;
    public bool ephemeral;
    public float server_ts;
    public string client_request_id;
}

[System.Serializable]
public class ObjectTransformedMessage
{
    public string type;
    public string object_id;
    public string by_user_id;
    public ObjectTransform transform;
    public float server_ts;
    public long client_ts;
}

[System.Serializable]
public class ObjectDestroyedMessage
{
    public string type;
    public string object_id;
    public string by_user_id;
    public float server_ts;
    public string reason;
}

[System.Serializable]
public class ObjectTransform
{
    public Position position;
    public Rotation rotation;
    public Scale scale;
}

[System.Serializable]
public class Scale
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ObjectData
{
    public string color;
}

[System.Serializable]
public class SpawnObjectMessage
{
    public string type;
    public string prefab;
    public ObjectTransform transform; 
    public string client_request_id;
}

[System.Serializable]
public class TransformObjectMessage
{
    public string type;
    public string object_id;
    public ObjectTransform transform; 
    public long client_ts;
}

[System.Serializable]
public class DestroyObjectMessage
{
    public string type;
    public string object_id;
    public string reason;
}