using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Mappings : ScriptableObject {
	public TileMapping[] mappings;
	public SpawnableMapping[] spawnableMappings;
}
