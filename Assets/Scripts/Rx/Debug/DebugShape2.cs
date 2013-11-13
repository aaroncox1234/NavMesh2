using UnityEngine;
using System.Collections.Generic;

namespace Rx
{
	public class DebugShape2 : MonoBehaviour
	{
		public static bool hackHide = false;
		
		protected Color unselectedVertexColor = Color.grey;
		protected Color selectedVertexColor = Color.white;	
		protected Color controlledVertexColor = Color.blue;
		protected Color highlightedVertexColor = Color.red;
		
		protected Color unselectedEdgeColor = Color.grey;
		protected Color selectedEdgeColor = Color.white;
		protected Color controlledEdgeColor = Color.blue;
		protected Color highlightedEdgeColor = Color.red;
		
		protected Vector3 defaultVertexSize = new Vector3( 3.0f, 2.5f, 1.0f );
		protected Vector3 hoverVertexSize = new Vector3( 5.0f, 4.5f, 1.0f );		
		protected Vector3 controlledVertexSize = new Vector3( 5.0f, 4.5f, 1.0f );
		
		protected Color pointOfIntersectionColor = Color.red;
		protected Vector3 pointOfIntersectionSize = new Vector3( 6.0f, 6.0f, 1.0f );
		
		// For polygons only.
		protected List<Vector2> fusedVertices = new List<Vector2>();
		
		public List<Vector2> vertices = new List<Vector2>();
		
		protected int minVertices = 0;
		protected int maxVertices = int.MaxValue;
		
		protected int hoverVertexIndex = -1;
		protected int controlledVertexIndex = -1;
		
		protected float sqMinDistanceToHoverVertex = 200.0f;
		
		public bool IsSelected { get; set; }
		public bool IsHighlighted { get; set; }
		public List<Vector2> PointsOfIntersection { get; set; }
		
		public List<Vector2> GetWorldVertices()
		{
			List<Vector2> result = new List<Vector2>( vertices.Count );
			
			Vector2 gameObjectPosition = new Vector2( transform.position.x, transform.position.y );
			
			for ( int index = 0; index < vertices.Count; ++index )
			{
				result.Add( vertices[index] + gameObjectPosition );
			}
			
			return result;
		}
		
		public void ReverseVertices()
		{
			vertices.Reverse();
			
			controlledVertexIndex = vertices.Count - controlledVertexIndex - 1;
		}
		
		public void ShiftUpVertices()
		{
			List<Vector2> originalVertices = new List<Vector2>( vertices );
			
			for (int src = 0; src < originalVertices.Count; ++src )
			{
				int dst = (src + 1) % originalVertices.Count;
				
				vertices[dst] = originalVertices[src];
			}
		}
		
		#region Events
		
		public virtual void OnMouseMove( Vector2 mousePosition )
		{
			hoverVertexIndex = -1;
			
			float sqNearestVertexDistance = float.MaxValue;
			
			for ( int index = 0; index < vertices.Count; ++index )
			{
				float sqDistanceToVertex = ( ToWorld2( vertices[index] ) - mousePosition ).SqrMagnitude();
				
				if ( ( sqDistanceToVertex < sqMinDistanceToHoverVertex ) && ( sqDistanceToVertex < sqNearestVertexDistance ) )
				{
					hoverVertexIndex = index;
				}
			}
		}
		
		public virtual void OnMouseDrag( Vector2 mousePosition )
		{
			if ( controlledVertexIndex != -1 )
			{
				vertices[controlledVertexIndex] = ToLocal2( mousePosition );
			}
		}
		
		public virtual void OnMouseDown( Vector2 mousePosition )
		{
			if ( hoverVertexIndex != -1 )
			{
				controlledVertexIndex = hoverVertexIndex;
			}
			else
			{
				controlledVertexIndex = -1;
			}
		}
		
