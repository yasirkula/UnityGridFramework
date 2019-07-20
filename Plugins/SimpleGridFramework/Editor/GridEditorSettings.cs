using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Serialization;

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

		// Size of the grids (a grid is a square)
		public float gridSize = 1f;

		// Number of grids
		public int gridlineCount = 50;

		// Alignment plane of the grids
		public GridEditor.GridAlignment gridAlignment = GridEditor.GridAlignment.XZ;

		// Position of the grids
		[FormerlySerializedAs( "gridYPos" )]
		public float gridPos = 0f;

		// How many units to shift the grids
		[FormerlySerializedAs( "gridShiftX" )]
		public float gridShiftAxis1 = 0f;
		[FormerlySerializedAs( "gridShiftZ" )]
		public float gridShiftAxis2 = 0f;

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
			GridEditor.GridAlignment tempEnum;

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
			tempEnum = (GridEditor.GridAlignment) EditorGUILayout.EnumPopup( "Grid Alignment", gridAlignment );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Alignment" );
				gridAlignment = tempEnum;
			}

			string posLabel, shiftXLabel, shiftZLabel;
			if( gridAlignment == GridEditor.GridAlignment.XZ )
			{
				posLabel = "Grid Y Pos";
				shiftXLabel = "Grid Shift X";
				shiftZLabel = "Grid Shift Z";
			}
			else if( gridAlignment == GridEditor.GridAlignment.XY )
			{
				posLabel = "Grid Z Pos";
				shiftXLabel = "Grid Shift X";
				shiftZLabel = "Grid Shift Y";
			}
			else
			{
				posLabel = "Grid X Pos";
				shiftXLabel = "Grid Shift Y";
				shiftZLabel = "Grid Shift Z";
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( posLabel, gridPos );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Pos" );
				gridPos = tempFloat;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( shiftXLabel, gridShiftAxis1 );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift Axis 1" );
				gridShiftAxis1 = tempFloat;
			}

			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.FloatField( shiftZLabel, gridShiftAxis2 );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift Axis 2" );
				gridShiftAxis2 = tempFloat;
			}
		}
	}
	#endregion

	public class GridEditorSettings : ScriptableObject
	{
		public const string SAVE_PATH = "Assets/Plugins/SimpleGridFramework/Settings.asset";

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

		public GridState GridState { get { return states[currentState]; } }
		public GameObject Prefab { get { return selectedPrefab; } }

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

		private void OnEnable()
		{
			if( states.Count == 0 )
				CreateState();
		}

		private void Reset()
		{
			if( states.Count == 0 )
				CreateState();
		}

		private void CreateState()
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

		public void UpdateStatesPopupList()
		{
			statesPopup = new string[states.Count];
			for( int i = 0; i < states.Count; i++ )
			{
				statesPopup[i] = ( i + 1 ) + ". " + states[i].name;
			}
		}

		public void UpdatePrefabStatesPopupList()
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
#if UNITY_2018_3_OR_NEWER
			if( selectedObject != null && PrefabUtility.GetPrefabInstanceHandle( selectedObject ) != null )
			{
				selectedPrefab = PrefabUtility.GetCorrespondingObjectFromSource( selectedObject );
#else
			if( selectedObject != null && PrefabUtility.GetPrefabObject( selectedObject ) != null )
			{
				selectedPrefab = (GameObject) PrefabUtility.GetPrefabParent( selectedObject );
#endif
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

				Vector3 rotation = selectedPrefabStates[currentPrefabState].rotation;
				rotation.y += yDegrees;

				while( rotation.y >= 360f )
					rotation.y -= 360f;
				while( rotation.y < 0f )
					rotation.y += 360f;

				selectedPrefabStates[currentPrefabState].rotation = rotation;
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
				EditorGUILayout.HelpBox( "No prefab is selected", MessageType.Info );
		}
	}
}