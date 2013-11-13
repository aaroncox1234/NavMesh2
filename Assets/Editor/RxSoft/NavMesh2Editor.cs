using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/* TODO

--------
Next Day
--------
 
Option to not store each stage of data in NavMesh2Builder. In fact, avoid duplication of data entirely ( after profiling on large data set ).
 
------------
Debug Shapes
------------		
	
RxAI::RxDebugPolygon2
(Editor)
	Integrate with Holes [Button]
	Triangulate with Holes [Button]
	Decompose to Convex [Button]
	Decompose to Convex with Holes [Button]
 
Task: Mark polygons as holes and boundaries. If a hole intersects a boundary, subtract the hole rather than adding it.
Task: Investigate more efficient way to find intersecting segments when there are a lot of segments
Task: Change most tests to take lists instead of assuming one-on-one tests
Task: Create a scene containing various test cases
Task: Try to replace EditablePolygon2 with RxAI::Polygon2Editor
Task: Option to center pivot of a RxAI::Polygon2
Task: Use better input for adding and removing vertices
Task: Get a C# project running and profile (see MonoGame)
 
------------------
NavMesh Generation
------------------
 
Workflow
	Place GameObjects with RxNavMesh2Boundary.
	Place a GameObject for the NavMesh. Give it a NavMesh component. Use the GameObject position as a seed point for the navmesh.
	Click Generate
	
Generation (during debugging, the editor should choose how far to proceed and should be able to draw the result of each step)
	Error Checking
		Error out if any degenerate polygon is discovered
	Boundary
		Find the polygon that contains the seed point. Error out if contained by multiple polygons.
		Fuse with any polygons that intersect
		Force winding to be CCW
	Holes
		Find every polygon contained by the boundary
		Fuse any holes that intersect
		Force winding of every hole to be CCW
	BoundaryWithHoles
		Integrate with holes
	Triangulation
		Triangle the result of intergrate the holes with the boundary
	Decomposition
		Reduce the triangles down to a near minimal set of polygons
	Linking
		Generate a graph of nodes and links	
 
RxAI::NavMesh2Shape
	GetPolygon()
	
RxAI::EditableNavMesh2Shape
	Polygon2
	
Task: Find best representation for triangle mesh and polygon mesh
Task: Draw debug of RxAI::NavMesh2 in game
Task: Test CW polygons throughout the process. Reverse when needed.
 
-------------
House Keeping
-------------

Make sure everything is in the RxAI namespace
Track down all TODOs in code
Find way to have members saved without declaring them public
Support undo history for all operations
Get rid of AsVector3 and AsVector2
Allow user to specify a plane to project vertices onto instead of assuming x,y plane
Remove all the unused bottom functions in Geometry2
Test a hole that intersects two boundaries
Test two boundaries that fuse to make holes

-------------
Optimization
-------------

Need fastest point in polygon test.
Need fastest ray against segments test.
Profile outside of Unity.
*/

/*
NavMesh2Node
{
	List<int> vertexIndices; // consider just storing the polygon here
	
	List<NavMesh2Node> adjacentNodes;
}

NavMesh2
{
	List<Vector2> vertices;

	List<NavMesh2Node> nodes;
}

for ( int i = 0; i < nodes.Count; ++i )
{
	for ( int j = i + 1; j < nodes.Count; j++ )
	{
		
	}
}

*/

