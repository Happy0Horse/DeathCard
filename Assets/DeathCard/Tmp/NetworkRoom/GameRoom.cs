using System.Collections.Generic;
using Mirror;

public class PlayerInfo
{
    public string name;
    public byte[] avatarData;
}

public class GameRoom
{
    public string roomId;
    public List<NetworkConnectionToClient> players = new List<NetworkConnectionToClient>();
    public HashSet<NetworkConnectionToClient> readyPlayers = new HashSet<NetworkConnectionToClient>();
    public Dictionary<NetworkConnectionToClient, PlayerInfo> playerInfos
        = new Dictionary<NetworkConnectionToClient, PlayerInfo>();
    public int maxPlayers = 4;
    public bool isStarted = false;
    public bool isCountingDown = false;

    public bool IsFull => players.Count >= maxPlayers;
    public bool AllReady => players.Count > 0 && readyPlayers.Count == players.Count;

    public GameRoom(string id)
    {
        roomId = id;
    }

    public bool AddPlayer(NetworkConnectionToClient conn, PlayerInfo info)
    {
        if (IsFull || isStarted) return false;
        players.Add(conn);
        playerInfos[conn] = info;
        return true;
    }

    public void RemovePlayer(NetworkConnectionToClient conn)
    {
        players.Remove(conn);
        readyPlayers.Remove(conn);
        playerInfos.Remove(conn);
    }

    public void SetReady(NetworkConnectionToClient conn, bool ready)
    {
        if (ready) readyPlayers.Add(conn);
        else readyPlayers.Remove(conn);
    }
}