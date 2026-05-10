using UnityEngine;
using System.Collections;
using System.Linq;
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
        NetworkServer.RegisterHandler<ManualStartMessage>(OnManualStart);
    }

    void OnManualStart(NetworkConnectionToClient conn, ManualStartMessage msg)
    {
        // Находим GameManager для этой комнаты
        string roomId = RoomManager.instance.GetRoomId(conn);
        // GameManager хранится per-room — нужно добавить в RoomManager
        RoomManager.instance.ManualStart(roomId);
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

        if (currentScene == "Maze_Scene")
        {
            MazeGenerator maze = null;
            while (maze == null || !maze.isReady)
            {
                maze = FindObjectOfType<MazeGenerator>();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (currentScene == "Game_Scene") // замените на название вашей хекс сцены
        {
            HexSpawnManager hexSpawn = null;
            while (hexSpawn == null)
            {
                hexSpawn = FindObjectOfType<HexSpawnManager>();
                yield return new WaitForSeconds(0.1f);
            }

            while (NetworkManager.startPositions.Count == 0)
                yield return new WaitForSeconds(0.1f);
        }

        if (conn.identity != null)
            NetworkServer.RemovePlayerForConnection(conn, true);

        Vector3 spawnPos = GetSpawnPoint();
        Debug.Log($"[GameNetworkManager] GetSpawnPoint вернул: {spawnPos}, всего позиций: {NetworkManager.startPositions.Count}");
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[GameNetworkManager] Игрок заспавнен на {player.transform.position}");
        NetworkServer.AddPlayerForConnection(conn, player);

        // Инициализируем HexGridNavigator если на хекс сцене
        if (currentScene == "Game_Scene")
        {
            HexGridNavigator navigator = player.GetComponentInChildren<HexGridNavigator>();
            HexViewManager viewManager = player.GetComponentInChildren<HexViewManager>();
            HexGrid grid = FindObjectOfType<HexGrid>();
            HexGridGenerator generator = FindObjectOfType<HexGridGenerator>();

            if (navigator != null && viewManager != null && grid != null)
            {
                // Находим ближайшую клетку к позиции спавна
                HexCell startCell = grid.Cells.Values
                    .OrderBy(c => Vector3.Distance(c.transform.position, player.transform.position))
                    .FirstOrDefault();

                if (startCell != null)
                {
                    viewManager.SetGridCenter(generator.transform);
                    navigator.Initialize(grid, startCell.coordinates, viewManager);
                }
            }
        }

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Только клиентские обработчики
        NetworkClient.RegisterHandler<GameStateMessage>(OnGameStateReceived);
        NetworkClient.RegisterHandler<MazeTimerMessage>(OnMazeTimerUpdate);
        NetworkClient.RegisterHandler<TimerUpdateMessage>(msg => { });
        NetworkClient.RegisterHandler<StartWaitMessage>(msg => { });
        NetworkClient.RegisterHandler<GameStartedMessage>(msg => { });
        NetworkClient.RegisterHandler<RoundOverMessage>(msg => { });
        NetworkClient.RegisterHandler<DeadlineReachedMessage>(msg => { });
        NetworkClient.RegisterHandler<OvertimeTickMessage>(msg => { });
    }

    void OnGameStateReceived(GameStateMessage msg)
    {
        Debug.Log($"[Client] GameState={msg.state}, Round={msg.round}, Scene={msg.sceneName}");
        if (!string.IsNullOrEmpty(msg.sceneName))
            ((GameNetworkManager)NetworkManager.singleton).ChangeToScene(msg.sceneName);
    }

    void OnMazeTimerUpdate(MazeTimerMessage msg)
    {
        MazeTimer timer = FindObjectOfType<MazeTimer>();
        if (timer != null)
            timer.UpdateTimer(msg.timeRemaining);
    }
}