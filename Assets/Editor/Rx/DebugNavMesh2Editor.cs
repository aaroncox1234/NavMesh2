using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Rx
{
	[CustomEditor( typeof( DebugNavMesh2 ), true )]
	public class DebugNavMesh2Editor : Editor
	{
		// TODO: Only generate and store this once.
		List< List<Vector2> > polygons = null;
		
		// TODO: Fix the pipeline
		List< List<Vector2> > holes = null;
		
		public override void OnInspectorGUI()
	    {
			base.OnInspectorGUI();
			
			DebugNavMesh2 selectedNavMesh = (DebugNavMesh2)target;
	        
			GUILayout.BeginVertical();
			
			selectedNavMesh.ShowBoundary = GUILayout.Toggle( selectedNavMesh.ShowBoundary, "Show Boundary" );
			selectedNavMesh.ShowTriangles = GUILayout.Toggle( selectedNavMesh.ShowTriangles, "Show Triangles" );
			selectedNavMesh.ShowDiagonals = GUILayout.Toggle( selectedNavMesh.ShowDiagonals, "Show Diagonals" );
			selectedNavMesh.ShowMesh = GUILayout.Toggle( selectedNavMesh.ShowMesh, "Show Mesh" );
			
			bool update = GUILayout.Button( "Update" );
			
			GUILayout.EndVertical();
			
			EditorUtility.SetDirty( selectedNavMesh );
			
			if ( update )
			{
				selectedNavMesh.Boundary = GenerateBoundary( selectedNavMesh.transform.position );
				selectedNavMesh.Triangles = Geometry2.TriangulatePolygon( selectedNavMesh.Boundary );
				selectedNavMesh.mesh = Mesh2.BuildFromTriangles( selectedNavMesh.Boundary, selectedNavMesh.Triangles );
			}
		}
		
		private List<Vector2> GenerateBoundary( Vector2 seedPoint )
		{
			BuildPolygonList();
			FuseOverlappingPolygons();
			
			List<Vector2> boundary = FindContainingPolygon( seedPoint );
			
			if ( boundary == null )
			{
				return new List<Vector2>();
			}
			
			FindHoles( boundary );
			
			IntegrateWithHoles( boundary );
			
			return boundary;
		}
		
		private void BuildPolygonList()
		{
			polygons = new List< List<Vector2> >();
			
			DebugPolygon2[] debugPolygons = FindObjectsOfType( typeof( DebugPolygon2 ) ) as DebugPolygon2[];				
			foreach ( DebugPolygon2 debugPolygon in debugPolygons )
			{
				polygons.Add( debugPolygon.GetWorldVertices() );
			}
		}
		
		// TODO: This is sloooow. A broad phase might help.
		// TODO: Put this into a standalone (just feed it a list of polygons) and profile some optimizations.
		// TODO: Just feed in a list of polygons and seed points and let the polygon system consume.
		// Build a list of every polygon in the world. Fuse overlapping polygons.
		private void FuseOverlappingPolygons()
		{			
			// TODO: This is slow. Every time we find an intersection, we go and test every polygon again. Might be better to generate a set of intersections
			// and merge that way. PROFILE.
			int currentPolygonIndex = 0;
			while ( currentPolygonIndex < polygons.Count - 1 )
			{
				List<Vector2> currentPolygon = polygons[currentPolygonIndex];
				
				int intersectingPolygonIndex = -1;
				for ( int candidatePolygonIndex = currentPolygonIndex + 1; candidatePolygonIndex < polygons.Count; ++candidatePolygonIndex )
				{
					if ( Geometry2.PolygonsIntersect( currentPolygon, polygons[candidatePolygonIndex] ) )
					{
						intersectingPolygonIndex = candidatePolygonIndex;
						break;
					}
				}
				
				if ( intersectingPolygonIndex == -1 )
				{
					++currentPolygonIndex;
				}
				else
				{
					polygons[currentPolygonIndex] = Geometry2.FusePolygons( polygons[currentPolygonIndex], polygons[intersectingPolygonIndex] );
					polygons.RemoveAt( intersectingPolygonIndex );
					
					// Start all over, since the new polygon might intersect a previously checked polygon.
					currentPolygonIndex = 0;
				}
			}
		}
		
		// Find the first containing polygon. TODO: might need to be more sophisticated.
		private List<Vector2> FindContainingPolygon( Vector2 position )
		{
			foreach ( List<Vector2> polygon in polygons )
			{
				if ( Geometry2.PolygonContainsPoint( polygon, position ) )
				{
					return polygon;
				}
			}
			
			return null;
		}
		
		private void FindHoles( List<Vector2> boundary )
		{
			holes = new List< List<Vector2> >();
			
			foreach ( List<Vector2> polygon in polygons )
			{
				if ( ( polygon != boundary ) && ( Geometry2.PolygonContainsPolygon( boundary, polygon ) ) )
				{
					holes.Add( polygon );
				}
			}
		}
		
		// TODO: test a hole completely blocked off by other holes
		private void IntegrateWithHoles( List<Vector2> boundary )
		{
			// TODO: enforce winding at a higher level than this. 
			if ( !Geometry2.PolygonIsCCW( boundary ) )
			{
				boundary.Reverse();
			}
			
			foreach ( List<Vector2> hole in holes )
			{
				if ( Geometry2.PolygonIsCCW( hole ) )
				{
					hole.Reverse();
				}
			}
			
			while ( holes.Count > 0 )
			{
				int holeIndex = -1;
				int boundaryVertexIndex = -1;
				int holeVertexIndex = -1;
				
				bool result = FindHoleToIntegrate( boundary, out holeIndex, out boundaryVertexIndex, out holeVertexIndex );
				
				// This should never happen.
				if ( !result )
				{
					// TODO: Make this sound more professional.
					Debug.LogError( "Somehow, all holes are blocked. This should be impossible." );
				}
				else
				{
					List<Vector2> verticesToInsert = new List<Vector2>();
					foreach ( Vector2 holeVertex in holes[holeIndex] )
					{
						verticesToInsert.Add( holeVertex );
					}		
					// TODO: document
					verticesToInsert.Add( holes[holeIndex][holeVertexIndex] );
					verticesToInsert.Add( boundary[boundaryVertexIndex] );
					
					boundary.InsertRange( boundaryVertexIndex + 1, verticesToInsert );
					
					holes.RemoveAt( holeIndex );
				}				
			}
		}
		
		// Find an edge from a vertex on the boundary to a vertex on the hole that does not intersect with the boundary
		/// or any hole.
		// Some holes may be initially obstructed by other holes. TODO fix this comment
		private bool FindHoleToIntegrate( List<Vector2> boundary, out int holeIndex, out int boundaryVertexIndex, out int holeVertexIndex )
		{
			List<Vector2> hole = null;
				
			for ( holeIndex = 0; holeIndex < holes.Count; ++holeIndex )
			{
				hole = holes[holeIndex];
				
				for ( boundaryVertexIndex = 0; boundaryVertexIndex < boundary.Count; ++boundaryVertexIndex )
				{
					for ( holeVertexIndex = 0; holeVertexIndex < hole.Count; ++holeVertexIndex )
					{
						List<Vector2> boundaryIntersectionPoints = Geometry2.SegmentAgainstPolygon( boundary[boundaryVertexIndex], hole[holeVertexIndex], boundary );
						if ( boundaryIntersectionPoints.Count == 0 )
						{
							// TODO: explain why this is this
							bool holeIntersection = false;
							for ( int otherHoleIndex = 0; otherHoleIndex < holes.Count; ++otherHoleIndex )
							{
								List<Vector2> holeIntersectionPoints = Geometry2.SegmentAgainstPolygon( boundary[boundaryVertexIndex], hole[holeVertexIndex], holes[otherHoleIndex] );
								if ( holeIntersectionPoints.Count > 0 )
								{
									holeIntersection = true;
									break;
								}
							}
							
							if ( !holeIntersection )
							{
								return true;
							}
						}
					}
				}
			}
			
			holeIndex = -1;
			boundaryVertexIndex = -1;
			holeVertexIndex = -1;
			return false;
		}
	}

}
