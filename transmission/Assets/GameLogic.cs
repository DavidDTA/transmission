using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public GameObject roadStraight;
	public GameObject roadIntersectionT;
	public GameObject roadTurn;
	public GameObject[] environmentTrees;
	public GameObject mainCamera;

	// speed constants
	private float speed = 1.8f;
	private float turnCameraSpeed = 2.3f; 
	private float rotateSpeed = 1f;


	// Used for turning
	private Quaternion targetRotation;
	private Side turnDirection = Side.Right;
	private Direction currentDirection;
	private Vector3 targetPoint;
	private Vector3 rotatePoint;
	private float turnAngle = 0f;
	private float targetAngle = 0f;



	// Positioning
	private Movement movementState = Movement.STRAIGHT;

	int roadbuildingZ = 0;
	int roadbuildingX = 0;

	// Levels
	Levels levelManager = new Levels();

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		targetPoint = levelManager.getNextLevel().instantiate (this, 0, 0, Direction.NORTH);

		targetRotation = mainCamera.transform.rotation;

		currentDirection = Direction.NORTH;
	}
	
	// Update is called once per frame
	void Update () {
		MoveCar ();

		// Listen for left and right turns.
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			turnDirection = Side.Right;
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			turnDirection = Side.Left;
		}
			
		mainCamera.transform.rotation = Quaternion.Lerp (mainCamera.transform.rotation, targetRotation , turnCameraSpeed * Time.deltaTime);
	}

	Vector3 getRotatePointOffset() {
		int x = 0;
		int y = 0;
		if (movementState == Movement.TURN_RIGHT) {
			switch (currentDirection) {
			case Direction.NORTH:
				x += 1;
				y -= 1;
				break;
			case Direction.EAST:
				x -= 1;
				y -= 1;
				break;
			case Direction.SOUTH:
				x -= 1;
				y += 1;
				break;
			case Direction.WEST:
				x += 1;
				y += 1;
				break;
			}
		} else if (movementState == Movement.TURN_LEFT) {
			switch (currentDirection) {
			case Direction.NORTH:
				x -= 1;
				y -= 1;
				break;
			case Direction.EAST:
				x -= 1;
				y += 1;
				break;
			case Direction.SOUTH:
				x += 1;
				y += 1;
				break;
			case Direction.WEST:
				x += 1;
				y -= 1;
				break;
			}
		}
		return new Vector3 (x, 0, y);
	}

	float getCurrentAngle() {
		switch (currentDirection) {
		case Direction.NORTH:
			return Mathf.PI / 2.0f;
		case Direction.EAST:
			return 0;
		case Direction.SOUTH:
			return -Mathf.PI / 2.0f;
		case Direction.WEST:
			return Mathf.PI;
		}
		return 0;
	}

	int mod(int x, int m) {
		return (x%m + m)%m;
	}

	void setTurn(Side direction) {
		switch (direction) {
		case Side.Right:
			targetRotation = mainCamera.transform.rotation * Quaternion.AngleAxis (90, Vector3.up);
			movementState = Movement.TURN_RIGHT;
			turnAngle = getCurrentAngle () + (Mathf.PI / 2f);
			targetAngle = turnAngle - Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod ((int)currentDirection + 1, 4));
			break;
		case Side.Left: 
			targetRotation = mainCamera.transform.rotation * Quaternion.AngleAxis (-90, Vector3.up);
			movementState = Movement.TURN_LEFT;
			turnAngle = getCurrentAngle () - (Mathf.PI / 2f);
			targetAngle = turnAngle + Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection - 1, 4));
			break;
		}
		targetPoint = levelManager.getNextLevel().instantiate(this, 0, 0, Direction.NORTH);
	}

	Vector3 getCarPos() {
		return new Vector3 (
			mainCamera.transform.position.x,
			0f,
			mainCamera.transform.position.z
		);
	}

	void MoveCar() {
		// Move car; either to continue straight or to turn.
		switch (movementState) {
		case Movement.STRAIGHT:
			mainCamera.transform.position += new Vector3 (
					mainCamera.transform.forward.x,
					0f,
					mainCamera.transform.forward.z
				) * speed * Time.deltaTime;
			if (Vector3.Distance(getCarPos(), targetPoint) < 1) {
				setTurn(turnDirection);
			}
			break;
		case Movement.TURN_LEFT:
			turnAngle += rotateSpeed * Time.deltaTime;
			var offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				mainCamera.transform.rotation = targetRotation;
			}
			break;
		case Movement.TURN_RIGHT:
			turnAngle -= rotateSpeed * Time.deltaTime;
			offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				mainCamera.transform.rotation = targetRotation;
			}
			break;
		}
	}

}

public enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

public enum Movement {
	STRAIGHT, TURN_LEFT, TURN_RIGHT
}

public enum Side {Right, Left};

