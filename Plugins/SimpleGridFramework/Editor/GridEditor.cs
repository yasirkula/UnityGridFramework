using UnityEngine;
using UnityEditor;

namespace SimpleGridFramework
{
	public class GridEditor : EditorWindow
	{
		public enum GridAlignment { XZ = 0, XY = 1, YZ = 2 };

		private const string PREF_ENABLED = "SGF_Enabled";
		private const string PREF_SHOW_GRIDS = "SGF_ShowGrids";
		private const string PREF_SNAP_TO_GRID = "SGF_SnapToGrid";

		private GridEditorSettings settings;
		private GridState gridSettings;

		// Is the framework enabled
		private bool isEnabled;

		// Should the grids be drawn
		private bool showGrids;

		// Is snap-to-grid enabled
		private bool snapToGrid;

		// Settings cached from gridSettings on every Update call
		private GridAlignment gridAlignment;
		private int gridlineCount;
		private float gridSize;
		private float gridPos;
		private float gridShiftAxis1;
		private float gridShiftAxis2;

		// Material used to draw the grid lines on Scene view
		private Material lineMaterial;

		// Preview object that is used to show where the instantiation will happen
		private Transform prefabPreview;

		private Vector3 areaSpawnStartPos;
		private int areaSpawnLength1;
		private int areaSpawnLength2;

		// Some variables necessary for the framework to work correctly
		private bool mousePressValid = false;
		private bool keyDown = false;
		private bool leftMouseButtonDown = false;
		private bool rightMouseButtonDown = false;

		private Vector2 scrollPos = Vector2.zero;

		// Called when the framework window is opened 
		[MenuItem( "Window/Grid Editor" )]
		public static void Init()
		{
			GridEditor window = GetWindow<GridEditor>();
			window.titleContent = new GUIContent( "Grid Editor" );
			window.minSize = new Vector2( 300f, 150f );
			window.Show();
		}

		// Script is initialized
		private void OnEnable()
		{
			LoadSettings();

			// Register to certain events
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= SceneUpdate;
			SceneView.duringSceneGui += SceneUpdate;
#else
			SceneView.onSceneGUIDelegate -= SceneUpdate;
			SceneView.onSceneGUIDelegate += SceneUpdate;
#endif

			Undo.undoRedoPerformed -= OnUndoRedo;
			Undo.undoRedoPerformed += OnUndoRedo;

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;

			OnSelectionChanged();

			// Repaint the editor window when the script is reloaded
			Repaint();
			Refresh();
		}

		// Window is destroyed
		void OnDisable()
		{
			Tools.hidden = false;

			// Destroy prefab preview object when the editor window is closed
			if( prefabPreview )
				DestroyImmediate( prefabPreview.gameObject );

			// Unregister from events
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= SceneUpdate;
#else
			SceneView.onSceneGUIDelegate -= SceneUpdate;
#endif

			Undo.undoRedoPerformed -= OnUndoRedo;
			Selection.selectionChanged -= OnSelectionChanged;
		}

		private void LoadSettings()
		{
			isEnabled = EditorPrefs.GetBool( PREF_ENABLED, true );
			showGrids = EditorPrefs.GetBool( PREF_SHOW_GRIDS, true );
			snapToGrid = EditorPrefs.GetBool( PREF_SNAP_TO_GRID, true );

			settings = AssetDatabase.LoadAssetAtPath<GridEditorSettings>( GridEditorSettings.SAVE_PATH );
			if( !settings )
			{
				System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName( GridEditorSettings.SAVE_PATH ) );

				settings = CreateInstance<GridEditorSettings>();
				AssetDatabase.CreateAsset( settings, GridEditorSettings.SAVE_PATH );
				AssetDatabase.SaveAssets();
			}
		}

		private void OnUndoRedo()
		{
			settings.OnSelectionChanged();

			Repaint();
			Refresh();
		}

		private void OnSelectionChanged()
		{
			settings.OnSelectionChanged();

			Repaint();
			Refresh();
		}

		// Refresh the prefab preview
		private void Refresh()
		{
			Vector3 pos = Vector3.zero;

			if( prefabPreview )
			{
				pos = prefabPreview.localPosition;
				DestroyImmediate( prefabPreview.gameObject );
			}

			if( isEnabled )
			{
				GameObject obj = settings.Prefab;

				if( obj != null )
				{
					prefabPreview = ( (GameObject) Instantiate( obj ) ).transform;
					prefabPreview.localPosition = pos;

					// Preview object should not be visible in the Hierarchy
					prefabPreview.gameObject.hideFlags = HideFlags.HideAndDontSave;
					prefabPreview.gameObject.layer = 2; // IgnoreRaycast
					prefabPreview.localEulerAngles = settings.PrefabState.rotation;
				}
			}

			SceneView.RepaintAll();
		}

