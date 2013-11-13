using UnityEngine;
using System.Collections.Generic;


// TODO: re-evaluate whether touching is containment. might need two functions. (at least update all documention)
// 			when ear cutting, if we don't consider touching as containment, we'll cut ears wrong

namespace Rx
{
	public class Geometry2
	{
		// Returns twice the area of the triangle defined by the three points.
		// If the result is positive, the triangle is CCW.
		// If the result is negative, the triangle is CW.
		// If the result is 0, the triangle is degenerate.
		// Citation: Real Time Collision Detection by Christer Ericson 2005, Page 152.
		public static float SignedTriangleArea( Vector2 a, Vector2 b, Vector2 c )
		{
			// Take the parallelagram defined at the origin by (b - a) and (c - b).
			// The determinant of the matrix formed by the resulting vectors gives us the signed area of the parallelagram.
			// We could then divide by 2 to get the actual area of the triangle, but the actual area isn't useful to our use cases.
			
			return (b.x - a.x) * (c.y - b.y) - (b.y - a.y) * (c.x - b.x);
		}
		
		// Returns true if the segments ab and cd intersect. Colinear points are not considered to be intersecting.
		public static bool SegmentsIntersect( Vector2 a, Vector2 b, Vector2 c, Vector2 d )
		{
			Vector2 dummy = new Vector2();
			return SegmentAgainstSegment( a, b, c, d, out dummy );
		}
		
		public static bool SegmentAgainstSegment( Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersectionPoint )
		{
			float t1;
			float t2;
			
			// Find the intersection point of the infinite lines formed by the ray and the segment.
			if ( LineAgainstLine( a, b, c, d, out t1, out t2 ) )
			{
				// Test if the intersection point is on both segments.
				if ( ( t1 > 0.0f ) && ( t1 <= 1.0f ) && ( t2 > 0.0f ) && ( t2 <= 1.0f ) )
				{
					intersectionPoint = a + t1 * (b - a);
					return true;
				}
			}
			
			intersectionPoint = Vector2.zero;
			return false;
		}		
		
		public static bool RayIntersectsSegment( Vector2 rayStart, Vector2 rayDir, Vector2 segStart, Vector2 segEnd )
		{
			Vector2 dummy = new Vector2();
			return RayAgainstSegment( rayStart, rayDir, segStart, segEnd, out dummy );
		}
		
		public static bool RayAgainstSegment( Vector2 rayStart, Vector2 rayDir, Vector2 segStart, Vector2 segEnd, out Vector2 intersectionPoint )
		{
			float rayt;
			float segt;
			
			// Find the intersection point of the infinite lines formed by the ray and the segment.
			if ( LineAgainstLine( rayStart, rayStart + rayDir, segStart, segEnd, out rayt, out segt ) )
			{
				// Test if the intersection point is along the ray and on the segment.
				if ( ( rayt > 0.0f ) && ( segt > 0.0f ) && ( segt <= 1.0f ) )
				{
					intersectionPoint = rayStart + rayt * rayDir;
					return true;
				}
			}
			
			intersectionPoint = Vector2.zero;
			return false;
		}		
		
		// Find the intersection point of two infinite lines, if there is one.
		// Citation: http://geomalgorithms.com/a05-_intersect-1.html
		public static bool LineAgainstLine( Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out float at, out float bt )
		{
			// First check if the lines are parallel using the usual dot product test.
			
			Vector2 aDir = a1 - a0;
			Vector2 bDir = b1 - b0;
			
			Vector2 bDirPerp = new Vector2( bDir.y, -bDir.x );
			
			float dirDot = Vector2.Dot( bDirPerp, aDir );
			
			if ( Mathf.Abs( dirDot ) > 0.00001f )
			{
				// Let ab be the vector from a to b.
				// The vector from a0 to the intersection point (i) can be described by both of the following (we need to find at and bt):
				//		a0ToI = i - a0
				//		a0ToI = ab + bt * bDir	(trace from a to b, then along b to the intersection point)
				// Write the above as an equation:
				//		i - a0 = ab + bt * bDir
				// The above equation holds true if we apply the dot product of both sides to aDirPerp:
				//		aDirPerp dot (i - a0) = aDirPerp dot (ab + bt * bDir)
				// Because (i - a0) must point in the same direction as aDir, we know that the left side of the above equation is zero at the intersection point:
				// 		0 = aDirPerp dot (ab + bt * bDir)
				// Solving for bt, and using the same logic to find at, yields the following code.
				
				Vector2 aDirPerp = new Vector2( aDir.y, -aDir.x );
				Vector2 ab = b0 - a0;
				
				at = Vector2.Dot( bDirPerp, ab ) / dirDot;
				bt = Vector2.Dot( aDirPerp, ab ) / dirDot;
				
				return true;
			}
			else
			{
				// TODO: check colinear
				at = 0.0f;
				bt = 0.0f;

				return false;
			}
		}
		