public abstract class Road {
	private void instantiate(GameObject template, int x, int z, int rotation) {
		GameLogic.Instantiate (template, new Vector3(2 * x, 0, 2 * z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)));
	}
	public abstract void instantiate (GameLogic gameLogic, int x, int z, Direction heading);
	public abstract Vector3 environmentObjectOffset (Direction heading, Side side);
	public abstract void updatePositionAndHeading (ref int x, ref int z, ref Direction heading);
	private static Vector3 shiftFrame(Vector3 unit) {
		return unit - new Vector3 (1, 0, 1);
	}
	private static void moveTowardHeading(ref int x, ref int z, Direction heading) {
		switch (heading) {
		case Direction.NORTH:
			z++;
			break;
		case Direction.EAST:
			x++;
			break;
		case Direction.SOUTH:
			z--;
			break;
		case Direction.WEST:
			x--;
			break;
		}
	}
	public class Straight : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadStraight, x, z, (int) heading);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			return shiftFrame(new Vector3 (side == Side.Left ? .125f : 1.875f, 0, Random.value * 2));
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class TurnLeft : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadTurn, x, z, (int) heading - 1);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			float r = side == Side.Left ? .125f : 1.875f;
			float along = Random.value * Mathf.PI / 2;
			return shiftFrame(new Vector3 (Mathf.Cos(along), 0, Mathf.Sin(along)) * r);
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			heading = (Direction) (((int) heading - 1) % 4);
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class TurnRight : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadTurn, x, z, (int) heading - 2);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			float r = side == Side.Right ? .125f : 1.875f;
			float along = Random.value * Mathf.PI / 2;
			return shiftFrame(new Vector3 (1 - Mathf.Cos(along), 0, Mathf.Sin(along)) * r);
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			heading = (Direction) (((int) heading + 1) % 4);
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class IntersectionT : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 1);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			float r = .125f;
			float along = Random.value * Mathf.PI / 2;
			if (side == Side.Left) {
				return shiftFrame (new Vector3 (Mathf.Cos (along) * r, 0, Mathf.Sin (along) * r));
			}
			return shiftFrame (new Vector3 (2 - Mathf.Cos (along) * r, 0, Mathf.Sin (along) * r));
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			// Can not move from here
		}
	}
	public class BranchLeft : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 2);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			if (side == Side.Right) {
				return shiftFrame(new Vector3 (1.875f, 0, Random.value * 2));
			}
			float r = .125f;
			float along = Random.value * Mathf.PI / 2;
			if (Random.value > 0.5) {
				return shiftFrame(new Vector3 (Mathf.Cos(along) * r, 0, Mathf.Sin(along) * r));
			}
			return shiftFrame(new Vector3 (Mathf.Cos(along) * r, 0, 2 - Mathf.Sin(along) * r));
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class BranchRight : Road {
		public override void instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 1);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			if (side == Side.Left) {
				return shiftFrame(new Vector3 (.125f, 0, Random.value * 2));
			}
			float r = .125f;
			float along = Random.value * Mathf.PI / 2;
			if (Random.value > 0.5) {
				return shiftFrame(new Vector3 (2 - Mathf.Cos(along) * r, 0, Mathf.Sin(along) * r));
			}
			return shiftFrame(new Vector3 (2 - Mathf.Cos(along) * r, 0, 2 - Mathf.Sin(along) * r));
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			moveTowardHeading (ref x, ref z, heading);
		}
	}
}
	
public abstract class EnvironmentObject {
	public EnvironmentObject() {
	}

	public abstract GameObject pickTemplate (GameLogic gameLogic);
}
	
public class Tree : EnvironmentObject {
	public Color color;

	public Tree(Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		GameObject[] treeArray = gameLogic.environmentTrees;
		return treeArray[(int) (Random.value * treeArray.Length) % treeArray.Length];
	}
}
	
public class Sign : EnvironmentObject {
	public Color color;

	public Sign(Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		return null;
	}
}

public struct RoadSegment {
	public Road road;
	public KeyValuePair<Side, EnvironmentObject>[] environmentObjects;

	public RoadSegment(Road road, KeyValuePair<Side, EnvironmentObject>[] environmentObjects) {
		this.road = road;
		this.environmentObjects = environmentObjects;
	}

}
	
public struct Level {
	public string rules;
	public Side correctTurn;
	public RoadSegment[] road;

	public Level(string rules, Side correctTurn, RoadSegment[] road) {
		this.rules = rules;
		this.correctTurn = correctTurn;
		this.road = road;
	}

	public Vector3 instantiate(GameLogic gameLogic, int x, int z, Direction heading) {
		foreach (RoadSegment segment in road) {
			segment.road.instantiate (gameLogic, x, z, heading);
			foreach (KeyValuePair<Side, EnvironmentObject> environmentObject in segment.environmentObjects) {
				GameLogic.Instantiate (environmentObject.Value.pickTemplate(gameLogic), new Vector3(2 * x, 0, 2 * z) + segment.road.environmentObjectOffset(heading, environmentObject.Key), Quaternion.identity);
			}
			segment.road.updatePositionAndHeading (ref x, ref z, ref heading);
		}
		return new Vector3 (x, 0, z) * 2;
	}
}
	
class Levels {
	int levelIndex = 0;
	Level[] levels = new Level[] { level1, level2, level3, level4, level5, level6, level7, level8 }; 

	public Level getNextLevel() {
		return levels[levelIndex];
		levelIndex = (levelIndex + 1) % levels.Length;
	}

	public static Level level1 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level2 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level3 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Sign(Color.blue)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level4 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Sign(Color.red)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level5 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green all other rules",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Sign(Color.red)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level6 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.black)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level7 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level8 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});
}
