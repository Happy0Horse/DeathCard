using UnityEngine;
using System.Collections;
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
        StartCoroutine(SpawnWhenReady(conn));
    }

    IEnumerator SpawnWhenReady(NetworkConnectionToClient conn)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"СЕРВЕР: SpawnWhenReady, сцена={currentScene}");

        if (currentScene == "Maze_Scene")
        {
            MazeGenerator maze = null;

            while (maze == null || !maze.isReady)
            {
                maze = FindObjectOfType<MazeGenerator>();
                Debug.Log($"СЕРВЕР: Ждём лабиринт, maze={maze}, isReady={maze?.isReady}");
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log($"СЕРВЕР: Лабиринт готов, спавнпоинтов={NetworkManager.startPositions.Count}");
        }

        Vector3 spawnPos = GetSpawnPoint();
        Debug.Log($"СЕРВЕР: Спавним на {spawnPos}, всего спавнпоинтов={NetworkManager.startPositions.Count}");

        if (conn.identity != null)
            NetworkServer.RemovePlayerForConnection(conn, true);

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

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