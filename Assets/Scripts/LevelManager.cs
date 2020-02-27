using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Abstracts scene navigation while providing support for the grammar-based map generation.
/// </summary>
public static class LevelManager
{
    private const string SCENE_NAME = "TestGame";
    private const string MAIN_MENU_SCENE_NAME = "TestMenu";

    public static string error = null;

    private static int currentScene = -1;
    private static Dictionary<int, string> names = new Dictionary<int, string>();
    private static Dictionary<int, string> maps = new Dictionary<int, string>();
    private static Dictionary<int, Block[]> blocks = new Dictionary<int, Block[]>();

    /// <summary>
    /// Insert some default values into the data dictionaries.
    /// </summary>
    public static void Seed()
    {
        #if UNITY_EDITOR
            PlayerPrefs.DeleteAll();
        #endif

        if (names.Count <= 0)
        {
            Add ("basic_movement", "XEX\nXOX\nXSX", new Block[]{Block.Move, Block.Speak});
            Add ("rotate_right", "XXXXX\nXXOEX\nXXOXX\nXXSXX\nXXXXX", new Block[]{Block.Move, Block.RotateRight});
            Add (
                "conditional", "OOOOO\nOXXXO\nOXXXO\nOXXXO\nSXXXE",
                new Block[]{Block.Move, Block.RotateRight, Block.IfSpaceIsTraversable, Block.WhileNotAtExit}
            );
            Add (
                "interact", "1XX\nOAE\nSXX", new Block[] {
                    Block.Move, Block.RotateRight, Block.Interact, Block.WhileTraversable, Block.WhileNotAtExit
                }
            );
            Add (
                "advanced_interact", "OOOOO\nAXXXO\nAXEBO\nO1XX2\nSXXXX", new Block[] {
                    Block.Move, Block.RotateRight, Block.RotateLeft, Block.Interact, Block.IfSpaceIsActivatable,
                    Block.WhileNotAtExit, Block.WhileTraversable, Block.Speak
                }
            );
        }
    }

    /// <summary>
    /// Load the generic scene for all levels and set the appropriate global id for convenient information pulling.
    /// </summary>
    /// <param name="id">The ID of the scene that needs to be loaded.</param>
    public static void Load(int id)
    {
        error = null;
        currentScene = id;
        SceneManager.LoadScene(SCENE_NAME);
    }

    /// <summary>
    /// Returns to the Main Menu scene.
    /// </summary>
    /// <param name="errorMessage">An optional string that sets the level manager's error flag.</param>
    public static void GoToMainMenu(string errorMessage = null) {
        error = errorMessage;
        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
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
    /// Given an <c>id</c> representation some scene, calculate the complexity.
    /// Complexity depends on a number of factors such as how many buttons are used and the size of the level.
    /// </summary>
    /// <param name="id">The <c>id</c> of the scene</param>
    /// <returns>A <c>float</c> representation of the complexity, from 0 to 1</returns>
    public static float GetComplexityForID(int id)
    {
        string map = GetMapForID(id);
        float complexity = 0.0f;

        complexity += map.Count(tile => tile == MapController.START_TILE);
        complexity += map.Count(tile => tile == MapController.NORMAL_TILE) * 0.5f;
        complexity += map.Count(tile => tile == MapController.END_TILE);

        foreach (char character in "12".ToCharArray())
            complexity += map.Count(tile => tile == character) * 5.0f;

        complexity += map.Count(tile => tile == 'X') * 0.25f;

        return complexity / (float) map.Count();
    }

    /// <summary>
    /// Given the <c>score</c> as an integer, determine if it is higher and store it.
    /// </summary>
    /// <param name="score">The score that should be checked and store</param>
    public static void SetScore(int score) => SetScoreForLevel(currentScene, score);

    /// <summary>
    /// Given the <c>score</c> as an integer, determine if it is higher and store it.
    /// Performs the action for a given level.
    /// </summary>
    /// <param name="id">The level's <c>id</c></param>
    /// <param name="score">The score that should be checked and store</param>
    public static void SetScoreForLevel(int id, int score)
    {
        int existingScore = PlayerPrefs.GetInt(names[id], 0);
        if (existingScore is 0 || score < existingScore) PlayerPrefs.SetInt(names[id], score);
    }

    /// <summary>
    /// Adds the defined map to the next free space in the level dictionaries.
    /// </summary>
    /// <param name="name">A <c>string</c> representation of the level name</param>
    /// <param name="map">The map as a 'square', <c>"\n"</c> seperated string</param>
    /// <param name="availableBlocks">An array of <c>Block</c>s that are usable on this level</param>
    public static int Add(string name, string map, Block[] availableBlocks)
    {
        int id = NextValidID();
        names.Add(id, name);
        maps.Add(id, map);
        blocks.Add(id, availableBlocks);
        return id;
    }

    /// <summary>
    /// Clears the values in the LevelManager
    /// </summary>
    public static void Clear()
    {
        error = null;
        currentScene = -1;
        maps = new Dictionary<int, string>();
        names = new Dictionary<int, string>();
        blocks = new Dictionary<int, Block[]>();
    }

    /// <summary>
    /// Loops through the <c>names</c>, <c>maps</c> and <c>blocks</c> arrays until a free key is found.
    /// </summary>
    /// <returns>An ID value that is not in use</returns>
    private static int NextValidID()
    {
        int id = names.Keys.Count > 0 ? names.Keys.Last() : 1;
        while(names.ContainsKey(id) && maps.ContainsKey(id) && blocks.ContainsKey(id)) id++;
        return id;
    }
}