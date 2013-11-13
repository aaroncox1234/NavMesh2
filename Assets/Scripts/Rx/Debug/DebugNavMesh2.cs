using UnityEngine;
using System.Collections.Generic;

namespace Rx
{	
	public class DebugNavMesh2 : MonoBehaviour
	{
		public bool ShowBoundary { get; set; }
		public List<Vector2> Boundary = new List<Vector2>();
		
		public bool ShowTriangles { get; set; }		
		public List<int> Triangles = new List<int>();
		
		public bool ShowDiagonals { get; set; }		
		public List<Vector2> Diagonals = new List<Vector2>();
		
		public bool ShowMesh { get; set; }		
		public Mesh2 mesh = null;
		
		public virtual void OnDrawGizmos()
	    {
			if ( ShowTriangles && Triangles != null )
			{
				DrawTriangles( Triangles, Color.blue );
			}
			
			if ( ShowBoundary && Boundary != null )
			{
				DrawPolygon( Boundary, Color.white );
			}
			
			if ( ShowDiagonals && Diagonals != null && mesh != null )
			{
				//foreach ( Mesh2.EdgeProcessingInfo info in mesh.edgeInfo )
				{
					//if ( info.node2 != null )
					{
						// We need 6 vertices in total.
						// TODO: Bail if first is concave.
						// TODO: might be safer to store Vertor2 for all these on the edgeinfo. That way indices don't get messy.
						// 			will also have to update the edgeinfo for the new polygon when merging (can we build it from existing data?)
						
						/*int nextIndexForNode1 = (info.endIndexInNode1 + 1) % info.node1.vertexIndices.Count;
						int prevIndexForNode1 = (info.startIndexInNode1 > 0 ) ? ( info.startIndexInNode1 - 1 ) : ( info.node1.vertexIndices.Count - 1 );
						
						int nextIndexForNode2 = (info.endIndexInNode2 + 1) % info.node2.vertexIndices.Count;
						int prevIndexForNode2 = (info.startIndexInNode2 > 0 ) ? ( info.startIndexInNode2 - 1 ) : ( info.node2.vertexIndices.Count - 1 );
						
						int v0 = info.node1.vertexIndices[ prevIndexForNode1 ];
						int v1 = info.node2.vertexIndices[ info.endIndexInNode2 ];
						int v2 = info.node2.vertexIndices[ nextIndexForNode2 ];
						
						int v3 = info.node2.vertexIndices[ prevIndexForNode2 ];
						int v4 = info.node1.vertexIndices[ info.endIndexInNode1 ];
						int v5 = info.node1.vertexIndices[ nextIndexForNode1 ];
						
						if ( ( Geometry2.SignedTriangleArea( mesh.Vertices[v0], mesh.Vertices[v1], mesh.Vertices[v2] ) > 0 ) &&
							 ( Geometry2.SignedTriangleArea( mesh.Vertices[v3], mesh.Vertices[v4], mesh.Vertices[v5] ) > 0 ) )
						{
							Gizmos.color = Color.yellow;
							DrawLine( mesh.Vertices[info.startVertexIndex], mesh.Vertices[info.endVertexIndex] );
						}*/
					}
				/*Gizmos.color = Color.green;
				DrawLine( mesh.Vertices[v0], mesh.Vertices[v1] );
				Gizmos.color = Color.blue;
				DrawLine( mesh.Vertices[v1], mesh.Vertices[v2] );
				
				Gizmos.color = Color.red;
				DrawLine( mesh.Vertices[v3], mesh.Vertices[v4] );
				Gizmos.color = Color.black;
				DrawLine( mesh.Vertices[v4], mesh.Vertices[v5] );*/
				}
				
				/*Vector2 a = mesh.Vertices[ info.node1.vertexIndices[ info.startIndexInNode1 ] ];
				Vector2 b = mesh.Vertices[ info.node1.vertexIndices[ info.endIndexInNode1 ] ];
				
				int nextForNode1 = (info.endIndexInNode1 + 1) % info.node1.vertexIndices.Count;
				Vector2 c = mesh.Vertices[ info.node1.vertexIndices[ nextForNode1 ] ];
				
				int prevForNode2 = info.startIndexInNode2 - 1;
				if ( prevForNode2 < 0 )
				{
					prevForNode2 = info.node2.vertexIndices.Count - 1;
				}
				Vector2 d =  mesh.Vertices[ info.node2.vertexIndices[ prevForNode2 ] ];
				
				Gizmos.color = Color.green;
				DrawLine( a, b );
				DrawLine( b, c );
				DrawLine( b, d );*/
				
				/*foreach ( Mesh2.EdgeProcessingInfo info in mesh.edgeInfo )
				{
					if ( info.nodes.Count > 2 )
					{
						Debug.Log( "FOUND MORE THAN TWO" );
					}
					
					if ( info.nodes.Count == 0 )
					{
						Debug.Log( "FOUND NONE" );
					}
					
					if ( info.nodes.Count == 2 )
					{
						Vector2 center1 = info.nodes[0].center;
						Vector2 center2 = info.nodes[1].center;
						
						Gizmos.color = Color.cyan;
						Gizmos.DrawLine ( new Vector3( center1.x, center1.y, 0.0f ), new Vector3( center2.x, center2.y, 0.0f ) );
					}
					else
					{
						Vector2 start = mesh.Vertices[ info.startIndex ];
						Vector2 end = mesh.Vertices[ info.endIndex ];
						
						Gizmos.color = Color.black;
						Gizmos.DrawLine ( new Vector3( start.x, start.y, 0.0f ), new Vector3( end.x, end.y, 0.0f ) );
					}
				}*/
				/*
				Gizmos.color = Color.black;
				for ( int i = 0; i < Diagonals.Count; i += 2 )
				{
					Gizmos.DrawLine ( new Vector3( Diagonals[i].x, Diagonals[i].y, 0.0f ), new Vector3( Diagonals[i+1].x, Diagonals[i+1].y, 0.0f ) );
				}
				*/
			}
			
			if ( ShowMesh && mesh != null )
			{
				DrawMesh( mesh, Color.green );
			}
	    }
					
