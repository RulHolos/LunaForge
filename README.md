<p align="center" width="50%">
    <img width="20%" src="https://raw.githubusercontent.com/RulHolos/LunaForge/main/LunaForge/Images/Icon.png">
</p><h1 align="center">LunaForge</h1>
<h4 align="center">

([Currently in Alpha](https://github.com/RulHolos/LunaForge/releases))

</h4>

<div align="center">
    
[![Build Docs](https://github.com/RulHolos/LunaForge/actions/workflows/docfx-build-publish.yml/badge.svg)](https://github.com/AtaeKurri/LunaForge/actions/workflows/docfx-build-publish.yml)
[![Build Editor](https://github.com/RulHolos/LunaForge/actions/workflows/build.yml/badge.svg)](https://github.com/RulHolos/LunaForge/actions/workflows/build.yml)

</div>

**LunaForge** is a code editor and generator targeting the LuaSTG engine (all main branches) based on THlib.

## Usage

Placeholder

## Differences with SharpX

The main "why" point with LunaForge is "[SharpX](https://github.com/Sharp-X-Team/LuaSTG-Editor-Sharp-X) already exists, so why?"
- LunaForge uses a folder-based approach for projects instead of a single-file based approach.
- LunaForge uses ImGui instead of WPF.
- LunaForge allows you to set an entry point for your project, making it easier to structure your project.
- Projects are sharable by defaullt since all files are relative to the root of the project.
- LunaForge is meant to be cross-platform for Windows, Mac and Linux. (not the case yet)

## Compilation

To compile your project to a LuaSTG-ready game, LunaForge goes through all your files in the project folder.
It first tries to compile all .lfd (definition files) to .lua scripts.
Then, after this has been done, the compiler checks for every file's checksums.
If the file has changed since the last time it was packed into the game, it will be repacked.

After all this has been done, the editor tries to start LuaSTG.

**Please be aware that (almost) EVERY file inside your project folder will be packed. This can result in a large game size.**
Note: Only the .lunalorge invisible folder (and its contents) and the Project.lfp file won't get included in the packing process.

## For Linux

Linux version is **NOT** stable.

Please install [xsel](https://github.com/kfish/xsel) and make it callable.

## For MacOS

MacOS version is **NOT** stable.

No testing has been made yet.

## Plugins

LunaForge supports user-made plugins.
Plugins allow for modifying/adding features to the editor. Their most important use is for adding nodes to the editor.

Plugins comes in two types: `Editor Plugins` (which are loaded globally) and `Project Plugins` (which are loaded on a per-project basis).

See the [plugin documentation](https://rulholos.github.io/LunaForge/plugins.html) to see how to create your editor plugins.

## Credits

All files from the **LunaForge.GUI.ImGuiFileDialog** namespace are taken and modified from [Dalamud](https://github.com/goatcorp/Dalamud).