		// Always keep the prefab preview up-to-date
		private void Update()
		{
			gridSettings = settings.GridState;

			gridAlignment = gridSettings.gridAlignment;
			gridlineCount = gridSettings.gridlineCount;
			gridSize = gridSettings.gridSize;
			gridPos = gridSettings.gridPos;
			gridShiftAxis1 = gridSettings.gridShiftAxis1;
			gridShiftAxis2 = gridSettings.gridShiftAxis2;

			if( settings.Prefab && prefabPreview )
			{
				prefabPreview.localEulerAngles = settings.PrefabState.rotation;
				SceneView.RepaintAll();
			}

			Tools.hidden = isEnabled;
		}

		// Draw the contents of the editor window
		private void OnGUI()
		{
			scrollPos = EditorGUILayout.BeginScrollView( scrollPos );

			// Draw base settings
			GUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			isEnabled = GUILayout.Toggle( isEnabled, "Enabled", GUI.skin.button );
			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool( PREF_ENABLED, isEnabled );
				Refresh();
			}

			EditorGUI.BeginChangeCheck();
			showGrids = GUILayout.Toggle( showGrids, "Show Grids", GUI.skin.button );
			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool( PREF_SHOW_GRIDS, showGrids );
				SceneView.RepaintAll();
			}

			GUI.enabled = isEnabled;
			EditorGUI.BeginChangeCheck();
			snapToGrid = GUILayout.Toggle( snapToGrid, "Snap To Grid", GUI.skin.button );
			if( EditorGUI.EndChangeCheck() )
				EditorPrefs.SetBool( PREF_SNAP_TO_GRID, snapToGrid );
			GUI.enabled = true;

			GUILayout.EndHorizontal();

			// Draw grid and prefab settings
			EditorGUI.BeginChangeCheck();
			settings.Draw();
			if( EditorGUI.EndChangeCheck() )
				SceneView.RepaintAll();

			EditorGUILayout.EndScrollView();
		}

		// Draw gridlines on Scene view
		// Code was inspired from: http://docs.unity3d.com/ScriptReference/GL.html
		private void DrawGridlines( SceneView scenePanel )
		{
			if( !lineMaterial )
			{
				lineMaterial = new Material( Shader.Find( "Hidden/Internal-Colored" ) )
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}

			lineMaterial.SetPass( 0 );

			GL.Begin( GL.LINES );
			GL.Color( gridSettings.gridlineColor );

			Vector3 pos = scenePanel.camera.transform.position;

			if( gridAlignment == GridAlignment.XZ )
			{
				pos.x -= pos.x % gridSize;
				pos.z -= pos.z % gridSize;

				float startX = pos.x - gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float startZ = pos.z - gridlineCount * gridSize * 0.5f + gridShiftAxis2;
				float endX = pos.x + gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float endZ = pos.z + gridlineCount * gridSize * 0.5f + gridShiftAxis2;

				float x = startX;
				float y = gridPos;
				float z = startZ;

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( startX, y, z );
					GL.Vertex3( endX, y, z );

					z += gridSize;
				}

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( x, y, startZ );
					GL.Vertex3( x, y, endZ );

					x += gridSize;
				}
			}
			else if( gridAlignment == GridAlignment.XY )
			{
				pos.x -= pos.x % gridSize;
				pos.y -= pos.y % gridSize;

				float startX = pos.x - gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float startY = pos.y - gridlineCount * gridSize * 0.5f + gridShiftAxis2;
				float endX = pos.x + gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float endY = pos.y + gridlineCount * gridSize * 0.5f + gridShiftAxis2;

				float x = startX;
				float y = startY;
				float z = gridPos;

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( startX, y, z );
					GL.Vertex3( endX, y, z );

					y += gridSize;
				}

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( x, startY, z );
					GL.Vertex3( x, endY, z );

					x += gridSize;
				}
			}
			else
			{
				pos.y -= pos.y % gridSize;
				pos.z -= pos.z % gridSize;

				float startY = pos.y - gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float startZ = pos.z - gridlineCount * gridSize * 0.5f + gridShiftAxis2;
				float endY = pos.y + gridlineCount * gridSize * 0.5f + gridShiftAxis1;
				float endZ = pos.z + gridlineCount * gridSize * 0.5f + gridShiftAxis2;

				float x = gridPos;
				float y = startY;
				float z = startZ;

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( x, startY, z );
					GL.Vertex3( x, endY, z );

					z += gridSize;
				}

				for( int i = 0; i <= gridlineCount; i++ )
				{
					GL.Vertex3( x, y, startZ );
					GL.Vertex3( x, y, endZ );

					y += gridSize;
				}
			}

			GL.End();
		}

		// Called each time Scene View updates
		private void SceneUpdate( SceneView scenePanel )
		{
			// Draw gridlines on screen
			if( showGrids )
				DrawGridlines( scenePanel );

			// If the framework is enabled
			if( !isEnabled || !settings.Prefab )
				return;

			PrefabState prefabSettings = settings.PrefabState;
			Event e = Event.current;

			if( e.type == EventType.KeyDown )
			{
				if( !rightMouseButtonDown && !keyDown )
				{
					if( e.keyCode == KeyCode.Q )
					{
						if( !e.control )
							settings.RotateSelectedPrefab( -45f );
						else
							settings.PreviousPrefabState();

						Repaint();

						keyDown = true;
					}
					else if( e.keyCode == KeyCode.E )
					{
						if( !e.control )
							settings.RotateSelectedPrefab( 45f );
						else
							settings.NextPrefabState();

						Repaint();

						keyDown = true;
					}
				}
			}
			else if( e.type == EventType.KeyUp )
			{
				if( e.keyCode == KeyCode.Q || e.keyCode == KeyCode.E )
					keyDown = false;
			}

			if( e.isMouse )
			{
				// Find the position to show the preview object at
				Ray ray = HandleUtility.GUIPointToWorldRay( e.mousePosition );
				float distance;

				Plane plane;
				if( gridAlignment == GridAlignment.XZ )
					plane = new Plane( Vector3.up, new Vector3( 0, gridPos, 0 ) );
				else if( gridAlignment == GridAlignment.XY )
					plane = new Plane( Vector3.forward, new Vector3( 0, 0, gridPos ) );
				else
					plane = new Plane( Vector3.right, new Vector3( gridPos, 0, 0 ) );

				// Calculate cursor's collision point with the gridlines
				if( !plane.Raycast( ray, out distance ) )
					return;

				Vector3 rayPos = ray.GetPoint( distance );

				// Snap the point if snapToGrid is enabled
				if( snapToGrid )
				{
					if( gridAlignment == GridAlignment.XZ )
					{
						rayPos.x -= gridShiftAxis1;
						rayPos.z -= gridShiftAxis2;

						if( rayPos.x > 0 )
							rayPos.x = rayPos.x - rayPos.x % gridSize + gridSize * 0.5f;
						else
							rayPos.x = rayPos.x - rayPos.x % gridSize - gridSize * 0.5f;

						if( rayPos.z > 0 )
							rayPos.z = rayPos.z - rayPos.z % gridSize + gridSize * 0.5f;
						else
							rayPos.z = rayPos.z - rayPos.z % gridSize - gridSize * 0.5f;

						rayPos.x += gridShiftAxis1;
						rayPos.z += gridShiftAxis2;
					}
					else if( gridAlignment == GridAlignment.XY )
					{
						rayPos.x -= gridShiftAxis1;
						rayPos.y -= gridShiftAxis2;

						if( rayPos.x > 0 )
							rayPos.x = rayPos.x - rayPos.x % gridSize + gridSize * 0.5f;
						else
							rayPos.x = rayPos.x - rayPos.x % gridSize - gridSize * 0.5f;

						if( rayPos.y > 0 )
							rayPos.y = rayPos.y - rayPos.y % gridSize + gridSize * 0.5f;
						else
							rayPos.y = rayPos.y - rayPos.y % gridSize - gridSize * 0.5f;

						rayPos.x += gridShiftAxis1;
						rayPos.y += gridShiftAxis2;
					}
					else
					{
						rayPos.y -= gridShiftAxis1;
						rayPos.z -= gridShiftAxis2;

						if( rayPos.y > 0 )
							rayPos.y = rayPos.y - rayPos.y % gridSize + gridSize * 0.5f;
						else
							rayPos.y = rayPos.y - rayPos.y % gridSize - gridSize * 0.5f;

						if( rayPos.z > 0 )
							rayPos.z = rayPos.z - rayPos.z % gridSize + gridSize * 0.5f;
						else
							rayPos.z = rayPos.z - rayPos.z % gridSize - gridSize * 0.5f;

						rayPos.y += gridShiftAxis1;
						rayPos.z += gridShiftAxis2;
					}
				}

				prefabPreview.localPosition = rayPos + prefabSettings.shift;

				if( e.type == EventType.MouseDown )
				{
					if( e.button != 0 )
						rightMouseButtonDown = true;
					else if( !leftMouseButtonDown )
					{
						leftMouseButtonDown = true;

						if( e.alt || rightMouseButtonDown )
							mousePressValid = false;
						else
						{
							GUIUtility.hotControl = GUIUtility.GetControlID( GetHashCode(), FocusType.Passive );
							mousePressValid = true;

							areaSpawnStartPos = prefabPreview.localPosition;
							areaSpawnLength1 = 0;
							areaSpawnLength2 = 0;
						}
					}
				}
				else if( e.type == EventType.MouseDrag )
				{
					Vector3 deltaPos = prefabPreview.localPosition - areaSpawnStartPos;

					if( gridAlignment == GridAlignment.XZ )
					{
						areaSpawnLength1 = Mathf.RoundToInt( deltaPos.x / gridSize );
						areaSpawnLength2 = Mathf.RoundToInt( deltaPos.z / gridSize );
					}
					else if( gridAlignment == GridAlignment.XY )
					{
						areaSpawnLength1 = Mathf.RoundToInt( deltaPos.x / gridSize );
						areaSpawnLength2 = Mathf.RoundToInt( deltaPos.y / gridSize );
					}
					else
					{
						areaSpawnLength1 = Mathf.RoundToInt( deltaPos.y / gridSize );
						areaSpawnLength2 = Mathf.RoundToInt( deltaPos.z / gridSize );
					}
				}
				else if( e.type == EventType.MouseUp )
				{
					if( e.button != 0 )
						rightMouseButtonDown = false;
					else
					{
						leftMouseButtonDown = false;

						if( mousePressValid && !rightMouseButtonDown )
						{
							mousePressValid = false;
							GUIUtility.hotControl = 0;

							Vector3 spawnPosChange1, spawnPosChange2;
							if( gridAlignment == GridAlignment.XZ )
							{
								spawnPosChange1 = new Vector3( gridSize, 0f, 0f );
								spawnPosChange2 = new Vector3( 0f, 0f, gridSize );
							}
							else if( gridAlignment == GridAlignment.XY )
							{
								spawnPosChange1 = new Vector3( gridSize, 0f, 0f );
								spawnPosChange2 = new Vector3( 0f, gridSize, 0f );
							}
							else
							{
								spawnPosChange1 = new Vector3( 0f, gridSize, 0f );
								spawnPosChange2 = new Vector3( 0f, 0f, gridSize );
							}

							if( areaSpawnLength1 < 0 )
							{
								spawnPosChange1 = -spawnPosChange1;
								areaSpawnLength1 = -areaSpawnLength1;
							}
							if( areaSpawnLength2 < 0 )
							{
								spawnPosChange2 = -spawnPosChange2;
								areaSpawnLength2 = -areaSpawnLength2;
							}

							for( int i = 0; i <= areaSpawnLength1; i++ )
							{
								for( int j = 0; j <= areaSpawnLength2; j++ )
								{
									GameObject instance;
									if( settings.Prefab.transform.root == settings.Prefab.transform )
										instance = (GameObject) PrefabUtility.InstantiatePrefab( settings.Prefab );
									else
										instance = (GameObject) Instantiate( settings.Prefab );

									instance.transform.position = areaSpawnStartPos + spawnPosChange1 * i + spawnPosChange2 * j;
									instance.transform.eulerAngles = prefabSettings.rotation;
									Undo.RegisterCreatedObjectUndo( instance, "Create Object" );
								}
							}
						}
					}
				}
			}


			// Draw area spawn preview
			if( mousePressValid && leftMouseButtonDown && !rightMouseButtonDown )
			{
				Color c = Handles.color;
				Handles.color = Color.green;

				if( gridAlignment == GridAlignment.XZ )
					Handles.DrawWireCube( areaSpawnStartPos + new Vector3( areaSpawnLength1, 0, areaSpawnLength2 ) * gridSize * 0.5f, new Vector3( Mathf.Abs( areaSpawnLength1 ) + 1, 0.1f, Mathf.Abs( areaSpawnLength2 ) + 1 ) * gridSize );
				else if( gridAlignment == GridAlignment.XY )
					Handles.DrawWireCube( areaSpawnStartPos + new Vector3( areaSpawnLength1, areaSpawnLength2, 0 ) * gridSize * 0.5f, new Vector3( Mathf.Abs( areaSpawnLength1 ) + 1, Mathf.Abs( areaSpawnLength2 ) + 1, 0.1f ) * gridSize );
				else
					Handles.DrawWireCube( areaSpawnStartPos + new Vector3( 0, areaSpawnLength1, areaSpawnLength2 ) * gridSize * 0.5f, new Vector3( 0.1f, Mathf.Abs( areaSpawnLength1 ) + 1, Mathf.Abs( areaSpawnLength2 ) + 1 ) * gridSize );

				Handles.color = c;
			}
		}
	}
}