		private void DrawLine( Vector2 a, Vector2 b )
		{
			Gizmos.DrawLine( new Vector3( a.x, a.y, 0.0f ), new Vector3( b.x, b.y, 0.0f ) );
		}
		
		private void DrawPolygon( List<Vector2> polygon, Color color )
		{
			Gizmos.color = color;
			
			for ( int index = 0; index < polygon.Count; ++index )
			{
				int nextIndex = (index + 1) % polygon.Count;
				Gizmos.DrawLine( new Vector3( polygon[index].x, polygon[index].y, 0.0f ), new Vector3( polygon[nextIndex].x, polygon[nextIndex].y, 0.0f ) );
			}
		}
		
		private void DrawTriangles( List<int> triangles, Color color )
		{
			for ( int i = 0; i < triangles.Count; i += 3 )
			{
				DrawTriangle( Boundary[ triangles[i] ], Boundary[ triangles[i+1] ], Boundary[ triangles[i+2] ], color );
			}
		}
		
		private void DrawTriangle( Vector2 a, Vector2 b, Vector2 c, Color color )
		{
			Gizmos.color = color;
			
			Gizmos.DrawLine( new Vector3( a.x, a.y, 0.0f ), new Vector3( b.x, b.y, 0.0f ) );
			Gizmos.DrawLine( new Vector3( b.x, b.y, 0.0f ), new Vector3( c.x, c.y, 0.0f ) );
			Gizmos.DrawLine( new Vector3( c.x, c.y, 0.0f ), new Vector3( a.x, a.y, 0.0f ) );
		}
		
		private void DrawMesh( Mesh2 mesh, Color color )
		{
			foreach ( Mesh2Node node in mesh.Nodes )
			{
				for ( int i = 0; i < node.vertexIndices.Count; ++i )
				{
					int j = (i + 1) % node.vertexIndices.Count;
					
					Gizmos.color = Color.magenta;
					Gizmos.DrawLine( mesh.Vertices[ node.vertexIndices[i] ], mesh.Vertices[ node.vertexIndices[j] ] );
				}
			}
		}
	}	
}
