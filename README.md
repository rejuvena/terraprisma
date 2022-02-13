# Terraprisma
_Terraprisma_ is a bootstrapper for [tModLoader](https://github.com/tModLoader/tModLoader) designed to allow mass, single-load patching. _Terraprisma_ is designed to run as its own program, which loads mods as necessary, then loads the _tModLoader_ assembly into memory in order to allow patches to be applied. After patching is finished, _tModLoader_ is launched as normal, preserving launch arguments, etc.

## Purpose
_tModLoader_ loads mods in a non-permanent state, making them unloadable and flushing away any changes. This means it also loads mods in-game after tModLoader itself starts up. This can pose as an issue for larger projects that need to transform the assembly in ways not possible at runtime. _Terraprisma_ is designed to solve this issue.

**_Terraprisma_ is <ins>not</ins> meant to be used to load content.** Only for applying patches. Any applied patches should be designed to maximize compatibilitly and stability. Please use defensive programming.

## Building
You will need the .NET 6 SDK in order to build this program.

To test this with a tModLoader build, please copy and rename `example.locations.targets` to `locations.targets` in `./src/`. After doing this, open it and replace `INSERT HERE` with the root directory of tModLoader.