		// TODO
		public static List<Vector2> RayAgainstPolygon( Vector2 rayStart, Vector2 rayDir, List<Vector2> polygon )
		{
			// Test intersection between the ray and every edge of the polygon.
			
			List<Vector2> intersectionPoints = new List<Vector2>();
			
			for ( int edgeStartIndex = 0; edgeStartIndex < polygon.Count; ++edgeStartIndex )
			{
				int edgeEndIndex = (edgeStartIndex + 1 ) % polygon.Count;
				
				Vector2 intersectionPoint = Vector2.zero;
				bool intersection = RayAgainstSegment( rayStart, rayDir, polygon[edgeStartIndex], polygon[edgeEndIndex], out intersectionPoint );
				
				if ( intersection )
				{
					intersectionPoints.Add( intersectionPoint );
				}
			}
			
			return intersectionPoints;
		}
		
		// Returns a list of intersection points between the segment ab and the polygon.
		// This is a "slow" operation that works on complex polygons. If the polygon is guaranteed to be convex, 
		// use SegmentIntersectsConvexPolygon().
		public static List<Vector2> SegmentAgainstPolygon( Vector2 a, Vector2 b, List<Vector2> polygon )
		{
			// Test intersection between the segment and every edge of the polygon.
			
			List<Vector2> intersectionPoints = new List<Vector2>();
			
			for ( int edgeStartIndex = 0; edgeStartIndex < polygon.Count; ++edgeStartIndex )
			{
				int edgeEndIndex = (edgeStartIndex + 1 ) % polygon.Count;
				
				Vector2 intersectionPoint = Vector2.zero;
				bool intersection = SegmentAgainstSegment( a, b, polygon[edgeStartIndex], polygon[edgeEndIndex], out intersectionPoint );
				
				if ( intersection )
				{
					intersectionPoints.Add( intersectionPoint );
				}
			}
			
			return intersectionPoints;
		}
		
		// O(n^2) test for polygon convexity. This is intended for validating geometry offline.
		public static bool PolygonIsConvex( List<Vector2> polygon )
		{
			// For every edge of the polygon, test to see if every point of the polygon is on one side of the edge.
			// If any two points lie on either side of an edge, the polygon is concave.
			
			for ( int edgeStartIndex = 0; edgeStartIndex < polygon.Count; ++edgeStartIndex )
			{
				int edgeEndIndex = (edgeStartIndex + 1 ) % polygon.Count;
				
				// Test the winding of the triangle formed by (edgeStart, edgeEnd, vertex) using the signed area of the triangle.
				// If the sign changes for any vertex, then we have found two vertices that lie on either side of the edge.
				float previousArea = 0.0f;
				for ( int vertexIndex = 0; vertexIndex < polygon.Count; ++vertexIndex )
				{
					if ( ( vertexIndex != edgeStartIndex ) && ( vertexIndex != edgeEndIndex ) )
					{
						float area = SignedTriangleArea( polygon[edgeStartIndex], polygon[edgeEndIndex], polygon[vertexIndex] );
						
						// Note: An area of zero means the point is colinear with the edge, which is valid for a convex polygon.
						if ( ( area * previousArea ) < 0.0f )
						{
							return false;
						}
						
						previousArea = area;
					}
				}
			}
			
			return true;
		}
		
		// Determine if the polygon's vertices are defined in a CCW or CW order.
		// Citation: http://en.wikipedia.org/wiki/Shoelace_formula
		public static bool PolygonIsCCW( List<Vector2> polygon )
		{
			// Calculate the signed area of the polygon using the Shoelace formula. The sign of the result determines winding order.		
			float area = 0;
			
			for ( int vertexIndex = 0; vertexIndex < polygon.Count; ++vertexIndex )
			{
				int nextVertexIndex = (vertexIndex + 1) % polygon.Count;
				
				area += ( polygon[vertexIndex].x * polygon[nextVertexIndex].y ) - ( polygon[vertexIndex].y * polygon[nextVertexIndex].x );
			}
			
			return ( area > 0.0f );
		}
		
