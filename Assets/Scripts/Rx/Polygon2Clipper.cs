using UnityEngine;
using System.Collections.Generic;

// TODO: clean up to differentiate Merge and Clip. Also change terminology to use Merge and Clip (no additive, subtractive)

namespace Rx
{
	// TODO: try broadphases all over the place
	// TODO: Test to see how many times GeneratePolygon gets called. It seems to get called more than expected.
	// TODO: document me. i take in stuff and make copy of stuff.
	// TODO: should we do stuff in place??? must profile first...
	// http://davis.wpi.edu/~matt/courses/clipping/
	public class Polygon2Clipper
	{
		#region Types
		
		private enum IntersectionInfo
		{
			None,
			Entering,
			Exiting
		};
		
		private class PolygonProcessingInfo
		{
			public VertexProcessingInfo vertexListHead;
			
			public List<Vector2> vertices;
			
			public bool isIntersecting;	// intersecting with anything
			
			public PolygonProcessingInfo( List<Vector2> vertices )
			{
				this.vertexListHead = null;
				this.vertices = vertices;
				this.isIntersecting = false;
			}
		}
		
		private class VertexProcessingInfo
		{
			public Vector2 position;			
			public bool isInside;						// True if this vertex is inside another polygon
			public bool isVisited; 
			public bool isIntersectionPoint;
			public bool isEntering;						// Only valid when isIntersectionPoint is true.
			public bool isSubtractive;	// TODO: try moving this to PolygonProcessingInfo
			
			// If an intersection point, this points to the duplicate intersection point made in the other polygon. Null otherwise.
			public VertexProcessingInfo neighbor;
			
			public VertexProcessingInfo prev;
			public VertexProcessingInfo next;
			
			public VertexProcessingInfo( Vector2 position, bool isIntersectionPoint, bool isSubtractive)
			{
				this.position = position;
				this.isInside = false;
				this.isVisited = false;
				this.isIntersectionPoint = isIntersectionPoint;
				this.isEntering = false;
				this.isSubtractive = isSubtractive;
				
				this.neighbor = null;
				
				this.prev = null;
				this.next = null;
			}
		}
		
		#endregion
		
		#region Data Members
			
		private List<PolygonProcessingInfo> additivePolygonInfos;
		private List<PolygonProcessingInfo> subtractivePolygonInfos;
		
		#endregion
		
		#region Initialization
		
		public Polygon2Clipper()
		{			
			additivePolygonInfos = new List<PolygonProcessingInfo>();
			subtractivePolygonInfos = new List<PolygonProcessingInfo>();
		}
		
		public void Reset()
		{
			additivePolygonInfos.Clear();
			subtractivePolygonInfos.Clear();
		}
		
		public void AddAdditivePolygon( List<Vector2> polygon )
		{
			additivePolygonInfos.Add( new PolygonProcessingInfo( polygon ) );
		}

		public void AddSubtractivePolygon( List<Vector2> polygon )
		{
			subtractivePolygonInfos.Add( new PolygonProcessingInfo( polygon ) );
		}
		
		#endregion
		
		#region Processing
		
		// TODO: why does this get called multiple times?
		public List< List<Vector2> > FusePolygons()
		{
			ForceWinding();
			
			CreateProcessingInfoLists( additivePolygonInfos, false );
			//UpdateProcessingInfoListsWithContainment( additivePolygonInfos );
			UpdateProcessingInfoListsWithIntersections( additivePolygonInfos );
			
			//WriteDebugOutput(); 
			
			List< List<Vector2> > result = Fuse();
			
			// Add polygons that aren't intersecting with anything.
			foreach ( PolygonProcessingInfo polygonInfo in additivePolygonInfos )
			{
				if ( !polygonInfo.isIntersecting )
				{
					result.Add( polygonInfo.vertices );
				}
			}
			
			return result;
		}
		
