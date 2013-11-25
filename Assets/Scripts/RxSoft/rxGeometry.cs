using UnityEngine;
using System.Collections.Generic;

namespace RxSoft
{
	public class rxGeometry
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

		// O(n^2) test for polygon convexity. This is intended for validating geometry offline.
		public static bool IsPolygonConvex( List<Vector2> polygon )
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
		public static bool IsPolygonCCW( List<Vector2> polygon )
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
	}
}