		public virtual void OnKeyDown( KeyCode keyCode, Vector2 mousePosition )
		{
			switch ( keyCode )
			{
				case KeyCode.I:
				{
				
					if ( vertices.Count < maxVertices )
					{
						if ( controlledVertexIndex == -1 )
						{
							vertices.Add( mousePosition );
						
							controlledVertexIndex = vertices.Count - 1;
						}
						else
						{
							vertices.Insert( controlledVertexIndex + 1, ToLocal2( mousePosition ) );
						
							controlledVertexIndex = controlledVertexIndex + 1;
						}
					}
				}
				break;
				
				case KeyCode.K:
				{
					if ( ( controlledVertexIndex != -1 ) && ( vertices.Count > minVertices ) )
					{
						vertices.RemoveAt( controlledVertexIndex );
						controlledVertexIndex = -1;
					}
				}
				break;
			}
		}
		
		#endregion
		
		#region Drawing
		
		public virtual void OnDrawGizmos()
	    {
			if ( hackHide )
			{
				return;
			}
			
			if ( vertices.Count >= 0 )
			{
				if ( !IsSelected )
				{
					controlledVertexIndex = -1;
				}
				
				DrawEdges();
				
				DrawVertices();
				
				DrawPointsOfIntersection();
			}
	    }
		
		protected void DrawEdges()
		{
			for ( int index = 0; index < vertices.Count; ++index )
			{
				int nextIndex = index + 1;
				if ( nextIndex == vertices.Count )
				{
					nextIndex = 0;
				}
				
				Gizmos.color = unselectedEdgeColor;
				if ( IsHighlighted )
				{
					Gizmos.color = highlightedEdgeColor;
				}
				else if ( index == controlledVertexIndex )
				{
					Gizmos.color = controlledEdgeColor;
				}
				else if ( IsSelected )
				{
					Gizmos.color = selectedEdgeColor;
				}
				
				Gizmos.DrawLine( ToWorld3( vertices[index] ), ToWorld3( vertices[nextIndex] ) );
			}
		}
		
		protected void DrawVertices()
		{
			for ( int index = 0; index < vertices.Count; ++index )
			{
				if ( IsHighlighted )
				{
					Gizmos.color = highlightedVertexColor;
					Gizmos.DrawCube( ToWorld3( vertices[index] ), defaultVertexSize );
				}				
				else if ( index == controlledVertexIndex )
				{
					Gizmos.color = controlledVertexColor;
					Gizmos.DrawCube( ToWorld3( vertices[index] ), controlledVertexSize );
				}
				else
				{
					if ( IsSelected )
					{				 
						Gizmos.color = selectedVertexColor;
					}
					else
					{
						Gizmos.color = unselectedVertexColor;
					}
					
					if ( index == hoverVertexIndex )
					{					
						Gizmos.DrawCube( ToWorld3( vertices[index] ), hoverVertexSize );
					}
					else
					{
						Gizmos.DrawCube( ToWorld3( vertices[index] ), defaultVertexSize );
					}
				}
			}
		}
		
		protected void DrawPointsOfIntersection()
		{
			if ( PointsOfIntersection != null )
			{
				foreach ( Vector2 point in PointsOfIntersection )
				{
					Gizmos.color = pointOfIntersectionColor;
					Gizmos.DrawCube( new Vector3( point.x, point.y, transform.position.z ), pointOfIntersectionSize );
				}
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
		
		public void CenterGizmo()
		{
			// Just do a simple, unweighted average to find the "center" of the polygon.
			
			if ( vertices.Count > 0 )
			{
				Vector2 curPosition = new Vector2( transform.position.x, transform.position.y );
				
				Vector2 center = Vector2.zero;
				
				foreach ( Vector2 vertex in vertices )
				{
					center += vertex + curPosition;
				}
				
				center /= vertices.Count;
				
				Vector2 positionDelta = center - curPosition;
				
				for ( int index = 0; index < vertices.Count; ++index )
				{
					vertices[index] -= positionDelta;
				}
				
				transform.position = new Vector3( center.x, center.y, transform.position.z );
			}
		}
		
		#endregion
	}
}

