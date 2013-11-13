using UnityEngine;
using System.Collections;

public class Helpers
{
	public static Vector3 AsVector3( Vector2 vector, float z )
	{
		return new Vector3( vector.x, vector.y, z );
	}
	
	public static Vector2 AsVector2( Vector3 vector )
	{
		return new Vector2( vector.x, vector.y );
	}
}
