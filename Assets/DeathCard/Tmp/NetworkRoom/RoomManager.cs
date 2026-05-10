using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    public string gameSceneName = "Game_Scene";

    private List<GameRoom> rooms = new List<GameRoom>();
    private Dictionary<NetworkConnectionToClient, GameRoom> playerRoomMap
        = new Dictionary<NetworkConnectionToClient, GameRoom>();

    private int roomCounter = 0;

    private Dictionary<string, GameManager> _gameManagers = new Dictionary<string, GameManager>();

    void Awake()
    {
        instance = this;
    }

    public string GetRoomId(NetworkConnectionToClient conn)
    {
        if (playerRoomMap.ContainsKey(conn))
            return playerRoomMap[conn].roomId;
        return null;
    }

    public void JoinMatchmaking(NetworkConnectionToClient conn, string playerName, byte[] avatarData)
    {
        GameRoom room = rooms.Find(r => !r.IsFull && !r.isStarted);

        if (room == null)
        {
            room = new GameRoom("room_" + roomCounter++);
            rooms.Add(room);
        }

        PlayerInfo info = new PlayerInfo { name = playerName, avatarData = avatarData };
        room.AddPlayer(conn, info);
        playerRoomMap[conn] = room;

        SendLobbyUpdate(room);
    }

    public void LeaveMatchmaking(NetworkConnectionToClient conn)
    {
        if (!playerRoomMap.ContainsKey(conn)) return;

        GameRoom room = playerRoomMap[conn];
        room.RemovePlayer(conn);
        playerRoomMap.Remove(conn);

        if (room.players.Count == 0)
        {
            if (room.isStarted)
                DestroyRoom(room);
            else
                rooms.Remove(room);
        }
        else
        {
            SendLobbyUpdate(room);
        }
    }

    public void SetReady(NetworkConnectionToClient conn, bool ready)
    {
        if (!playerRoomMap.ContainsKey(conn)) return;

        GameRoom room = playerRoomMap[conn];
        room.SetReady(conn, ready);

        SendLobbyUpdate(room);

        if (room.AllReady && !room.isCountingDown)
            StartCoroutine(StartCountdown(room));
    }

    void SendLobbyUpdate(GameRoom room)
    {
        string[] names = new string[4];
        bool[] ready = new bool[4];
        byte[][] avatars = new byte[4][];

        for (int i = 0; i < 4; i++)
        {
            if (i < room.players.Count)
            {
                var conn = room.players[i];
                var info = room.playerInfos[conn];
                names[i] = info.name;
                ready[i] = room.readyPlayers.Contains(conn);
                avatars[i] = info.avatarData ?? new byte[0];
            }
            else
            {
                names[i] = "";
                ready[i] = false;
                avatars[i] = new byte[0];
            }
        }

        LobbyUpdateMessage msg = new LobbyUpdateMessage
        {
            playerCount = room.players.Count,
            maxPlayers = room.maxPlayers,
            isCountingDown = room.isCountingDown,
            name0 = names[0], ready0 = ready[0], avatar0 = avatars[0],
            name1 = names[1], ready1 = ready[1], avatar1 = avatars[1],
            name2 = names[2], ready2 = ready[2], avatar2 = avatars[2],
            name3 = names[3], ready3 = ready[3], avatar3 = avatars[3],
        };

        foreach (var conn in room.players)
            conn.Send(msg);
    }

    IEnumerator StartCountdown(GameRoom room)
    {
        room.isCountingDown = true;

        for (int i = 10; i >= 0; i--)
        {
            if (!room.AllReady)
            {
                room.isCountingDown = false;
                SendLobbyUpdate(room);
                yield break;
            }

            CountdownMessage msg = new CountdownMessage { secondsLeft = i };
            foreach (var conn in room.players)
                conn.Send(msg);

            yield return new WaitForSeconds(1f);
        }

        StartRoom(room);
    }

    void StartRoom(GameRoom room)
    {
        room.isStarted = true;

        foreach (var conn in room.players)
            conn.Send(new RoomStartMessage { roomId = room.roomId });

        // Даём клиентам время получить сообщение
        StartCoroutine(InitGameManagerDelayed(room));
    }

    IEnumerator InitGameManagerDelayed(GameRoom room)
    {
        yield return new WaitForSeconds(0.5f);

        GameObject gmObj = new GameObject($"GameManager_{room.roomId}");
        GameManager gm = gmObj.AddComponent<GameManager>();
        DontDestroyOnLoad(gmObj);
        _gameManagers[room.roomId] = gm;
        gm.Initialize(room.roomId);
    }

    public void ManualStart(string roomId)
    {
        if (_gameManagers.TryGetValue(roomId, out GameManager gm))
            gm.StartGame();
    }

    public void ChangeRoomScene(string roomId, string sceneName)
    {
        GameRoom room = rooms.Find(r => r.roomId == roomId);
        if (room == null) return;

        StartCoroutine(ChangeRoomSceneCoroutine(room, sceneName));
    }

    IEnumerator ChangeRoomSceneCoroutine(GameRoom room, string sceneName)
    {
        if (room.scene.IsValid())
            yield return SceneManager.UnloadSceneAsync(room.scene);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return op;

        room.scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        foreach (var conn in room.players)
        {
            if (conn.identity != null)
                SceneManager.MoveGameObjectToScene(conn.identity.gameObject, room.scene);
        }
    }

    public void SendToRoom<T>(string roomId, T msg) where T : struct, NetworkMessage
    {
        GameRoom room = rooms.Find(r => r.roomId == roomId);
        if (room == null) return;

        foreach (var conn in room.players)
            conn.Send(msg);
    }
    
    public void OnPlayerDisconnected(NetworkConnectionToClient conn)
    {
        if (!playerRoomMap.ContainsKey(conn)) return;

        GameRoom room = playerRoomMap[conn];
        room.RemovePlayer(conn);
        playerRoomMap.Remove(conn);

        if (room.players.Count == 0)
        {
            DestroyRoom(room);
        }
        else
        {
            SendLobbyUpdate(room);
        }
    }
    
    void DestroyRoom(GameRoom room)
    {
        Debug.Log($"[RoomManager] Уничтожаем комнату {room.roomId}");

        // Уничтожаем GameManager
        if (_gameManagers.TryGetValue(room.roomId, out GameManager gm))
        {
            if (gm != null)
                Destroy(gm.gameObject);
            _gameManagers.Remove(room.roomId);
        }

        // Выгружаем сцену
        if (room.scene.IsValid())
            StartCoroutine(UnloadRoomScene(room));

        rooms.Remove(room);
    }

    IEnumerator UnloadRoomScene(GameRoom room)
    {
        yield return SceneManager.UnloadSceneAsync(room.scene);
        Debug.Log($"[RoomManager] Сцена комнаты {room.roomId} выгружена");
    }

    public void SendChat(NetworkConnectionToClient conn, string text)
    {
        if (!playerRoomMap.ContainsKey(conn)) return;
        if (string.IsNullOrEmpty(text)) return;

        GameRoom room = playerRoomMap[conn];
        string senderName = room.playerInfos[conn].name;

        // Ограничиваем длину сообщения
        if (text.Length > 100) text = text.Substring(0, 100);

        ChatMessage msg = new ChatMessage
        {
            senderName = senderName,
            text = text
        };

        // Рассылаем только игрокам в той же комнате
        foreach (var player in room.players)
            player.Send(msg);
    }

    public void HandleDomeBroken(string roomId)
    {
        if (_gameManagers.TryGetValue(roomId, out GameManager gm))
            gm.TriggerDomeBroken();
    }

    public GameManager GetGameManager(string roomId)
    {
        _gameManagers.TryGetValue(roomId, out GameManager gm);
        return gm;
    }
}