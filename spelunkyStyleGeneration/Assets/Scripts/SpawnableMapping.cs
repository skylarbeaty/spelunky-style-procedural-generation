using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableMapping {
	public string name;
	public GameObject prefab;
	public Color color;
	public enum SpawnableType {ground,air,treasure};
	public SpawnableType type;
	public int minSpawns;
	public int maxSpawns;
}