		public List< List<Vector2> > ClipPolygons()
		{
			ForceWinding();
			
			// TODO: rename the hell out of this stuff
			CreateProcessingInfoLists( additivePolygonInfos, false );
			CreateProcessingInfoLists( subtractivePolygonInfos, true );
			//UpdateProcessingInfoListsWithContainment( additivePolygonInfos );	// We don't consider subtractive polygons for containment			
			//UpdateProcessingInfoListsWithIntersections( additivePolygonInfos ); // assume additive polygons have already been merged (change this in the future)
			//UpdateProcessingInfoListsWithIntersections( subtractivePolygonInfos ); // assume subtractive polygons have already been merged (change this in the future)
			UpdateProcessingInfoListsWithIntersections( additivePolygonInfos, subtractivePolygonInfos );
		
			//WriteDebugOutput();
			
			
			List< List<Vector2> > result = Fuse(); // TODO
			
			// Add polygons that aren't intersecting with anything.
			foreach ( PolygonProcessingInfo polygonInfo in additivePolygonInfos )
			{
				if ( !polygonInfo.isIntersecting )
				{
					result.Add( polygonInfo.vertices );
				}
			}
			
			return result;
		}
		//todo: after clipping, keep holes that don't intersect with anything
		private void ForceWinding()
		{
			foreach ( PolygonProcessingInfo polygonInfo in additivePolygonInfos )
			{
				if ( !Geometry2.PolygonIsCCW( polygonInfo.vertices ) )
				{
					polygonInfo.vertices.Reverse();
				}
			}
			
			foreach ( PolygonProcessingInfo polygonInfo in subtractivePolygonInfos )
			{
				if ( Geometry2.PolygonIsCCW( polygonInfo.vertices ) )
				{
					polygonInfo.vertices.Reverse();
				}
			}
		}
		
		private void CreateProcessingInfoLists( List<PolygonProcessingInfo> polygonInfos, bool isSubtractive )
		{
			foreach ( PolygonProcessingInfo polygonInfo in polygonInfos )
			{
				polygonInfo.vertexListHead = InitializeProcessingInfo( polygonInfo.vertices, isSubtractive );
			}
		}
			
		// todo: change "start" to "head"
		private VertexProcessingInfo InitializeProcessingInfo( List<Vector2> polygon, bool isSubtractive )
		{
			VertexProcessingInfo start = null;
			VertexProcessingInfo prev = null;
			VertexProcessingInfo current = null;
			
			foreach ( Vector2 position in polygon )
			{
				current = new VertexProcessingInfo( position, false, isSubtractive );
				
				if ( prev != null )
				{
					prev.next = current;
				}
				
				current.prev = prev;
				
				if ( start == null )
				{
					start = current;
				}
				
				prev = current;
			}
			
			start.prev = current;
			current.next = start;
			
			return start;
		}
		
		// TODO: Is containment info needed for every vertex?
		/*private void UpdateProcessingInfoListsWithContainment( List<PolygonProcessingInfo> polygonInfos )
		{
			foreach ( PolygonProcessingInfo currentPolygon in polygonInfos )
			{
				VertexProcessingInfo currentVertex = currentPolygon.vertexListHead;
			
				do
				{
					foreach ( PolygonProcessingInfo otherPolygon in polygonInfos )
					{
						if ( ( otherPolygon != currentPolygon ) && Geometry2.PolygonContainsPoint( otherPolygon.vertices, currentVertex.position ) )
						{
							currentVertex.isInside = true;
							break;
						}
					}
				}
				while ( currentVertex != currentPolygon.vertexListHead );
			}
		}*/
		
		private void UpdateProcessingInfoListsWithIntersections( List<PolygonProcessingInfo> polygonInfos )
		{
			for ( int indexA = 0; indexA < polygonInfos.Count; ++indexA )
			{
				for ( int indexB = indexA + 1; indexB < polygonInfos.Count; ++indexB )
				{
					TestIntersections( polygonInfos[indexA], polygonInfos[indexB] );
				}
			}
		}
		
		private void UpdateProcessingInfoListsWithIntersections( List<PolygonProcessingInfo> polygonInfos1, List<PolygonProcessingInfo> polygonInfos2 )
		{
			foreach ( PolygonProcessingInfo info1 in polygonInfos1 )
			{
				foreach ( PolygonProcessingInfo info2 in polygonInfos2 )
				{
					TestIntersections( info1, info2 );
				}
			}
		}
		
