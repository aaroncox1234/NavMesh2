using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RxSoft
{
	[CustomEditor(typeof(rxNavMesh), true)]
	public class rxNavMeshEditor : Editor
	{
		#region Public Members

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			rxNavMesh targetNavMesh = target as rxNavMesh;
			
			GUILayout.BeginVertical();
			
			bool collectPolygons = GUILayout.Button( "Step 1: Collect Polygons" );
			bool clip = GUILayout.Button( "Step 2: Clip Polygons" );
			bool integrateHoles = GUILayout.Button( "Step 3: Integrate Holes" );
			bool triangulate = GUILayout.Button( "Step 4: Triangulate" );
			bool simplify = GUILayout.Button( "Step 5: Simplify" );
			bool createNavMesh = GUILayout.Button( "Step 6: Create NavMesh" );

			GUILayout.EndVertical();

			if ( collectPolygons )
			{
				CollectPolygons();

				targetNavMesh.ProcessingSet = builder.ProcessingSet;
				EditorUtility.SetDirty( targetNavMesh );
			}
		}

		#endregion

		#region Private Members

		private rxNavMeshBuilder builder = null;

		private void CollectPolygons()
		{
			rxCustomPolygon[] polygons = FindObjectsOfType( typeof(rxCustomPolygon) ) as rxCustomPolygon[];

			rxProcessingSet processingSet = new rxProcessingSet();

			foreach ( rxCustomPolygon polygon in polygons )
			{
				rxProcessingPolygon processingPolygon = new rxProcessingPolygon( polygon.GetWorldVertices() );

				if ( polygon.isHole )
				{
					processingSet.subtractivePolygons.Add( processingPolygon );
				}
				else
				{
					processingSet.additivePolygons.Add( processingPolygon );
				}
			}

			builder = new rxNavMeshBuilder();
			builder.ProcessingSet = processingSet;
		}

		#endregion
	}
}
