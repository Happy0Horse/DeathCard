using Mirror;

public static class MatchExtensions
{
    public static void SetMatchId(this NetworkIdentity identity, string matchId)
    {
        NetworkMatch match = identity.GetComponent<NetworkMatch>();
        if (match != null)
            match.matchId = matchId.ToGuid();
    }
}