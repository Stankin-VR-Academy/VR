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