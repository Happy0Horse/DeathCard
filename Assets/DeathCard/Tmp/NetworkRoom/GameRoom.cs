using System.Collections.Generic;
using Mirror;

public class GameRoom
{
    public string roomId;
    public List<NetworkConnectionToClient> players = new List<NetworkConnectionToClient>();
    public HashSet<NetworkConnectionToClient> readyPlayers = new HashSet<NetworkConnectionToClient>();
    public int maxPlayers = 4;
    public bool isStarted = false;
    public bool isCountingDown = false;

    public bool IsFull => players.Count >= maxPlayers;
    public bool AllReady => players.Count > 0 && readyPlayers.Count == players.Count;

    public GameRoom(string id)
    {
        roomId = id;
    }

    public bool AddPlayer(NetworkConnectionToClient conn)
    {
        if (IsFull || isStarted) return false;
        players.Add(conn);
        return true;
    }

    public void RemovePlayer(NetworkConnectionToClient conn)
    {
        players.Remove(conn);
        readyPlayers.Remove(conn);
    }

    public void SetReady(NetworkConnectionToClient conn, bool ready)
    {
        if (ready) readyPlayers.Add(conn);
        else readyPlayers.Remove(conn);
    }
}