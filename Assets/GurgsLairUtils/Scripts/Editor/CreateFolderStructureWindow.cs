using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CreateFolderStructureWindow : EditorWindow
{
    private string projectName = "";

    [MenuItem("Dojo SDK/Grug Lair/Create Folder Structure")]
    public static void ShowWindow()
    {
        GetWindow<CreateFolderStructureWindow>("Create Folder Structure");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter Project Name", EditorStyles.boldLabel);
        projectName = EditorGUILayout.TextField("Project Name", projectName);

        if (GUILayout.Button("Create"))
        {
            if (!string.IsNullOrEmpty(projectName))
            {
                CreateFolderStructure(projectName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please enter a valid project name.", "OK");
            }
        }
    }

    private void CreateFolderStructure(string projectName)
    {
        // Base directory
        string baseDir = Path.Combine("Assets", projectName);

        // List of subfolders to create
        List<string> subfolders = new List<string>
        {
            "Textures",
            "Fonts",
            "Scripts",
            "Prefabs",
            "Images",
            "Sounds",
            "Scenes",
            "Animations",
            "Materials",

            "Tilemaps",

            "Scripts/Models",
            "Scripts/Static",
            "Scripts/UI",
            "Scripts/Functionality",
            "Scripts/Editor",
            "Scripts/Gameplay",
            "Scripts/Misc",
            "Scripts/Unused",
            "Scripts/Models/UnusedModels",
            "Scripts/Static/DojoGenerated",

            "Sounds/Background",
            "Sounds/SFX",

        };

        // Create the directories
        foreach (var subfolder in subfolders)
        {
            string dir = Path.Combine(baseDir, subfolder);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"Created directory: {dir}");
            }
            else
            {
                Debug.Log($"Directory already exists: {dir}");
            }
        }

        Directory.CreateDirectory(Path.Combine("Assets", "Resources"));
    }
}
