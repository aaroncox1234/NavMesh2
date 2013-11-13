using UnityEngine;
using System.Collections.Generic;

namespace Rx
{
	public class Mesh2Node
	{
		public List<int> vertexIndices = new List<int>();
		public List<int> neighborIndices = new List<int>();
		public Vector2 center;
	}
	
	public class Mesh2
	{		
		protected List<Vector2> vertices;
		public List<Vector2> Vertices { get { return vertices; } }
		
		protected List<Mesh2Node> nodes;
		public List<Mesh2Node> Nodes { get { return nodes; }
		
		//REMOEV ME
			set { nodes = value; }
		}
		
		public Mesh2()
		{
			vertices = null;
			nodes = null;
		}
		
		public Mesh2( List<Vector2> triangles )
		{
			BuildFromTriangleList( triangles );
		}
		
		public List<EdgeProcessingInfo> edgeInfo = new List<EdgeProcessingInfo>();
		public static Mesh2 BuildFromTriangles( List<Vector2> vertices, List<int> triangles )
		{
			Mesh2 mesh = CreateMeshFromTriangles( vertices, triangles );
			
			/*List<EdgeProcessingInfo>*/ mesh.edgeInfo = ComputeEdgeProcessingInfo( mesh );
			
			TrimDiagonals( mesh, mesh.edgeInfo );
			
			return mesh;
		}
		
		private static Mesh2 CreateMeshFromTriangles( List<Vector2> vertices, List<int> triangles )
		{
			Mesh2 mesh = new Mesh2();		
			mesh.vertices = vertices;
			mesh.nodes = new List<Mesh2Node>();
			
			for ( int triangleIndex = 0; triangleIndex < triangles.Count; triangleIndex += 3 )
			{
				Mesh2Node node = new Mesh2Node();
				node.vertexIndices.Add( triangles[triangleIndex] );
				node.vertexIndices.Add( triangles[triangleIndex + 1] );
				node.vertexIndices.Add( triangles[triangleIndex + 2] );				
				node.center = ComputeMeshNodeCenter( mesh, node );
				
				mesh.Nodes.Add( node );
			}
			
			return mesh;
		}
		
		// This structure assumes that an edge can border no more than two polygons.
		/*private*/public class EdgeProcessingInfo
		{
			// TODO: document what these index into
			// TODO: assert if more than two nodes.
			
			public int startVertexIndex;
			public int endVertexIndex;
			
			public int node1 = -1;
			public int startIndexInNode1;
			public int endIndexInNode1;
			
			public int node2 = -1;
			public int startIndexInNode2;
			public int endIndexInNode2;
		}
		
