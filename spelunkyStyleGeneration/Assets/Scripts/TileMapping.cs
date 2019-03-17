using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMapping {
	public Color color;
	public GameObject prefab;
	public float weight;//optional for probablistic blocks
	public enum SpecialTileType {none,groundBlock,airBlock,ground,door};
	public SpecialTileType tileType = SpecialTileType.none;
}