		private void TestIntersections( PolygonProcessingInfo polygon1, PolygonProcessingInfo polygon2 )
		{
			VertexProcessingInfo polygon1ProcessingInfoStart = polygon1.vertexListHead;
			VertexProcessingInfo polygon2ProcessingInfoStart = polygon2.vertexListHead;
			
			VertexProcessingInfo polygon1Current = polygon1ProcessingInfoStart;
			
			Vector2 edge1Start;
			Vector2 edge1End;
			
			List<VertexProcessingInfo> edge1IntersectionInfo = new List<VertexProcessingInfo>();
			
			Vector2 intersectionPoint = Vector2.zero;
			
			bool isEntering = !Geometry2.PolygonContainsPoint( polygon2.vertices, polygon1ProcessingInfoStart.position );
			
			do
			{
				edge1Start = polygon1Current.position;
				edge1End = polygon1Current.next.position;
				
				edge1IntersectionInfo.Clear();
				
				VertexProcessingInfo polygon2Current = polygon2ProcessingInfoStart;

				Vector2 edge2Start;
				Vector2 edge2End;				
				
				do
				{
					edge2Start = polygon2Current.position;
					edge2End = polygon2Current.next.position;
					
					if ( Geometry2.SegmentAgainstSegment( edge1Start, edge1End, edge2Start, edge2End, out intersectionPoint ) )
					{
						polygon1.isIntersecting = true;
						polygon2.isIntersecting = true;
						
						VertexProcessingInfo newInfoForPolygon1 = new VertexProcessingInfo( intersectionPoint, true, polygon1Current.isSubtractive );
						edge1IntersectionInfo.Add( newInfoForPolygon1 );
						
						VertexProcessingInfo newInfoForPolygon2 = new VertexProcessingInfo( intersectionPoint, true, polygon2Current.isSubtractive );
						newInfoForPolygon2.prev = polygon2Current;
						newInfoForPolygon2.next = polygon2Current.next;
						
						polygon2Current.next.prev = newInfoForPolygon2;
						polygon2Current.next = newInfoForPolygon2;
						
						polygon2Current = newInfoForPolygon2;
						
						newInfoForPolygon1.neighbor = newInfoForPolygon2;
						newInfoForPolygon2.neighbor = newInfoForPolygon1;
						
						// Test containment for the new intersection point. (create a helper so this can share code with the containment process, unless we can run the containment process after this process...)
						// TODO: be really sure we don't need to test containment for cutting polygons
						foreach ( PolygonProcessingInfo otherPolygon in additivePolygonInfos )
						{
							if ( ( otherPolygon != polygon1 ) && ( otherPolygon != polygon2 ) && Geometry2.PolygonContainsPoint( otherPolygon.vertices, intersectionPoint ) )
							{
								newInfoForPolygon1.isInside = true;
								newInfoForPolygon2.isInside = true;
								break;
							}
						}
					}
					
					polygon2Current = polygon2Current.next;
				}
				while ( polygon2Current != polygon2ProcessingInfoStart );
				
				// TODO: is this slow? describe me
				edge1IntersectionInfo.Sort( 
					(info1, info2) => Vector2.Distance( edge1Start, info1.position ) < Vector2.Distance( edge1Start, info2.position ) ? -1 : 1
				);
				
				foreach ( VertexProcessingInfo info in edge1IntersectionInfo )
				{
					info.prev = polygon1Current;
					info.next = polygon1Current.next;
					
					info.isEntering = isEntering;
					info.neighbor.isEntering = !isEntering;
					
					isEntering = !isEntering;
					
					polygon1Current.next.prev  = info;
					polygon1Current.next = info;
					
					polygon1Current = info;
				}
				
				polygon1Current = polygon1Current.next;
			}
			while ( polygon1Current != polygon1ProcessingInfoStart );
		}
		
