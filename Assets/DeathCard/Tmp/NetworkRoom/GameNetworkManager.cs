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
    }

    void OnJoinMatchmaking(NetworkConnectionToClient conn, JoinMatchmakingMessage msg)
    {
        RoomManager.instance.JoinMatchmaking(conn);
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

        // Назначаем matchId игроку
        string matchId = RoomManager.instance.GetRoomId(conn);
        if (matchId != null)
        {
            NetworkIdentity identity = conn.identity;
            if (identity != null)
                identity.SetMatchId(matchId);
        }
    }
}