		private static List<EdgeProcessingInfo> ComputeEdgeProcessingInfo( Mesh2 mesh )
		{
			List<EdgeProcessingInfo> result = new List<EdgeProcessingInfo>();
			
			Dictionary<long, EdgeProcessingInfo> edgeLookupTable = new Dictionary<long, EdgeProcessingInfo>();
			
			for ( int nodeIndex = 0; nodeIndex < mesh.Nodes.Count; ++nodeIndex )
			{
				List<int> nodeVertexIndices = mesh.Nodes[nodeIndex].vertexIndices;
				
				for ( int index = 0; index < nodeVertexIndices.Count; ++index )
				{
					int nextindex = (index + 1) % nodeVertexIndices.Count;
					
					int edgeStartIndex = nodeVertexIndices[index];
					int edgeEndIndex = nodeVertexIndices[nextindex];
					
					int hashStart = edgeStartIndex;
					int hashEnd = edgeEndIndex;
					
					// Swap to ensure (x,y) hashes the same as (y,x).
					if ( hashStart > hashEnd )
					{
						int swap = hashStart;
						hashStart = hashEnd;
						hashEnd = swap;
					}
					
					long hash = ((long)hashStart << 32) + hashEnd;
					
					EdgeProcessingInfo edgeInfo = null;			
					if ( !edgeLookupTable.TryGetValue( hash, out edgeInfo) )
					{
						edgeInfo = new EdgeProcessingInfo();
						edgeInfo.startVertexIndex = edgeStartIndex;
						edgeInfo.endVertexIndex = edgeEndIndex;
						
						edgeInfo.node1 = nodeIndex;
						edgeInfo.startIndexInNode1 = index;
						edgeInfo.endIndexInNode1 = nextindex;
						
						edgeLookupTable.Add( hash, edgeInfo );
						
						result.Add( edgeInfo );						
					}
					else
					{
						edgeInfo.node2 = nodeIndex;
						edgeInfo.startIndexInNode2 = index;
						edgeInfo.endIndexInNode2 = nextindex;
					}
				}
			}
			
			return result;
		}
		
		
		private static void TrimDiagonals( Mesh2 mesh, List<EdgeProcessingInfo> edges )
		{
			// Lookup to handle removed nodes.
			List<int> nodeLookup = new List<int>();
			for ( int i = 0; i < mesh.Nodes.Count; ++i )
			{
				nodeLookup.Add( i );
			}
			
			foreach ( EdgeProcessingInfo edge in edges )
			{
				if ( edge.node2 != -1 )
				{
					int node1 = nodeLookup[ edge.node1 ];
					int node2 = nodeLookup[ edge.node2 ];
					
					// We have to keep looking these up because they change as we merge.
					int startIndexInNode1 = mesh.Nodes[node1].vertexIndices.IndexOf( edge.startVertexIndex );
					int endIndexInNode1 = mesh.Nodes[node1].vertexIndices.IndexOf( edge.endVertexIndex );
					//int startIndexInNode2 = mesh.Nodes[node2].vertexIndices.IndexOf( edge.startVertexIndex );
					//int endIndexInNode2 = mesh.Nodes[node2].vertexIndices.IndexOf( edge.endVertexIndex );
					int endIndexInNode2 = mesh.Nodes[node2].vertexIndices.IndexOf( edge.startVertexIndex );
					int startIndexInNode2 = mesh.Nodes[node2].vertexIndices.IndexOf( edge.endVertexIndex );
					
					int nextIndexForNode1 = (endIndexInNode1 + 1) % mesh.Nodes[node1].vertexIndices.Count;
					int prevIndexForNode1 = (startIndexInNode1 > 0 ) ? ( startIndexInNode1 - 1 ) : ( mesh.Nodes[node1].vertexIndices.Count - 1 );
					
					int nextIndexForNode2 = (endIndexInNode2 + 1) % mesh.Nodes[node2].vertexIndices.Count;
					int prevIndexForNode2 = (startIndexInNode2 > 0 ) ? ( startIndexInNode2 - 1 ) : ( mesh.Nodes[node2].vertexIndices.Count - 1 );
					
					int v0 = mesh.Nodes[node1].vertexIndices[ prevIndexForNode1 ];
					int v1 = mesh.Nodes[node2].vertexIndices[ endIndexInNode2 ];
					int v2 = mesh.Nodes[node2].vertexIndices[ nextIndexForNode2 ];
					
					int v3 = mesh.Nodes[node2].vertexIndices[ prevIndexForNode2 ];
					int v4 = mesh.Nodes[node1].vertexIndices[ endIndexInNode1 ];
					int v5 = mesh.Nodes[node1].vertexIndices[ nextIndexForNode1 ];
					
					if ( ( Geometry2.SignedTriangleArea( mesh.Vertices[v0], mesh.Vertices[v1], mesh.Vertices[v2] ) > 0 ) &&
						 ( Geometry2.SignedTriangleArea( mesh.Vertices[v3], mesh.Vertices[v4], mesh.Vertices[v5] ) > 0 ) )
					{
						List<int> vertexIndicesFromNode1 = new List<int>();
						int indexInNode1 = nextIndexForNode1;
						while ( indexInNode1 != startIndexInNode1 )
						{
							vertexIndicesFromNode1.Add( mesh.Nodes[node1].vertexIndices[ indexInNode1 ] );
							
							indexInNode1 = (indexInNode1 + 1) % mesh.Nodes[node1].vertexIndices.Count;
						}
						mesh.Nodes[node2].vertexIndices.InsertRange( startIndexInNode2 + 1, vertexIndicesFromNode1 );
						
						// TODO: terrible
						for ( int i = 0; i < nodeLookup.Count; ++i )
						{
							if ( nodeLookup[i] == node1 )
							{
								nodeLookup[i] = node2;
							}
						}
						//nodeLookup[node1] = node2;
					}
				}
			}
			
			// TODO: clean up all these crazy indices into indices. See what this helps with: EdgeInfo points to MeshInfos, MeshInfos point to EdgeInfos. Just update both together.
			
			// Removes dupes
			HashSet<int> nodesToKeep = new HashSet<int>( nodeLookup );
			// TODO: would it be better to store list of TOREMOVE, sort it, and remove with weird logic?
			
			// TODO: when determining neighbors, record the indices of the edge vertices (should be easy, info is on the remaining edge infos)
			
			// TODO: avoid this, or just return the new list...			
			
			List<Mesh2Node> newNodeList = new List<Mesh2Node>();
			
			foreach ( int nodeIndex in nodesToKeep )
			{
				newNodeList.Add( mesh.Nodes[nodeIndex] );
			}
			
			mesh.Nodes = newNodeList;
		}
		
		/*
					
						Node nodes[]
						int nodeIndices[] = 0 to #nodes					
						int nodesToRemove[]
					
						merge
							merge node1 vertices into node2
							set nodeIndices[node1] to node2
							remove diagonal from lookup table						
							add node1 index to indices to remove
							visit remaining edges of node2
							*/
		
