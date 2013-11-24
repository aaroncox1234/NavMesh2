using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( EditablePolygon2), true )]
public class Polygon2dEditor : Editor
{
	void OnSceneGUI()
    {
		EditablePolygon2 targetPolygon = (EditablePolygon2)target;
		
		// Get the mouse position.
		Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
		Vector3 mousePosition = ray.GetPoint( ( targetPolygon.transform.position - Camera.current.transform.position ).magnitude );
		
		switch ( Event.current.type )
		{
			case EventType.mouseDrag:
			{
				targetPolygon.SetSelectedVertexPosition( mousePosition );
			
				EditorUtility.SetDirty( targetPolygon );
			}
			break;
			
			case EventType.mouseMove:
			{
				targetPolygon.UpdateHoverVertex( mousePosition );
			
				EditorUtility.SetDirty( targetPolygon );
			}
			break;
			
			case EventType.mouseDown:
			{
				targetPolygon.UpdateSelectedVertex( mousePosition );
			
				EditorUtility.SetDirty( targetPolygon );
			}
			break;
			
			case EventType.keyDown:
			{
				if ( Event.current.keyCode == KeyCode.I )
				{
					targetPolygon.InsertVertex( mousePosition );
				
					EditorUtility.SetDirty( targetPolygon );
				}
				else if ( Event.current.keyCode == KeyCode.K )
				{
					targetPolygon.DeleteSelectedVertex();
				
					EditorUtility.SetDirty( targetPolygon );
				}
			}
			break;
			
			case EventType.layout:
			{
				// This blocks mouse clicks from deselecting the nav mesh while in edit mode.
				int controlID = GUIUtility.GetControlID(FocusType.Passive);
            	HandleUtility.AddDefaultControl(controlID);			
			}
			break;
		}
    }
	
	public override void OnInspectorGUI()
    {
		EditablePolygon2 targetPolygon = (EditablePolygon2)target;
        
		GUILayout.BeginVertical();
		
		// TODO: display winding GUILayout.Label( "Visualization", EditorStyles.boldLabel );
		bool reverseWindingClicked = GUILayout.Button( "Reverse Winding" );
		
		GUILayout.EndVertical();
		
		if ( reverseWindingClicked )
		{
			targetPolygon.ReverseWinding();
			
        	EditorUtility.SetDirty( target );
		}
	}
}
