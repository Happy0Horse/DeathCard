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
        NetworkServer.RegisterHandler<SendChatMessage>(OnChatMessage); // добавьте это
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
}