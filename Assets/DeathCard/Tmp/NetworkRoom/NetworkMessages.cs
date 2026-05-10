using Mirror;
using System;

public struct JoinMatchmakingMessage : NetworkMessage
{
    public string playerName;
    public byte[] avatarData; // JPG bytes
}

public struct LeaveMatchmakingMessage : NetworkMessage { }

public struct PlayerReadyMessage : NetworkMessage
{
    public bool isReady;
}

public struct LobbyUpdateMessage : NetworkMessage
{
    public int playerCount;
    public int maxPlayers;
    public bool isCountingDown;

    public string name0; public bool ready0; public byte[] avatar0;
    public string name1; public bool ready1; public byte[] avatar1;
    public string name2; public bool ready2; public byte[] avatar2;
    public string name3; public bool ready3; public byte[] avatar3;
}

public struct CountdownMessage : NetworkMessage
{
    public int secondsLeft;
}

public struct RoomStartMessage : NetworkMessage
{
    public string roomId;
}

public static class StringExtensions
{
    public static System.Guid ToGuid(this string str)
    {
        byte[] bytes = new byte[16];
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
        System.Array.Copy(strBytes, bytes, System.Math.Min(strBytes.Length, 16));
        return new System.Guid(bytes);
    }
}

public struct ChatMessage : NetworkMessage
{
    public string senderName;
    public string text;
}

public struct SendChatMessage : NetworkMessage
{
    public string text;
}

public struct SpawnRequestMessage : NetworkMessage { }

public struct GameStateMessage : NetworkMessage
{
    public int state;
    public int round;
    public string sceneName;
}

public struct TimerUpdateMessage : NetworkMessage
{
    public float timeRemaining;
    public float nextDistribution;
    public int cardsPerInterval;
}

public struct StartWaitMessage : NetworkMessage
{
    public float timeUntilStart;
}

public struct GameStartedMessage : NetworkMessage { }

public struct RoundOverMessage : NetworkMessage
{
    public int round;
    public float transitionTime;
}

public struct DeadlineReachedMessage : NetworkMessage { }

public struct OvertimeTickMessage : NetworkMessage
{
    public float damage;
}

public struct ManualStartMessage : NetworkMessage { }

public struct MazeTimerMessage : NetworkMessage
{
    public float timeRemaining;
}