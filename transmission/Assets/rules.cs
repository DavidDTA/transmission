using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rules : MonoBehaviour {

	private enum Side {Right, Left};

	private struct Position {
		public int roadSection;
		public Side side;

		public Position (int roadSection, Side side) {
			this.roadSection = roadSection;
			this.side = side;
		}
	}

	private class EnvironmentObject {
		public Position pos;
		public string type;

		public EnvironmentObject(Position pos) {
			this.pos = pos;
		}
	}

	private class Tree : EnvironmentObject {
		public Color color;

		public Tree(Position pos, Color color) {
			this.pos = pos;
			this.color = color;
		}
	}

	private class Sign : EnvironmentObject {
		public Color color;

		public Sign(Position pos, Color color) {
			this.pos = pos;
			this.color = color;
		}
	}

	private struct Level {
		public string rules;
		public Side correctTurn;
		public EnvironmentObject[] environmentObjects;

		public Level(string rules, Side correctTurn, EnvironmentObject[] environmentObjects) {
			this.environmentObjects = environmentObjects;
			this.correctTurn = correctTurn;
			this.rules = rules;
		}
	}

	private Level level1 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Left,
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
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right".
		Side.Left,
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
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right".
		Side.Right,
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
		"If you see a black tree, switch yellow and green all other rules".
		Side.Right,
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
		"If you see a black tree, switch yellow and green for all other rules".
		Side.Right,
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
		new EnvironmentObject[] {
			new Tree(new Position(2, Side.Right), Color.yellow),
			new Tree(new Position(3, Side.Left), Color.yellow),
			new Tree(new Position(5, Side.Right), Color.green),
			new Tree(new Position(5, Side.Left), Color.green),
			new Tree(new Position(6, Side.Right), Color.yellow),
		}
	);



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