		// Determine if the point is contained within the polygon.
		// This assumes the caller has already ensured that the polygon is convex and CCW.
		// Citation: Real Time Collision Detection by Christer Ericson 2005, Page 201-203.
		public static bool ConvexPolygonContainsPoint( List<Vector2> polygon, Vector2 point )
		{
			// Cut the polygon in half with a line segment from the first vertex to the middle vertex.
			// If the point lies to the left of the line segment, repeat the process with the left half
			// of the polygon. Otherwise, repeat the process with the right half of the polygon.
			
			int lowIndex = 0;
			int highIndex = polygon.Count;
			
			while ( lowIndex + 1 < highIndex )
			{
				int midIndex = (lowIndex + highIndex) / 2;
				
				// Point is to the left of the edge from polygon[0] to polygon[midIndex] if the triangle formed by those two
				// vertices and the test point is CCW.
				if ( IsTriangleCCW( polygon[0], polygon[midIndex], point ) )
				{
					lowIndex = midIndex;
				}
				// Otherwise point is to the right of the edge.
				else
				{
					highIndex = midIndex;
				}
			}
			
			// If lowIndex is 0, the point lies to the right of the first edge of the polygon (and thus outside the polygon).
			// If highIndex is polygon.Count, the point lies to the left of the last edge of the polygon ( and thus outside the polygon).
			if ( lowIndex == 0 || highIndex == polygon.Count )
			{
				return false;
			}
			
			// The point is inside the polygon if it is to the left of the remaining edge defined by polygon[lowIndex] and polygon[highIndex].
			return IsTriangleCCW( polygon[lowIndex], polygon[highIndex], point );
		}
		
		// Determine if the point is contained within the polygon. The polygon can be complex.
		// If you know the polygon is convex, use ConvexPolygonContainsPoint().
		// Citation: Real Time Collision Detection by Christer Ericson 2005, Page 203.
		public static bool PolygonContainsPoint( List<Vector2> polygon, Vector2 point )
		{
			// Count how many edges of the polygon are intersected by a ray starting from the point and
			// going along the x axis. The point is contained if an odd number of edges intersect.
			
			Vector2 rayDir = new Vector2( 1.0f, 0.0f );
			
			List<Vector2> intersectionPoints = RayAgainstPolygon( point, rayDir, polygon );
			
			return ( intersectionPoints.Count % 2 == 1 );
		}
		
		// O(mn) test for polygon intersection. The polygons can be complex.
		// Returns a list of points of intersection.
		public static List<Vector2> PolygonAgainstPolygon( List<Vector2> polygon1, List<Vector2> polygon2 )
		{
			// Test every edge of polygon1 against every edge of polygon2.
			
			List<Vector2> intersectionPoints = new List<Vector2>();
			
			for ( int startIndex1 = 0; startIndex1 < polygon1.Count; ++startIndex1 )
			{
				int endIndex1 = ( startIndex1 + 1 ) % polygon1.Count;
				
				for ( int startIndex2 = 0; startIndex2 < polygon2.Count; ++startIndex2 )
				{
					int endIndex2 = ( startIndex2 + 1 ) % polygon2.Count;
					
					Vector2 intersectionPoint;
					if ( SegmentAgainstSegment( polygon1[startIndex1], polygon1[endIndex1], polygon2[startIndex2], polygon2[endIndex2], out intersectionPoint ) )
					{
						intersectionPoints.Add( intersectionPoint );
					}				
				}
			}
			
			return intersectionPoints;
		}
		
		public static bool PolygonsIntersect( List<Vector2> polygon1, List<Vector2> polygon2 )
		{
			List<Vector2> result = PolygonAgainstPolygon( polygon1, polygon2 );
			return ( result.Count > 0 );
		}
		
		// Determine if the first polygon completely contains the second. The polygons can be complex.
		public static bool PolygonContainsPolygon( List<Vector2> container, List<Vector2> candidate )
		{
			// The container must contain every vertex of the candidate.		
			foreach ( Vector2 candidateVertex in candidate )
			{
				if ( !PolygonContainsPoint( container, candidateVertex ) )
				{
					return false;
				}
			}
			
			// No edges of the candidate can leave the container.
			if ( PolygonsIntersect( container, candidate ) )
			{
				return false;
			}
			
			return true;
		}
		
