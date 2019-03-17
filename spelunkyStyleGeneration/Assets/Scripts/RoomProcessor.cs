using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomProcessor : MonoBehaviour {
	Texture2D tileMap, tileMapProcessed, tileMapProcessing;
	public Room refRoom;
	public Texture2D[] sideTiles, normalTiles, dropTiles, dropThroughTiles, landingTiles;
	public Texture2D[] groundBlocks, airBlocks, enterGroundBlocks, enterAirBlocks;
	Texture2D[] tileSet;
	public Mappings mappingsHolder;
	TileMapping[] mappings;
	bool needToSetDoor = false;
	public Texture2D ProcessRoom (Room _refRoom) {//input/output of system
		refRoom = _refRoom;
		mappings = mappingsHolder.mappings;
		if(refRoom.exit || refRoom.enter)
			needToSetDoor = true; //to check when setting blocks
		tileSet = PickTileMapSet(refRoom.type);
		tileMap = PickTileMap(tileSet);
		ProcessTileMap();
		return tileMapProcessed;
	}

	Texture2D[] PickTileMapSet(Room.roomType type){
		Texture2D[] ret = sideTiles;
		switch(type){
		case Room.roomType.side:
			ret = sideTiles;
			break;
		case Room.roomType.normal:
			ret = normalTiles;
			break;
		case Room.roomType.drop:
			ret = dropTiles;
			break;
		case Room.roomType.landing:
			ret = landingTiles;
			break;
		case Room.roomType.dropThrough:
			ret = dropThroughTiles;
			break;
		}
		return ret;
	}

	Texture2D PickTileMap(Texture2D[] TileMaps){
		int index = Mathf.RoundToInt(UnityEngine.Random.value * (TileMaps.Length - 1));
		return TileMaps[index];
	}

	void ProcessTileMap(){//process all probability tiles and add blocks
		LevelGenerator LG = GetComponent<LevelGenerator>();
		tileMapProcessing = LG.CopyTexture(tileMap);//make a copy to edit
		tileMapProcessed = LG.BlankTex(tileMap.width, tileMap.height);//new map to write to
		for(int x = 0; x < tileMapProcessing.width; x++){
			for (int y = tileMapProcessing.height; y >= 0; y--){
				Color lookingAtColor = tileMapProcessing.GetPixel(x,y);
				TileMapping thisMapping = null;
				Color processedColor = ProcessColor(lookingAtColor,ref thisMapping);
				if (thisMapping != null){
					if (thisMapping.tileType == TileMapping.SpecialTileType.groundBlock){
						InsertBlock(x,y,true);
						continue;
					}
					if (thisMapping.tileType == TileMapping.SpecialTileType.airBlock){
						InsertBlock(x,y,false);
						continue;
					}
				}
				tileMapProcessed.SetPixel(x,y,processedColor);
			}
		}
		float chanceToFlip = 0.5f;
		if (UnityEngine.Random.value > chanceToFlip){//flip entire map half the time
			tileMapProcessed = FlipTexture(tileMapProcessed);
		}
	}
	Color ProcessColor(Color startColor){
		TileMapping reference = null;
		return ProcessColor(startColor,ref reference);
	}
	Color ProcessColor(Color startColor, ref TileMapping mappingHolder){
		Color retColor = new Color(1,1,1,0);
		if (startColor.a != 0.0f){//skip blanks
			TileMapping lookingAt = Array.Find(mappings, TileMapping => TileMapping.color == startColor);
			if (lookingAt == null)
				print("Mapping not found for color: " + startColor);
			else if (UnityEngine.Random.value < lookingAt.weight){
				retColor = startColor;//roll for probabilistic tiles
				mappingHolder = lookingAt;
			}
		}
		return retColor;
	}
	Texture2D FlipTexture(Texture2D tex){//flips around x
		LevelGenerator LG = GetComponent<LevelGenerator>();
		Texture2D retTex = LG.BlankTex(tex.width,tex.height);
		for (int x = 0; x < tex.width; x++){
			for (int y = 0; y < tex.height; y++){
				Color pixel = tex.GetPixel(x,y);
				retTex.SetPixel(tex.width - 1 - x, y, pixel);
			}
		}
		return retTex;
	}
	void InsertBlock(int startX, int startY, bool isGround){
		Texture2D block;
		if (needToSetDoor){
			if (isGround)
				block = PickTileMap(enterGroundBlocks);
			else
				block = PickTileMap(enterAirBlocks);
			needToSetDoor = false;
		}else if (isGround)
			block = PickTileMap(groundBlocks);
		else
			block = PickTileMap(airBlocks);
		float chanceToFlip = 0.5f;
		if (UnityEngine.Random.value < chanceToFlip){
			block = FlipTexture(block);//flip the block half the time
		}
		for(int x = block.width -1; x >= 0; x--){//add block to tiles to be processed
			for (int y = block.height -1; y >= 0; y--){
				Color lookingAtColor = block.GetPixel(x,y);
				if (x == 0 && y == block.height -1){
					lookingAtColor = ProcessColor(lookingAtColor);//first would miss processing in callee
					tileMapProcessed.SetPixel(startX, startY, lookingAtColor);
				}
				tileMapProcessing.SetPixel(startX + x, startY - (block.height - 1 - y), lookingAtColor);
			}
		}
	}
}