/* TODO
 
Just have editor place a bunch of EditablePolygon objects
Draw a circle or something to show the location of the NavMesh2 seed point
Find the polygons that contain the seed point, error out if there are multiple

Generalize the concept of a polygon to a component that feeds in polygon data (so anything can be a navmesh polygon) 
 

Try to use List for everything
   
make sure polygons are the correct winding
make sure polygons aren't degenerate. 
show error when building if they do.
 
Find way to remove need for data to be public to be saved

Show list of names of intersecting polygons (in the scene view or in the GUI)
 
Change polygon editor over to use OnSceneGUI. Remove EditablePolygon completely.
Support undo for polygon and navmesh operations. 

Be able to specify a plane to draw the navmesh onto. Transform this plane to x,y plane to do calculations.
OR
Use 3D for everything (hmmmm)


Create a Mesh class that stores Triangle and Polygon meshes (list of vertices and list of indicies into that list)
 

Avoid slowdown with NavMesh2Editor. Shouldn't go hunting and error checking every time the user clicks the thing. Maybe have an update button.

 
Validate winding of boundary and holes. Error and draw red if bad. Also check intersecting polygons.

Hunt down casts from 2d to 3d and vice versa

Expose each step of the pipeline to the Settings window

Option to center pivot

Convert boundary into a simple polygon, instead of storing it as an editable polygon (i.e. copy it)
Store mesh as list of Vector2 (vertices) and int (3 per triangle)
Remove Vertex2d and Triangle2d
Debug draw navmesh in game
Add window to generate navmesh, etc, instead of silly toggles
Create TestLine, TestTriangle, TestPoint. Have them take pointers to different types of things they can interact with.
Look into what to do about accessing List by index. Might be faster to be regrowing an array instead. Profile!
Investigate choose the "best" boundary and hole vertices (will it matter after reduction?)
Investigate faster way to find hole vertex. When testing lines against polygon, try testing against the whole poly (there might be an optimization for this)
move all to rx dir

improve keys for polygon editor
 
selecting holes is wonky. sometimes can't select or drag vertices
do something about not being able to edit properly in perspective

Clear out TODOs 
Document code and workflow

Re-investigate generating a Unity mesh.

Rename triangle2d and geometry2d

Warn about orphaned holes

Once the 2d stuff comes out, compared how it works vs all this stupid code that assumes X,Y plane

 
THEN
Simplification
Search
*/

using Rx;

[CustomEditor( typeof(NavMesh2) )]
public class NavMesh2Editor : Editor 
{
	protected NavMesh2 targetNavMesh = null;
	
	protected NavMesh2Boundary boundary = null;
	
	protected List<NavMesh2Hole> holes = new List<NavMesh2Hole>();
	
	protected List<Polygon2> polygonsIntersectingBoundary = new List<Polygon2>();
	
	protected Polygon2 boundaryWithHoles = null;
	
	protected List<Triangle2d> triangles = null;
	
	protected List<Polygon2> polygons = null;
	
	protected bool drawBoundary = true;
	protected bool drawHoles = true;
	protected bool drawTriangles = true;
	protected bool drawPolygons = true;
	
	protected bool buildClicked = false;
 
    public void OnEnable()
    {
        targetNavMesh = (NavMesh2)target;
		
		UpdateData();
    }
		
	public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();
		
		GUILayout.BeginVertical();
		
		GUILayout.Label( "Visualization", EditorStyles.boldLabel );
		drawBoundary = EditorGUILayout.Toggle( "Draw Boundary", drawBoundary );
		drawHoles = EditorGUILayout.Toggle( "Draw Holes", drawHoles );
		drawTriangles = EditorGUILayout.Toggle( "Draw Triangles", drawTriangles );
		drawPolygons = EditorGUILayout.Toggle( "Draw Polygons", drawPolygons );

		buildClicked = GUILayout.Button( "Build" );
		
		GUILayout.EndVertical();
		
