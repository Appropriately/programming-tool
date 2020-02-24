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
    /// Given a string map, validates the structure and generates the appropriate map array.
    /// </summary>
    /// <param name="stringMap">The string representation of a map, nxn in size seperated by \n</param>
    /// <returns>Whether the creation was successful or not</returns>
    public bool Create(string stringMap) {
        if (InvalidMap(stringMap)) return false;
        map = StringToMapArray(stringMap);
        return true;
    }

    /// <summary>
    /// Render the map at the given point.
    /// </summary>
    /// <param name="position">The position at which the map will be centred on</param>
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
    /// Resets the player's position by placing them at the start coordinates of this map.
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
    /// <returns>The appropriate float mutliplier for scaling</returns>
    public static float Scale() {
        // TODO: Improve the logic here for better scaling
        return DEFAULT_SCALE;
    }

    /// <summary>
    /// Returns whether the given position is traversable. A tile can be traversable if its not empty or an appropriate
    /// button has been pressed.
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>Whether the tile at the position is traversable or not</returns>
    public bool IsTraversable(int x, int y) => !OutOfBounds(x, y) && IsPositionTraversable(x, y);

    /// <summary>
    /// Returns whether the given position is an interactable button or not.
    /// </summary>
    /// <param name="x">The x-coordinate to check</param>
    /// <param name="y">The y-coordinate to check</param>
    /// <returns>Whether the tile at the position is a button or not</returns>
    public bool IsButton(int x, int y) => !OutOfBounds(x, y) && Array.Exists(buttonChars, e => e == map[x, y]);

    /// <summary>
    /// Given coordinates, returns whether the position is traversable.
    /// </summary>
    /// <param name="x">X co-ordinate</param>
    /// <param name="y">Y co-ordinate</param>
    /// <returns>A boolean representing whether it is traversable</returns>
    public bool IsPositionTraversable(int x, int y)
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
    /// <returns>Whether coordinates are out of bounds or not.</returns>
    private bool OutOfBounds(int x, int y)
    {
        int length = map.GetLength(0);
        return x < 0 || x >= length || y < 0 || y >= length;
    }

    private char[,] StringToMapArray(string stringMap) {
        int length = stringMap.Split('\n').First().Length;
        char[,] array = new char[length, length];

        int x = 0, y = length - 1;
        foreach (char character in stringMap) {
            if (character == 'S') {
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

    private bool InvalidMap(string stringMap) {
        if (stringMap.ToCharArray().Count(c => c == START_TILE) != 1) return true;
        if (stringMap.ToCharArray().Count(c => c == END_TILE) < 1) return true;

        string[] stringMapArray = stringMap.Split('\n');
        if (stringMapArray.Any(x => x.Length != stringMapArray.First().Length)) return true;
        return false;
    }

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
