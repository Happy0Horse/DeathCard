using Mirror;
using System;

public static class StringExtensions
{
    public static System.Guid ToGuid(this string str)
    {
        // Конвертируем строку roomId в Guid
        byte[] bytes = new byte[16];
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
        System.Array.Copy(strBytes, bytes, System.Math.Min(strBytes.Length, 16));
        return new System.Guid(bytes);
    }
}

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