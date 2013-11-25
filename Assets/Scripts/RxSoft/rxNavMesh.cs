using UnityEngine;
using System.Collections.Generic;

namespace RxSoft
{
	public class rxNavMesh : MonoBehaviour
	{
		public rxProcessingSet ProcessingSet { get; set; }

		public virtual void OnDrawGizmosSelected()
		{
			if ( ProcessingSet != null )
			{
				foreach ( rxProcessingPolygon polygon in ProcessingSet.additivePolygons )
				{
					DrawPolygon( polygon.vertices, Color.white, transform.position.z );
				}

				foreach ( rxProcessingPolygon polygon in ProcessingSet.subtractivePolygons )
				{
					DrawPolygon( polygon.vertices, Color.blue, transform.position.z );
				}
			}
		}

		private void DrawPolygon( List<Vector2> polygon, Color color, float drawZ )
		{
			Gizmos.color = color;
			
			for ( int edgeStartIndex = 0; edgeStartIndex < polygon.Count; ++edgeStartIndex )
			{
				int edgeEndIndex = ( edgeStartIndex + 1 ) % polygon.Count;
				
				Gizmos.DrawLine( new Vector3( polygon[edgeStartIndex].x, polygon[edgeStartIndex].y, drawZ ), new Vector3( polygon[edgeEndIndex].x, polygon[edgeEndIndex].y, drawZ ) );
			}
		}
	}
}