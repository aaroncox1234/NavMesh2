using UnityEngine;
using System.Collections;

public class NavMesh2Boundary : EditablePolygon2
{
	public override void OnDrawGizmosSelected()
	{
		vertexDrawColor = Color.grey;
		hoverVertexDrawColor = Color.grey;
		selectedVertexDrawColor = Color.white;
		edgeDrawColor = Color.grey;
		selectedEdgeDrawColor = Color.white;
		
		base.OnDrawGizmosSelected();
	}
}
