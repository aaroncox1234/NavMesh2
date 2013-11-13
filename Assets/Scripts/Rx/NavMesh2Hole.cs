using UnityEngine;
using System.Collections;

public class NavMesh2Hole : EditablePolygon2
{
	public override void OnDrawGizmosSelected()
	{
		vertexDrawColor = Color.blue;
		hoverVertexDrawColor = Color.blue;
		selectedVertexDrawColor = Color.cyan;
		edgeDrawColor = Color.blue;
		selectedEdgeDrawColor = Color.cyan;
		
		base.OnDrawGizmosSelected();
	}
}
