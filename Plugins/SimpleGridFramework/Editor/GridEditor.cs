using UnityEngine;
using UnityEditor;

/**
 * Open source Grid Framework for creating grid-based levels 
 * easily in Unity3D game engine
 *
 * @author Suleyman Yasir KULA
 */

namespace SimpleGridFramework
{
	public class GridEditor : EditorWindow
	{
		private static GridEditor instance = null;
		private static Material lineMaterial;

		private GridEditorSettings settings;
		private GridState gridSettings;

		// Settings cached from gridSettings on every Update call
		private int gridlineCount;
		private float gridSize;
		private float gridYPos;
		private float gridShiftX;
		private float gridShiftZ;

		// Preview object that is used to show where
		// the instantiation will happen
		private Transform prefabPreview;

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
			instance = GetWindow<GridEditor>();
			instance.titleContent = new GUIContent( "Grid Editor" );
			instance.minSize = new Vector2( 210f, 70f );

			if( !lineMaterial )
			{
				lineMaterial = new Material( Shader.Find( "Hidden/Internal-Colored" ) );
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}
		
		// Script is initialized
		void OnEnable()
		{
			if( instance == null )
				Init();

			LoadSettings();

			// Register to certain events
			SceneView.onSceneGUIDelegate -= SceneUpdate;
			SceneView.onSceneGUIDelegate += SceneUpdate;

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
			// Destroy prefab preview object when the editor window is closed
			if( prefabPreview != null )
				DestroyImmediate( prefabPreview.gameObject );
			
			// Unregister from events
			SceneView.onSceneGUIDelegate -= SceneUpdate;
			Undo.undoRedoPerformed -= OnUndoRedo;
			Selection.selectionChanged -= OnSelectionChanged;
		}
		
		private void LoadSettings()
		{
			settings = AssetDatabase.LoadAssetAtPath<GridEditorSettings>( GridEditorSettings.SAVE_PATH );
			if( settings == null )
			{
				settings = ScriptableObject.CreateInstance<GridEditorSettings>();
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

			if( prefabPreview != null )
			{
				pos = prefabPreview.localPosition;
				DestroyImmediate( prefabPreview.gameObject );
			}

			if( settings.isEnabled )
			{
				GameObject obj = settings.Prefab;

				if( obj != null )
				{
					prefabPreview = ( (GameObject) PrefabUtility.InstantiatePrefab( obj ) ).transform;
					prefabPreview.localPosition = pos;

					// Preview object should not be visible in the Hierarchy
					prefabPreview.gameObject.hideFlags = HideFlags.HideAndDontSave;
					prefabPreview.gameObject.layer = 2; //IgnoreRaycast
					prefabPreview.eulerAngles = settings.PrefabState.rotation;
				}
			}

			SceneView.RepaintAll();
		}

		// Always keep the prefab preview up-to-date
		void Update()
		{
			gridSettings = settings.GridState;

			gridlineCount = gridSettings.gridlineCount;
			gridSize = gridSettings.gridSize;
			gridYPos = gridSettings.gridYPos;
			gridShiftX = gridSettings.gridShiftX;
			gridShiftZ = gridSettings.gridShiftZ;

			if( settings.Prefab != null && prefabPreview != null )
			{
				prefabPreview.eulerAngles = settings.PrefabState.rotation;
				SceneView.RepaintAll();
			}
		}

		// Draw the contents of the editor window
		void OnGUI()
		{
			scrollPos = EditorGUILayout.BeginScrollView( scrollPos );

			EditorGUI.BeginChangeCheck();
			instance.settings.Draw();
			if( EditorGUI.EndChangeCheck() )
			{
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndScrollView();
		}

		// Draw gridlines on Scene view
		// Code was inspired from: http://docs.unity3d.com/ScriptReference/GL.html
		void DrawGridlines( SceneView scenePanel )
		{
			lineMaterial.SetPass( 0 );
			
			Vector3 pos = scenePanel.camera.transform.position;
			pos.x -= pos.x % gridSize;
			pos.z -= pos.z % gridSize;

			float startX = pos.x - gridlineCount * gridSize * 0.5f + gridShiftX;
			float startZ = pos.z - gridlineCount * gridSize * 0.5f + gridShiftZ;
			float endX = pos.x + gridlineCount * gridSize * 0.5f + gridShiftX;
			float endZ = pos.z + gridlineCount * gridSize * 0.5f + gridShiftZ;

			GL.Begin( GL.LINES );
			GL.Color( gridSettings.gridlineColor );

			float x = startX;
			float y = gridYPos;
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

			GL.End();
		}

		// Called each time Scene View updates
		void SceneUpdate( SceneView scenePanel )
		{
			// Draw gridlines on screen
			if( settings.showGrids )
				DrawGridlines( scenePanel );

			// If the framework is enabled
			if( settings.isEnabled && settings.Prefab != null )
			{
				PrefabState prefabSettings = settings.PrefabState;
				Event e = Event.current;

				if( e.type == EventType.KeyDown )
				{
					if( !keyDown )
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
					Plane plane = new Plane( Vector3.up, new Vector3( 0, gridYPos, 0 ) );
					Camera sceneCam = scenePanel.camera;
					Vector2 mousePos = e.mousePosition;
					mousePos.y = sceneCam.pixelRect.height - mousePos.y;
					Ray ray = sceneCam.ScreenPointToRay( mousePos );
					float distance;

					// Calculate at where mouse collides with the gridlines
					if( plane.Raycast( ray, out distance ) )
					{
						Vector3 rayPos = ray.GetPoint( distance );

						// Snap the point if snapToGrid is enabled
						if( settings.snapToGrid )
						{
							rayPos.x -= gridShiftX;
							rayPos.z -= gridShiftZ;

							if( rayPos.x > 0 )
								rayPos.x = rayPos.x - rayPos.x % gridSize + gridSize * 0.5f;
							else
								rayPos.x = rayPos.x - rayPos.x % gridSize - gridSize * 0.5f;

							if( rayPos.z > 0 )
								rayPos.z = rayPos.z - rayPos.z % gridSize + gridSize * 0.5f;
							else
								rayPos.z = rayPos.z - rayPos.z % gridSize - gridSize * 0.5f;

							rayPos.x += gridShiftX;
							rayPos.z += gridShiftZ;
						}

						prefabPreview.position = rayPos + prefabSettings.shift;

						if( e.type == EventType.MouseDown )
						{
							if( e.button != 0 )
								rightMouseButtonDown = true;
							else if( !leftMouseButtonDown )
							{
								leftMouseButtonDown = true;

								// If it is a "click" for instantiating the prefab
								if( !e.alt && !rightMouseButtonDown )
								{
									GUIUtility.hotControl = GUIUtility.GetControlID( GetHashCode(), FocusType.Passive );
									mousePressValid = true;

									GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab( settings.Prefab );
									instance.transform.position = prefabPreview.position;
									instance.transform.eulerAngles = prefabSettings.rotation;
									Undo.RegisterCreatedObjectUndo( instance, "Create " + instance.name );
								}
								else
								{
									mousePressValid = false;
								}
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
								}
							}
						}
					}
				}
			}
		}
	}
}