		// Generates a single polygon that contains the boundary of both polygons. This is an unoptimized process intended for offline use only.
		// Does not handle holes. Any holes that would be created by merging the two polygons will be lost.
		// Assumes the caller has already ensured that both polygons are CCW and that they intersect.
		public static List<Vector2> FusePolygons( List<Vector2> polygon1, List<Vector2> polygon2 )
		{
			// Insert vertices into both polygons at each intersection point. Remove any vertices contained inside the other polygon.
			
			List<Vector2> polygon1WithIntersections = FusePolygons_PolygonWithIntersections( polygon1, polygon2 );
			List<Vector2> polygon2WithIntersections = FusePolygons_PolygonWithIntersections( polygon2, polygon1 );
			
			// Starting in one of the polygons, walk along the boundaries of each polygon, adding each vertex to the result.
			// When an intersection point is found (when both polygons have the same vertex), switch to the other polygon.
			
			List<Vector2> result = new List<Vector2>();
			
			List<Vector2> currentList = polygon1WithIntersections;
			List<Vector2> otherList = polygon2WithIntersections;
			int currentIndex = 0;
				
			while ( ( result.Count == 0 ) || ( result[0] != currentList[currentIndex] ) )
			{
				result.Add( currentList[currentIndex] );
				
				int nextIndex = (currentIndex + 1) % currentList.Count;
				
				int indexInOtherList = FusePolygons_FindOverlappingVertexIndex( otherList, currentList[nextIndex] );
				
				if ( indexInOtherList == -1 )
				{
					currentIndex = nextIndex;
				}
				else
				{
					List<Vector2> tempSwap = currentList;
					currentList = otherList;
					otherList = tempSwap;
					
					currentIndex = indexInOtherList;
				}
			}
			
			return result;
		}
		
		private static List<Vector2> FusePolygons_PolygonWithIntersections( List<Vector2> polygon1, List<Vector2> polygon2 )
		{
			List<Vector2> result = new List<Vector2>();		
			
			for ( int start = 0; start < polygon1.Count; ++start )
			{			
				int end = (start + 1) % polygon1.Count;
				
				List<Vector2> intersectionPoints = SegmentAgainstPolygon( polygon1[start], polygon1[end], polygon2 );
				
				intersectionPoints.Sort( 
					(vertex1, vertex2) => Vector2.Distance( polygon1[start], vertex1 ) < Vector2.Distance( polygon1[start], vertex2 ) ? -1 : 1
				);
				
				if ( !PolygonContainsPoint( polygon2, polygon1[start] ) )
				{
					result.Add( polygon1[start] );
				}
				
				result.AddRange( intersectionPoints );
			}
			
			return result;
		}
		
		private static int FusePolygons_FindOverlappingVertexIndex( List<Vector2> polygon, Vector2 vertex )
		{
			for ( int index = 0; index < polygon.Count; ++index )
			{
				float sqrDistanceToVertex = ( polygon[index] - vertex ).SqrMagnitude();
				
				if ( sqrDistanceToVertex < 0.00001f )
				{
					return index;
				}
			}
			
			return -1;
		}
		
		// Determines if the point is within the triangle. // TODO: optional? Returns false if the point overlaps one of the triangle's vertices.
		public static bool TriangleContainsPoint( Vector2 a, Vector2 b, Vector2 c, Vector2 p )
		{
			// The triangle contains the point if ABP, BCP, and CAP all have the same winding.
			
			float signABP = SignedTriangleArea( a, b, p );
			float signBCP = SignedTriangleArea( b, c, p );
			
			if ( signABP * signBCP < 0 )
			{
				return false;
			}
			
			float signCAP = SignedTriangleArea( c, a, p );
			
			if ( signABP * signCAP < 0 )
			{
				return false;
			}
			
			return true;
		}
		
