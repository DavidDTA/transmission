﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public const string SERVER_URL = "http://9e24ab9f.ngrok.io";
	public const string BEGINNING_INSTRUCTIONS = "You're on the phone with your friend Tran. He's lost. In other words, Tran's Missin'. Help him get home! Use these instructions to guide him home.";

	public GameObject roadStraight;
	public GameObject roadIntersectionT;
	public GameObject roadTurn;
	public GameObject[] environmentTrees;
	public GameObject car;
	public Color[] greens;
	public Color[] yellows;
	public Color[] reds;
	public Color[] blues;

	public List<GameObject> garbage = new List<GameObject> ();

	//
	private Vector3 carInitalPosition;

	// speed constants
	private const float speed = 2.8f;
	private const float turnCameraSpeed = 3.8f; 
	private const float rotateSpeed = 1.5f;


	// Used for turning
	private Quaternion targetRotation;
	private Movement turnDirection = Movement.STRAIGHT;
	private Direction currentDirection;

	private int targetX = 0;
	private int targetZ = 0;
	private Direction targetHeading = Direction.NORTH;
	private Vector3 rotatePoint;
	private float turnAngle = 0f;
	private float targetAngle = 0f;



	// Positioning
	private Movement movementState = Movement.STRAIGHT;

	// Levels
	Levels levelManager = new Levels();

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);

		carInitalPosition = car.transform.position;
		targetRotation = car.transform.rotation;

		leftArrow = car.transform.Find ("LeftArrow").gameObject;
		rightArrow = car.transform.Find ("RightArrow").gameObject;

		sendInstructions (BEGINNING_INSTRUCTIONS);
	}
	void start() {
		car.transform.position = carInitalPosition;

		turnDirection = Movement.STRAIGHT;
		SetBlink (turnDirection);
		currentDirection = Direction.NORTH;

		targetX = 0;
		targetZ = 0;
		targetHeading = Direction.NORTH;
		rotatePoint = Vector3.zero;
		turnAngle = 0;
		targetAngle = 0;

		movementState = Movement.STRAIGHT;

		foreach (GameObject gar in garbage) {
			GameLogic.Destroy (gar);
		}
		levelManager.restart ();
		garbage = levelManager.getCurrentLevel().instantiate (this, ref targetX, ref targetZ, ref targetHeading);
	}
	
	// Update is called once per frame
	void Update () {
		MoveCar ();

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (movementState == Movement.STOP) {
				start ();
			}
		}

		// Listen for left and right turns.
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			turnDirection = Movement.TURN_RIGHT;
			SetBlink (Movement.TURN_RIGHT);
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			turnDirection = Movement.TURN_LEFT;
			SetBlink (Movement.TURN_LEFT);
		}
			
		car.transform.rotation = Quaternion.Lerp (car.transform.rotation, targetRotation , turnCameraSpeed * Time.deltaTime);
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

	void setTurn(Movement direction) {
		Vector3 targetPoint = new Vector3 (2 * targetX, 0, 2 * targetZ);
		switch (direction) {
		case Movement.TURN_RIGHT:
			targetRotation = car.transform.rotation * Quaternion.AngleAxis (90, Vector3.up);
			movementState = Movement.TURN_RIGHT;
			turnAngle = getCurrentAngle () + (Mathf.PI / 2f);
			targetAngle = turnAngle - Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod ((int)currentDirection + 1, 4));
			break;
		case Movement.TURN_LEFT: 
			targetRotation = car.transform.rotation * Quaternion.AngleAxis (-90, Vector3.up);
			movementState = Movement.TURN_LEFT;
			turnAngle = getCurrentAngle () - (Mathf.PI / 2f);
			targetAngle = turnAngle + Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection - 1, 4));
			break;
		case Movement.STRAIGHT: 
			targetRotation = car.transform.rotation * Quaternion.AngleAxis (-90, Vector3.up);
			movementState = Movement.TURN_LEFT;
			turnAngle = getCurrentAngle () - (Mathf.PI / 2f);
			targetAngle = turnAngle + Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection - 1, 4));
			break;
		}
	}

	Vector3 getCarPos() {
		return new Vector3 (
			car.transform.position.x,
			0f,
			car.transform.position.z
		);
	}

	void gameOver () {
		sendInstructions (BEGINNING_INSTRUCTIONS);
		movementState = Movement.STOP;
	}

	void MoveCar() {
		Vector3 targetPoint = new Vector3 (2 * targetX, 0, 2 * targetZ);
		// Move car; either to continue straight or to turn.
		switch (movementState) {
		case Movement.STRAIGHT:
			car.transform.Translate(Vector3.forward * speed * Time.deltaTime);
			if (Vector3.Distance (getCarPos (), targetPoint) < 1) {
				if (levelManager.getCurrentLevel ().correctTurn == Side.Left && turnDirection == Movement.TURN_LEFT) {
					setTurn (Movement.TURN_LEFT);
					new Road.TurnLeft().updatePositionAndHeading(ref targetX, ref targetZ, ref targetHeading);
					levelManager.advance ();
					garbage.AddRange(levelManager.getCurrentLevel().instantiate(this, ref targetX, ref targetZ, ref targetHeading));
				} else if (levelManager.getCurrentLevel ().correctTurn == Side.Right && turnDirection == Movement.TURN_RIGHT) {
					setTurn (Movement.TURN_RIGHT);
					new Road.TurnRight().updatePositionAndHeading(ref targetX, ref targetZ, ref targetHeading);
					levelManager.advance ();
					garbage.AddRange(levelManager.getCurrentLevel().instantiate(this, ref targetX, ref targetZ, ref targetHeading));
				} else {
					gameOver ();
				}
			}
			break;
		case Movement.TURN_LEFT:
			turnAngle += rotateSpeed * Time.deltaTime;
			var offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			car.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				car.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				car.transform.rotation = targetRotation;
				SetBlink (Movement.STRAIGHT);
			}
			break;
		case Movement.TURN_RIGHT:
			turnAngle -= rotateSpeed * Time.deltaTime;
			offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			car.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				car.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				car.transform.rotation = targetRotation;
				SetBlink (Movement.STRAIGHT);
			}
			break;
		}
	}

	GameObject leftArrow, rightArrow;



	public void SetBlink(Movement direction) {
		resetArrows ();

		if (direction == Movement.TURN_RIGHT) {
			StartCoroutine ("Blink", rightArrow);
		} else if (direction == Movement.TURN_LEFT) {
			StartCoroutine ("Blink", leftArrow);
		}
	}

	void resetArrows() {
		StopAllCoroutines ();
				rightArrow.GetComponent<Renderer> ().material.color = Color.gray;
				leftArrow.GetComponent<Renderer> ().material.color = Color.gray;
	}

	IEnumerator Blink(GameObject arrow) {
		while (true) {
			if (arrow.GetComponent<Renderer> ().material.color.Equals(Color.yellow)) {
				arrow.GetComponent<Renderer> ().material.color = Color.gray;
			} else {
				arrow.GetComponent<Renderer> ().material.color = Color.yellow;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
	public void sendInstructions(string rules) {
		byte[] data = System.Text.Encoding.UTF8.GetBytes (rules);
		byte[] array = new byte[1000];
		for (int i = 0; i < data.Length; i++) {
			array [i] = data [i];
		}
		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add ("content-length", data.Length.ToString());
		new WWW (GameLogic.SERVER_URL, array, headers);
	}
}

public enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

public enum Movement {
	STRAIGHT, TURN_LEFT, TURN_RIGHT, STOP
}

public enum Side {Right, Left};


public abstract class Road {
	private GameObject instantiate(GameObject template, int x, int z, int rotation) {
		return GameLogic.Instantiate (template, new Vector3(2 * x, 0, 2 * z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)));
	}
	public abstract GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading);
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
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadStraight, x, z, (int) heading);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			Vector3 offset = shiftFrame(new Vector3 (side == Side.Left ? .125f : 1.875f, 0, Random.value * 2));
			if (heading == Direction.EAST || heading == Direction.WEST) {
				return new Vector3 (offset.z, 0, offset.x);
			}
			return offset;
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class TurnLeft : Road {
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadTurn, x, z, (int) heading - 1);
		}
		public override Vector3 environmentObjectOffset(Direction heading, Side side) {
			float r = side == Side.Left ? .125f : 1.875f;
			float along = Random.value * Mathf.PI / 2;
			return shiftFrame(new Vector3 (Mathf.Cos(along), 0, Mathf.Sin(along)) * r);
		}
		public override void updatePositionAndHeading (ref int x, ref int z, ref Direction heading) {
			heading = (Direction) (((int) heading + 3) % 4);
			moveTowardHeading (ref x, ref z, heading);
		}
	}
	public class TurnRight : Road {
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadTurn, x, z, (int) heading - 2);
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
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 1);
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
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 2);
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
		public override GameObject instantiate (GameLogic gameLogic, int x, int z, Direction heading) {
			return instantiate (gameLogic.roadIntersectionT, x, z, (int) heading - 1);
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
		
	protected T randomChoice<T>(T[] array) {
		float value = Random.value;
		if (value >= 1) {
			value = 0;
		}
		return array [(int)(Random.value * array.Length)];
	}

	protected Color[] getColorArray(GameLogic gameLogic, Color color) {
		if (color == Color.yellow) {
			return gameLogic.yellows;
		}
		if (color == Color.green) {
			return gameLogic.greens;
		}
		if (color == Color.red) {
			return gameLogic.reds;
		}
		if (color == Color.blue) {
			return gameLogic.blues;
		}
		throw new MissingReferenceException ("Unknown color");
	}

	public abstract GameObject instantiate (GameLogic gameLogic, Vector3 position, Quaternion rotation);
}
	
