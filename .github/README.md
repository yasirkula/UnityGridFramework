# Unity Simple Grid Framework

![screenshot](https://yasirkula.files.wordpress.com/2017/02/grideditor.png)

This plugin helps you place prefabs on a customizable grid easily.

## FEATURES

- Simple user-interface
- Easy-to-use
- Undo-redo support
- Ability to set different grid states and per-prefab prefab states
- Works in XZ, XY and YZ space

## INSTALLATION

There are 4 ways to install this plugin:

- import [GridFramework.unitypackage](https://github.com/yasirkula/UnityGridFramework/releases) via *Assets-Import Package*
- clone/[download](https://github.com/yasirkula/UnityGridFramework/archive/master.zip) this repository and move the *Plugins* folder to your Unity project's *Assets* folder
- *(via Package Manager)* add the following line to *Packages/manifest.json*:
  - `"com.yasirkula.gridframework": "https://github.com/yasirkula/UnityGridFramework.git",`
- *(via [OpenUPM](https://openupm.com))* after installing [openupm-cli](https://github.com/openupm/openupm-cli), run the following command:
  - `openupm add com.yasirkula.gridframework`

## HOW TO

- Open **Window/Grid Editor**
- Select a prefab from Project view (or a prefab instance from Hierarchy view)
- Hover mouse cursor over Scene view and left click to place the prefab
- Drag the mouse to place multiple instances at once

## HINTS

- Use *Q* and *E* keys to rotate the selected prefab around its *Y-axis* easily (Scene View should have the focus)
- Use *CTRL+Q* and *CTRL+E* keys to switch between prefab states easily (Scene View should have the focus)
- Settings for the framework will be stored at *Plugins/SimpleGridFramework/Settings.asset*
