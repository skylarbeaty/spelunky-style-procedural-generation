using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnablesGenerator : MonoBehaviour {
	List<Vector2Int> allValid = new List<Vector2Int>(), treasureSpawns = new List<Vector2Int>();
	List<Vector2Int> airSpawns = new List<Vector2Int>(), groundSpawns = new List<Vector2Int>();
	List<Vector2Int> doorLocations = new List<Vector2Int>();
	Texture2D levelMap;
	struct treasureWieght{
		public  Vector2Int position;
		public float weight;
	}
	List<treasureWieght> treasureWeights = new List<treasureWieght>();
	public Texture2D GenerateEnemies(Texture2D _levelMap){
		LevelGenerator LG = GetComponent<LevelGenerator>();
		levelMap = LG.CopyTexture(_levelMap); //make sure not to edit original texture
		for (int x = 0; x < levelMap.width; x++){
			for (int y = 0; y < levelMap.height; y++){
				Color pixelColor = levelMap.GetPixel(x,y);
				if (pixelColor.a == 0.0f)
					allValid.Add(new Vector2Int(x,y));//find all empty points in level
				TileMapping tile = Array.Find(LG.mappingsHolder.mappings, TileMapping => TileMapping.color == pixelColor);
				if (tile != null && tile.tileType == TileMapping.SpecialTileType.door)
					doorLocations.Add(new Vector2Int(x,y));
			}
		}
		
		//remove positions near enterance
		List<Vector2Int> positionsToRemove = new List<Vector2Int>();
		float minDistFromDoor = 8;
		Vector2Int doorPos = (doorLocations[0].y > doorLocations[1].y) ? doorLocations[0] : doorLocations[1];
		foreach(Vector2Int pos in allValid)
			if (Vector2Int.Distance(pos,doorPos) < minDistFromDoor)
				positionsToRemove.Add(pos);
		foreach(Vector2Int posToRemove in positionsToRemove)
			allValid.Remove(posToRemove);

		foreach (Vector2Int pos in allValid){ //test for valid spawn types
			if (CanSpawnGround(pos))
				groundSpawns.Add(pos);
			if (CanSpawnAir(pos))
				airSpawns.Add(pos);
			if (CanSpawnTreasure(pos))
				treasureSpawns.Add(pos);
		}
		AssignTreasureWeights();
		return CreateEnemyTexture();
	}
	bool CanSpawnGround(Vector2Int pos){
		bool ret = true;
		if (!TileIsGround(pos + Vector2Int.down))
			ret = false;
		else if (!TileIsAir(pos + Vector2Int.up))
			ret = false;
		return ret;
	}
	bool CanSpawnAir(Vector2Int pos){
		bool ret = true;
		if (!TileIsAir(pos + Vector2Int.down))
			ret = false;
		else if (!TileIsGround(pos + Vector2Int.up))
			ret = false;
		return ret;
	}
	bool CanSpawnTreasure(Vector2Int pos){
		bool ret = true;
		if (!TileIsGround(pos + Vector2Int.down))
			ret = false;
		return ret;
	}
	bool TileIsGround(Vector2Int checkPos){
		TileMapping[] mappings = GetComponent<LevelGenerator>().mappingsHolder.mappings;
		bool ret = false;
		Color tileColor = levelMap.GetPixel(checkPos.x,checkPos.y);
		TileMapping tile = Array.Find(mappings, TileMapping => TileMapping.color == tileColor);
		if (tile != null && tile.tileType == TileMapping.SpecialTileType.ground)
			ret = true;
		return ret;
	}
	bool TileIsAir(Vector2Int checkPos){
		bool ret = false;
		if (levelMap.GetPixel(checkPos.x,checkPos.y).a == 0.0f)
			ret = true;
		return ret;
	}
	void AssignTreasureWeights(){// weight treasure to spawning in cramped spaces
		foreach (Vector2Int pos in treasureSpawns){
			float weight = 0.1f;// small starting chance to spawn
			if (TileIsGround(pos + Vector2Int.up))
				weight += 0.3f;//raise weight for each ajacent ground tile
			if (TileIsGround(pos + Vector2Int.left))
				weight += 0.3f;
			if (TileIsGround(pos + Vector2Int.right))
				weight += 0.3f;
			treasureWieght treasure = new treasureWieght();
			treasure.position = pos;
			treasure.weight = weight;
			treasureWeights.Add(treasure);
		}
	}
	Texture2D CreateEnemyTexture(){
		LevelGenerator LG = GetComponent<LevelGenerator>();
		Texture2D retTex = LG.BlankTex(levelMap.width, levelMap.height);
		SpawnableMapping[] mappings = LG.mappingsHolder.spawnableMappings;
		List<Vector2Int> takenPositions = new List<Vector2Int>();
		foreach(SpawnableMapping mapping in mappings){//add spawns for each enemy
			int numToMake = Mathf.RoundToInt(UnityEngine.Random.Range(mapping.minSpawns,mapping.maxSpawns));
			// print(mapping.name + ": " + numToMake);
			while (numToMake > 0){
				List<Vector2Int> possiblePositions;
				switch(mapping.type){
					case SpawnableMapping.SpawnableType.ground:
						possiblePositions = groundSpawns;
						break;
					case SpawnableMapping.SpawnableType.air:
						possiblePositions = airSpawns;
						break;
					default:
						possiblePositions = treasureSpawns;
						break;
				}
				if (possiblePositions.Count == 0){
					print("Out of positions");
					break;
				}
				int posIndex;
				int iteratations = 0;
				do{//select a position that hasn't been used
					posIndex = Mathf.RoundToInt( UnityEngine.Random.value * (possiblePositions.Count - 1) );
					if (iteratations > 100){
						print("iterated too long");
						break;
					}
					iteratations++;
				}while (!IsValidPosition(possiblePositions[posIndex],takenPositions,mapping));
				takenPositions.Add(possiblePositions[posIndex]);
				Vector2Int pos = possiblePositions[posIndex];
				retTex.SetPixel(pos.x,pos.y,mapping.color);
				numToMake--;
			}
		}
		return retTex;
	}
	bool IsValidPosition(Vector2Int pos, List<Vector2Int> takenPositions, SpawnableMapping mapping){
		bool ret = true;
		if (takenPositions.Contains(pos))
			ret = false;//dont pick used position
		if (takenPositions.Contains(pos + Vector2Int.down) || takenPositions.Contains(pos + Vector2Int.up)
			|| takenPositions.Contains(pos + Vector2Int.left) || takenPositions.Contains(pos + Vector2Int.right))
			ret = false;//dont pick adjacent to used position
		if (mapping.type == SpawnableMapping.SpawnableType.treasure){
			float weight = treasureWeights.Find(treasureWieght => treasureWieght.position == pos).weight;
			if (weight < UnityEngine.Random.value)
				ret = false;// weight treasure spawning
		}
		return ret;
	}
}