		private static Vector2 ComputeMeshNodeCenter( Mesh2 mesh, Mesh2Node node )
		{
			Vector2 sum = Vector2.zero;
			foreach ( int vertexIndex in node.vertexIndices )
			{
				sum += mesh.Vertices[ vertexIndex ];
			}
			sum /= node.vertexIndices.Count;
			return sum;
		}
		
		
		
		
		
		
		private void BuildFromTriangleList( List<Vector2> triangles )
		{
			vertices.Clear();
			nodes.Clear();
			
			// TODO: remove this
			vertexTable.Clear();
			
			for ( int triangleIndex = 0; triangleIndex < triangles.Count; triangleIndex += 3 )
			{
				Mesh2Node node = new Mesh2Node();				
				node.center = ( triangles[triangleIndex] + triangles[triangleIndex+1] + triangles[triangleIndex+2] ) / 3.0f;
				
				nodes.Add( node );				
				int nodeIndex = nodes.Count -1;
				
				VertexInfo vertexInfoA = AddVertexForNode( triangles[triangleIndex], nodeIndex );
				VertexInfo vertexInfoB = AddVertexForNode( triangles[triangleIndex+1], nodeIndex );
				VertexInfo vertexInfoC = AddVertexForNode( triangles[triangleIndex+2], nodeIndex );
				
				AddEdgeForVertices( vertexInfoA, vertexInfoB );
				AddEdgeForVertices( vertexInfoB, vertexInfoC );
				AddEdgeForVertices( vertexInfoC, vertexInfoA );
			}
		}
		
		// Struct used when building a mesh out of a list of vertices.
		private class VertexInfo
		{
			public int vertexIndex = -1;
			public List<int> nodeIndices = new List<int>();
		}
			
		// TDOO: See about exposing this information in some way. It's very useful.
		// TODO: Do we consider it an edge if it's on the boundary? Mark those as unique? Useful for things like ray casting, where we'd ignore shared edges...
		private class EdgeInfo
		{
			public VertexInfo vertex1Info;
			public VertexInfo vertex2Info;
		}
		
		// TODO: only store this stuff during initialization
		private Dictionary<Vector2, VertexInfo> vertexTable = new Dictionary<Vector2, VertexInfo>();
		private Dictionary<int, EdgeInfo> edgeTable = new Dictionary<int, EdgeInfo>();
		
		// Add the vertex to the node and mark the node as a neighbor of any other nodes that contain the vertex.
		// Returns TODO
		private VertexInfo AddVertexForNode( Vector2 vertex, int nodeIndex )
		{
			int vertexIndex = -1;
			
			VertexInfo vertexInfo = null;
			
			// If the vertex already exists, update neighbor info for all nodes that share it and register the node with the vertex.
			if ( vertexTable.TryGetValue( vertex, out vertexInfo) )
			{
			    foreach ( int neighborIndex in vertexInfo.nodeIndices )
				{
					nodes[neighborIndex].neighborIndices.Add( nodeIndex );
					nodes[nodeIndex].neighborIndices.Add( neighborIndex );
				}
				
				vertexInfo.nodeIndices.Add( nodeIndex );
				
				vertexIndex = vertexInfo.vertexIndex;
			}
			// If the vertex doesn't already exist, add it to the mesh and register it with the vertex table.
			else
			{
				vertices.Add( vertex );
				
				vertexInfo = new VertexInfo();
				vertexInfo.vertexIndex = vertices.Count - 1;
				vertexInfo.nodeIndices.Add( nodeIndex );
				vertexTable.Add( vertex, vertexInfo );
				
				vertexIndex = vertices.Count - 1;
			}
			
			nodes[nodeIndex].vertexIndices.Add( vertexIndex );
			
			return vertexInfo;
		}
		
		// TODO: Is the hash above safe at all (for detecting overlapping vertices)?
		// TODO: Is there a better way to do the below? 
		private void AddEdgeForVertices( VertexInfo vertex1Info, VertexInfo vertex2Info )
		{
			// TODO: this could be dangerous. Understand it better...
			int edgeHash;
			if ( vertex1Info.vertexIndex < vertex2Info.vertexIndex )
			{
				edgeHash = vertex1Info.vertexIndex * 1000 + vertex2Info.vertexIndex;
			}
			else
			{
				edgeHash = vertex2Info.vertexIndex * 1000 + vertex1Info.vertexIndex;
			}
			
			if ( !edgeTable.ContainsKey( edgeHash ) )
			{
				EdgeInfo edgeInfo = new EdgeInfo();
				edgeInfo.vertex1Info = vertex1Info;
				edgeInfo.vertex2Info = vertex2Info;
				
				edgeTable.Add( edgeHash, edgeInfo );
			}
		}
		
		public List<Vector2> debugEdges()
		{
			List<Vector2> result = new List<Vector2>();
			
			foreach ( KeyValuePair<int, EdgeInfo> info in edgeTable )
			{
				result.Add( vertices[ info.Value.vertex1Info.vertexIndex ] );
				result.Add( vertices[ info.Value.vertex2Info.vertexIndex ] );
			}
			
			return result;
		}
	}
}

