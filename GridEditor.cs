using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;
 
/**
 * Open source Grid Framework 
 * for creating grid-based levels 
 * easily in Unity3D game engine
 *
 * @author Suleyman Yasir KULA
 */
 
// Save/load functions were inspired from:
// http://www.fizixstudios.com/labs/do/view/id/unity-file-management-and-xml

// Main class to handle everything related with the framework
public class GridEditor : EditorWindow
{
	// Inner class to store the properties of a prefab
	private class PrefabData
	{
		public string name = "unnamed";
		public GameObject prefab;
		public Vector3 rotation = Vector3.zero;
		public Vector3 shift;
		
		// Draw GUI controllers to be able to tweak settings via Unity editor
		// (this function is called by OnGUI function)
		public void Draw()
		{
			name = EditorGUILayout.TextField( "Name: ", name );
			prefab = ( GameObject ) EditorGUILayout.ObjectField( "Prefab: ", prefab, typeof( GameObject ), false );
			rotation = EditorGUILayout.Vector3Field( "Rotation: ", rotation );
			shift = EditorGUILayout.Vector3Field( "Shift: ", shift );
		}
	}
	
	// Is the framework enabled
	private static bool isEnabled = true;
	// Should the grids be drawn
	private static bool showGrids = true;
	// Is snap-to-grid enabled
	private static bool snapToGrid = true;
	
	// Some boring variables necessary for
	// the framework to work
	private static bool delegated = false;
	private static GridEditor instance = null;
	private static Material lineMaterial;
	
	// Color of the grids drawn on the Scene view
	private static Color gridlineColor = new Color( 0.8f, 0.8f, 0.8f, 1f );
	
	// Size of the grids in both X and Z directions
	// (a grid is a square)
	private static float gridSize = 1f;
	
	// Number of grids drawn in X and Z directions
	// (gridLineCount^2 grids are drawn on XZ-plane)
	private static int gridlineCount = 50;
	
	// Y-position of the grids
	private static float gridYPos = 0f;
	
	// How many units to shift the grids in X
	// and Z directions
	private static float gridShiftX = 0f;
	private static float gridShiftZ = 0f;
	
	// List of prefabs that user introduced to the framework
	private static List<PrefabData> prefabList = new List<PrefabData>();
	private static int activePrefab = -1;
	
	// Preview object that is used to show where
	// the instantiation will happen
	private static Transform prefabPreview;
	
	// Some more boring variables necessary for
	// the framework to work
	private static Vector2 eventMousePressPos = Vector2.one;
	private static bool mousePressValid = false;
	private static bool rightMouseButtonDown = false;
	
	private static Vector2 scrollPos = Vector2.zero;
	
	// Location of the save file
	private const string savefile = "Assets/Editor/GridFrameworkSavedData.xml";

	// Called when the framework window is opened 
	[MenuItem( "Level Editor/Grid Editor" )]
	public static void Init()
	{
		instance = ( GridEditor ) EditorWindow.GetWindow( typeof( GridEditor ) );
		instance.title = "Grid Editor";

		// Delegate the framework so that it works in cooperation with Scene View
		if( !delegated )
		{
			SceneView.onSceneGUIDelegate += instance.LevelEditorUpdate;
			delegated = true;
		}
		
		Refresh();
	}

	// Refresh the prefab preview
	static void Refresh()
	{
		if( prefabPreview != null )
			DestroyImmediate( prefabPreview.gameObject );
				
		if( activePrefab >= 0 && activePrefab < prefabList.Count )
		{
			if( isEnabled )
			{
				GameObject obj = prefabList[activePrefab].prefab;
				
				if( obj != null )
				{
					prefabPreview = ( ( GameObject ) PrefabUtility.InstantiatePrefab( obj ) ).transform;
					
					// Preview object should not be visible in the Hierarchy
					prefabPreview.gameObject.hideFlags = HideFlags.HideAndDontSave;
					prefabPreview.gameObject.layer = 2; //IgnoreRaycast
					prefabPreview.eulerAngles = prefabList[activePrefab].rotation;
				}
			}
		}
		
		SceneView.RepaintAll();
	}
	
