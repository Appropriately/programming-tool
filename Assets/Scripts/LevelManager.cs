using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Abstracts scene navigation while providing support for the grammar-based map generation.
/// </summary>
public static class LevelManager
{
    private const string SCENE_NAME = "TestGame";
    private const string MAIN_MENU_SCENE_NAME = "TestMenu";

    public static string error = null;

    private static int currentScene = 2;
    private static Dictionary<int, string> names = new Dictionary<int, string>();
    private static Dictionary<int, string> maps = new Dictionary<int, string>();
    private static Dictionary<int, Block[]> blocks = new Dictionary<int, Block[]>();

    /// <summary>
    /// Insert some default values into the data dictionaries.
    /// </summary>
    public static void Seed()
    {
        if (names.Count <= 0)
        {
            names.Add(1, "Basic movement");
            maps.Add(1, "XEX\nXOX\nXSX");
            blocks.Add(1, new Block[]{Block.Move});

            names.Add(2, "Rotate Right");
            maps.Add(2, "XXXXX\nXXOEX\nXXOXX\nXXSXX\nXXXXX");
            blocks.Add(2, new Block[]{Block.Move, Block.RotateRight});

            names.Add(3, "Long Rotate Right");
            maps.Add(3, "OOOOO\nOXXXO\nOXXXO\nOXXXO\nSXXXE");
            blocks.Add(3, new Block[]{Block.Move, Block.RotateRight});

            names.Add(4, "Left and Right rotate");
            maps.Add(4, "XXXXE\nXXXOO\nXXOOX\nXOOXX\nXSXXX");
            blocks.Add(4, new Block[]{Block.Move, Block.RotateRight, Block.RotateLeft});
        }
    }

    /// <summary>
    /// Load the generic scene for all levels and set the appropriate sceneID for convenient information pulling
    /// </summary>
    /// <param name="sceneID">The ID of the scene that needs to be set</param>
    public static void Load(int sceneID)
    {
        error = null;
        currentScene = sceneID;
        SceneManager.LoadScene(SCENE_NAME);
    }

    public static void GoToMainMenu() {
        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }

    public static void ErrorToMainMenu(string errorMessage) {
        error = errorMessage;
        GoToMainMenu();
    }

    /// <summary>
    /// Returns a dictionary of information about the Level Manager's registered scenes.
    /// </summary>
    /// <returns>A dictionary containing the sceneID and the name as a string</returns>
    public static Dictionary<int, string> GetLevels() => names;

    public static string GetName() => GetNameForID(currentScene);
    public static string GetMap() => GetMapForID(currentScene);
    public static Block[] GetBlocks() => GetBlocksForID(currentScene);

    public static string GetNameForID(int sceneID) => names[sceneID];
    public static string GetMapForID(int sceneID) => maps[sceneID];
    public static Block[] GetBlocksForID(int sceneID) => blocks[sceneID];
}