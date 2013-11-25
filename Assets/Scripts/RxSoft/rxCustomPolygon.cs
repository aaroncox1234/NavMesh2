using UnityEngine;
using System.Collections.Generic;

namespace RxSoft
{
	public class rxCustomPolygon : MonoBehaviour
	{
		#region Public Members

		public bool isHole = false;

		public rxCustomPolygon()
		{
		}

		public virtual void OnDrawGizmos()
		{
			InitializeIfEmpty();
			
			DrawEdges( false );
			DrawVertices( false );
		}

		public virtual void OnDrawGizmosSelected()
		{
			InitializeIfEmpty();

			DrawEdges( true);
			DrawVertices( true );
		}

		public List<Vector2> GetWorldVertices()
		{
			List<Vector2> result = new List<Vector2>( localVertices.Count );

			for ( int index = 0; index < localVertices.Count; ++index )
			{
				result.Add( GetWorldVertex2D( index ) );
			}
			
			return result;
		}

		#endregion

		#region Editor Events

		public void OnMouseMove( Vector2 mousePosition )
		{
			hoverVertexIndex = FindVertexUnderCursor( mousePosition );
		}

		public void OnMouseDown( Vector2 mousePosition )
		{
			selectedVertexIndex = FindVertexUnderCursor( mousePosition );
		}

		public void OnMouseDrag( Vector2 mousePosition )
		{
			if ( selectedVertexIndex != -1 )
			{
				localVertices[selectedVertexIndex] = mousePosition - GetPosition2D();
			}
		}

		public void InsertVertex()
		{
			int previousIndex = ( selectedVertexIndex != -1 ) ? selectedVertexIndex : localVertices.Count - 1;
			int nextIndex = ( previousIndex + 1 ) % localVertices.Count;

			Vector2 insertPosition = ( localVertices[previousIndex] + localVertices[nextIndex] ) / 2.0f;

			int newIndex = nextIndex;

			localVertices.Insert( newIndex, insertPosition );

			selectedVertexIndex = newIndex;
		}

		public void DeleteVertex()
		{
			if ( ( selectedVertexIndex != -1 ) && ( localVertices.Count > 3 ) )
			{
				localVertices.RemoveAt( selectedVertexIndex );

				--selectedVertexIndex;
			}
		}

		public void ReverseWinding()
		{
			localVertices.Reverse();

			if ( selectedVertexIndex != -1 )
			{
				selectedVertexIndex = localVertices.Count - selectedVertexIndex - 1;
			}
		}

		public void CenterGizmo()
		{
			if ( localVertices.Count == 0 )
			{
				return;
			}
			
			// Do a simple, unweighted average to find the "center" of the polygon.	
			Vector2 center = Vector2.zero;
			for ( int index = 0; index < localVertices.Count; ++index )
			{
				center += GetWorldVertex2D( index );
			}
			center /= localVertices.Count;
			
			Vector2 positionDelta = center - GetPosition2D();				
			for ( int index = 0; index < localVertices.Count; ++index )
			{
				localVertices[index] -= positionDelta;
			}
			
			SetPosition2D( center );
		}

		#endregion

		#region Private Members

		public List<Vector2> localVertices = new List<Vector2>();

		private int hoverVertexIndex = -1;
		private int selectedVertexIndex = -1;

		private int FindVertexUnderCursor( Vector2 mousePosition )
		{
			int result = -1;

			float minDistanceToHover = 14.0f;
			float sqMinDistanceToHover = minDistanceToHover * minDistanceToHover;

			float sqClosestDistance = float.MaxValue;
			
			for ( int index = 0; index < localVertices.Count; ++index )
			{
				float sqCurrentDistance = ( GetWorldVertex2D(index) - mousePosition ).SqrMagnitude();
				
				if ( ( sqCurrentDistance < sqMinDistanceToHover ) && ( sqCurrentDistance < sqClosestDistance ) )
				{
					result = index;
					sqClosestDistance = sqCurrentDistance;
				}
			}

			return result;
		}

		private Vector2 GetPosition2D()
		{
			return new Vector2( transform.position.x, transform.position.y );
		}

		private void SetPosition2D( Vector2 position)
		{
			transform.position = new Vector3( position.x, position.y, transform.position.z );
		}

		private Vector2 GetWorldVertex2D( int vertexIndex )
		{
			return GetPosition2D() + localVertices[vertexIndex];
		}

		private Vector3 GetWorldVertex3D( int vertexIndex )
		{
			return transform.position + new Vector3( localVertices[vertexIndex].x, localVertices[vertexIndex].y, GetDrawZ() );
		}

		private float GetDrawZ()
		{
			return transform.position.z;
		}

		private void DrawEdges( bool isSelected )
		{
			for ( int index = 0; index < localVertices.Count; ++index )
			{
				int nextIndex = (index + 1) % localVertices.Count;

				Vector3 start = GetWorldVertex3D( index );
				Vector3 end = GetWorldVertex3D( nextIndex );

				if ( isSelected )
				{
					Gizmos.color = Color.white;
				}
				else
				{
					Gizmos.color = Color.grey;
				}

				// When the polygon is selected in the Scene View, draw a line from the selected vertex in the direction of the polygon's winding.
				if ( isSelected && ( index == selectedVertexIndex ) )
				{
					Vector3 mid = ( end + start ) / 2.0f;

					Gizmos.color = Color.blue;
					Gizmos.DrawLine( start, mid );

					Gizmos.color = Color.white;
					Gizmos.DrawLine( mid, end );
				}
				else
				{
					Gizmos.DrawLine( start, end );
				}
			}
		}

		private void DrawVertices( bool isSelected )
		{
			// Only draw vertices when the polygon is selected in the Scene View.
			if ( !isSelected )
			{
				return;
			}

			for ( int index = 0; index < localVertices.Count; ++index )
			{
				if ( index == selectedVertexIndex )
				{
					Gizmos.color = Color.blue;
				}
				else if ( index == hoverVertexIndex )
				{
					Gizmos.color = Color.white;
				}
				else
				{
					Gizmos.color = Color.white;
				}

				Vector3 drawSize;
				if ( index == hoverVertexIndex )
				{
					drawSize = new Vector3( 15.0f, 15.0f, 1.0f );
				}
				else
				{
					drawSize = new Vector3( 10.0f, 10.0f, 1.0f );
				}
				
				Gizmos.DrawCube( GetWorldVertex3D( index ), drawSize );
			}	
		}

		private void InitializeIfEmpty()
		{
			if ( localVertices.Count == 0 )
			{
				localVertices.Add( new Vector2( -100.0f, 100.0f ) );
				localVertices.Add( new Vector2( -100.0f, -100.0f ) );
				localVertices.Add( new Vector2( 100.0f, -100.0f ) );
			}
		}
		
		#endregion
	}
}
