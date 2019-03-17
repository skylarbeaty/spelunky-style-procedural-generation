using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelGenerator : MonoBehaviour {

	[System.NonSerialized] //hide public var from inspector completely
	public int gridWidth = 4, gridHeight = 4;
	Room[,] grid;
	Vector3 roomSize = new Vector3(10, 8, 0);
	float tileSize = 16;
	Texture2D[,] roomTileMaps;
	Texture2D fullMap, spawnablesMap;
	public Mappings mappingsHolder;
	TileMapping[] mappings;
	void Start()
	{
		grid = GetComponent<PathGenerator>().GeneratePath();
		roomTileMaps = ProcessMaps();
		fullMap = CombineMaps(roomTileMaps);
		spawnablesMap = GetComponent<SpawnablesGenerator>().GenerateEnemies(fullMap);

		SpawnMap();
	}
	Texture2D[,] ProcessMaps(){
		Texture2D[,] ret = new Texture2D[gridWidth,gridHeight];
		for (int x = 0; x < gridWidth; x++)
			for (int y = 0; y < gridHeight; y++)
				ret[x,y] = GetComponent<RoomProcessor>().ProcessRoom(grid[x,y]);
		return ret;
	}
	Texture2D CombineMaps(Texture2D[,] maps){
		Vector2Int fullMapSize = new Vector2Int((int)roomSize.x * gridWidth, (int)roomSize.y * gridHeight);
		Texture2D ret = BlankTex(fullMapSize.x + 2, fullMapSize.y + 2);
		for(int x = 0; x < ret.width - 2; x++){
			for(int y = 0; y < ret.height - 2; y++){
				int toSetX = Mathf.FloorToInt(x/roomSize.x);
				int toSetY = gridHeight - 1 - Mathf.FloorToInt(y/roomSize.y);
				int pixelX = x - toSetX * (int)roomSize.x;
				int pixelY = y - Mathf.FloorToInt(y/roomSize.y) * (int)roomSize.y;
				// print("maps[" + toSetX + ", " + toSetY + "] pixel at[" + pixelX + ", " + pixelY + "]");
				Color toSet = maps[toSetX,toSetY].GetPixel(pixelX, pixelY);
				ret.SetPixel(x + 1, y + 1, toSet); 
			}
		}
		//fill border
		for(int x = 0; x < ret.width; x++)
			for(int y = 0; y < ret.height; y++)
				if (x == 0 || y == 0 || x == ret.width - 1 || y == ret.height - 1)
					ret.SetPixel(x, y, Color.black); //black is ground tile 
		return ret;
	}
	public Texture2D BlankTex(int width,int length){
		//texture with all transparent pixels
		Texture2D newTex = new Texture2D(width,length);
		for (int x = 0; x < width; x++){
			for (int y = 0; y < length; y++){
				Color pixel = new Color(0,0,0,0);
				newTex.SetPixel(x,y,pixel);
			}
		}
		return newTex;
	}
	public Texture2D CopyTexture(Texture2D copyFromTex){
		Texture2D retTex = BlankTex(copyFromTex.width,copyFromTex.height);
		for (int x = 0; x < copyFromTex.width; x++){
			for (int y = 0; y < copyFromTex.height; y++){
				Color pixel = copyFromTex.GetPixel(x,y);
				retTex.SetPixel(x,y,pixel);
			}
		}
		return retTex;
	}


	void SpawnMap(){
		mappings = mappingsHolder.mappings;
		for(int x = 0; x < fullMap.width; x++){
			for (int y = 0; y < fullMap.height; y++){
				Color pixelColor = fullMap.GetPixel(x,y);
				if (pixelColor.a == 0)
					continue;
				TileMapping tile = Array.Find(mappings, TileMapping => TileMapping.color == pixelColor);
				Vector3 thisOffset = new Vector3(tileSize * (float) x , tileSize * (float) y , 0);
				Vector3 spawnPos = transform.position + thisOffset;
				if (tile != null && tile.prefab != null)//avoid errors during testing
					Instantiate(tile.prefab, spawnPos, Quaternion.identity).transform.parent = this.transform;
				else
					print("no prefab for this mapping");
			}
		}
		SpawnableMapping[] spawnablesMappings = mappingsHolder.spawnableMappings;
		for(int x = 0; x < spawnablesMap.width; x++){
			for (int y = 0; y < spawnablesMap.height; y++){
				Color pixelColor = spawnablesMap.GetPixel(x,y);
				if (pixelColor.a == 0)
					continue;
				SpawnableMapping tile = Array.Find(spawnablesMappings, SpawnableMapping => SpawnableMapping.color == pixelColor);
				Vector3 thisOffset = new Vector3(tileSize * (float) x , tileSize * (float) y , 0);
				Vector3 spawnPos = transform.position + thisOffset;
				if (tile != null && tile.prefab != null)//avoid errors during testing
					Instantiate(tile.prefab, spawnPos, Quaternion.identity).transform.parent = this.transform;
				else
					print("Could not instanciate spawnable with pixel color: " + pixelColor);
			}
		}
	}
}