# Unity Simple Grid Framework
![screenshot](https://yasirkula.files.wordpress.com/2017/02/grideditor.png)

This plugin helps you place prefabs on a customizable grid easily.

## Features
- Simple user-interface
- Easy-to-use
- Undo-redo support
- Ability to set different grid states and per-prefab prefab states
- Works in XZ, XY and YZ space

## Upgrading From Previous Versions
Delete *Editor/GridEditor.cs* and *Editor/GridEditorSettings.cs* before upgrading the plugin (do not delete *Editor/GridEditorSettings.asset*).

## How To
- Import *GridFramework.unitypackage* to your project
- Open **Window/Grid Editor**
- Select a prefab from Project view (or a prefab instance from Hierarchy view)
- Hover mouse cursor over Scene view and left click to place the prefab

## Hints
- Use *Q* and *E* keys to rotate the selected prefab around its *Y-axis* easily (Scene View should have the focus)
- Use *CTRL+Q* and *CTRL+E* keys to switch between prefab states easily (Scene View should have the focus)
- Settings for the framework will be stored at *Plugins/SimpleGridFramework/Settings.asset*
