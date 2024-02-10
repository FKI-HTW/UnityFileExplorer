using System;

namespace CENTIS.UnityFileExplorer
{
    public class NodeInformation
    {
        public ENodeType Type;
        public string Name;
        public string Path;
        public DateTime CreatedAt;
        public DateTime UpdatedAt;
        public string CreatedBy;
    }

    public enum ENodeType
    {
        Drive,
        Folder,
        File
    }
}
