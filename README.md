# Programming Tool

> A playable version of the game (using WebGL) can be found on the [project's site](https://appropriately.github.io/programming-tool/).

## About

A tool built in Unity to teach people how to program. Focuses on the main programming concepts like __conditionals__ and __loops__ rather than teaching actual programming languages. Additionally, it teaches through the use of puzzles. New programming "nodes" are included in each level to slowly introduce the new concepts

> ![Early version of a complex level design](docs/images/complex_level.png)</br>
> Early version of a complex level, before any graphics were included

Some of the key folders in the project are:

- `docs/` contains project documentation as well as the GitHub pages
- `Assets/` is where all of the code for the project lies
  - `Assets/Tests` holds both Play and Edit mode test suites
  - `Assets/Scripts` contains all the scripts that were written by me, to interact with **GameObjects**.

## Setup

1. Install the Unity game engine
   - The project was built for Unity version `2019.3`, but later versions should also be supported.
2. Clone the project into Unity's project folder `git clone git@github.com:Appropriately/programming-tool.git`.
3. Run Unity and open the project.
   - Dependencies should be automatically pulled.

## Features

- A `Node` system
  - Built using object-orientated principles to reduce duplicated code.
  - Extensive use of C# documentation.
- Grammar based level generation
  - Only one scene needed for all levels.
  - Allows for easy modification and level testing.
- Custom localisation support
  - Uses Unity's built-in `SystemLanguage` support.
  - Reads `csv` files and caches `token, translation` pairs to speed up translation.
