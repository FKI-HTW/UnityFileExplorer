namespace CENTIS.UnityFileExplorer
{
    public enum ENodeFailedToLoad
    {
        /// <summary>
        /// If the caller has insufficient permissions to load the node
        /// </summary>
        MissingPermissions,
        /// <summary>
        /// If the node's path is too long
        /// </summary>
        PathTooLong,
        /// <summary>
        /// If the node is of an invalid type or failed to load for unknown reasons
        /// </summary>
        InvalidNode
    }
}
