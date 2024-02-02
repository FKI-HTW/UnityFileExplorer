using System.IO;

namespace CENTIS.UnityFileExplorer
{
    public static class ExplorerExtensions
    {
        public static NodeInformation GetNodeInformation(this DriveInfo driveInfo)
        {
            return new()
            {
                Name = driveInfo.Name
			};
        }

        public static NodeInformation GetNodeInformation(this DirectoryInfo directoryInfo)
        {
            return new()
            {
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
				Name = fileInfo.Name,
				Path = fileInfo.DirectoryName,
				CreatedAt = fileInfo.CreationTime,
				UpdatedAt = fileInfo.LastWriteTime
			};
		}
    }
}
