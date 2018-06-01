namespace ConstellationSimulator.Network.Message
{

    /// <summary>
    /// Allowed message-types.
    /// </summary>
    internal enum MessageType
    {
        Rreq = 1,
        Rrep = 2,
        Rerr = 3,
        Rrepack = 4,
        Data = 5,
        Hello = 6
    }
}
