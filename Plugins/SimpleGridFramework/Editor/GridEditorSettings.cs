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

		public PrefabState( string name, PrefabState copyFrom )
		{
			this.name = name;
			this.rotation = copyFrom.rotation;
			this.shift = copyFrom.shift;
		}

		public void Draw( GridEditorSettings settings, bool isDefaultState )
		{
			EditorGUI.BeginChangeCheck();
			string _name = EditorGUILayout.TextField( "Name", name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab State Name" );
				if( !isDefaultState )
					name = _name;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( _name, rotation, shift ) );

				settings.UpdatePrefabStatesPopupList();
			}

			EditorGUI.BeginChangeCheck();
			Vector3 _rotation = EditorGUILayout.Vector3Field( "Rotation", rotation );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab Rotation" );
				if( !isDefaultState )
					rotation = _rotation;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( name, _rotation, shift ) );
			}

			EditorGUI.BeginChangeCheck();
			Vector3 _shift = EditorGUILayout.Vector3Field( "Shift", shift );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Prefab Shift" );
				if( !isDefaultState )
					shift = _shift;
				else
					settings.CreatePrefabStateForSelection( new PrefabState( name, rotation, _shift ) );
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

		public GridState( string name, GridState copyFrom )
		{
			this.name = name;
			this.gridlineColor = copyFrom.gridlineColor;
			this.gridSize = copyFrom.gridSize;
			this.gridlineCount = copyFrom.gridlineCount;
			this.gridAlignment = copyFrom.gridAlignment;
			this.gridPos = copyFrom.gridPos;
			this.gridShiftAxis1 = copyFrom.gridShiftAxis1;
			this.gridShiftAxis2 = copyFrom.gridShiftAxis2;
		}

		public void Draw( GridEditorSettings settings )
		{
			EditorGUI.BeginChangeCheck();
			string _name = EditorGUILayout.TextField( "Name", name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change State Name" );
				name = _name;

				settings.UpdateStatesPopupList();
			}

			EditorGUI.BeginChangeCheck();
			Color _gridlineColor = EditorGUILayout.ColorField( "Gridline Color", gridlineColor );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Gridline Color" );
				gridlineColor = _gridlineColor;
			}

			EditorGUI.BeginChangeCheck();
			float _gridSize = EditorGUILayout.FloatField( "Grid Size", gridSize );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Size" );
				gridSize = _gridSize;
			}

			EditorGUI.BeginChangeCheck();
			int _gridlineCount = EditorGUILayout.IntField( "Gridline Count", gridlineCount );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Gridline Count" );
				gridlineCount = _gridlineCount;
			}

			EditorGUI.BeginChangeCheck();
			GridEditor.GridAlignment _gridAlignment = (GridEditor.GridAlignment) EditorGUILayout.EnumPopup( "Grid Alignment", gridAlignment );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Alignment" );
				gridAlignment = _gridAlignment;
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
			float _gridPos = EditorGUILayout.FloatField( posLabel, gridPos );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Pos" );
				gridPos = _gridPos;
			}

			EditorGUI.BeginChangeCheck();
			float _gridShiftAxis1 = EditorGUILayout.FloatField( shiftXLabel, gridShiftAxis1 );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift Axis 1" );
				gridShiftAxis1 = _gridShiftAxis1;
			}

			EditorGUI.BeginChangeCheck();
			float _gridShiftAxis2 = EditorGUILayout.FloatField( shiftZLabel, gridShiftAxis2 );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( settings, "Change Grid Shift Axis 2" );
				gridShiftAxis2 = _gridShiftAxis2;
			}
		}
	}
	#endregion

	public class GridEditorSettings : ScriptableObject
	{
		public const string SAVE_PATH = "Assets/Plugins/SimpleGridFramework/Settings.asset";

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

		private bool showGridSettings = false;
		private bool showPrefabSettings = true;

		public GridState GridState
		{
			get
			{
				if( currentState >= states.Count )
					currentState = Mathf.Max( 0, states.Count - 1 );

				return states[currentState];
			}
		}

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

				if( currentPrefabState >= selectedPrefabStates.Count )
					currentPrefabState = Mathf.Max( 0, selectedPrefabStates.Count - 1 );

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

		private void CreateState( GridState referenceState = null )
		{
			int index = 1;
			string name = "State " + index;
			for( int i = 0; i < states.Count; i++ )
			{
				if( states[i].name == name )
				{
					index++;
					name = "State " + index;
					i = -1;
				}
			}

			states.Add( referenceState != null ? new GridState( name, referenceState ) : new GridState( name ) );
		}

		public void UpdateStatesPopupList()
		{
			statesPopup = new string[states.Count];
			for( int i = 0; i < states.Count; i++ )
				statesPopup[i] = ( i + 1 ) + ". " + states[i].name;
		}

		public void UpdatePrefabStatesPopupList()
		{
			if( selectedPrefabStates == null )
				prefabStatesPopup = new string[] { "1. default" };
			else
			{
				prefabStatesPopup = new string[selectedPrefabStates.Count];
				for( int i = 0; i < selectedPrefabStates.Count; i++ )
					prefabStatesPopup[i] = ( i + 1 ) + ". " + selectedPrefabStates[i].name;
			}
		}

		public void OnSelectionChanged()
		{
			GameObject selectedObject = Selection.activeGameObject;
#if UNITY_2018_3_OR_NEWER
			if( selectedObject != null && PrefabUtility.IsPartOfAnyPrefab( selectedObject ) )
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
			if( !selectedPrefab )
				return;

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

		public void RotateSelectedPrefab( float yDegrees )
		{
			if( !selectedPrefab )
				return;

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
			Color c = GUI.color;

			// Draw horizontal line
			// Credit: http://answers.unity3d.com/questions/216584/horizontal-line.html
			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

			if( currentState >= states.Count )
				currentState = states.Count - 1;

			showGridSettings = EditorGUILayout.Foldout( showGridSettings, "Show Grid Settings", true );
			if( showGridSettings )
			{
				GUILayout.BeginHorizontal();

				EditorGUI.BeginChangeCheck();
				int _state = EditorGUILayout.Popup( "State", currentState, statesPopup );
				if( EditorGUI.EndChangeCheck() )
				{
					Undo.RecordObject( this, "Change State" );
					currentState = _state;
				}

				GUI.color = Color.yellow;

				if( GUILayout.Button( "+", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
				{
					Undo.RecordObject( this, "Create State" );
					CreateState( states[currentState] );

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
			}

			// Draw horizontal line
			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

			if( !selectedPrefab )
				EditorGUILayout.HelpBox( "No prefab is selected", MessageType.Info );
			else
			{
				GUI.enabled = false;
				EditorGUILayout.ObjectField( "Prefab", selectedPrefab, typeof( GameObject ), false );
				GUI.enabled = true;

				showPrefabSettings = EditorGUILayout.Foldout( showPrefabSettings, "Show Prefab Settings", true );
				if( showPrefabSettings )
				{
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
						int _state = EditorGUILayout.Popup( "Prefab State", currentPrefabState, prefabStatesPopup );
						if( EditorGUI.EndChangeCheck() )
						{
							Undo.RecordObject( this, "Change Prefab State" );
							currentPrefabState = _state;
						}

						state = selectedPrefabStates[currentPrefabState];
						isDefaultState = false;
					}

					GUI.color = Color.yellow;

					if( GUILayout.Button( "+", GUILayout.Width( 25f ), GUILayout.Height( 14f ) ) )
					{
						Undo.RecordObject( this, "Create Prefab State" );
						CreatePrefabStateForSelection( new PrefabState( "unnamed", selectedPrefabStates[currentPrefabState] ), true );

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
			}
		}
	}
}