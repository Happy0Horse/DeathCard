using UnityEngine;
using Mirror;

public class GameNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<JoinMatchmakingMessage>(OnJoinMatchmaking);
        NetworkServer.RegisterHandler<LeaveMatchmakingMessage>(OnLeaveMatchmaking);
        NetworkServer.RegisterHandler<PlayerReadyMessage>(OnPlayerReady);
        NetworkServer.RegisterHandler<SendChatMessage>(OnChatMessage);
        NetworkServer.RegisterHandler<SpawnRequestMessage>(OnSpawnRequest);
    }

    // public override void OnServerSceneChanged(string sceneName)
    // {
    //     base.OnServerSceneChanged(sceneName);
    //     NetworkServer.RegisterHandler<JoinMatchmakingMessage>(OnJoinMatchmaking);
    //     NetworkServer.RegisterHandler<LeaveMatchmakingMessage>(OnLeaveMatchmaking);
    //     NetworkServer.RegisterHandler<PlayerReadyMessage>(OnPlayerReady);
    //     NetworkServer.RegisterHandler<SendChatMessage>(OnChatMessage);
    //     NetworkServer.RegisterHandler<SpawnRequestMessage>(OnSpawnRequest);
    //     Debug.Log($"Сервер сменил сцену на {sceneName}, обработчики перерегистрированы");
    // }

    void OnSpawnRequest(NetworkConnectionToClient conn, SpawnRequestMessage msg)
    {
        Debug.Log("СЕРВЕР: OnSpawnRequest получен!");

        if (conn.identity != null)
        {
            Debug.Log("Уничтожаем старый identity");
            NetworkServer.RemovePlayerForConnection(conn, true);
        }

        Debug.Log($"Спавним игрока на {GetSpawnPoint()}");
        GameObject player = Instantiate(playerPrefab, GetSpawnPoint(), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"Игрок заспавнен: {player}");

        string matchId = RoomManager.instance.GetRoomId(conn);
        if (matchId != null)
            player.GetComponent<NetworkIdentity>().SetMatchId(matchId);
    }

    void OnChatMessage(NetworkConnectionToClient conn, SendChatMessage msg)
    {
        RoomManager.instance.SendChat(conn, msg.text);
    }

    void OnJoinMatchmaking(NetworkConnectionToClient conn, JoinMatchmakingMessage msg)
    {
        RoomManager.instance.JoinMatchmaking(conn, msg.playerName, msg.avatarData);
    }

    void OnLeaveMatchmaking(NetworkConnectionToClient conn, LeaveMatchmakingMessage msg)
    {
        RoomManager.instance.LeaveMatchmaking(conn);
    }

    void OnPlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage msg)
    {
        RoomManager.instance.SetReady(conn, msg.isReady);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RoomManager.instance.OnPlayerDisconnected(conn);
        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        string matchId = RoomManager.instance.GetRoomId(conn);
        if (matchId != null && conn.identity != null)
            conn.identity.SetMatchId(matchId);
    }

    public override void OnClientSceneChanged()
    {
        Debug.Log($"OnClientSceneChanged: сцена={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, identity={NetworkClient.connection?.identity}");
        if (NetworkClient.connection != null && NetworkClient.connection.identity == null)
        {
            Debug.Log("Отправляем SpawnRequestMessage");
            NetworkClient.Send(new SpawnRequestMessage());
        }
    }

    Vector3 GetSpawnPoint()
    {
        if (startPositions.Count > 0)
        {
            Transform sp = startPositions[Random.Range(0, startPositions.Count)];
            return sp.position;
        }
        return Vector3.zero;
    }

    public void ChangeToScene(string sceneName)
    {
        var method = typeof(NetworkManager).GetMethod("ClientChangeScene", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(this, new object[] { sceneName, SceneOperation.Normal, false });
    }
}