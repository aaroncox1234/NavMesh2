using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
/*
			test cases
				complex boundaries
				complex boundaries complex holes
				overlapping complex everything
				single complex boundary with some holes (no edge case testing, just standard stuff)
				common edge case all in one
			try cutting a triangulated hole into a triangulated polygon (just start with two triangles, move up from there)
			try merging two polygons:
				cut poly1 with poly2's edges
				cut poly2 with poly1's edges
				remove poly1 triangles contained in poly2
				remove poly2 triangles contained in poly1*/

// TODO
// Make edge cases tests
//	Closed boundary
//  Closed hole
//  Degenerate
//		Self intersecting
//		Too small
//		Colinear overlap
//		Intersect through vertices
//  Colinear
namespace Rx
{
	[CustomEditor( typeof( NewNavMesh2 ), true )]
	public class NewNavMesh2Editor : Editor
	{
		#region Data Members
		
		private NavMesh2Builder builder = new NavMesh2Builder();
		
		//private bool hideDebugShapes = false;
		//private bool drawInputBoundaries = false;
		//private bool drawInputHoles = false;
		//private bool drawGroups = false;
		//private bool drawBoundaryTriangles = false;
		//private bool drawHoleTriangles = false;
		
		private bool drawPolygonIndices = false;
		private bool drawVertexIndices = false;
		private bool drawVertexPositions = false;
		
		private int previousBuildStep = -1;
		private int buildStep = 0; 
		
		#endregion
		
		#region Build
		
		public override void OnInspectorGUI()
	    {
			base.OnInspectorGUI();
			
			NewNavMesh2 selectedNavMesh = (NewNavMesh2)target;
	        
			GUILayout.BeginVertical();
			
			//hideDebugShapes = GUILayout.Toggle( hideDebugShapes, "Hide Debug Shapes" );
			//drawInputBoundaries = GUILayout.Toggle( drawInputBoundaries, "Draw Input Boundaries" );
			//drawInputHoles = GUILayout.Toggle( drawInputHoles, "Draw Input Holes" );
			//drawGroups = GUILayout.Toggle( drawGroups, "Draw Groups" );
			//drawBoundaryTriangles = GUILayout.Toggle( drawBoundaryTriangles, "Draw Boundary Triangles" );
			//drawHoleTriangles = GUILayout.Toggle( drawHoleTriangles, "Draw Hole Triangles" );
			
			drawPolygonIndices = GUILayout.Toggle( drawPolygonIndices, "Draw Polygon Indices" );
			drawVertexIndices = GUILayout.Toggle( drawVertexIndices, "Draw Vertex Indices" );
			drawVertexPositions = GUILayout.Toggle( drawVertexPositions, "Draw Vertex Positions" );
			
			buildStep = (int)GUILayout.HorizontalSlider( (float)buildStep, 0.0f, 10.0f );
			switch ( buildStep )
			{
				case 0:
				{
					GUILayout.Label( "Build Step: None" );
				}
				break;
				
				case 1:
				{
					GUILayout.Label( "Build Step: Fuse Boundaries" );
				}
				break;
				
				case 2:
				{
					GUILayout.Label( "Build Step: Fuse Holes" );
				}
				break;
				
				case 3:
				{
					GUILayout.Label( "Build Step: Clip Holes" );
				}
				break;
				
				case 4:
				{
					GUILayout.Label( "Build Step: Generate Convex" );
				}
				break;
			}
			
			bool build = GUILayout.Button( "Build" );
			
			GUILayout.EndVertical();
			
			EditorUtility.SetDirty( selectedNavMesh );
			
			if ( build || buildStep != previousBuildStep )
			{
				Build();
			}
			
			DebugShape2.hackHide = true;
		}
		
//		private void Build()
//		{
//			builder.Clear();
//			
//			CollectBoundaries();
//			CollectHoles();
//			
//			builder.Build();
//		}
		
		private void CollectBoundaries()
		{
			DebugBoundary2[] boundaries = Object.FindObjectsOfType( typeof(DebugBoundary2) ) as DebugBoundary2[];
			
			foreach ( DebugBoundary2 boundary in boundaries )
			{
				builder.AddBoundary( boundary.GetWorldVertices() );
			}
		}
		
		private void CollectHoles()
		{
			DebugHole2[] holes = Object.FindObjectsOfType( typeof(DebugHole2) ) as DebugHole2[];
			
			foreach ( DebugHole2 hole in holes )
			{
				builder.AddHole( hole.GetWorldVertices() );
			}
		}
		
		private void Build()
		{
			builder.Clear();
			
			CollectBoundaries();
			CollectHoles();
			
			builder.Build( buildStep );
		}
		
		#endregion
		
		#region Debug Drawing
		
		public void OnSceneGUI()
		{
			Draw();
			
//			if ( drawBoundaryTriangles )
//			{
//				DrawBoundaryTriangles();
//			}
//			
//			if ( drawHoleTriangles )
//			{
//				DrawHoleTriangles();
//			}
//			
//			if ( drawInputBoundaries )
//			{
//				DrawInputBoundaries();
//			}
//			
//			if ( drawInputHoles )
//			{
//				DrawInputHoles();
//			}
//			
//			if ( drawGroups )
//			{
//				DrawGroups();
//			}
	    }
		
