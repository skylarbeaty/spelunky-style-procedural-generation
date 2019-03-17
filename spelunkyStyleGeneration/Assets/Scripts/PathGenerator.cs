using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour {
	int gridWidth, gridHeight;
	Room[,] grid;
	Room lastRoom;
	int dir = 0;
	Vector2Int lastSpawnPos;
	public Room[,] GeneratePath () {
		LevelGenerator LG = GetComponent<LevelGenerator>();
		gridWidth = LG.gridWidth;
		gridHeight = LG.gridHeight;
		grid = new Room[gridWidth, gridHeight];
		FindPath();
		FillEmpties();
		SetDropThrough();
		PrintPath();
		return grid;
	}

	void FindPath(){
		//start room
		int x = 0;
		int y = 0;
		x = Mathf.RoundToInt(Random.value * (gridWidth - 1));
		grid[x,y] = new Room(Room.roomType.normal,true,false);
		lastRoom = grid[x,y];
		lastSpawnPos = new Vector2Int(x,y);
		// setup for loop
		Vector2Int nextSpace;
		int cycles = 0;
		int maxCycles = 1000;
		//create rest of rooms on main path
		while (cycles < maxCycles){
			if (dir == 0) //for start and if last move was down
				nextSpace = NextSpaceRandomizer(lastSpawnPos.x, lastSpawnPos.y);
			else //else pick b/w dir and down
				nextSpace = DirectionalNextSpaceRandomizer(lastSpawnPos.x, lastSpawnPos.y,dir);
			if (MustGoDown(nextSpace)){ //check if over the edge and needs to go down
				dir = -dir;
				nextSpace = lastSpawnPos + Vector2Int.up;
			}
			if (AtBottom(nextSpace)){ //if picked down && at bottom: set as end, break
				lastRoom.exit = true;
				return;
			}
			bool goingDown = (lastSpawnPos.y < nextSpace.y);
			if (goingDown)
				lastRoom.type = Room.roomType.drop;//room above needs exit on bottom
			grid[nextSpace.x, nextSpace.y] = new Room(Room.roomType.normal, false, false);
			lastRoom = grid[nextSpace.x, nextSpace.y];
			lastSpawnPos = nextSpace;
			if (goingDown)
				lastRoom.type = Room.roomType.landing;//new room needs opening on top
			cycles++;
			if (cycles == maxCycles)//may want to restart scene in this case. Not sure this is possible, though
				print("Error: loop creating rooms went over 1000 interations");
		}
	}

	Vector2Int NextSpaceRandomizer(int x, int y){//selects space to the left/right/down
		Vector2Int currentPos = new Vector2Int(x,y);
		float rand = Random.value * 5;
		if (rand < 2){
			dir = -1;
			return currentPos + Vector2Int.left;
		}
		if (rand < 4){
			dir = 1;
			return currentPos + Vector2Int.right;
		}
		dir = 0;
		return currentPos + Vector2Int.up;
	}

	Vector2Int DirectionalNextSpaceRandomizer(int x, int y, int direction){//selects b/w current dir and down
		Vector2Int currentPos = new Vector2Int(x,y);
		float rand = Random.value * 5;
		if (rand < 4)
			return currentPos + (Vector2Int.right * direction);
		dir = 0;
		return currentPos + Vector2Int.up;
	}

	bool MustGoDown(Vector2 pos){//true if space is off grid (left/right)
		if (pos.x < 0)
			return true;
		if (pos.x > gridWidth - 1)
			return true;
		return false;
	}
	bool AtBottom(Vector2 pos){//true if space is off grid (down only)
		return (pos.y > gridHeight - 1);
	}

	void SetDropThrough(){//accounts for cases where path drops twice in a row
		for (int x = 0; x < gridWidth; x++){
			for (int y = 1; y < gridHeight; y++){
				Room thisRoom = grid[x,y];
				if (thisRoom.type == Room.roomType.drop)
					if (grid[x,y-1].type == Room.roomType.drop || grid[x,y-1].type == Room.roomType.dropThrough)
						thisRoom.type = Room.roomType.dropThrough;
			}
		}
	}

	void FillEmpties(){
		for (int x = 0; x < gridWidth; x++){
			for (int y = 0; y < gridHeight; y++){
				if (grid[x,y] != null)
					continue;
				grid[x,y] = new Room(Room.roomType.side, false, false);
			}
		}
	}

	void PrintPath(){
		for (int y = 0; y < gridHeight; y++){
			string line = "[ ";
			for (int x = 0; x < gridWidth; x++){
				line += TypeNumber(grid[x,y].type) + " ";
			}
			line += "]";
			print (line);
		}
	}
	int TypeNumber(Room.roomType type){//converts data for PrintPath
		switch(type){
			case Room.roomType.normal:
				return 1;
			case Room.roomType.drop:
				return 2;
			case Room.roomType.landing:
				return 3;
			case Room.roomType.dropThrough:
				return 4;
			default:
				return 0;
		}
	}
}