		if ( GUI.changed )
		{
        	EditorUtility.SetDirty( target );
		}
	}
	
	public void OnSceneGUI()
	{		
		if ( drawBoundary )
		{
			DrawBoundary();
		}
		
		if ( drawHoles )
		{
			DrawHoles();	
		}
		
		if ( drawTriangles )
		{
			DrawTriangles();
		}
		
		if ( drawPolygons )
		{
			DrawPolygons();
		}
		
		DrawInvalidGeometry();
	}
	
	protected void UpdateData()
	{
		UpdateBoundary();
		
		TestAgainstOtherBoundaries();
		
		UpdateHoles();		
		
		TestHolesAgainstEachOther();
		
		IntegrateHolesWithBoundary();
		
		GenerateTriangles();
	}
	
	protected void UpdateBoundary()
	{
		boundary = targetNavMesh.boundary;
	}
	
	protected void TestAgainstOtherBoundaries()
	{
		if ( boundary != null )
		{
			Polygon2 boundaryAsPolygon = boundary.GetCopyOfPolygon();
			
			NavMesh2Boundary[] foundBoundaries = Object.FindObjectsOfType( typeof(NavMesh2Boundary) ) as NavMesh2Boundary[];
			
			foreach ( NavMesh2Boundary foundBoundary in foundBoundaries )
			{
				Polygon2 foundBoundaryAsPolygon = foundBoundary.GetCopyOfPolygon();
				
				if ( Geometry2.PolygonIntersectsPolygon( boundaryAsPolygon, foundBoundaryAsPolygon ) )
				{
					polygonsIntersectingBoundary.Add( foundBoundaryAsPolygon);
				}
			}
		}
	}
	
	// TODO: Check out http://alienryderflex.com/polygon/ for an efficient point in polygon test that doesn't require triangulation.
	protected void UpdateHoles()
	{
		if ( boundary != null )
		{
			holes.Clear();
		
			NavMesh2Hole[] foundHoles = Object.FindObjectsOfType( typeof(NavMesh2Hole) ) as NavMesh2Hole[];
			
			// Triangulate the boundary. The triangles will be used for containment tests of the holes.

			Polygon2 boundaryPolygon = boundary.GetCopyOfPolygon();
			
			List<Triangle2d> triangles = null;
			Geometry2.TriangulatePolygonX( boundaryPolygon.Vertices, out triangles );
	
			foreach ( NavMesh2Hole foundHole in foundHoles )
			{
				Polygon2 foundHoleAsPolygon = foundHole.GetCopyOfPolygon();
				
				if ( Geometry2.PolygonIntersectsPolygon( boundaryPolygon, foundHoleAsPolygon ) )
				{
					polygonsIntersectingBoundary.Add( foundHoleAsPolygon );
				}
				else if ( Geometry2.TrianglesContainPoints( triangles, foundHoleAsPolygon.Vertices ) )
				{
					holes.Add( foundHole );
				}
			}
		}
	}
	
	// TODO: Make this function not suck so bad.
	protected void TestHolesAgainstEachOther()
	{
		bool problemFound = false;
		
		// TODO: rename!!!
		foreach ( NavMesh2Hole outerHole in holes )
		{
			Polygon2 outerPolygon = outerHole.GetCopyOfPolygon();
			
			// TODO: Remove dumb conversion.
			outerPolygon.ReverseWinding();
			
			// TODO: This is horrible
			List<Triangle2d> triangles = null;
			Geometry2.TriangulatePolygonX( outerPolygon.Vertices, out triangles );
			
			// TODO: We have to run through every hole again for containment, but we shouldn't for intersection. OR we should make it so we don't have to for either...
			foreach ( NavMesh2Hole innerHole in holes )
			{
				Polygon2 innerPolygon = innerHole.GetCopyOfPolygon();
				
				if ( ( outerHole != innerHole ) &&
					 ( Geometry2.PolygonIntersectsPolygon( innerPolygon, outerPolygon ) ||
					   Geometry2.TrianglesContainPoints( triangles, innerPolygon.Vertices ) ) )
				{
					// TODO: See about dealing with iterators properly, instead of just bailing and recursing.
					polygonsIntersectingBoundary.Add( outerPolygon );
					polygonsIntersectingBoundary.Add( innerPolygon );
					
					holes.Remove( outerHole );
					holes.Remove( innerHole );
						
					problemFound = true;
					
					break;
				}
			}
			
			if ( problemFound )
			{
				break;
			}
		}
		
		// Keep recursing until no problem is found.
		if ( problemFound )
		{
			TestHolesAgainstEachOther();
		}
	}
	
	protected void IntegrateHolesWithBoundary()
	{
		if ( boundary != null )
		{			
			boundaryWithHoles = boundary.GetCopyOfPolygon();
			
			foreach ( NavMesh2Hole hole in holes )
			{
				IntegrateHoleWithBoundary( hole.GetCopyOfPolygon() );
			}
		}
	}
	
	protected void IntegrateHoleWithBoundary( Polygon2 hole )
	{
		int index = 0;
		int insertIndex = -1;
		
		foreach ( Vector2 boundaryVertex in boundaryWithHoles.Vertices )
		{
			// HACK: Avoid detecting intersection for overlapping vertices. At the very least, move this logic or equivalent to Geometry2 and make it optional.
			// 		 Investigate better solutions when also investigating optimizing for line vs segments instead of line vs segment in polygon test.
			Vector2 testLineStart = hole.Vertices[0];
			Vector2 testLineEnd = boundaryVertex;
			Vector2 lineDir = ( testLineEnd - testLineStart ).normalized;
			testLineStart += lineDir * 0.0001f;
			testLineEnd -= lineDir * 0.0001f;
			
			if ( !Geometry2.SegmentIntersectsPolygon( testLineStart, testLineEnd, boundaryWithHoles ) &&
				 !Geometry2.SegmentIntersectsPolygon( testLineStart, testLineEnd, hole ) )
			{
				insertIndex = index;
				break;
			}
			
			++index;
		}
		
		if ( insertIndex == -1 )
		{
			// TODO: log which hole and the name of the navmesh.
			Debug.LogError( "Failed to find vertex on boundary to connect to hole." );
		}
		
		List<Vector2> verticesToInsert = new List<Vector2>();
		foreach ( Vector2 holeVertex in hole.Vertices )
		{
			verticesToInsert.Add( holeVertex );
		}		
		verticesToInsert.Add( hole.Vertices[0] );
		verticesToInsert.Add( boundaryWithHoles.Vertices[insertIndex] );
		
		boundaryWithHoles.InsertVertices( insertIndex + 1, verticesToInsert );
	}
	
	protected void GenerateTriangles()
	{
		if ( boundaryWithHoles != null )
		{
			Geometry2.TriangulatePolygonX( boundaryWithHoles.Vertices, out triangles );
		}
	}
	
	protected void DrawBoundary()
	{
		if ( boundary != null )
		{
			DrawPolygon( boundary.GetCopyOfPolygon(), Color.white );
		}
	}
	
	protected void DrawHoles()
	{
		foreach ( NavMesh2Hole hole in holes )
		{
			DrawPolygon( hole.GetCopyOfPolygon(), Color.blue );
		}
	}
	
	protected void DrawInvalidGeometry()
	{
		foreach ( Polygon2 polygon in polygonsIntersectingBoundary )
		{
			DrawPolygon( polygon, Color.red );
		}
	}
	
	protected void DrawTriangles()
	{
		if ( triangles != null )
		{
			foreach ( Triangle2d triangle in triangles )
			{
				Vector3 a = Helpers.AsVector3( triangle.a, targetNavMesh.transform.position.z );
				Vector3 b = Helpers.AsVector3( triangle.b, targetNavMesh.transform.position.z );
				Vector3 c = Helpers.AsVector3( triangle.c, targetNavMesh.transform.position.z );
				
				Handles.color = Color.grey;
				Handles.DrawLine( a, b );
				Handles.DrawLine( b, c );
				Handles.DrawLine( c, a );
			}
		}
	}
	
	protected void DrawPolygons()
	{
		if ( polygons != null )
		{
			foreach ( Polygon2 polygon in polygons )
			{
				DrawPolygon( polygon, Color.green );
			}
		}
	}
	
	protected void DrawPolygon( Polygon2 polygon, Color color )
	{
		Vector3[] drawArray = new Vector3[ polygon.Vertices.Count + 1 ];
		
		for ( int index = 0; index < polygon.Vertices.Count; ++index )
		{
			drawArray[index] = Helpers.AsVector3( polygon.Vertices[index], targetNavMesh.transform.position.z );
		}
		
		drawArray[ drawArray.Length - 1 ] = Helpers.AsVector3( polygon.Vertices[0], targetNavMesh.transform.position.z );
		
		Handles.color = color;
		Handles.DrawPolyLine( drawArray );
	}
}
