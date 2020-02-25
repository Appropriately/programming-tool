using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles map validation, creation and rendering. Contains a collection of helper functions to support map navigation
/// and location validating.
/// </summary>
public class MapController : MonoBehaviour
{
    public const char START_TILE = 'S';
    public const char END_TILE = 'E';
    public const char NORMAL_TILE = 'O';

    private const float DEFAULT_SCALE = 1.0f;
    private static readonly char[] activatableChars = { 'A', 'B' };
    private static readonly char[] buttonChars = { '1', '2' };

    public char[,] map;
    public List<char> activated = new List<char>();
    public List<GameObject> activatable;

    private int startCoordinateX, startCoordinateY;

    /// <summary>
    /// Given a <c>string</c> representation of a map, validate the structure and generate the appropriate map array.
    /// </summary>
    /// <param name="stringMap">The <c>string</c> representation of a map, nxn in size seperated by <c>"\n"</c></param>
    /// <returns>A <c>bool</c> representing whether the creation was successful or not</returns>
    public bool Create(string stringMap) {
        if (InvalidMap(stringMap)) return false;
        map = StringToMapArray(stringMap);
        return true;
    }

    /// <summary>
    /// Render the map at the given location.
    /// </summary>
    /// <param name="position">A <c>Vector3</c> representing the position at which the map will be centred on</param>
    public void Render(Vector3 position) {
        int length = map.GetLength(0);
        float offset = (length * Scale() * 0.5f) + (Scale() * 0.5f);

        position -= new Vector3(offset, offset);

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < length; y++) {
                Vector3 newPosition = position + new Vector3((x + 1) * Scale(), (y + 1) * Scale(), 0);
                CharacterToRenderedTile(map[x,y], newPosition);
            }
        }
    }

    /// <summary>
    /// Resets map related values, such as the player's position and what buttons/tiles have been activated.
    /// </summary>
    public void Reset() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            PlayerController component = player.GetComponent<PlayerController>();
            component.coordinateX = startCoordinateX;
            component.coordinateY = startCoordinateY;
            component.Reset();
        }

        foreach (GameObject obj in activatable) obj.SetActive(false);
        activated.Clear();
    }

    /// <summary>
    /// The scale of items in the scene.
    /// </summary>
    /// <returns>The appropriate <c>float</c> mutliplier for scaling</returns>
    public static float Scale() {
        // TODO: Improve the logic here for better scaling
        return DEFAULT_SCALE;
    }

    /// <summary>
    /// Returns whether the given position is traversable.
    /// A tile can be traversable if its not empty or an appropriate button has been pressed.
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>A <c>bool</c> representation of whether the tile at the position is traversable or not</returns>
    public bool IsTraversable(int x, int y) => !OutOfBounds(x, y) && IsTileTraversable(x, y);

    /// <summary>
    /// Returns whether the given position is an interactable button or not.
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>A <c>bool</c> representation of whether the tile at the position is a button or not</returns>
    public bool IsButton(int x, int y) => !OutOfBounds(x, y) && Array.Exists(buttonChars, e => e == map[x, y]);

    /// <summary>
    /// Given coordinates, returns whether the position is traversable.
    /// Note that it does not handle issues due the coordinates being out of bounds.
    /// <seealso cref="MapController.IsTraversable(int, int)"/>
    /// </summary>
    /// <param name="x">X co-ordinate</param>
    /// <param name="y">Y co-ordinate</param>
    /// <returns>A <c>bool</c> representing whether the tile at the position is traversable</returns>
    public bool IsTileTraversable(int x, int y)
    {
        switch (map[x, y])
        {
            case NORMAL_TILE:
            case START_TILE:
            case END_TILE:
                return true;
            default:
                return activated.Contains(map[x,y]);
        }
    }

    /// <summary>
    /// Returns whether the given coordinates are out of bounds.
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>A <c>bool</c> representing whether the coordinates are out of bounds</returns>
    private bool OutOfBounds(int x, int y)
    {
        int length = map.GetLength(0);
        return x < 0 || x >= length || y < 0 || y >= length;
    }


    /// <summary>
    /// Converts a given <c>string</c> map into a char array that can be interpreted by the map controller.
    /// Map errors aren't checked, so it is recommended you use <see cref="MapController.InvalidMap(string)"/> first.
    /// </summary>
    /// <param name="stringMap">A <c>string</c> representation </param>
    /// <returns>A 2D array of <c>char</c></returns>
    private char[,] StringToMapArray(string stringMap) {
        int length = stringMap.Split('\n').First().Length;
        char[,] array = new char[length, length];

        int x = 0, y = length - 1;
        foreach (char character in stringMap) {
            if (character == START_TILE) {
                startCoordinateX = x;
                startCoordinateY = y;
            }
            if (character == '\n') {
                y--;
                x = 0;
            } else {
                array[x, y] = character;
                x++;
            }
        }

        return array;
    }

    /// <summary>
    /// Checks the given map and determines whether it is to the application's specification.
    /// </summary>
    /// <param name="stringMap">A <c>string</c> representation </param>
    /// <returns>A <c>bool</c> representation of whether the map is valid</returns>
    private bool InvalidMap(string stringMap) {
        if (stringMap.ToCharArray().Count(c => c == START_TILE) != 1) return true;
        if (stringMap.ToCharArray().Count(c => c == END_TILE) < 1) return true;

        string[] stringMapArray = stringMap.Split('\n');
        if (stringMapArray.Any(x => x.Length != stringMapArray.First().Length)) return true;
        return false;
    }

    /// <summary>
    /// Given an appropriate <c>char</c>, instantiate a new tile <c>GameObject</c>.
    /// The colour, scripts and other characteristics are determined by the given <c>char</c>.
    /// Unexpected <c>char</c>s will be rendered as nothing.
    /// </summary>
    /// <param name="character">The <c>char</c> representation of the tile</param>
    /// <param name="position">The <c>Vector3</c> location of the newly instantiated tile</param>
    private void CharacterToRenderedTile(char character, Vector3 position) {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = $"Tile '{character.ToString()}'";
        tile.transform.position = position;

        Vector3 scale = tile.transform.localScale;
        tile.transform.localScale = new Vector3(scale.x * Scale(), scale.y * Scale(), 0.2f);
        switch (character)
        {
            case START_TILE:
                tile.gameObject.tag = "Respawn";
                tile.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case NORMAL_TILE:
                tile.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case END_TILE:
                tile.gameObject.tag = "Finish";
                tile.GetComponent<Renderer>().material.color = Color.red;
                break;
            default:
                if (Array.Exists(activatableChars, e => e == character)) {
                    tile.GetComponent<Renderer>().material.color = Color.yellow;

                    Activatable component = tile.AddComponent<Activatable>();
                    component.type = character;

                    activatable.Add(tile);
                    tile.SetActive(false);
                } else if (Array.Exists(buttonChars, e => e == character)) {
                    tile.GetComponent<Renderer>().material.color = Color.cyan;
                } else {
                    Destroy(tile);
                }
                break;
        }
    }
}