	// Save all the data stored in the framework into a file
	void Save()
	{
		XmlDocument xml = new XmlDocument();
		XmlElement root = xml.CreateElement( "GridFrameworkSettings" );
		xml.AppendChild( root );
		
		XmlElement e;
		
		e = xml.CreateElement( "showGrids" );
		if( showGrids )
			e.InnerText = "1";
		else
			e.InnerText = "0";
		root.AppendChild( e );
		
		e = xml.CreateElement( "isEnabled" );
		if( isEnabled )
			e.InnerText = "1";
		else
			e.InnerText = "0";
		root.AppendChild( e );
		
		e = xml.CreateElement( "snapToGrid" );
		if( snapToGrid )
			e.InnerText = "1";
		else
			e.InnerText = "0";
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridColor" );
		e.SetAttribute( "r", "" + gridlineColor.r );
		e.SetAttribute( "g", "" + gridlineColor.g );
		e.SetAttribute( "b", "" + gridlineColor.b );
		e.SetAttribute( "a", "" + gridlineColor.a );
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridSize" );
		e.InnerText = "" + gridSize;
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridCount" );
		e.InnerText = "" + gridlineCount;
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridYPos" );
		e.InnerText = "" + gridYPos;
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridShiftX" );
		e.InnerText = "" + gridShiftX;
		root.AppendChild( e );
		
		e = xml.CreateElement( "gridShiftZ" );
		e.InnerText = "" + gridShiftZ;
		root.AppendChild( e );
		
		e = xml.CreateElement( "prefabCount" );
		e.InnerText = "" + prefabList.Count;
		root.AppendChild( e );
		
		foreach( PrefabData pd in prefabList )
		{
			e = xml.CreateElement( "prefab" );
			e.SetAttribute( "name", "" + pd.name );
			e.SetAttribute( "prefabLocation", "" + AssetDatabase.GetAssetPath( pd.prefab ) );
			e.SetAttribute( "rotX", "" + pd.rotation.x );
			e.SetAttribute( "rotY", "" + pd.rotation.y );
			e.SetAttribute( "rotZ", "" + pd.rotation.z );
			e.SetAttribute( "shiftX", "" + pd.shift.x );
			e.SetAttribute( "shiftY", "" + pd.shift.y );
			e.SetAttribute( "shiftZ", "" + pd.shift.z );
			root.AppendChild( e );
		}
		
		// XML output code inspired from: 
		// http://stackoverflow.com/questions/203528/what-is-the-simplest-way-to-get-indented-xml-with-line-breaks-from-xmldocument
		// answered by: Neil C. Obremski
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "  ",
			NewLineChars = "\r\n",
			NewLineHandling = NewLineHandling.Replace
		};
		
