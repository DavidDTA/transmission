using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public GameObject roadStraight;
	public GameObject roadIntersectionT;
	public GameObject roadTurn;
	public GameObject[] environmentTrees;

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		instantiateRoad (Road.STRAIGHT, 0, 0, Direction.NORTH);
		instantiateRoad (Road.TURN_LEFT, 0, 1, Direction.NORTH);
		instantiateRoad (Road.TURN_RIGHT, -1, 1, Direction.WEST);
		instantiateRoad (Road.BRANCH_LEFT, -1, 2, Direction.NORTH);
		instantiateRoad (Road.TURN_RIGHT, -1, 3, Direction.NORTH);
		instantiateRoad (Road.TURN_LEFT, 0, 3, Direction.EAST);
		instantiateRoad (Road.INTERSECTION_T, 0, 4, Direction.NORTH);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private GameObject instantiateRoad(Road roadType, int x, int z, Direction heading) {
		switch (roadType) {
		case Road.STRAIGHT:
			return instantiateRoad (roadStraight, x, z, (int) heading);
		case Road.TURN_LEFT:
			return instantiateRoad (roadTurn, x, z, (int) heading - 1);
		case Road.TURN_RIGHT:
			return instantiateRoad (roadTurn, x, z, (int) heading - 2);
		case Road.BRANCH_LEFT:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading);
		case Road.BRANCH_RIGHT:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading - 2);
		case Road.INTERSECTION_T:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading - 1);
		default:
			return null;
		}
	}

	private GameObject instantiateRoad(GameObject roadType, int x, int z, int rotation) {
		return Instantiate (roadType, new Vector3(2 * x, 0, 2 * z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)));
	}
}

enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

enum Side {Right, Left};

enum Road {
	STRAIGHT, TURN_LEFT, TURN_RIGHT, INTERSECTION_T, BRANCH_LEFT, BRANCH_RIGHT
}
	
struct Position {
	public int roadSection;
	public Side side;

	public Position (int roadSection, Side side) {
		this.roadSection = roadSection;
		this.side = side;
	}
}
	
class EnvironmentObject {
	public Position pos;

	public EnvironmentObject(Position pos) {
		this.pos = pos;
	}
}
	
class Tree : EnvironmentObject {
	public Color color;

	public Tree(Position pos, Color color) : base(pos) {
		this.color = color;
	}
}
	
class Sign : EnvironmentObject {
	public Color color;

	public Sign(Position pos, Color color) : base(pos) {
		this.color = color;
	}
}
	
struct Level {
	public string rules;
	public Side correctTurn;
	public int roadLength;
	public EnvironmentObject[] environmentObjects;

	public Level(string rules, Side correctTurn, int roadLength, EnvironmentObject[] environmentObjects) {
		this.environmentObjects = environmentObjects;
		this.correctTurn = correctTurn;
		this.rules = rules;
		this.roadLength = roadLength;
	}
}

public class Levels {

	private Level level1 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Left,
		10,
		new EnvironmentObject[] {
			new Tree(new Position(1, Side.Left), Color.yellow),
			new Tree(new Position(2, Side.Right), Color.green),
			new Tree(new Position(4, Side.Left), Color.yellow),
			new Tree(new Position(7, Side.Left), Color.green),
			new Tree(new Position(10, Side.Right), Color.yellow),
		}
	);

	private Level level2 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Right,
		5,
		new EnvironmentObject[] {
			new Tree(new Position(1, Side.Right), Color.yellow),
			new Tree(new Position(2, Side.Right), Color.green),
			new Tree(new Position(4, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(5, Side.Right), Color.yellow),
		}
	);

	private Level level3 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Left,
		8,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(2, Side.Left), Color.yellow),
			new Tree(new Position(2, Side.Right), Color.green),
			new Tree(new Position(4, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(5, Side.Left), Color.yellow),
			new Sign(new Position(8, Side.Right), Color.blue),
		}
	);

	private Level level4 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Right,
		8,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(2, Side.Left), Color.yellow),
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(4, Side.Left), Color.green),
			new Tree(new Position(5, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Right), Color.green),
			new Sign(new Position(8, Side.Left), Color.red),
		}
	);

	private Level level5 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green all other rules",
		Side.Right,
		8,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.green),
			new Tree(new Position(2, Side.Left), Color.yellow),
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(4, Side.Left), Color.green),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(5, Side.Right), Color.yellow),
			new Sign(new Position(8, Side.Left), Color.red),
		}
	);

	private Level level6 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules",
		Side.Right,
		7,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(3, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Right), Color.green),
			new Tree(new Position(5, Side.Left), Color.black),
			new Tree(new Position(6, Side.Right), Color.yellow),
		}
	);

	private Level level7 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Left,
		6,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Left), Color.yellow),
			new Tree(new Position(3, Side.Left), Color.green),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(5, Side.Right), Color.yellow),
			new Tree(new Position(5, Side.Right), Color.yellow),
			new Tree(new Position(6, Side.Right), Color.yellow),
		}
	);

	private Level level8 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Right,
		6,
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(3, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Right), Color.green),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(6, Side.Right), Color.yellow),
		}
	);
}