public class Tree : EnvironmentObject {
	public Color color;

	public Tree(Color color) {
		this.color = color;
	}

	public override GameObject instantiate(GameLogic gameLogic, Vector3 position, Quaternion rotation) {
		GameObject tree = GameLogic.Instantiate (randomChoice(gameLogic.environmentTrees), position, rotation);
		tree.GetComponent<Renderer>().materials[1].color = randomChoice(getColorArray(gameLogic, this.color));
		return tree;
	}
}

public class Sign : EnvironmentObject {
	public Color color;

	public Sign(Color color) {
		this.color = color;
	}

	public override GameObject instantiate(GameLogic gameLogic, Vector3 position, Quaternion rotation) {
		return new Tree (Color.blue).instantiate (gameLogic, position, rotation);
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

	public List<GameObject> instantiate(GameLogic gameLogic, ref int x, ref int z, ref Direction heading) {
		List<GameObject> garbage = new List<GameObject> ();
		gameLogic.sendInstructions (rules);
		foreach (RoadSegment segment in road) {
			garbage.Add(segment.road.instantiate (gameLogic, x, z, heading));
			foreach (KeyValuePair<Side, EnvironmentObject> environmentObject in segment.environmentObjects) {
				garbage.Add(environmentObject.Value.instantiate(gameLogic, new Vector3(2 * x, 0, 2 * z) + segment.road.environmentObjectOffset(heading, environmentObject.Key), Quaternion.identity));
			}
			segment.road.updatePositionAndHeading (ref x, ref z, ref heading);
		}
		return garbage;
	}
}
	
class Levels {
	int levelIndex = 0;
	Level[] levels = new Level[] { level1, level2, level3, level4, level5, level6, level7, level8 }; 

	public void advance() {
		levelIndex = (levelIndex + 1) % levels.Length;
	}

	public Level getCurrentLevel() {
		return levels [levelIndex];
	}

	public void restart() {
		levelIndex = 0;
	}

	public static Level level1 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br> <br><br> <br><br> ",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
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
		"If one side of the road has more yellow trees, turn to that side<br><br> <br><br> <br><br> ",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level3 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br> <br><br> ",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
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
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Sign(Color.blue)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level4 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br> <br><br> ",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),			
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.blue)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.red)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level5 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br>" +
		"If you see a blue tree, count green trees instead of yellow<br><br> ",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level6 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br>" +
		"If you see a blue tree, count green trees instead of yellow<br><br> ",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.blue)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level7 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br>" +
		"If you see a blue tree, count green trees instead of yellow<br><br>" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.red)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});

	public static Level level8 = new Level(
		"If one side of the road has more yellow trees, turn to that side<br><br>" +
		"UNLESS there is a red tree. Red trees mean turn right always<br><br>" +
		"If you see a blue tree, count green trees instead of yellow<br><br>" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
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
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(new Road.Straight(), new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(new Road.IntersectionT(), new KeyValuePair<Side, EnvironmentObject>[] {}),
		});
}
