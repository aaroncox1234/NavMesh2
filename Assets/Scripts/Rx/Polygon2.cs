using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Polygon2
{
	public List<Vector2> vertices = new List<Vector2>();
	
	public int NumVertices
	{
		get
		{
			return vertices.Count;
		}
	}
	
	public List<Vector2> Vertices
	{
		get
		{
			return vertices;
		}
	}
	
	// TODO: make this the norm
	public Vector2[] VertexArray
	{
		get
		{
			Vector2[] dontDoItLikeThis = new Vector2[ vertices.Count ];
			int index = 0;
			foreach ( Vector2 vertex in vertices )
			{
				dontDoItLikeThis[index++] = vertex;
			}
			return dontDoItLikeThis;
		}
	}
	
	public void InsertVertex( int index, Vector2 position )
	{	
		vertices.Insert( index, position );
	}
	
	public void InsertVertices( int index, List<Vector2> verticesToInsert )
	{
		vertices.InsertRange( index, verticesToInsert );
	}
	
	public void RemoveVertex( int index )
	{
		vertices.RemoveAt( index );
	}
	
	public void SetVertexPosition( int index, Vector2 position )
	{		
		vertices[index] = position;
	}
	
	public int IndexOfNearestVertex( Vector2 position, float sqMaxDistance )
	{
		int nearestVertexIndex = -1;
		
		float sqDistanceToNearestVertex = float.MaxValue;
		
		int vertexIndex = 0;
		
		foreach ( Vector2 vertex in vertices )
		{			
			float sqDistanceToVertex = Vector2.SqrMagnitude( position - vertex );
			
			if ( ( sqDistanceToVertex <= sqMaxDistance ) && ( sqDistanceToVertex < sqDistanceToNearestVertex ) )
			{
				nearestVertexIndex = vertexIndex;
				sqDistanceToNearestVertex = sqDistanceToVertex;
			}
			
			++vertexIndex;
		}
		
		return nearestVertexIndex;
	}
	
	public void ReverseWinding()
	{
		vertices.Reverse();
	}
}
