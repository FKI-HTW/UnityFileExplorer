using System.IO;

namespace CENTIS.UnityFileExplorer
{
    public static class ExplorerExtensions
    {
        public static NodeInformation GetNodeInformation(this DriveInfo driveInfo)
        {
            return new()
            {
                Type = ENodeType.Drive,
                Name = driveInfo.Name
			};
        }

        public static NodeInformation GetNodeInformation(this DirectoryInfo directoryInfo)
        {
            return new()
            {
                Type = ENodeType.Folder,
                Name = directoryInfo.Name,
                Path = directoryInfo.Parent?.FullName,
                CreatedAt = directoryInfo.CreationTime,
                UpdatedAt = directoryInfo.LastWriteTime
            };
        }

        public static NodeInformation GetNodeInformation(this FileInfo fileInfo)
        {
			return new()
			{
                Type = ENodeType.File,
				Name = fileInfo.Name,
				Path = fileInfo.DirectoryName,
				CreatedAt = fileInfo.CreationTime,
				UpdatedAt = fileInfo.LastWriteTime
			};
		}
    }
}