		using( XmlWriter output = XmlWriter.Create( savefile, settings ) ) 
		{
			xml.Save( output );
		}
	}
	
	// Retrieve the saved data from the XML file
	void Load()
	{
		if( File.Exists( savefile ) )
		{
			XmlDocument xml = new XmlDocument();
			xml.Load( savefile );
			
			prefabList = new List<PrefabData>();
			activePrefab = -1;
			
			XmlNodeList root = xml.GetElementsByTagName( "GridFrameworkSettings" );
			XmlNode settings = root[0];
			foreach( XmlNode setting in settings.ChildNodes )
			{
				if( setting.Name == "showGrids" )
				{
					if( setting.InnerText == "0" )
						showGrids = false;
					else
						showGrids = true;
				}
				else if( setting.Name == "isEnabled" )
				{
					if( setting.InnerText == "0" )
						isEnabled = false;
					else
						isEnabled = true;
				}
				else if( setting.Name == "snapToGrid" )
				{
					if( setting.InnerText == "0" )
						snapToGrid = false;
					else
						snapToGrid = true;
				}
				else if( setting.Name == "gridColor" )
				{
					float r, g, b, a;
					if( float.TryParse( setting.Attributes[ "r" ].Value, out r ) &&
						float.TryParse( setting.Attributes[ "g" ].Value, out g ) &&
						float.TryParse( setting.Attributes[ "b" ].Value, out b ) &&
						float.TryParse( setting.Attributes[ "a" ].Value, out a ) )
					{
						gridlineColor = new Color( r, g, b, a );
					}
				}
				else if( setting.Name == "gridSize" )
				{
					float value;
					if( float.TryParse( setting.InnerText, out value ) )
						gridSize = value;
				}
				else if( setting.Name == "gridCount" )
				{
					int value;
					if( int.TryParse( setting.InnerText, out value ) )
						gridlineCount = value;
				}
				else if( setting.Name == "gridYPos" )
				{
					float value;
					if( float.TryParse( setting.InnerText, out value ) )
						gridYPos = value;
				}
				else if( setting.Name == "gridShiftX" )
				{
					float value;
					if( float.TryParse( setting.InnerText, out value ) )
						gridShiftX = value;
				}
				else if( setting.Name == "gridShiftZ" )
				{
					float value;
					if( float.TryParse( setting.InnerText, out value ) )
						gridShiftZ = value;
				}
				else if( setting.Name == "prefab" )
				{
					PrefabData pd = new PrefabData();
					
					pd.name = setting.Attributes[ "name" ].Value;
					
					string prefabLocation = setting.Attributes[ "prefabLocation" ].Value;
					if( prefabLocation != "" )
					{
						pd.prefab = (GameObject) AssetDatabase.LoadMainAssetAtPath( prefabLocation );
					}
					
					float x, y, z;
					if( float.TryParse( setting.Attributes[ "rotX" ].Value, out x ) &&
						float.TryParse( setting.Attributes[ "rotY" ].Value, out y ) &&
						float.TryParse( setting.Attributes[ "rotZ" ].Value, out z ) )
					{
						pd.rotation = new Vector3( x, y, z );
					}
					
					if( float.TryParse( setting.Attributes[ "shiftX" ].Value, out x ) &&
						float.TryParse( setting.Attributes[ "shiftY" ].Value, out y ) &&
						float.TryParse( setting.Attributes[ "shiftZ" ].Value, out z ) )
					{
						pd.shift = new Vector3( x, y, z );
					}
					
					prefabList.Add( pd );
				}
			}
		}
		
		Refresh();
	}
	
	// Always keep the prefab preview up-to-date
	void Update()
	{
		if( activePrefab >= 0 && prefabPreview != null )
		{
			prefabPreview.eulerAngles = prefabList[activePrefab].rotation;
		}
	}
	
	// Draw the contents of the editor window
	void OnGUI()
	{
		// If Unity is just opened or a script is changed
		if( !delegated )
		{
			Color col = GUI.color;
			GUI.color = Color.cyan;
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Box( "Framework is not initialized!" );
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space( 10 );
			
			GUI.color = Color.green;
			if( GUILayout.Button( "INITIALIZE", GUILayout.Height( 50 ) ) )
			{
				Init();
			}
			GUI.color = col;
			
			return;
		}
		
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		
		scrollPos = EditorGUILayout.BeginScrollView( scrollPos );
		
		Color c = GUI.color;
		/*GUI.color = Color.yellow;
		if( GUILayout.Button( "Refresh", GUILayout.Height( 35 ) ) )
		{
			Refresh();
		}*/
		
		// Draw save/load buttons
		GUILayout.BeginHorizontal();
		GUI.color = Color.yellow;
		if( GUILayout.Button( "Save\nSettings", GUILayout.Height( 35 ) ) )
		{
			Save();
		}
		
		GUI.color = Color.cyan;
		if( GUILayout.Button( "Load\nSettings", GUILayout.Height( 35 ) ) )
		{
			Load();
		}
		GUILayout.EndHorizontal();
		GUI.color = c;
		
		// Horizontal line code
		// taken from: http://answers.unity3d.com/questions/216584/horizontal-line.html
		// answered by: Steven 1
		GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );
		
		// Draw the properties of the framework on the editor window
		c = GUI.color;
		GUI.color = Color.green;
		GUILayout.Box( "CONTROL PANEL", GUILayout.ExpandWidth( true ), GUILayout.Height( 35 ) );
		GUI.color = c;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Show Grids: " );
		if( showGrids )
		{
			if( GUILayout.Button( "Enabled" ) )
			{
				showGrids = false;
				Refresh();
			}
		}
		else
		{
			if( GUILayout.Button( "Disabled" ) )
			{
				showGrids = true;
				Refresh();
			}
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Framework: " );
		if( isEnabled )
		{
			if( GUILayout.Button( "Enabled" ) )
			{
				isEnabled = false;
				Refresh();
				
				if( prefabPreview != null )
					DestroyImmediate( prefabPreview.gameObject );
			}
		}
		else
		{
			if( GUILayout.Button( "Disabled" ) )
			{
				isEnabled = true;
				Refresh();
			}
		}
		GUILayout.EndHorizontal();
		
		if( !isEnabled )
			GUI.enabled = false;
			
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Snap To Grid: " );
		if( snapToGrid )
		{
			if( GUILayout.Button( "Enabled" ) )
			{
				snapToGrid = false;
			}
		}
		else
		{
			if( GUILayout.Button( "Disabled" ) )
			{
				snapToGrid = true;
			}
		}
		GUILayout.EndHorizontal();
		
		GUI.enabled = true;
		
		GUILayout.Space( 20 );
		
		// Draw a horizontal line (splitter)
		GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );
		
		c = GUI.color;
		GUI.color = Color.green;
		GUILayout.Box( "GRID PROPERTIES", GUILayout.ExpandWidth( true ), GUILayout.Height( 35 ) );
		GUI.color = c;
		
		gridlineColor = EditorGUILayout.ColorField( "Color: ", gridlineColor );
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Size: " );
		if( GUILayout.Button( "-" ) )
		{
			gridSize -= 0.5f;
			if( gridSize < 0.5f )
				gridSize = 0.5f;
			SceneView.RepaintAll();
		}
		gridSize = EditorGUILayout.FloatField( gridSize );
		if( GUILayout.Button( "+" ) )
		{
			gridSize += 0.5f;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Count: " );
		if( GUILayout.Button( "-" ) )
		{
			gridlineCount -= 10;
			if( gridlineCount < 10 )
				gridlineCount = 10;
			SceneView.RepaintAll();
		}
		gridlineCount = EditorGUILayout.IntField( gridlineCount );
		if( GUILayout.Button( "+" ) )
		{
			gridlineCount += 10;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Y Position: " );
		if( GUILayout.Button( "-" ) )
		{
			gridYPos -= 0.5f;
			SceneView.RepaintAll();
		}
		gridYPos = EditorGUILayout.FloatField( gridYPos );
		if( GUILayout.Button( "+" ) )
		{
			gridYPos += 0.5f;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Shift X: " );
		if( GUILayout.Button( "-" ) )
		{
			gridShiftX -= 0.5f;
			SceneView.RepaintAll();
		}
		gridShiftX = EditorGUILayout.FloatField( gridShiftX );
		if( GUILayout.Button( "+" ) )
		{
			gridShiftX += 0.5f;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Shift Z: " );
		if( GUILayout.Button( "-" ) )
		{
			gridShiftZ -= 0.5f;
			SceneView.RepaintAll();
		}
		gridShiftZ = EditorGUILayout.FloatField( gridShiftZ );
		if( GUILayout.Button( "+" ) )
		{
			gridShiftZ += 0.5f;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Space( 20 );
		
		// Draw a horizontal line (splitter)
		GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );
		
		c = GUI.color;
		GUI.color = Color.green;
		GUILayout.Box( "PREFABS", GUILayout.ExpandWidth( true ), GUILayout.Height( 35 ) );
		GUI.color = c;
		
		c = GUI.color;
		
		if( activePrefab == -1 )
			GUI.color = Color.cyan;
			
		if( GUILayout.Button( "None" ) )
		{
			activePrefab = -1;
			
			Refresh();
		}
		
		for( int i = 0; i < activePrefab; i++ )
		{
			if( GUILayout.Button( prefabList[i].name ) )
			{
				activePrefab = i;
				
				Refresh();
			}
		}
		
		if( activePrefab != -1 )
		{
			GUI.color = Color.cyan;
			GUILayout.Button( prefabList[activePrefab].name );
		}
		
		GUI.color = c;
		
		for( int i = activePrefab + 1; i < prefabList.Count; i++ )
		{
			if( GUILayout.Button( prefabList[i].name ) )
			{
				activePrefab = i;
				
				Refresh();
			}
		}
		
		GUI.color = new Color( 0.7f, 0.8f, 0.15f, 1f );
		if( GUILayout.Button( "New Prefab" ) )
		{
			// Add a new prefab to the prefabs list
			prefabList.Add( new PrefabData() );
			activePrefab = prefabList.Count - 1;
			
			Refresh();
		}
		GUI.color = c;
		
		if( activePrefab >= 0 && activePrefab < prefabList.Count )
		{
			// Draw the properties of the selected prefab on the window
			// and repaint the Scene View in case the prefab is changed
			GameObject prevValue = prefabList[activePrefab].prefab;
			prefabList[activePrefab].Draw();
			if( prefabList[activePrefab].prefab != prevValue )
				Refresh();
			
			GUILayout.Space( 10 );
			
			GUI.color = new Color( 1f, 0.4f, 0.4f, 1f );
			if( GUILayout.Button( "Remove Prefab" ) )
			{
				// Remove selected prefab from the list
				prefabList.RemoveAt( activePrefab );
				
				activePrefab--;
				if( activePrefab < 0 && prefabList.Count > 0 )
					activePrefab = 0;
					
				Refresh();
			}
			GUI.color = c;
		}
		
		EditorGUILayout.EndScrollView();
	}
	
	// Create a simple material to draw the gridlines
	// Code taken from: http://docs.unity3d.com/ScriptReference/GL.html
	static void CreateLineMaterial() 
	{
		if( !lineMaterial ) 
		{
			lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off Cull Off Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }" );
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	// Draw gridlines on the Scene view
	// Code was inspired from: http://docs.unity3d.com/ScriptReference/GL.html
	void DrawGridlines( Vector3 pos ) 
	{
		CreateLineMaterial();
		lineMaterial.SetPass( 0 );
		
		float startX = pos.x - gridlineCount * gridSize * 0.5f + gridShiftX;
		float startZ = pos.z - gridlineCount * gridSize * 0.5f + gridShiftZ;
		float endX = pos.x + gridlineCount * gridSize * 0.5f + gridShiftX;
		float endZ = pos.z + gridlineCount * gridSize * 0.5f + gridShiftZ;
		
		GL.Begin( GL.LINES );
		GL.Color( gridlineColor );
		
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
	void LevelEditorUpdate( SceneView scenePanel )
	{
		// Draw gridlines on screen
		if( showGrids )
		{
			Vector3 pos = scenePanel.camera.transform.position;
			pos.x -= pos.x % gridSize;
			pos.z -= pos.z % gridSize;
			
			DrawGridlines( pos );
		}
		
		// If the framework is enabled
		if( isEnabled )
		{
			Event e = Event.current;
			
			if( e.type == EventType.KeyUp )
			{
				if( e.keyCode == KeyCode.T && activePrefab != -1 )
					prefabList[activePrefab].rotation.y -= 45f;
				if( e.keyCode == KeyCode.Y && activePrefab != -1 )
					prefabList[activePrefab].rotation.y += 45f;
			}
			
			if( e.isMouse )
			{
				// If a prefab is selected from the prefabs list
				if( prefabPreview != null )
				{
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
						if( snapToGrid )
						{
							rayPos.x -= gridShiftX;
							rayPos.z -= gridShiftZ;
							
							if( rayPos.x > 0 )
								rayPos.x = rayPos.x -rayPos.x % gridSize + gridSize * 0.5f;
							else
								rayPos.x = rayPos.x - rayPos.x % gridSize - gridSize * 0.5f;
							
							if( rayPos.z > 0 )
								rayPos.z = rayPos.z - rayPos.z % gridSize + gridSize * 0.5f;
							else
								rayPos.z = rayPos.z - rayPos.z % gridSize - gridSize * 0.5f;
								
							rayPos.x += gridShiftX;
							rayPos.z += gridShiftZ;
						}
						
						prefabPreview.position = rayPos + prefabList[activePrefab].shift;
						
						if( e.type == EventType.MouseDown )
						{
							if( e.button != 0 )
								rightMouseButtonDown = true;
							else
							{
								if( !e.alt && !rightMouseButtonDown )
								{
									eventMousePressPos = e.mousePosition;
									GUIUtility.hotControl = GUIUtility.GetControlID( GetHashCode(), FocusType.Passive );
									mousePressValid = true;
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
								if( mousePressValid && !rightMouseButtonDown )
								{
									mousePressValid = false;
									GUIUtility.hotControl = 0;
									
									// If it is a "click" for instantiating the prefab
									if( ( e.mousePosition - eventMousePressPos ).sqrMagnitude < 5f )
									{
										/*RaycastHit hit;
										bool checkCollision = Physics.Raycast( ray, out hit, distance );*/
										GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab( prefabList[activePrefab].prefab );
										instance.transform.position = rayPos + prefabList[activePrefab].shift;
										instance.transform.eulerAngles = prefabList[activePrefab].rotation;
										Undo.RegisterCreatedObjectUndo( instance, "Create " + instance.name );
									}
								}
							}
						}
						
						SceneView.RepaintAll();
					}
				}
			}
		}
	}
	
	// Repaint the editor window when the script is reloaded
	void OnEnable()
	{
		this.Repaint();
	}
	
	// Destroy prefab preview object when the editor window is closed
	void OnDisable()
	{
		if( prefabPreview != null )
			DestroyImmediate( prefabPreview.gameObject );
				
		if( delegated && instance != null )
		{
			SceneView.onSceneGUIDelegate -= instance.LevelEditorUpdate;
			delegated = false;
		}
	}
}
