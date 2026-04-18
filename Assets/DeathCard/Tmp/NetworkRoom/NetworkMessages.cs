using Mirror;

public struct JoinMatchmakingMessage : NetworkMessage { }
public struct LeaveMatchmakingMessage : NetworkMessage { }

public struct PlayerReadyMessage : NetworkMessage
{
    public bool isReady;
}

// Убираем вложенный struct, передаём массивы отдельно
public struct LobbyUpdateMessage : NetworkMessage
{
    public int playerCount;
    public int maxPlayers;
    public bool isCountingDown;

    // Фиксированные поля для 4 игроков
    public string name0; public bool ready0;
    public string name1; public bool ready1;
    public string name2; public bool ready2;
    public string name3; public bool ready3;
}

public struct CountdownMessage : NetworkMessage
{
    public int secondsLeft;
}

public struct RoomStartMessage : NetworkMessage
{
    public string roomId;
}