		// Triangulate using ear cutting.
		// Returns a list of indices into the polygon's list of vertices. Every interval of three indices forms a triangle.
		// Citation: Real Time Collision Detection by Christer Ericson 2005, Page 496.
		public static List<int> TriangulatePolygon( List<Vector2> polygon )
		{
			List<int> result = new List<int>();
			
			// For each vertex, store the index of the previous and next vertex. Vertices can be cut from the polygon by removing their indices from these arrays.
			
			int[] prevIndices = new int[ polygon.Count ];
			int[] nextIndices = new int[ polygon.Count ];
			
			for ( int i = 0; i < polygon.Count; ++i )
			{
				prevIndices[i] = i - 1;
				nextIndices[i] = i + 1;
			}
			
			prevIndices[0] = polygon.Count - 1;
			nextIndices[polygon.Count - 1] = 0;
			
			// Consider each triple of consecutive vertices (v0, v1, v2) to be a triangle.
			// The triangle is an ear if a) it doesn't contain any other vertex from the polygon and b) it is convex (juts out of the polygon, not in it).
			// When an ear is found, remove it from the polygon.
		
			int remainingVertices = polygon.Count;
			int vertexIndex = 0;
			
			while ( remainingVertices >= 3 )
			{				
				bool earFound = true;
				
				// The triangle is convex if it has the same winding as the polygon (assume all polygons are CCW).
				if ( SignedTriangleArea( polygon[ prevIndices[vertexIndex] ], polygon[ vertexIndex ], polygon[ nextIndices[vertexIndex] ] ) > 0 )
				{
					// Check all remaining vertices not part of the current triangle.
					int testVertexIndex = nextIndices[ nextIndices[vertexIndex] ];
					while ( testVertexIndex != prevIndices[vertexIndex] )
					{
						if ( TriangleContainsPoint( polygon[ prevIndices[vertexIndex] ], polygon[ vertexIndex ], polygon[ nextIndices[vertexIndex] ], polygon[ testVertexIndex ] ) )
						{
							earFound = false;
							break;
						}
						
						testVertexIndex = nextIndices[testVertexIndex];
					}
				}
				else
				{
					earFound = false;
				}
				
				if ( earFound )
				{
					result.Add( prevIndices[vertexIndex] );
					result.Add( vertexIndex );
					result.Add( nextIndices[vertexIndex] );
					
					// Cut v1 out of the polygon and move back one vertex.
					remainingVertices -= 1;
					prevIndices[ nextIndices[vertexIndex] ] = prevIndices[vertexIndex];
					nextIndices[ prevIndices[vertexIndex] ] = nextIndices[vertexIndex];
				}
				
				vertexIndex = nextIndices[vertexIndex];
			}
			
			return result;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		// Returns true if the segments ab and cd intersect. Colinear points are not considered to be intersecting.
		// Returns the point of intersection as a parameter.
		// Citation: Real Time Collision Detection by Christer Ericson 2005, Page 152-153.
		/*public static bool SegmentsIntersect( Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 pointOfIntersection )
		{
			// Check if points a and b are on opposite sides of segment cd by testing that the winding of triangle abc is 
			// different than abd (draw some examples to see why this works).
			float areaABC = SignedTriangleArea( a, b, c );
			float areaABD = SignedTriangleArea( a, b, d );
			
			// The windings are different if the signs are different.
			if ( areaABC * areaABD < 0.0f )
			{
				// Do the corresponding check to determine if points c and d are on opposite sides of segment ab.
				float areaCDA = SignedTriangleArea( c, d, a );
				float areaCDB = SignedTriangleArea( c, d, b );
				
				if ( areaCDA * areaCDB < 0.0f )
				{
					// Points on sement ab are determined by the equation P(t) = a + t * (b - a).
					// See source of citation for derivation of t.
					float t = areaCDA / (areaCDA - areaCDB);
					pointOfIntersection = a + t * (b - a);
					
					return true;
				}
			}		
			
			pointOfIntersection = Vector2.zero;
			return false;
		}*/
		
		
		
		
		
		
		
	
		// Triangulate using ear cutting.
		// See Page 496 of Real Time Collision Detection by Christer Ericson for derivation.
		public static List<Vector2> TriangulatePolygonAA( List<Vector2> polygon )
		{
			List<Vector2> result = new List<Vector2>();
			
			// For each vertex, store the index of the previous and next vertex. Vertices can be cut from the polygon by removing their indices from these arrays.
			
			int[] prevIndices = new int[ polygon.Count ];
			int[] nextIndices = new int[ polygon.Count ];
			
			for ( int i = 0; i < polygon.Count; ++i )
			{
				prevIndices[i] = i - 1;
				nextIndices[i] = i + 1;
			}
			
			prevIndices[0] = polygon.Count - 1;
			nextIndices[polygon.Count - 1] = 0;
			
			// Consider each triple of consecutive vertices to be a triangle.
			// The triangle is an ear if a) it doesn't contain any other vertex from the polygon and b) it is convex (juts out of the polygon, not in it).
			// When an ear is found, remove it from the polygon.
			
			int remainingVertices = polygon.Count;
			int outerIndex = 0;
			
			while ( remainingVertices >= 3 )
			{
				Vector2 a = polygon[ prevIndices[outerIndex] ];
				Vector2 b = polygon[ outerIndex ];
				Vector2 c = polygon[ nextIndices[outerIndex] ];
				
				bool earFound = true;
				
				// The triangle is convex if it has the same winding as the polygon (assume all polygons are CCW).
				if ( SignedTriangleArea( a, b, c ) > 0 )
				{
					// Check all remaining vertices not part of the current triangle.
					int innerIndex = nextIndices[ nextIndices[outerIndex] ];
					while ( innerIndex != prevIndices[outerIndex] )
					{
						if ( TriangleContainsPoint( a, b, c, polygon[innerIndex] ) )
						{
							earFound = false;
							break;
						}
						
						innerIndex = nextIndices[innerIndex];
					}
				}
				else
				{
					earFound = false;
				}
				
				if ( earFound )
				{
					result.Add( a );
					result.Add( b );
					result.Add( c );
					
					// Cut the "b" point of the ear out of the polygon and move back one vertex.
					remainingVertices -= 1;
					prevIndices[ nextIndices[outerIndex] ] = prevIndices[outerIndex];
					nextIndices[ prevIndices[outerIndex] ] = nextIndices[outerIndex];
					outerIndex = prevIndices[outerIndex];
				}
				else
				{
					outerIndex = nextIndices[outerIndex];
				}
			}
			
			return result;
		}
		
		public static List<Vector2> TriangulatePolygonNEW( List<Vector2> polygon )
		{
			List<Vector2> dummyConvexDiagonals;
			return TriangulatePolygonNEWX( polygon, out dummyConvexDiagonals );
		}
		
		// every 3 entries forms a triangle
		// every 2 entries forms a diagonal
		public static List<Vector2> TriangulatePolygonNEWX( List<Vector2> polygon, out List<Vector2> convexDiagonals )
		{
			List<Vector2> result = new List<Vector2>();
			convexDiagonals = new List<Vector2>();
			
			// For each vertex, store the index of the previous and next vertex. Vertices can be cut from the polygon by removing their indices from these arrays.
			
			int[] prevIndices = new int[ polygon.Count ];
			int[] nextIndices = new int[ polygon.Count ];
			
			for ( int i = 0; i < polygon.Count; ++i )
			{
				prevIndices[i] = i - 1;
				nextIndices[i] = i + 1;
			}
			
			prevIndices[0] = polygon.Count - 1;
			nextIndices[polygon.Count - 1] = 0;
			
			// Consider each triple of consecutive vertices to be a triangle.
			// The triangle is an ear if a) it doesn't contain any other vertex from the polygon and b) it is convex (juts out of the polygon, not in it).
			// When an ear is found, remove it from the polygon.
		
			int remainingVertices = polygon.Count;
			int outerIndex = 0;
			
			while ( remainingVertices >= 3 )
			{
				int aIndex = prevIndices[outerIndex];
				int bIndex = outerIndex;
				int cIndex = nextIndices[outerIndex];
				
				Vector2 a = polygon[aIndex];
				Vector2 b = polygon[bIndex];
				Vector2 c = polygon[cIndex];
				
				bool earFound = true;
				
				// The triangle is convex if it has the same winding as the polygon (assume all polygons are CCW).
				if ( SignedTriangleArea( a, b, c ) > 0 )
				{
					// Check all remaining vertices not part of the current triangle.
					int innerIndex = nextIndices[ nextIndices[outerIndex] ];
					while ( innerIndex != prevIndices[outerIndex] )
					{
						if ( TriangleContainsPoint( a, b, c, polygon[innerIndex] ) )
						{
							earFound = false;
							break;
						}
						
						innerIndex = nextIndices[innerIndex];
					}
				}
				else
				{
					earFound = false;
				}
				
				if ( earFound )
				{
					result.Add( a );
					result.Add( b );
					result.Add( c );
					
					// By cutting "b" out of the polygon, a diagonal will be introduced from "a" to "c".
					// Record the diagonal if it is convex (i.e. the polygon is convex at both vertices of the edge).
					if ( ( SignedTriangleArea( polygon[ prevIndices[aIndex] ], a, b ) > 0 ) &&
						 ( SignedTriangleArea( b, c, polygon[ nextIndices[cIndex] ] ) > 0 ) )
					{
						convexDiagonals.Add( a );
						convexDiagonals.Add( c );
					}
					
					// Cut the "b" point of the ear out of the polygon and move back one vertex.
					remainingVertices -= 1;
					prevIndices[ nextIndices[outerIndex] ] = prevIndices[outerIndex];
					nextIndices[ prevIndices[outerIndex] ] = nextIndices[outerIndex];
					outerIndex = prevIndices[outerIndex];
				}
				else
				{
					outerIndex = nextIndices[outerIndex];
				}
			}
			
			return result;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		public static bool SameSign( float a, float b )
		{
			return ( a * b >= 0.0f );
		}
		
		// Define the cross product in 2D as ( (perpendicular of u) dot v ).
		public static float Cross2D( Vector2 u, Vector2 v )
		{
			return u.y * v.x - u.x * v.y;
		}
		
		// The triangle is CCW if c is on the left of (b - a).
		public static bool IsTriangleCCW( Vector2 a, Vector2 b, Vector2 c )
		{
			return ( Cross2D( (a-b), (c-b) ) > 0 );
		}
		
		// Returns true if every point is contained by at least one triangle.
		// TODO: switch everything to arrays.
		public static bool TrianglesContainPoints( List<Triangle2d> triangles, List<Vector2> points )
		{
			foreach ( Vector2 point in points )
			{
				bool pointContained = false;
				
				foreach ( Triangle2d triangle in triangles )
				{
					if ( TriangleContainsPoint( triangle.a, triangle.b, triangle.c, point ) )
					{
						pointContained = true;
						break;
					}
				}
				
				if ( !pointContained )
				{
					return false;
				}
			}
			
			return true;
		}
		
		// Triangulate using ear cutting.
		// See Page 496 of Real Time Collision Detection by Christer Ericson for derivation.
		// Perf: Create a better representation for the triangles. There'll be a lot of redundancy with the current strategy (overlapping vertices/edges).
		public static void TriangulatePolygonX( List<Vector2> vertices, out List<Triangle2d> triangles )
		{
			triangles = new List<Triangle2d>( vertices.Count - 2 );
			int triangleCount = 0;
			
			// For each vertex, store the index of the previous and next vertex. Vertices can be cut from the polygon by removing their indices from these arrays.
			
			int[] prevIndices = new int[ vertices.Count ];
			int[] nextIndices = new int[ vertices.Count ];
			
			for ( int i = 0; i < vertices.Count; ++i )
			{
				prevIndices[i] = i - 1;
				nextIndices[i] = i + 1;
			}
			
			prevIndices[0] = vertices.Count - 1;
			nextIndices[vertices.Count - 1] = 0;
			
			// Consider each triple of consequtive vertices to be a triangle.
			// The triangle is an ear if a) it doesn't contain any other vertex from the polygon and b) it is convex (juts out of the polygon, not in it).
			// When an ear is found, remove it from the polygon.
			
			int remainingVertices = vertices.Count;
			int outerIndex = 0;
			
			while ( remainingVertices >= 3 )
			{
				Vector2 a = vertices[ prevIndices[outerIndex] ];
				Vector2 b = vertices[ outerIndex ];
				Vector2 c = vertices[ nextIndices[outerIndex] ];
				
				bool earFound = true;
				
				// The triangle is convex if it has the same winding as the polygon (assume all polygons are CCW).
				if ( IsTriangleCCW( a, b, c ) )
				{
					// Check all remaining vertices not part of the current triangle.
					int innerIndex = nextIndices[ nextIndices[outerIndex] ];
					while ( innerIndex != prevIndices[outerIndex] )
					{
						if ( TriangleContainsPoint( a, b, c, vertices[innerIndex] ) )
						{
							earFound = false;
							break;
						}
						
						innerIndex = nextIndices[innerIndex];
					}
				}
				else
				{
					earFound = false;
				}
				
				if ( earFound )
				{
					triangles.Insert( triangleCount, new Triangle2d( a, b, c ) );
					triangleCount++;
					
					// Cut the "b" point of the ear out of the polygon and move back one vertex.
					remainingVertices -= 1;
					prevIndices[ nextIndices[outerIndex] ] = prevIndices[outerIndex];
					nextIndices[ prevIndices[outerIndex] ] = nextIndices[outerIndex];
					outerIndex = prevIndices[outerIndex];
				}
				else
				{
					outerIndex = nextIndices[outerIndex];
				}
			}
			
			if ( triangleCount != vertices.Count - 2 )
			{
				Debug.LogError( "TriangulatePolygon generated " + triangleCount + " triangles. Expected to generate " + (vertices.Count - 2) + " triangles." );
			}
		}
		
		// TODO: Work out the math to understand intersection. See Page 152. Research optimization for testing against multiple segments.
		// Taken from http://forum.unity3d.com/threads/17384-Line-Intersection
		public static bool SegmentIntersectsSegment( Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2 )
		{
			// HACK: avoid overlapping points reporting collision. don't keep this permanent.
			if ( start1 == start2 || start1 == end2 || end1 == start2 || end1 == end2 )
			{
				return false;
			}
				
				
	    	Vector2 a = end1 - start1;
	    	Vector2 b = start2 - end2;
	    	Vector2 c = start1 - start2; 
	
	    	float alphaNumerator = b.y*c.x - b.x*c.y;
	    	float alphaDenominator = a.y*b.x - a.x*b.y;
	  		float betaNumerator  = a.x*c.y - a.y*c.x;
	  		float betaDenominator  = a.y*b.x - a.x*b.y;
	
	        bool doIntersect = true;
	
	    	if (alphaDenominator == 0 || betaDenominator == 0)
			{
	        	doIntersect = false;
	    	}
			else
			{
				if (alphaDenominator > 0)
				{
	            	if (alphaNumerator < 0 || alphaNumerator > alphaDenominator) 
					{
	                	doIntersect = false;
	            	}
	    	    }
				else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
				{
	            	doIntersect = false;
	        	}
	
	        	if (doIntersect && betaDenominator > 0)
				{
	            	if (betaNumerator < 0 || betaNumerator > betaDenominator) 
					{
	                	doIntersect = false;
	 	            }
	
	       		} 
				else if (betaNumerator > 0 || betaNumerator < betaDenominator)
				{
	            	doIntersect = false;
	        	}
	
	    	}
	
	    	return doIntersect;
		}
		
		// TODO: look into a better way to do this. (after adding some basic test types so it's easier to verify everything). maybe some sort of sweep and prune.
		// Tests intersection only. Containment returns false.
		public static bool SegmentIntersectsPolygon( Vector2 segmentStart, Vector2 segmentEnd, Polygon2 polygon )
		{		
			Vector2 prevVertex = new Vector2( float.NaN, float.NaN );
			
			foreach ( Vector2 vertex in polygon.Vertices )
			{
				if ( !float.IsNaN( prevVertex.x ) )
				{
					// TODO: see if we can optimize for testing a segment against a bunch of other segments (i.e. polygon)
					if ( SegmentIntersectsSegment( segmentStart, segmentEnd, prevVertex, vertex ) )
					{
						return true;
					}
				}
				
				prevVertex = vertex;
			}
			
			if ( SegmentIntersectsSegment( segmentStart, segmentEnd, prevVertex, polygon.Vertices[0] ) )
			{
				return true;
			}
			
			return false;
		}
		
		// TODO: look into a better way to do this. (after adding some basic test types so it's easier to verify everything). maybe some sort of sweep and prune.
		public static bool PolygonIntersectsPolygon( Polygon2 polygon1, Polygon2 polygon2 )
		{
			Vector2 prevVertex = new Vector2( float.NaN, float.NaN );
			
			foreach ( Vector2 vertex in polygon1.Vertices )
			{
				if ( !float.IsNaN( prevVertex.x ) )
				{
					if ( SegmentIntersectsPolygon( prevVertex, vertex, polygon2 ) )
					{
						return true;
					}
				}
				
				prevVertex = vertex;
			}
			
			if ( SegmentIntersectsPolygon( prevVertex, polygon1.Vertices[0], polygon2 ) )
			{
				return true;
			}
			
			return false;
		}
	}
}