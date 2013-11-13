using UnityEngine;
using System.Collections;

public class EditablePolygon2 : MonoBehaviour
{
	// todo: not public
	public Polygon2 polygon = new Polygon2();
	
	protected int hoverVertexIndex = -1;
	protected int selectedVertexIndex = -1;
	
	protected const float sqVertexSelectionRange = 16.0f;
	
	protected Color vertexDrawColor = Color.grey;
	protected Color hoverVertexDrawColor = Color.grey;
	protected Color selectedVertexDrawColor = Color.white;
	protected Color edgeDrawColor = Color.grey;
	protected Color selectedEdgeDrawColor = Color.white;
	
	public Polygon2 GetCopyOfPolygon()
	{
		Polygon2 copy = new Polygon2();
		
		foreach ( Vector2 vertex in polygon.Vertices )
		{
			copy.vertices.Add( ToWorld2( vertex ) );
		}
		
		return copy;
	}
	
	public void InsertVertex( Vector2 position )
	{
		int index;
		
		if ( selectedVertexIndex != -1 )
		{
			index = selectedVertexIndex + 1;
		}
		else
		{
			index = polygon.NumVertices;
		}
		
		polygon.InsertVertex( index, ToLocal2( position ) );
		
		selectedVertexIndex = index;
	}
	
	public void DeleteSelectedVertex()
	{
		if ( selectedVertexIndex != -1 )
		{
			polygon.RemoveVertex( selectedVertexIndex );
			
			selectedVertexIndex = -1;
		}
	}
	
	public void SetSelectedVertexPosition( Vector2 position )
	{
		if ( selectedVertexIndex != -1 )
		{
			polygon.SetVertexPosition( selectedVertexIndex, ToLocal2( position ) );
		}
	}
	
	public void UpdateHoverVertex( Vector2 mousePosition )
	{
		hoverVertexIndex = polygon.IndexOfNearestVertex( ToLocal2( mousePosition ), sqVertexSelectionRange );
	}
	
	public void UpdateSelectedVertex( Vector2 mousePosition )
	{
		selectedVertexIndex = polygon.IndexOfNearestVertex( ToLocal2( mousePosition ), sqVertexSelectionRange );
	}
	
	public void ReverseWinding()
	{
		selectedVertexIndex = polygon.NumVertices - selectedVertexIndex - 1;
		
		polygon.ReverseWinding();
	}
		
	public virtual void OnDrawGizmosSelected()
    {
		if ( polygon.NumVertices >= 0 )
		{
			DrawEdges();
			
			DrawVertices();
		}
    }
	
	private void DrawEdges()
	{
		Vector2 firstVertex = new Vector2( float.NaN, float.NaN );
		Vector2 previousVertex = Vector2.zero;
		
		int index = 0;
		
		foreach ( Vector2 vertex in polygon.Vertices )
		{				
			if ( float.IsNaN( firstVertex.x ) )
			{
				firstVertex = vertex;
			}
			else
			{
				Gizmos.color = edgeDrawColor;
				if ( ( index - 1 ) == selectedVertexIndex )
				{
					Gizmos.color = selectedEdgeDrawColor;
				}				
				
				Gizmos.DrawLine( ToWorld3( previousVertex ), ToWorld3( vertex ) );
			}
			
			previousVertex = vertex;
					
			++index;
		}
		
		if ( polygon.NumVertices >= 3 )
		{
			Gizmos.color = edgeDrawColor;
			if ( ( index - 1 ) == selectedVertexIndex )
			{
				Gizmos.color = selectedEdgeDrawColor;
			}
			
			Gizmos.DrawLine( ToWorld3( previousVertex ), ToWorld3( firstVertex ) );
		}
	}
	
	private void DrawVertices()
	{
		Vector3 vertexDrawSize = new Vector3( 6.0f, 5.0f, 1.0f );
		Vector3 selectedVertexDrawSize = new Vector3( 10.0f, 9.0f, 1.0f );
		
		Vector3 drawSize;
		int index = 0;
		
		foreach ( Vector2 vertex in polygon.Vertices )
		{	
			if ( index == selectedVertexIndex )
			{
				drawSize = selectedVertexDrawSize;
				Gizmos.color = selectedVertexDrawColor;	
			}
			else if ( index == hoverVertexIndex )
			{
				drawSize = selectedVertexDrawSize;
				Gizmos.color = hoverVertexDrawColor;
			}
			else
			{
				drawSize = vertexDrawSize;
				Gizmos.color = vertexDrawColor;
			}
			
			Gizmos.DrawCube( ToWorld3( vertex ), drawSize );
			
			++index;
		}
	}
				
	protected Vector2 ToLocal2( Vector2 worldPosition )
	{
		return worldPosition - Helpers.AsVector2( transform.position );
	}
				
	protected Vector2 ToWorld2( Vector2 localPosition )
	{
		return localPosition + Helpers.AsVector2( transform.position );
	}
	
	protected Vector3 ToWorld3( Vector2 localPosition )
	{
		return Helpers.AsVector3( localPosition, 0.0f ) + transform.position;
	}
}
