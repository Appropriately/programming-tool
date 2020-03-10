# Programming Tool

## About

A tool built in Unity to teach people how to program. Focuses on the main programming concepts like __conditionals__ and __loops__ rather than teaching actual programming languages. Additionally, it teaches through the use of puzzles. New programming "nodes" are included in each level to slowly introduce the new concepts

> ![Early version of a complex level design](docs/images/complex_level.png)</br>
> Early version of a complex level, before any graphics were included

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
