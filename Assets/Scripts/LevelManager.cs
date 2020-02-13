using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Abstracts scene navigation while providing support for the grammar-based map generation.
/// </summary>
public static class LevelManager
{
    private const string SCENE_NAME = "TestGame";
    private const string MAIN_MENU_SCENE_NAME = "TestMenu";

    public static string error = null;

    private static int currentScene;
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
            AddLevel(1, "Basic Movement", "XEX\nXOX\nXSX", new Block[]{Block.Move, Block.Speak});
            AddLevel(
                2, "Rotate Right", "XXXXX\nXXOEX\nXXOXX\nXXSXX\nXXXXX",
                new Block[]{Block.Move, Block.RotateRight}
            );
            AddLevel(
                3, "Long boy", "XXXEXXX\nXXXOXXX\nXXXOXXX\nXXXOXXX\nXXXOXXX\nXXXOXXX\nXXXSXXX",
                new Block[]{Block.Move, Block.WhileNotAtExit}
            );
            AddLevel(
                4, "Conditional Test", "OOOOO\nOXXXO\nOXXXO\nOXXXO\nSXXXE",
                new Block[]{Block.Move, Block.RotateRight, Block.IfSpaceIsTraversable, Block.WhileNotAtExit}
            );
            AddLevel(
                5, "Left and Right rotate", "XXXXE\nXXXOO\nXXOOX\nXOOXX\nXSXXX",
                new Block[]{Block.Move, Block.RotateRight, Block.RotateLeft}
            );
            AddLevel(
                6, "Complexity experiment", "OOOOE\nOOOOO\nOOOOO\nOOOOO\nSOOOO",
                new Block[]{Block.Move, Block.RotateRight, Block.RotateLeft}
            );
        }
    }

    /// <summary>
    /// Load the generic scene for all levels and set the appropriate id for convenient information pulling
    /// </summary>
    /// <param name="id">The ID of the scene that needs to be set</param>
    public static void Load(int id)
    {
        error = null;
        currentScene = id;
        SceneManager.LoadScene(SCENE_NAME);
    }

    public static void GoToMainMenu() {
        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }

    public static void GoToMainMenu(string errorMessage) {
        error = errorMessage;
        GoToMainMenu();
    }

    /// <summary>
    /// Returns a dictionary of information about the Level Manager's registered scenes.
    /// </summary>
    /// <returns>A dictionary containing the id and the name as a string</returns>
    public static Dictionary<int, string> GetLevels() => names;

    public static string GetName() => GetNameForID(currentScene);
    public static string GetMap() => GetMapForID(currentScene);
    public static Block[] GetBlocks() => GetBlocksForID(currentScene);

    /// <summary>
    /// Get the complexity of the currently selected level
    /// </summary>
    /// <returns>A float representation of the complexity, from 0 to 1</returns>
    public static float GetComplexity() => GetComplexityForID(currentScene);

    public static string GetNameForID(int id) => names[id];
    public static string GetMapForID(int id) => maps[id];
    public static Block[] GetBlocksForID(int id) => blocks[id];

    /// <summary>
    /// Given some Scene, use the scene's map to calculate the complexity
    /// </summary>
    /// <param name="id">The ID of the scene</param>
    /// <returns>A float representation of the complexity, from 0 to 1</returns>
    public static float GetComplexityForID(int id)
    {
        string map = GetMapForID(id);
        float complexity = 0.0f;

        complexity += map.Count(tile => tile == MapController.NORMAL_TILE) * 0.5f;
        complexity += map.Count(tile => tile == MapController.END_TILE);

        return complexity / (float) map.Count();
    }

    /// <summary>
    /// Clears the values in the LevelManager
    /// </summary>
    public static void Clear()
    {
        error = null;
        names = new Dictionary<int, string>();
        maps = new Dictionary<int, string>();
        blocks = new Dictionary<int, Block[]>();
    }

    /// <summary>
    /// Convenience function that aids in the adding of levels to the static variables.
    /// </summary>
    /// <param name="id">The level's ID as an integer</param>
    /// <param name="name">A string representation of the level name</param>
    /// <param name="map">The map as a square, \n seperated string</param>
    /// <param name="availableBlocks">The list of blocks that are usable on this level</param>
    private static void AddLevel(int id, string name, string map, Block[] availableBlocks)
    {
        names.Add(id, name);
        maps.Add(id, map);
        blocks.Add(id, availableBlocks);
    }
}