		private void Draw()
		{
			if ( buildStep == 0 )
			{
				DrawPolygons( builder.boundaries, Color.white );
				DrawPolygons( builder.holes, Color.blue );
			}
			else if ( buildStep == 1 )
			{
				DrawPolygons( builder.boundaries, Color.white );
				DrawPolygons( builder.holes, Color.blue );
			}
			else if ( buildStep == 2 )
			{
				DrawPolygons( builder.boundaries, Color.white );
				DrawPolygons( builder.holes, Color.blue );
			}
			else if ( buildStep == 3 )
			{
				DrawPolygons( builder.boundaries, Color.white );
				DrawPolygons( builder.holes, Color.blue );
			}
		}
		
		private void DrawPolygons( List<NavMesh2Builder.PolygonProcessingInfo> polygons, Color color )
		{		
			int polygonIndex = 0;
			
			foreach ( NavMesh2Builder.PolygonProcessingInfo polygon in polygons )
			{
				DrawPolygon( polygon.vertices, color, polygonIndex );
				
				++polygonIndex;
			}
		}
		
//		private void DrawInputBoundaries()
//		{
//			foreach ( List<Vector2> polygon in builder.inputBoundaries )
//			{
//				DrawPolygon( polygon, Color.white );
//			}
//		}
//		
//		private void DrawInputHoles()
//		{
//			foreach ( List<Vector2> polygon in builder.inputHoles )
//			{
//				DrawPolygon( polygon, Color.black );
//			}
//		}
//		
//		private void DrawGroups()
//		{
//			if ( ( builder.meshData == null ) || ( builder.meshData.Count == 0 ) )
//			{
//				return;
//			}
//			
//			Color boundaryColor = new Color( 0.0f, 1.0f, 0.0f, 0.2f );
//			Color holeColor = new Color( 0.0f, 0.0f, 1.0f, 0.2f );
//			
//			float colorStep = 0.8f / builder.meshData.Count;
//			
//			foreach ( NavMesh2Builder.MeshProcessingInfo mesh in builder.meshData )
//			{
//				foreach ( List<Vector2> boundary in mesh.boundaries )
//				{
//					DrawPolygon( boundary, boundaryColor );
//				}
//				
//				foreach ( List<Vector2> hole in mesh.holes )
//				{
//					DrawPolygon( hole, holeColor );
//				}
//				
//				boundaryColor.a += colorStep;
//				holeColor.a += colorStep;
//			}
//		}
//		
//		private void DrawBoundaryTriangles()
//		{			
//			foreach ( NavMesh2Builder.MeshProcessingInfo meshInfo in builder.meshData )
//			{
//				if ( meshInfo.boundaries.Count != meshInfo.triangulatedBoundaries.Count )
//				{
//					Debug.LogError( "Encountered a MeshProcessingInfo with " + meshInfo.boundaries.Count + " boundaries and " + meshInfo.triangulatedBoundaries.Count + " triangulated boundaries." );
//				}
//				
//				for ( int boundaryIndex = 0; boundaryIndex < meshInfo.boundaries.Count; ++boundaryIndex )
//				{
//					DrawIndexedTriangle( meshInfo.boundaries[boundaryIndex], meshInfo.triangulatedBoundaries[boundaryIndex], Color.cyan );
//				}
//			}
//		}
//		
//		private void DrawHoleTriangles()
//		{			
//			foreach ( NavMesh2Builder.MeshProcessingInfo meshInfo in builder.meshData )
//			{
//				if ( meshInfo.holes.Count != meshInfo.triangulatedHoles.Count )
//				{
//					Debug.LogError( "Encountered a MeshProcessingInfo with " + meshInfo.holes.Count + " holes and " + meshInfo.triangulatedHoles.Count + " triangulated holes." );
//				}
//				
//				for ( int holeIndex = 0; holeIndex < meshInfo.holes.Count; ++holeIndex )
//				{
//					DrawIndexedTriangle( meshInfo.holes[holeIndex], meshInfo.triangulatedHoles[holeIndex], Color.blue );
//				}
//			}
//		}
//		
//		private void DrawIndexedTriangle( List<Vector2> vertices, List<int> triangles, Color color )
//		{
//			for ( int a = 0; a < triangles.Count; a += 3 )
//			{
//				int b = a + 1;
//				int c = a + 2;
//				
//				DrawLine( vertices[ triangles[a] ], vertices[ triangles[b] ], color );
//				DrawLine( vertices[ triangles[b] ], vertices[ triangles[c] ], color );
//				DrawLine( vertices[ triangles[c] ], vertices[ triangles[a] ], color );
//			}
//		}
		
		private void DrawPolygon( List<Vector2> polygon, Color color, int polygonIndex )
		{
			for ( int edgeStartIndex = 0; edgeStartIndex < polygon.Count; ++edgeStartIndex )
			{
				string vertString = "";
				
				if ( drawPolygonIndices )
				{
					vertString += polygonIndex;
				}
				
				if ( vertString.Length > 0 )
				{
					vertString += ":";
				}
				
				if ( drawVertexIndices )
				{
					vertString += edgeStartIndex;
				}
				
				if ( vertString.Length > 0 )
				{
					vertString += ":";
				}
				
				if ( drawVertexPositions )
				{
					vertString += polygon[edgeStartIndex].x + "," + polygon[edgeStartIndex].y;
				}
				
				Handles.Label( polygon[edgeStartIndex], vertString );
				
				int edgeEndIndex = (edgeStartIndex+1) % polygon.Count;
				
				DrawLine( polygon[edgeStartIndex], polygon[edgeEndIndex], color );
			}
		}
		
		private void DrawLine( Vector2 start, Vector2 end, Color color )
		{
			Handles.color = color;
			
			Handles.DrawLine( new Vector3( start.x, start.y, 0.0f ), new Vector3( end.x, end.y, 0.0f ) );
		}
		
		#endregion
	}
}
