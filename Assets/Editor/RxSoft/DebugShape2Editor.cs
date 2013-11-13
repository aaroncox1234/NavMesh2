using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Rx
{
	[CustomEditor( typeof( DebugShape2 ), true )]
	public class DebugShape2Editor : Editor
	{
		public void OnSceneGUI()
	    {
			DebugShape2 targetShape = (DebugShape2)target;
			
			UpdateSelection( targetShape );
			
			// Get the mouse position.
			Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
			Vector3 mousePosition = ray.GetPoint( ( targetShape.transform.position - Camera.current.transform.position ).magnitude );
			
			switch ( Event.current.type )
			{
				case EventType.mouseDrag:
				{
					targetShape.OnMouseDrag( mousePosition );
				
					EditorUtility.SetDirty( targetShape );
				}
				break;
				
				case EventType.mouseMove:
				{
					targetShape.OnMouseMove( mousePosition );
				
					EditorUtility.SetDirty( targetShape );
				}
				break;
				
				case EventType.mouseDown:
				{
					targetShape.OnMouseDown( mousePosition );
				
					EditorUtility.SetDirty( targetShape );
				}
				break;
				
				case EventType.keyDown:
				{
					targetShape.OnKeyDown( Event.current.keyCode, mousePosition );
				
					EditorUtility.SetDirty( targetShape );
				}
				break;
				
				case EventType.layout:
				{
					// This blocks mouse clicks from deselecting the nav mesh while in edit mode.
					int controlID = GUIUtility.GetControlID( FocusType.Passive );
	            	HandleUtility.AddDefaultControl( controlID );
				}
				break;
			}
	    }
		
		protected void UpdateSelection( DebugShape2 selectedShape )
		{
			DebugShape2[] debugShapes = FindObjectsOfType( typeof( DebugShape2 ) ) as DebugShape2[];
			
			foreach ( DebugShape2 debugShape in debugShapes )
			{
				debugShape.IsSelected = false;
			}
			
			selectedShape.IsSelected = true;
		}
		
		public override void OnInspectorGUI()
	    {
			base.OnInspectorGUI();
			
			ClearHighlighting();
			
			DebugShape2 selectedShape = (DebugShape2)target;
	        
			if ( selectedShape is DebugPoint2 )
			{
				OnDebugPointInspectorGUI( selectedShape as DebugPoint2 );
			}
			else if ( selectedShape is DebugLine2 )
			{
				OnDebugLineInspectorGUI( selectedShape as DebugLine2 );
			}
			else if ( selectedShape is DebugRay2 )
			{
				OnDebugRayInspectorGUI( selectedShape as DebugRay2 );
			}
			else if ( selectedShape is DebugTriangle2 )
			{
				OnDebugTriangleInspectorGUI( selectedShape as DebugTriangle2 );
			}
			else if ( selectedShape is DebugPolygon2 )
			{
				OnDebugPolygonInspectorGUI( selectedShape as DebugPolygon2 );
			}
			
			EditorUtility.SetDirty( selectedShape );
		}
		
		protected void ClearHighlighting()
		{
			DebugShape2[] debugShapes = FindObjectsOfType( typeof( DebugShape2 ) ) as DebugShape2[];
			
			foreach ( DebugShape2 debugShape in debugShapes )
			{
				debugShape.IsHighlighted = false;
				debugShape.PointsOfIntersection = new List<Vector2>();
			}
		}
		
		protected void OnDebugPointInspectorGUI( DebugPoint2 selectedDebugPoint )
		{
			GUILayout.BeginVertical();			
			selectedDebugPoint.CheckContainingPolygons = GUILayout.Toggle( selectedDebugPoint.CheckContainingPolygons, "Containment (Polygons)" );
			GUILayout.EndVertical();
			
			if ( selectedDebugPoint.CheckContainingPolygons )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				Vector2 worldPoint = selectedDebugPoint.GetWorldVertices()[0];
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					List<Vector2> worldPolygon = debugPolygon.GetWorldVertices();
					
					if ( Geometry2.PolygonIsConvex( worldPolygon ) )
					{
						if ( Geometry2.ConvexPolygonContainsPoint( worldPolygon, worldPoint ) )
						{
							debugPolygon.IsHighlighted = true;
							EditorUtility.SetDirty( debugPolygon );
						}
					}
					else
					{
						if ( Geometry2.PolygonContainsPoint( worldPolygon, worldPoint ) )
						{
							debugPolygon.IsHighlighted = true;
							EditorUtility.SetDirty( debugPolygon );
						}
					}
				}
			}			
		}
		
		protected void OnDebugLineInspectorGUI( DebugLine2 selectedDebugLine )
		{
			GUILayout.BeginVertical();			
			selectedDebugLine.CheckIntersectingSegments = GUILayout.Toggle( selectedDebugLine.CheckIntersectingSegments, "Intersection (Segments)" );
			selectedDebugLine.ShowPointsOfIntersectionWithSegments = GUILayout.Toggle( selectedDebugLine.ShowPointsOfIntersectionWithSegments, "Points of Intersection (Segment)" );
			selectedDebugLine.CheckIntersectingPolygons = GUILayout.Toggle( selectedDebugLine.CheckIntersectingPolygons, "Intersection (Polygons)" );
			bool centerGizmo = GUILayout.Button( "Center Gizmo" );			
			GUILayout.EndVertical();
			
			if ( selectedDebugLine.CheckIntersectingSegments || selectedDebugLine.ShowPointsOfIntersectionWithSegments )
			{
				DebugLine2[] debugLines = FindObjectsOfType( typeof( DebugLine2 ) ) as DebugLine2[];
				
				List<Vector2> selectedDebugLineVertices = selectedDebugLine.GetWorldVertices();
				
				List<Vector2> pointsOfIntersection = new List<Vector2>();
				
				foreach ( DebugLine2 debugLine in debugLines )
				{
					if ( debugLine != selectedDebugLine )
					{	
						List<Vector2> debugLineVertices = debugLine.GetWorldVertices();
						
						if ( selectedDebugLine.ShowPointsOfIntersectionWithSegments )
						{
							Vector2 pointOfIntersection;
						  
							if ( Geometry2.SegmentAgainstSegment( debugLineVertices[0], debugLineVertices[1], selectedDebugLineVertices[0], selectedDebugLineVertices[1], out pointOfIntersection ) )
							{
								pointsOfIntersection.Add( pointOfIntersection );
								
								if ( selectedDebugLine.CheckIntersectingSegments )
								{
									debugLine.IsHighlighted = true;
									EditorUtility.SetDirty( debugLine );
								}
							}
						}
						else
						{
							if ( Geometry2.SegmentsIntersect( debugLineVertices[0], debugLineVertices[1], selectedDebugLineVertices[0], selectedDebugLineVertices[1] ) )
							{
								debugLine.IsHighlighted = true;
								EditorUtility.SetDirty( debugLine );
							}
						}
					}
				}
				
				selectedDebugLine.PointsOfIntersection.AddRange( pointsOfIntersection );
				EditorUtility.SetDirty( selectedDebugLine );
			}
			
			if ( selectedDebugLine.CheckIntersectingPolygons )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				List<Vector2> selectedDebugLineVertices = selectedDebugLine.GetWorldVertices();
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					List<Vector2> pointsOfIntersection = Geometry2.SegmentAgainstPolygon( selectedDebugLineVertices[0], selectedDebugLineVertices[1], debugPolygon.GetWorldVertices() );
					
					if ( pointsOfIntersection.Count > 0 )
					{
						debugPolygon.IsHighlighted = true;
						EditorUtility.SetDirty( debugPolygon );
							
						selectedDebugLine.PointsOfIntersection.AddRange( pointsOfIntersection );
						EditorUtility.SetDirty( selectedDebugLine );
					}
				}
			}
			
			if ( centerGizmo )
			{
				selectedDebugLine.CenterGizmo();
			}
		}
		
		protected void OnDebugRayInspectorGUI( DebugRay2 selectedDebugRay )
		{
			GUILayout.BeginVertical();			
			selectedDebugRay.CheckIntersectingSegments = GUILayout.Toggle( selectedDebugRay.CheckIntersectingSegments, "Intersection (Segments)" );
			selectedDebugRay.ShowPointsOfIntersectionWithSegments = GUILayout.Toggle( selectedDebugRay.ShowPointsOfIntersectionWithSegments, "Points of Intersection (Segment)" );
			selectedDebugRay.CheckIntersectingPolygons = GUILayout.Toggle( selectedDebugRay.CheckIntersectingPolygons, "Intersection (Polygons)" );
			bool centerGizmo = GUILayout.Button( "Center Gizmo" );			
			GUILayout.EndVertical();
			
			List<Vector2> selectedDebugRayVertices = selectedDebugRay.GetWorldVertices();
			Vector2 selectedDebugRayStart = selectedDebugRayVertices[0];
			Vector2 selectedDebugRayDir = selectedDebugRayVertices[1] - selectedDebugRayVertices[0];
			
			if ( selectedDebugRay.CheckIntersectingSegments || selectedDebugRay.ShowPointsOfIntersectionWithSegments )
			{
				DebugLine2[] debugLines = FindObjectsOfType( typeof( DebugLine2 ) ) as DebugLine2[];
				
				List<Vector2> pointsOfIntersection = new List<Vector2>();
				
				foreach ( DebugLine2 debugLine in debugLines )
				{
					List<Vector2> debugLineVertices = debugLine.GetWorldVertices();
						
					if ( selectedDebugRay.ShowPointsOfIntersectionWithSegments )
					{
						Vector2 pointOfIntersection;
					  
						if ( Geometry2.RayAgainstSegment( selectedDebugRayStart, selectedDebugRayDir, debugLineVertices[0], debugLineVertices[1], out pointOfIntersection ) )
						{
							pointsOfIntersection.Add( pointOfIntersection );
							
							if ( selectedDebugRay.CheckIntersectingSegments )
							{
								debugLine.IsHighlighted = true;
								EditorUtility.SetDirty( debugLine );
							}
						}
					}
					else
					{
						if ( Geometry2.RayIntersectsSegment( selectedDebugRayStart, selectedDebugRayDir, debugLineVertices[0], debugLineVertices[1] ) )
						{
							debugLine.IsHighlighted = true;
							EditorUtility.SetDirty( debugLine );
						}
					}
				}
				
				selectedDebugRay.PointsOfIntersection.AddRange( pointsOfIntersection );
				EditorUtility.SetDirty( selectedDebugRay );
			}
			
			if ( selectedDebugRay.CheckIntersectingPolygons )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					List<Vector2> pointsOfIntersection = Geometry2.RayAgainstPolygon( selectedDebugRayStart, selectedDebugRayDir, debugPolygon.GetWorldVertices() );
					
					if ( pointsOfIntersection.Count > 0 )
					{
						debugPolygon.IsHighlighted = true;
						EditorUtility.SetDirty( debugPolygon );
							
						selectedDebugRay.PointsOfIntersection.AddRange( pointsOfIntersection );
						EditorUtility.SetDirty( selectedDebugRay );
					}
				}
			}
			
			if ( centerGizmo )
			{
				selectedDebugRay.CenterGizmo();
			}
		}
		
		protected void OnDebugTriangleInspectorGUI( DebugTriangle2 selectedDebugTriangle )
		{
			List<Vector2> debugTriangleVertices = selectedDebugTriangle.GetWorldVertices();
			
			string windingLabelText = "Winding: ";
			float area = Geometry2.SignedTriangleArea( debugTriangleVertices[0], debugTriangleVertices[1], debugTriangleVertices[2] );
			if ( area > 0 )
			{
				windingLabelText += "CCW";
			}
			else if ( area < 0 )
			{
				windingLabelText += "CW";
			}
			else
			{
				windingLabelText += "DEGENERATE";
			}
			
			GUILayout.BeginVertical();			
			GUILayout.Label( windingLabelText );
			selectedDebugTriangle.CheckPointsInside = GUILayout.Toggle( selectedDebugTriangle.CheckPointsInside, "Containing (Points)" );
			bool centerGizmo = GUILayout.Button( "Center Gizmo" );
			GUILayout.EndVertical();
			
			if ( selectedDebugTriangle.CheckPointsInside )
			{
				DebugPoint2[] debugPoints = FindObjectsOfType( typeof( DebugPoint2 ) ) as DebugPoint2[];
				
				foreach ( DebugPoint2 debugPoint in debugPoints )
				{						
					if ( Geometry2.TriangleContainsPoint( debugTriangleVertices[0], debugTriangleVertices[1], debugTriangleVertices[2], debugPoint.GetWorldVertices()[0] ) )
					{
						debugPoint.IsHighlighted = true;
						EditorUtility.SetDirty( debugPoint );
					}
				}
			}
			
			if ( centerGizmo )
			{
				selectedDebugTriangle.CenterGizmo();
			}
		}
		
		protected void OnDebugPolygonInspectorGUI( DebugPolygon2 selectedDebugPolygon )
		{
			DebugShape2.hackHide = false;
			
			List<Vector2> selectedDebugPolygonVertices = selectedDebugPolygon.GetWorldVertices();
			
			bool debugPolygonIsConvex = Geometry2.PolygonIsConvex( selectedDebugPolygonVertices );
			
			string convexityLabelText = "Convexity: ";
			if ( debugPolygonIsConvex )
			{
				convexityLabelText += "Convex";
			}
			else
			{
				convexityLabelText += "Concave";
			}
			
			string windingLabelText = "Winding: ";
			if ( Geometry2.PolygonIsCCW( selectedDebugPolygonVertices ) )
			{
				windingLabelText += "CCW";
			}
			else
			{
				windingLabelText += "CW";
			}
			
			GUILayout.BeginVertical();
			
			selectedDebugPolygon.CheckPointsInside = GUILayout.Toggle( selectedDebugPolygon.CheckPointsInside, "Containing (Points)" );
			selectedDebugPolygon.CheckIntersectingSegments = GUILayout.Toggle( selectedDebugPolygon.CheckIntersectingSegments, "Intersecting (Segments)" );
			selectedDebugPolygon.CheckIntersectingPolygons = GUILayout.Toggle( selectedDebugPolygon.CheckIntersectingPolygons, "Intersecting (Polygons)" );
			selectedDebugPolygon.CheckPolygonsInside = GUILayout.Toggle( selectedDebugPolygon.CheckPolygonsInside, "Containing (Polygons)" );			
			
			GUILayout.Label( convexityLabelText );
			GUILayout.Label( windingLabelText );
			bool fuseWithOverlappingPolygons = GUILayout.Button( "Fuse" );
			bool reverseWinding = GUILayout.Button( "Reverse Winding" );
			bool shiftUp = GUILayout.Button( "Shift Up" );
			bool checkForErrors = GUILayout.Button( "Check for Errors" );
			bool centerGizmo = GUILayout.Button( "Center Gizmo" );			
			
			GUILayout.EndVertical();			
			
			if ( reverseWinding )
			{
				selectedDebugPolygon.ReverseVertices();
			}
			
			if ( shiftUp )
			{
				selectedDebugPolygon.ShiftUpVertices();
			}
			
			if ( selectedDebugPolygon.CheckPointsInside )
			{
				DebugPoint2[] debugPoints = FindObjectsOfType( typeof( DebugPoint2 ) ) as DebugPoint2[];
				
				if ( Geometry2.PolygonIsConvex( selectedDebugPolygonVertices ) )
				{				
					foreach ( DebugPoint2 debugPoint in debugPoints )
					{						
						if ( Geometry2.ConvexPolygonContainsPoint( selectedDebugPolygonVertices, debugPoint.GetWorldVertices()[0] ) )
						{
							debugPoint.IsHighlighted = true;
							EditorUtility.SetDirty( debugPoint );
						}
					}
				}
				else
				{
					foreach ( DebugPoint2 debugPoint in debugPoints )
					{						
						if ( Geometry2.PolygonContainsPoint( selectedDebugPolygonVertices, debugPoint.GetWorldVertices()[0] ) )
						{
							debugPoint.IsHighlighted = true;
							EditorUtility.SetDirty( debugPoint );
						}
					}
				}				
			}
			
			if ( selectedDebugPolygon.CheckIntersectingSegments )
			{
				DebugLine2[] debugLines = FindObjectsOfType( typeof( DebugLine2 ) ) as DebugLine2[];
				
				List<Vector2> pointsOfIntersection = new List<Vector2>();
				
				foreach ( DebugLine2 debugLine in debugLines )
				{
					List<Vector2> debugLineVertices = debugLine.GetWorldVertices();
						
					List<Vector2> result = Geometry2.SegmentAgainstPolygon( debugLineVertices[0], debugLineVertices[1], selectedDebugPolygonVertices );
					if ( result.Count > 0 )
					{
						debugLine.IsHighlighted = true;
						EditorUtility.SetDirty( debugLine );
						
						pointsOfIntersection.AddRange( result );
					}
				}
				
				selectedDebugPolygon.PointsOfIntersection.AddRange( pointsOfIntersection );
				EditorUtility.SetDirty( selectedDebugPolygon );
			}
			
			if ( selectedDebugPolygon.CheckIntersectingPolygons )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				List<Vector2> pointsOfIntersection = new List<Vector2>();
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					if ( selectedDebugPolygon != debugPolygon )
					{
						List<Vector2> debugPolygonVertices = debugPolygon.GetWorldVertices();
						
						List<Vector2> result = Geometry2.PolygonAgainstPolygon( selectedDebugPolygonVertices, debugPolygonVertices );
						
						if ( result.Count > 0  )
						{
							debugPolygon.IsHighlighted = true;
							EditorUtility.SetDirty( debugPolygon );
							
							pointsOfIntersection.AddRange( result );
						}
					}
				}
				
				selectedDebugPolygon.PointsOfIntersection.AddRange( pointsOfIntersection );
				EditorUtility.SetDirty( selectedDebugPolygon );
			}
			
			if ( selectedDebugPolygon.CheckPolygonsInside )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					if ( selectedDebugPolygon != debugPolygon )
					{
						List<Vector2> debugPolygonVertices = debugPolygon.GetWorldVertices();
						
						if ( Geometry2.PolygonContainsPolygon( selectedDebugPolygonVertices, debugPolygonVertices ) )
						{
							debugPolygon.IsHighlighted = true;
							EditorUtility.SetDirty( debugPolygon );
						}
					}
				}
			}
			
			if ( fuseWithOverlappingPolygons )
			{
				DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				List< List<Vector2> > polygons = new List< List<Vector2> >( debugPolygons.Length );
				
				foreach ( DebugPolygon2 debugPolygon in debugPolygons )
				{
					polygons.Add( debugPolygon.GetWorldVertices() );
				}
				
				Polygon2Clipper clipper = new Polygon2Clipper();
				
				List< List<Vector2> > result = new List<List<Vector2>>();
				
				if ( polygons.Count == 2 )
				{
					// TODO: clip against all intersecting polygons
					clipper.Reset();
					clipper.AddAdditivePolygon( polygons[0] );
					clipper.AddAdditivePolygon( polygons[1] );
					
					result = clipper.FusePolygons();
				}
				
				GameObject parentObject = new GameObject( "FuseResult" );
				foreach ( List<Vector2> polygon in result )
				{
					GameObject newObject = new GameObject( "Polygon" );
					newObject.AddComponent( "DebugPolygon2" );
					DebugPolygon2 newDebugPolygon = newObject.GetComponent<DebugPolygon2>();
					newDebugPolygon.vertices = polygon;
					
					newObject.transform.parent = parentObject.transform;
				}
				
				/*DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];
				
				List<int> visitedPolygons = new List<int>();
				
				List<Vector2> result = selectedDebugPolygonVertices;
				
				for ( int i = 0; i < debugPolygons.Length; )
				{
					if ( !visitedPolygons.Contains( i ) && ( selectedDebugPolygon != debugPolygons[i] ) )
					{
						List<Vector2> debugPolygonVertices = debugPolygons[i].GetWorldVertices();
							
						if ( Geometry2.PolygonsIntersect( result, debugPolygonVertices ) )
						{
							result = Geometry2.FusePolygons( result, debugPolygonVertices );
							
							visitedPolygons.Add( i );
								
							i = 0;
						}
						else
						{
							++i;
						}
					}
					else
					{
						++i;
					}
				}
				
				GameObject newObject = new GameObject( "FuseResult" );
				newObject.AddComponent( "DebugPolygon2" );
				DebugPolygon2 newDebugPolygon = newObject.GetComponent<DebugPolygon2>();
				newDebugPolygon.vertices = result;*/
			}
			
			if ( checkForErrors )
			{				
				CheckPolygonForErrors( selectedDebugPolygon );
			}
			
			if ( centerGizmo )
			{
				selectedDebugPolygon.CenterGizmo();
			}
		}
		
		protected void CheckPolygonForErrors( DebugPolygon2 debugPolygon )
		{
			bool allClear = true;
			
			List<Vector2> vertices = debugPolygon.vertices;
			List<Vector2> worldVertices = debugPolygon.GetWorldVertices();
			
			// Polygons must have at least three points.
			if ( vertices.Count < 3 )
			{
				Debug.LogWarning( "Polygon [" + debugPolygon + "] doesn't have enough vertices." );
			}
			
			// Check for duplicate vertices.
			for ( int i = 0; i < vertices.Count; ++i )
			{
				for ( int j = i + 1; j < vertices.Count; ++j )
				{
					if ( vertices[i] == vertices[j] )
					{
						Debug.LogWarning( "Polygon [" + debugPolygon + "] has duplicate vertices at" + worldVertices[i] );
						allClear = false;
					}
				}
			}			
			
			// Check for self intersection.
			if ( Geometry2.PolygonsIntersect( worldVertices, worldVertices ) )
			{
				Debug.LogWarning( "Polygon [" + debugPolygon + "] intersects with itself." );
				allClear = false;
			}
			
			if ( allClear )
			{
				Debug.Log( "No problems found for polygon [" + debugPolygon + "]." );
			}
		}
	}
}
