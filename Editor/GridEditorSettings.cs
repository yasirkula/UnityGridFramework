using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimpleGridFramework
{
	#region Helper Classes
	[System.Serializable]
	public class PrefabStateHolder
	{
		public GameObject prefab;
		public List<PrefabState> states = new List<PrefabState>();

		public PrefabStateHolder( GameObject prefab )
		{
			this.prefab = prefab;
		}
	}

	[System.Serializable]
	public class PrefabState
	{
		public string name;
		
		public Vector3 rotation = Vector3.zero;
		public Vector3 shift = Vector3.zero;

		public PrefabState( string name )
		{
			this.name = name;
		}

		public PrefabState( string name, Vector3 rotation, Vector3 shift )
		{
			this.name = name;
			this.rotation = rotation;
			this.shift = shift;
		}
		
		public void Draw( GridEditorSettings settings, bool isDefaultState )
		{
			string tempStr;
			Vector3 tempV3;

			EditorGUI.BeginChangeCheck();
			tempStr = EditorGUILayout.TextField( "Name", name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab State Name" );
				if( !isDefaultState )
					name = tempStr;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( tempStr, rotation, shift ) );

				settings.UpdatePrefabStatesPopupList();
			}

			EditorGUI.BeginChangeCheck();
			tempV3 = EditorGUILayout.Vector3Field( "Rotation", rotation );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab Rotation" );
				if( !isDefaultState )
					rotation = tempV3;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( name, tempV3, shift ) );
			}

			EditorGUI.BeginChangeCheck();
			tempV3 = EditorGUILayout.Vector3Field( "Shift", shift );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab Shift" );
				if( !isDefaultState )
					shift = tempV3;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( name, rotation, tempV3 ) );
			}
		}
	}

	[System.Serializable]
	public class GridState
	{
		public string name;

		// Color of the grids drawn on the Scene view
		public Color gridlineColor = new Color( 0.8f, 0.8f, 0.8f, 1f );

		// Size of the grids in both X and Z directions
		// (a grid is a square)
		public float gridSize = 1f;

		// Number of grids drawn in X and Z directions
		// (gridLineCount^2 grids are drawn on XZ-plane)
		public int gridlineCount = 50;

		// Y-position of the grids
		public float gridYPos = 0f;

		// How many units to shift the grids in X
		// and Z directions
		public float gridShiftX = 0f;
		public float gridShiftZ = 0f;
		
		public GridState( string name )
		{
			this.name = name;
		}

		public void Draw( GridEditorSettings settings )
		{
			string tempStr;
			Color tempClr;
			float tempFloat;
			int tempInt;

			EditorGUI.BeginChangeCheck();
			tempStr = EditorGUILayout.TextField( "Name", name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change State Name" );
				name = tempStr;

				settings.UpdateStatesPopupList();
			}

			EditorGUI.BeginChangeCheck();
			tempClr = EditorGUILayout.ColorField( "Gridline Color", gridlineColor );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Gridline Color" );
				gridlineColor = tempClr;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( "Grid Size", gridSize );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Size" );
				gridSize = tempFloat;
			}

			EditorGUI.BeginChangeCheck();
			tempInt = EditorGUILayout.IntField( "Gridline Count", gridlineCount );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Gridline Count" );
				gridlineCount = tempInt;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( "Grid Y Pos", gridYPos );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Y Pos" );
				gridYPos = tempFloat;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( "Grid Shift X", gridShiftX );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift X" );
				gridShiftX = tempFloat;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( "Grid Shift Z", gridShiftZ );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift Z" );
				gridShiftZ = tempFloat;
			}
		}
	}
	#endregion

	public class GridEditorSettings : ScriptableObject
	{
		public const string SAVE_PATH = "Assets/Editor/GridEditorSettings.asset";

		// Is the framework enabled
		public bool isEnabled = true;

		// Should the grids be drawn
		public bool showGrids = true;

		// Is snap-to-grid enabled
		public bool snapToGrid = true;

		public List<GridState> states = new List<GridState>();
		public List<PrefabStateHolder> prefabStates = new List<PrefabStateHolder>();

		[HideInInspector]
		public int currentState;
		[HideInInspector]
		public int currentPrefabState;

		private string[] statesPopup;
		private string[] prefabStatesPopup;

		private GameObject selectedPrefab;
		private List<PrefabState> selectedPrefabStates;
		private PrefabState defaultPrefabState;

		public GridState GridState
		{
			get
			{
				return states[currentState];
			}
		}

		public GameObject Prefab
		{
			get
			{
				return selectedPrefab;
			}
		}

		public PrefabState PrefabState
		{
			get
			{
				if( selectedPrefabStates == null )
				{
					if( defaultPrefabState == null )
						defaultPrefabState = new PrefabState( "default" );

					return defaultPrefabState;
				}

				return selectedPrefabStates[currentPrefabState];
			}
		}

		void OnEnable()
		{
			if( states.Count == 0 )
				CreateState();
		}

		void Reset()
		{
			if( states.Count == 0 )
				CreateState();
		}

		internal void CreateState()
		{
			int index = 1;
			string name = "State " + index;
			for( int i = 0; i < states.Count; i++ )
			{
				if( states[i].name == name )
				{
					index++;
					name = "State " + index;
					i = 0;
				}
			}

			states.Add( new GridState( name ) );
		}

		internal void UpdateStatesPopupList()
		{
			statesPopup = new string[states.Count];
			for( int i = 0; i < states.Count; i++ )
			{
				statesPopup[i] = ( i + 1 ) + ". " + states[i].name;
			}
		}

		internal void UpdatePrefabStatesPopupList()
		{
			if( selectedPrefabStates == null )
				prefabStatesPopup = new string[] { "1. default" };
			else
			{
				prefabStatesPopup = new string[selectedPrefabStates.Count];
				for( int i = 0; i < selectedPrefabStates.Count; i++ )
				{
					prefabStatesPopup[i] = ( i + 1 ) + ". " + selectedPrefabStates[i].name;
				}
			}
		}

		public void OnSelectionChanged()
		{
			GameObject selectedObject = Selection.activeGameObject;
			if( selectedObject != null && PrefabUtility.GetPrefabObject( selectedObject ) != null )
			{
				selectedPrefab = (GameObject) PrefabUtility.GetPrefabParent( selectedObject );
				if( selectedPrefab == null )
					selectedPrefab = selectedObject;

				for( int i = 0; i < prefabStates.Count; i++ )
				{
					if( prefabStates[i].prefab == selectedPrefab )
					{
						selectedPrefabStates = prefabStates[i].states;

						UpdateStatesPopupList();
						UpdatePrefabStatesPopupList();

						return;
					}
				}

				selectedPrefabStates = null;
			}
			else
			{
				selectedPrefab = null;
				selectedPrefabStates = null;
			}

			UpdateStatesPopupList();
			UpdatePrefabStatesPopupList();
		}

		public void CreatePrefabStateForSelection( PrefabState state, bool forceNewState = false )
		{
			if( selectedPrefab != null )
			{
				for( int i = 0; i < prefabStates.Count; i++ )
				{
					if( prefabStates[i].prefab == selectedPrefab )
					{
						prefabStates[i].states.Add( state );
						return;
					}
				}

				PrefabStateHolder stateHolder = new PrefabStateHolder( selectedPrefab );
				if( forceNewState )
					stateHolder.states.Add( new PrefabState( "default" ) );
				stateHolder.states.Add( state );
				prefabStates.Add( stateHolder );

				selectedPrefabStates = stateHolder.states;
			}
		}

		public void RotateSelectedPrefab( float yDegrees )
		{
			if( selectedPrefab != null )
			{
				Undo.IncrementCurrentGroup();
				Undo.RecordObject( this, "Rotate Prefab" );
				if( selectedPrefabStates == null )
				{
					CreatePrefabStateForSelection( new PrefabState( "default" ) );
					currentPrefabState = 0;
				}

				selectedPrefabStates[currentPrefabState].rotation.y += yDegrees;
			}
		}

		public void PreviousPrefabState()
		{
			if( selectedPrefabStates == null || selectedPrefabStates.Count <= 1 )
				return;

			Undo.IncrementCurrentGroup();
			Undo.RecordObject( this, "Change Prefab State" );
			currentPrefabState--;
			if( currentPrefabState < 0 )
				currentPrefabState = selectedPrefabStates.Count - 1;
		}

		public void NextPrefabState()
		{
			if( selectedPrefabStates == null || selectedPrefabStates.Count <= 1 )
				return;

			Undo.IncrementCurrentGroup();
			Undo.RecordObject( this, "Change Prefab State" );
			currentPrefabState++;
			if( currentPrefabState >= selectedPrefabStates.Count )
				currentPrefabState = 0;
		}

		// Draw GUI controllers to be able to tweak settings via Unity editor
		// (this function is called by OnGUI function)
		public void Draw()
		{
			bool tempBool;
			int tempInt;

			EditorGUI.BeginChangeCheck();
			tempBool = EditorGUILayout.Toggle( "Show Grids", showGrids );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( this, "Toggle Show Grids" );
				showGrids = tempBool;
			}

			EditorGUI.BeginChangeCheck();
			tempBool = EditorGUILayout.Toggle( "Enabled", isEnabled );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( this, "Toggle Enabled" );
				isEnabled = tempBool;
			}

			if( !isEnabled )
				GUI.enabled = false;

			EditorGUI.BeginChangeCheck();
			tempBool = EditorGUILayout.Toggle( "Snap To Grid", snapToGrid );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( this, "Toggle Snap To Grid" );
				snapToGrid = tempBool;
			}

			GUI.enabled = true;

			// Draw horizontal line
			// Credit: http://answers.unity3d.com/questions/216584/horizontal-line.html
			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

			if( currentState >= states.Count )
				currentState = states.Count - 1;
			
			GUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			tempInt = EditorGUILayout.Popup( "State", currentState, statesPopup );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( this, "Change State" );
				currentState = tempInt;
			}

			Color c = GUI.color;
			GUI.color = Color.yellow;

			if( GUILayout.Button( "+", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
			{
				Undo.RecordObject( this, "Create State" );
				CreateState();

				currentState = states.Count - 1;

				UpdateStatesPopupList();
			}

			if( states.Count <= 1 )
				GUI.enabled = false;

			GUI.color = new Color( 1f, 0.5f, 0.5f );
			
			if( GUILayout.Button( "X", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
			{
				Undo.RecordObject( this, "Delete State" );
				states.RemoveAt( currentState );

				if( currentState >= states.Count )
					currentState = states.Count - 1;

				UpdateStatesPopupList();
			}

			GUI.color = c;
			GUI.enabled = true;

			GUILayout.EndHorizontal();

			states[currentState].Draw( this );

			// Draw horizontal line
			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

			if( selectedPrefab != null )
			{
				GUI.enabled = false;
				EditorGUILayout.ObjectField( "Prefab", selectedPrefab, typeof( GameObject ), false );
				GUI.enabled = true;

				GUILayout.BeginHorizontal();

				PrefabState state;
				bool isDefaultState;
				if( selectedPrefabStates == null || selectedPrefabStates.Count == 0 )
				{
					if( defaultPrefabState == null )
						defaultPrefabState = new PrefabState( "default" );

					EditorGUILayout.Popup( "Prefab State", 0, prefabStatesPopup );

					state = defaultPrefabState;
					isDefaultState = true;
				}
				else
				{
					if( currentPrefabState >= selectedPrefabStates.Count )
						currentPrefabState = selectedPrefabStates.Count - 1;
					
					EditorGUI.BeginChangeCheck();
					tempInt = EditorGUILayout.Popup( "Prefab State", currentPrefabState, prefabStatesPopup );
					if( EditorGUI.EndChangeCheck() )
					{
						Undo.RecordObject( this, "Change Prefab State" );
						currentPrefabState = tempInt;
					}

					state = selectedPrefabStates[currentPrefabState];
					isDefaultState = false;
				}

				GUI.color = Color.yellow;

				if( GUILayout.Button( "+", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
				{
					Undo.RecordObject( this, "Create Prefab State" );
					CreatePrefabStateForSelection( new PrefabState( "unnamed" ), true );

					currentPrefabState = selectedPrefabStates.Count - 1;

					UpdatePrefabStatesPopupList();
				}

				if( selectedPrefabStates == null || selectedPrefabStates.Count <= 1 )
					GUI.enabled = false;

				GUI.color = new Color( 1f, 0.5f, 0.5f );

				if( GUILayout.Button( "X", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
				{
					Undo.RecordObject( this, "Delete Prefab State" );
					selectedPrefabStates.RemoveAt( currentPrefabState );

					if( currentPrefabState >= selectedPrefabStates.Count )
						currentPrefabState = selectedPrefabStates.Count - 1;

					UpdatePrefabStatesPopupList();
				}

				GUI.color = c;
				GUI.enabled = true;

				GUILayout.EndHorizontal();

				state.Draw( this, isDefaultState );
			}
			else
			{
				EditorGUILayout.HelpBox( "No prefab is selected", MessageType.Info );
			}
		}
	}
}