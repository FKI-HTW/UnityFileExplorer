/*
===================================================================
Unity Assets by MAKAKA GAMES: https://makaka.org/o/all-unity-assets
===================================================================

Online Docs (Latest): https://makaka.org/unity-assets
Offline Docs: You have a PDF file in the package folder.

=======
SUPPORT
=======

First of all, read the docs. If it didn’t help, get the support.

Web: https://makaka.org/support
Email: info@makaka.org

If you find a bug or you can’t use the asset as you need, 
please first send email to info@makaka.org
before leaving a review to the asset store.

I am here to help you and to improve my products for the best.
*/

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace MakakaGames 
{
    public class GUIDRegeneratorMenu 
    {
        [MenuItem("Window/Makaka Games/Regenerate GUIDs for assets in specific folder", false, 100)]
        public static void RegenerateGUIDsForAssetsInSpecificFolder() 
        {
            string pathFolder = EditorUtility.OpenFolderPanel("Choose folder for regenerating assets GUIDs", "Assets/", String.Empty);

            if (!string.IsNullOrEmpty(pathFolder) && EditorUtility.DisplayDialog("GUIDs regeneration",
               "You are going to start the process of GUID regeneration. This may have unexpected results. \n\nMAKE A PROJECT BACKUP BEFORE PROCEEDING!",
               "Regenerate GUIDs", "Cancel")) 
            {
                try 
                {
                    AssetDatabase.StartAssetEditing();

                    GUIDRegenerator regenerator = new GUIDRegenerator(pathFolder);

                    regenerator.RegenerateGUIDs();
                }
                finally 
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
            }
        }
    }

    internal class GUIDRegenerator 
    {
        private static readonly string[] kDefaultFileExtensions = 
        {
            "*.meta",
            "*.mat",
            "*.anim",
            "*.prefab",
            "*.unity",
            "*.asset",
            "*.guiskin",
            "*.preset",
            "*.fontsettings",
			"*.controller",
            "*.pdf"
        };

        private readonly string _assetsPath;

        public GUIDRegenerator(string assetsPath) 
        {
            _assetsPath = assetsPath;
        }

        public void RegenerateGUIDs(string[] regeneratedExtensions = null) 
        {
            if (regeneratedExtensions == null) 
            {
                regeneratedExtensions = kDefaultFileExtensions;
            }

            // Get list of working files
            List<string> filesPaths = new List<string>();
            foreach (string extension in regeneratedExtensions) 
            {
                filesPaths.AddRange(
                    Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                    );
            }

            // Create dictionary to hold old-to-new GUID map
            Dictionary<string, string> guidOldToNewMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> guidsInFileMap = new Dictionary<string, List<string>>();

            // We must only replace GUIDs for Resources present in Assets. 
            // Otherwise built-in resources (shader, meshes etc) get overwritten.
            HashSet<string> ownGuids = new HashSet<string>();

            // Traverse all files, remember which GUIDs are in which files and generate new GUIDs
            int counter = 0;

            foreach (string filePath in filesPaths) 
            {
                if (!EditorUtility.DisplayCancelableProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath),
                    counter / (float) filesPaths.Count)) 
                {
                    string contents = File.ReadAllText(filePath);

                    IEnumerable<string> guids = GetGUIDs(contents);
                    bool isFirstGuid = true;
                    foreach (string oldGuid in guids) 
                    {
                        // First GUID in .meta file is always the GUID of the asset itself
                        if (isFirstGuid && Path.GetExtension(filePath) == ".meta") {
                            ownGuids.Add(oldGuid);
                            isFirstGuid = false;
                        }
                        // Generate and save new GUID if we haven't added it before
                        if (!guidOldToNewMap.ContainsKey(oldGuid)) {
                            string newGuid = Guid.NewGuid().ToString("N");
                            guidOldToNewMap.Add(oldGuid, newGuid);
                        }

                        if (!guidsInFileMap.ContainsKey(filePath))
                            guidsInFileMap[filePath] = new List<string>();

                        if (!guidsInFileMap[filePath].Contains(oldGuid)) {
                            guidsInFileMap[filePath].Add(oldGuid);
                        }
                    }

                    counter++;
                }
                else 
                {
                    UnityEngine.Debug.LogWarning("GUID regeneration canceled");
                    return;
                }
            }

            // Traverse the files again and replace the old GUIDs
            counter = -1;

            int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;

            foreach (string filePath in guidsInFileMap.Keys) 
            {
                EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float) guidsInFileMapKeysCount);
                counter++;

                string contents = File.ReadAllText(filePath);
                foreach (string oldGuid in guidsInFileMap[filePath]) 
                {
                    if (!ownGuids.Contains(oldGuid))
                        continue;

                    string newGuid = guidOldToNewMap[oldGuid];
                    if (string.IsNullOrEmpty(newGuid))
                        throw new NullReferenceException("newGuid == null");

                    contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                }

                File.WriteAllText(filePath, contents);
            }
        }

        private static IEnumerable<string> GetGUIDs(string text) 
        {
            const string guidStart = "guid: ";
            const int guidLength = 32;
            int textLength = text.Length;
            int guidStartLength = guidStart.Length;
            List<string> guids = new List<string>();

            int index = 0;
            while (index + guidStartLength + guidLength < textLength) 
            {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                string guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGUID(guid)) 
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        private static bool IsGUID(string text) 
        {
            for (int i = 0; i < text.Length; i++) 
            {
                char c = text[i];

                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z')))
                {
                    return false;
                }
            }

            return true;
        }

        private static string MakeRelativePath(string fromPath, string toPath) 
        {
            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }
    }
}

#endif  