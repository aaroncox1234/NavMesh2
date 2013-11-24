using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RxSoft
{
	[CustomEditor(typeof(rxCustomPolygon), true)]
	public class rxCustomPolygonEditor : Editor
	{
		public void OnSceneGUI()
		{
			rxCustomPolygon targetPolygon = target as rxCustomPolygon;

			// Get the mouse position.
			Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
			Vector3 mousePosition = ray.GetPoint( ( targetPolygon.transform.position - Camera.current.transform.position ).magnitude );

			switch ( Event.current.type )
			{
				case EventType.mouseDrag:
				{
					targetPolygon.OnMouseDrag( mousePosition );
					
					EditorUtility.SetDirty( targetPolygon );
				}
				break;
					
				case EventType.mouseMove:
				{
					targetPolygon.OnMouseMove( mousePosition );
					
					EditorUtility.SetDirty( targetPolygon );
				}
				break;
					
				case EventType.mouseDown:
				{
					targetPolygon.OnMouseDown( mousePosition );
						
					EditorUtility.SetDirty( targetPolygon );
				}
				break;
					
				case EventType.layout:
				{
					// This blocks mouse clicks from deselecting the target while editing.
					int controlID = GUIUtility.GetControlID( FocusType.Passive );
					HandleUtility.AddDefaultControl( controlID );
				}
				break;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			rxCustomPolygon targetPolygon = target as rxCustomPolygon;

			List<Vector2> targetVertices = targetPolygon.GetWorldVertices();

			GUILayout.BeginVertical();

			bool insertVertex = GUILayout.Button( "Insert Vertex" );
			bool deleteVertex = GUILayout.Button( "Delete Vertex" );

			bool reverseWinding = GUILayout.Button( "Reverse Winding" );
			bool centerGizmo = GUILayout.Button( "Center Gizmo" );

			GUILayout.Label( "Type: " + ( Geometry2D.IsPolygonConvex( targetVertices ) ? "Convex" : "Concave" ) );
			GUILayout.Label( "Winding: " + ( Geometry2D.IsPolygonCCW( targetVertices ) ? "CCW" : "CW" ) );
			
			GUILayout.EndVertical();

			if ( insertVertex )
			{
				targetPolygon.InsertVertex();
				EditorUtility.SetDirty( targetPolygon );
			}

			if ( deleteVertex )
			{
				targetPolygon.DeleteVertex();
				EditorUtility.SetDirty( targetPolygon );
			}

			if ( reverseWinding )
			{
				targetPolygon.ReverseWinding();
				EditorUtility.SetDirty( targetPolygon );
			}

			if ( centerGizmo )
			{
				targetPolygon.CenterGizmo();
				EditorUtility.SetDirty( targetPolygon );
			}
		}
	}
}
