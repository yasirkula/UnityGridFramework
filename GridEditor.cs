using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class GridEditor : EditorWindow
{
	[System.Serializable]
	private class PrefabData
	{
		public GameObject prefab;
		public Vector3 prefabRotation = Vector3.zero;
		
		public void Draw()
		{
			prefab = ( GameObject ) EditorGUILayout.ObjectField( "Prefab: ", prefab, typeof( GameObject ), false );
			prefabRotation = EditorGUILayout.Vector3Field( "Prefab Eğim: ", prefabRotation );
		}
	} 
	
	private static PrefabData pd;
	
	private static bool isEnabled = true;
	
	private static bool delegated = false;
	private static GridEditor instance = null;
	private static Material lineMaterial;
	
	private static Color gridlineColor = new Color( 0.8f, 0.8f, 0.8f, 1f );
	private static float gridSize = 1f;
	private static float halfGridSize = 0.5f;
	private static int gridlineCount = 50;
	private static float gridYPos = 0f;
	private static float gridShiftX = 0f;
	private static float gridShiftZ = 0f;
	private static GameObject prefab;
	private static Vector3 prefabRotation = Vector3.zero;
	
	private static Transform prefabPreview;
	
	private static Vector2 eventMousePressPos = Vector2.one;
	private static bool mousePressValid = false;

	[ MenuItem( "Level Editor/Grid Editor" ) ]
	public static void Init()
	{
		GridEditor editor = ( GridEditor ) EditorWindow.GetWindow( typeof( GridEditor ) );
		instance = editor;
		instance.title = "Grid Editor";
		pd = new PrefabData();

		if( !delegated )
		{
			SceneView.onSceneGUIDelegate += instance.LevelEditorUpdate;
			delegated = true;
		}

		Refresh();
	}

	static void Refresh()
	{
		if( prefab != null )
		{
			if( prefabPreview != null )
				DestroyImmediate( prefabPreview.gameObject );
			
			prefabPreview = ( (GameObject) PrefabUtility.InstantiatePrefab( prefab ) ).transform;
			prefabPreview.gameObject.hideFlags = HideFlags.HideAndDontSave;
			prefabPreview.gameObject.layer = 2; //IgnoreRaycast
			prefabPreview.eulerAngles = prefabRotation;
		}
		
		SceneView.RepaintAll();
	}
	
	void OnGUI()
	{
		Color c = GUI.color;
		GUI.color = Color.green;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box( "AÇ/KAPA" );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.color = c;
		
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
		
		GUILayout.Space( 20 );
		
		c = GUI.color;
		GUI.color = Color.green;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box( "GRID ÖZELLİKLER" );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.color = c;
		
		gridlineColor = EditorGUILayout.ColorField( "Grid Renk: ", gridlineColor );
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Grid Ebat: " );
		if( GUILayout.Button( "-" ) )
		{
			gridSize -= 0.5f;
			if( gridSize < 0.5f )
				gridSize = 0.5f;
			halfGridSize = gridSize * 0.5f;
			SceneView.RepaintAll();
		}
		gridSize = EditorGUILayout.FloatField( gridSize );
		if( GUILayout.Button( "+" ) )
		{
			gridSize += 0.5f;
			halfGridSize = gridSize * 0.5f;
			SceneView.RepaintAll();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Grid Sayısı: " );
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
		GUILayout.Label( "Grid Y Pozisyonu: " );
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
		GUILayout.Label( "Grid Shift X: " );
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
		GUILayout.Label( "Grid Shift Z: " );
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
		
		c = GUI.color;
		GUI.color = Color.green;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box( "PREFAB ÖZELLİKLER" );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.color = c;
		
		prefab = ( GameObject ) EditorGUILayout.ObjectField( "Prefab: ", prefab, typeof( GameObject ), false );
		prefabRotation = EditorGUILayout.Vector3Field( "Prefab Eğim: ", prefabRotation );
		
		GUILayout.Space( 20 );
		
		c = GUI.color;
		GUI.color = Color.yellow;
		if( GUILayout.Button( "Refresh" ) )
		{
			halfGridSize = gridSize * 0.5f;
			Refresh();
		}
		GUI.color = c;
		
		if( pd != null )
			pd.Draw();
	}
	
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

	void DrawGridlines( Vector3 pos ) 
	{
		if( isEnabled )
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
	}
	
	void LevelEditorUpdate( SceneView scenePanel )
	{
		if( isEnabled )
		{
			Event e = Event.current;
			Vector3 pos = scenePanel.camera.transform.position;
			pos.x -= pos.x % gridSize;
			pos.z -= pos.z % gridSize;
			
			DrawGridlines( pos );
			
			if( e.isMouse && e.button == 0 )
			{
				if( prefab != null && prefabPreview != null )
				{
					Plane plane = new Plane( Vector3.up, new Vector3( 0, gridYPos, 0 ) );
					Camera sceneCam = scenePanel.camera;
					Vector2 mousePos = e.mousePosition;
					mousePos.y = sceneCam.pixelRect.height - mousePos.y;
					Ray ray = sceneCam.ScreenPointToRay( mousePos );
					float distance;
					
					if( plane.Raycast( ray, out distance ) )
					{
						Vector3 rayPos = ray.GetPoint( distance );
						
						rayPos.x -= gridShiftX;
						rayPos.z -= gridShiftZ;
						
						if( rayPos.x > 0 )
							rayPos.x = rayPos.x -rayPos.x % gridSize + halfGridSize;
						else
							rayPos.x = rayPos.x - rayPos.x % gridSize - halfGridSize;
						
						if( rayPos.z > 0 )
							rayPos.z = rayPos.z - rayPos.z % gridSize + halfGridSize;
						else
							rayPos.z = rayPos.z - rayPos.z % gridSize - halfGridSize;
							
						rayPos.x += gridShiftX;
						rayPos.z += gridShiftZ;
						
						// !?!?!?!?!? Right click-left click işi bozuyor !?!?!?!??!?
						// !?!?!?!?!? prefab listesi !?!?!?!?!?!?
						
						prefabPreview.position = rayPos;
						
						if( e.type == EventType.MouseDown )
						{
							if( !e.alt )
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
						else if( e.type == EventType.MouseUp )
						{
							if( mousePressValid )
							{
								mousePressValid = false;
								GUIUtility.hotControl = 0;
								
								// if it is a "click" for instantiating the prefab
								if( ( e.mousePosition - eventMousePressPos ).sqrMagnitude < 5f )
								{
									if( !Physics.Raycast( ray, distance ) )
									{
										GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab( prefab );
										instance.transform.position = rayPos;
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