		// TODO: this might be slow just using visited flag. we have to check visited on every single vertex.
		// TODO: see what this can share with Clip()
		// TODO: should all this code be using a C# linked list instead of my own?
		private List< List<Vector2> > Fuse()
		{
			List< List<Vector2> > result = new List< List<Vector2> >();
			
			foreach ( PolygonProcessingInfo polygonInfo in additivePolygonInfos )
			{
				VertexProcessingInfo current = polygonInfo.vertexListHead;
				
				do
				{
					// TODO: store a separate list of intersection points to avoid looping over every vertex here
					//do a CCW test when choosing whether or not to switch at intersection points
					if ( !current.isVisited && current.isIntersectionPoint && !current.isInside )
					{
						//Debug.Log( "Entering: " + current.isEntering + " || Position: " + current.position.x + ", " + current.position.y );
						List<Vector2> fusedPolygon = GenerateFusedPolygon( current );
						
						// Ignore degenerate results.
						if ( fusedPolygon.Count >= 3 )
						{
							result.Add( fusedPolygon );
						}
					}
					
					current = current.next;
					
					//VertexProcessingInfo c = polygon1ProcessingInfoStart;
					//do
					//{
						//Debug.Log( "Vert: " + c.position.x + "," + c.position.y + " -visited: " + c.wasVisited + " -ip: " + c.isIntersectionPoint );
						//c = c.next;
					//} 
					//while ( c != polygon1ProcessingInfoStart );
				}
				while ( current != polygonInfo.vertexListHead );
			}
			
			return result;
		}
		
		// TODO: this seems to get called a lot during processing...
		private List<Vector2> GenerateFusedPolygon( VertexProcessingInfo intersectionPoint )
		{
			if ( !intersectionPoint.isIntersectionPoint )
			{
				Debug.LogError( "GenerateFusedPolygon() should only be called on intersection points." );
			}
			
			List<Vector2> result = new List<Vector2>();
			
			VertexProcessingInfo start = intersectionPoint;
			
			if ( !intersectionPoint.isEntering )
			{
				start = intersectionPoint.neighbor;
			}
			
			VertexProcessingInfo current = start;
			
			do
			{
				//Debug.Log( "Vert: " + current.position.x + "," + current.position.y + " -visited: " + current.wasVisited + " -ip: " + current.isIntersectionPoint );
				
				// TODO: is this inside test needed, since we always start at an exterior intersection point? if it's not needed, we don't need to do containment check for everything
				if ( /*!current.isInside &&*/ !current.isVisited )
				{
					result.Add( current.position );
				}
				
				current.isVisited = true;
				if ( current.isIntersectionPoint )
				{
					current.neighbor.isVisited = true;					
				}
				
				if ( current.isIntersectionPoint )
				{
					current = current.neighbor.next;
				}
				else
				{
					current = current.next;
				}
			}
			while ( current != start );
			
			return result;
		}
		
		private List<Vector2> GetPolygonFromProcessingInfo( VertexProcessingInfo start )
		{
			List<Vector2> result = new List<Vector2>();
			
			VertexProcessingInfo current = start;
			
			do
			{
				if ( !current.isInside )
				{					
					result.Add( current.position );
				}
				current = current.next;
			}
			while ( current != start );
			
			return result;
		}
		
		private void WriteDebugOutput()
		{
			string output = "Clipper DebugOutput...\n";

			foreach ( PolygonProcessingInfo polygonInfo in additivePolygonInfos )
			{
				output += "   Polygon...\n";
				
				VertexProcessingInfo current = polygonInfo.vertexListHead;
				
				do
				{
					output += "      Vertex...\n";
					output += "         Position: " + current.position.x + ", " + current.position.y + "\n";
					output += "         Inside: " + current.isInside + "\n";
					output += "         Intersection: " + current.isIntersectionPoint + "\n";
					output += "         Entering: " + current.isEntering + "\n";
					
					current = current.next;
				}
				while ( current != polygonInfo.vertexListHead );
			}

			Debug.Log( output );
		}
		
		#endregion
	}
}
