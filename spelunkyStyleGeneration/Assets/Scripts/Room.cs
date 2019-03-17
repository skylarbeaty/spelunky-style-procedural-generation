using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
	public enum roomType {side, normal, drop, landing, dropThrough}
	public roomType type = roomType.side;
	public bool enter = false, exit = false;
	public Room (roomType _type, bool _enter, bool _exit){
		type = _type;
		enter = _enter;
		exit = _exit;
	}
}
