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
        NetworkServer.RegisterHandler<DomeBrokenMessage>(OnDomeBroken);
        NetworkServer.RegisterHandler<GameOverMessage>(OnGameOver);
        NetworkServer.RegisterHandler<TimerFreezeMessage>(OnTimerFreeze);
        NetworkServer.RegisterHandler<PlayerDiedMessage>(OnPlayerDied);
    }

    void OnManualStart(NetworkConnectionToClient conn, ManualStartMessage msg)
    {
        // Находим GameManager для этой комнаты
        string roomId = RoomManager.instance.GetRoomId(conn);
        // GameManager хранится per-room — нужно добавить в RoomManager
        RoomManager.instance.ManualStart(roomId);
    }

    void OnDomeBroken(NetworkConnectionToClient conn, DomeBrokenMessage msg)
    {
        string roomId = RoomManager.instance.GetRoomId(conn);
        if (roomId != null)
            RoomManager.instance.HandleDomeBroken(roomId);
    }

    void OnSpawnRequest(NetworkConnectionToClient conn, SpawnRequestMessage msg)
    {
        StartCoroutine(SpawnWhenReady(conn));
    }

    void OnPlayerDied(NetworkConnectionToClient conn, PlayerDiedMessage msg)
    {
        Debug.Log($"[Server] Игрок умер, дисконнектим");
        conn.Disconnect();
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

    void OnGameOver(NetworkConnectionToClient conn, GameOverMessage msg)
    {
        string roomId = RoomManager.instance.GetRoomId(conn);
        if (roomId != null)
            RoomManager.instance.GetGameManager(roomId)?.EnterGameOver();
    }

    void OnTimerFreeze(NetworkConnectionToClient conn, TimerFreezeMessage msg)
    {
        string roomId = RoomManager.instance.GetRoomId(conn);
        if (roomId != null)
            RoomManager.instance.GetGameManager(roomId)?.SetTimerFreeze(msg.freeze);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Только клиентские обработчики
        NetworkClient.RegisterHandler<GameStateMessage>(OnGameStateReceived);
        NetworkClient.RegisterHandler<MazeTimerMessage>(OnMazeTimerUpdate);
        NetworkClient.RegisterHandler<OvertimeTickMessage>(OnOvertimeTick);
        NetworkClient.RegisterHandler<RoundOverMessage>(OnRoundOver);
        NetworkClient.RegisterHandler<DistributeCardsMessage>(OnDistributeCards);
        NetworkClient.RegisterHandler<TimerUpdateMessage>(msg => { });
        NetworkClient.RegisterHandler<StartWaitMessage>(msg => { });
        NetworkClient.RegisterHandler<GameStartedMessage>(msg => { });
        NetworkClient.RegisterHandler<DeadlineReachedMessage>(msg => { });
        NetworkClient.RegisterHandler<EndgameStartedMessage>(msg => { });
        NetworkClient.RegisterHandler<TimerFreezeMessage>(msg => { });
        NetworkClient.RegisterHandler<GameOverMessage>(msg => { });
    }

    void OnGameStateReceived(GameStateMessage msg)
    {
        Debug.Log($"[Client] GameState={msg.state}, Round={msg.round}");
        
        if (!string.IsNullOrEmpty(msg.sceneName))
            ((GameNetworkManager)NetworkManager.singleton).ChangeToScene(msg.sceneName);

        if (msg.state == 1)
        {
            Debug.Log($"[Client] Вызываем InitializeForRound({msg.round})");
            StartCoroutine(InitializeDomesWhenReady(msg.round));
        }
    }

    IEnumerator InitializeDomesWhenReady(int round)
    {
        float timeout = 10f;
        while (timeout > 0)
        {
            var dome = FindObjectOfType<SacrificeDome>();
            Debug.Log($"[Client] Ищем дом... dome={dome}");
            if (dome != null)
            {
                DomeInitializer.InitializeForRound(round);
                yield break;
            }
            timeout -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Debug.LogError("[Client] Дом не найден за 10 секунд!");
    }

    void OnMazeTimerUpdate(MazeTimerMessage msg)
    {
        MazeTimer timer = FindObjectOfType<MazeTimer>();
        if (timer != null)
            timer.UpdateTimer(msg.timeRemaining);
    }

    void OnOvertimeTick(OvertimeTickMessage msg)
    {
        foreach (var stat in FindObjectsOfType<PlayerStat>())
            stat.TakeDamage(msg.damage);
    }

    void OnRoundOver(RoundOverMessage msg)
    {
        foreach (var manager in FindObjectsOfType<CardManager>())
            manager.HandleRoundOver(msg.round);
    }

    void OnDistributeCards(DistributeCardsMessage msg)
    {
        foreach (var manager in FindObjectsOfType<CardManager>())
            manager.HandleGlobalCardDistribution(msg.count);
    }
}