using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    public string gameSceneName = "GameScene"; // замените на название вашей игровой сцены

    private List<GameRoom> rooms = new List<GameRoom>();
    private Dictionary<NetworkConnectionToClient, GameRoom> playerRoomMap
        = new Dictionary<NetworkConnectionToClient, GameRoom>();

    private int roomCounter = 0;

    void Awake()
    {
        instance = this;
    }

    public void JoinMatchmaking(NetworkConnectionToClient conn)
    {
        GameRoom room = rooms.Find(r => !r.IsFull && !r.isStarted);

        if (room == null)
        {
            room = new GameRoom("room_" + roomCounter++);
            rooms.Add(room);
            Debug.Log($"Создана комната {room.roomId}");
        }

        room.AddPlayer(conn);
        playerRoomMap[conn] = room;

        Debug.Log($"Игрок добавлен в {room.roomId} ({room.players.Count}/4)");

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
            rooms.Remove(room);
            Debug.Log($"Комната {room.roomId} удалена");
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

        for (int i = 0; i < 4; i++)
        {
            if (i < room.players.Count)
            {
                names[i] = $"Игрок {i + 1}";
                ready[i] = room.readyPlayers.Contains(room.players[i]);
            }
            else
            {
                names[i] = "";
                ready[i] = false;
            }
        }

        LobbyUpdateMessage msg = new LobbyUpdateMessage
        {
            playerCount = room.players.Count,
            maxPlayers = room.maxPlayers,
            isCountingDown = room.isCountingDown,
            name0 = names[0], ready0 = ready[0],
            name1 = names[1], ready1 = ready[1],
            name2 = names[2], ready2 = ready[2],
            name3 = names[3], ready3 = ready[3],
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
    }

    public void OnPlayerDisconnected(NetworkConnectionToClient conn)
    {
        LeaveMatchmaking(conn);
    }
}