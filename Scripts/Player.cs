using Godot;

public partial class Player : RigidBody3D
{

	//
	//
	//   The problem is that the staticbody3d rotation.x is always positive so it can't get down easily unless there is a way to make it negative while going down.
	// 	
	//		Good luck

	[ExportGroup("Player Settings")]
	[Export] public float xSensitivity;
	[Export] public float ySensitivity;
	[ExportGroup("Player Logic")]
	[Export] public float MaxSpeed;

	private Camera3D Camera;
	private Node3D Head;
	private Timer Timer_for_calculating_speed;
	private float speed_counter;
	private float falling_counter;
	private float falling_speed_limit_until_start_rolling = 10f;
	private float timer_rolling;
	private float WaitTime_until_the_player_stops_rolling = 1.5f;
	private bool First_push = true;

	Vector3 last_player_position = Vector3.Zero;


	public override void _Ready()
	{
		Camera = GetNode<Camera3D>("Head/Camera3D");
		Head = GetNode<Node3D>("Head");

		Timer_for_calculating_speed = GetNode<Timer>("Timer_for_calculating_speed");
		Timer_for_calculating_speed.Timeout += () => Timer_for_calculating_speed_timeout();
	}


    public override void _PhysicsProcess(double delta)
    {

		if(Input.MouseMode != Input.MouseModeEnum.Captured && Input.IsActionJustPressed("Show_Hide_mouse"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else if(Input.IsActionJustPressed("Show_Hide_mouse"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}



		Vector2 inputDir = Input.GetVector("Move_left", "Move_right", "Move_forward", "Move_backward");
		Vector3 direction = (Head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		Vector3 velocity = direction * MaxSpeed * (float)delta;


		if(LockRotation)
		{
			if(!(speed_counter > MaxSpeed) && !First_push)
			{
				ApplyCentralImpulse(velocity * 2f);
			}
			else if(First_push)
			{
				ApplyCentralImpulse(velocity * 3.5f);
				First_push = false;
			}
			else
			{
				ApplyCentralImpulse(velocity / 1.5f);
			}
		}

		if((speed_counter <= 1f) && !First_push)
		{
			First_push = true;
		}

		// Rolling Around stuff
		Rolling(delta);
		
    }


    public override void _Input(InputEvent @event)
	{
		if(Camera.Current)
		{
			if(@event is InputEventMouseMotion)
			{
				InputEventMouseMotion mouseMotion = @event as InputEventMouseMotion;
				Head.RotateY(-mouseMotion.Relative.X * xSensitivity * 0.0001f);
				Camera.RotateX(-mouseMotion.Relative.Y * ySensitivity * 0.0001f);

				// Preventing rotation around the body ( 360 rotation )
				Vector3 Camera_rotation = Camera.Rotation;
				Camera_rotation.X = Mathf.Clamp(Camera_rotation.X, Mathf.DegToRad(-90.0f), Mathf.DegToRad(90.0f));
				Camera.Rotation = Camera_rotation;
			}
		}
	}


	// Here is to calculate speed of the Player
	public void Timer_for_calculating_speed_timeout()
	{

		float calculate_the_distance_when_moving = Mathf.Sqrt
		(
			Mathf.Pow(GlobalPosition.X - last_player_position.X, 2)
			+
			Mathf.Pow(GlobalPosition.Z - last_player_position.Z, 2)
		);

		float calculate_the_distance_when_falling = Mathf.Sqrt
		(
			Mathf.Pow(GlobalPosition.Y - last_player_position.Y, 2)
		);

		speed_counter = calculate_the_distance_when_moving / (float)Timer_for_calculating_speed.WaitTime;
		falling_counter = calculate_the_distance_when_falling / (float)Timer_for_calculating_speed.WaitTime;

		speed_counter = Mathf.Abs(speed_counter);
		falling_counter = Mathf.Abs(falling_counter);

		last_player_position = GlobalPosition;
	}

	public void Rolling(double delta)
	{
		// Rolling Around stuff
		if((falling_counter > falling_speed_limit_until_start_rolling) && LockRotation)
		{
			LockRotation = false;
			ApplyImpulse
			(
				new Vector3(0.5f, 0, 1) * 1.1f,
				new Vector3(0, 5, 0)
			);
			CenterOfMass = new Vector3(0, 1, 0);
			Mass = 10;
			
		}
		if(!LockRotation  && (falling_counter <= 0.25f))
		{
			timer_rolling += (float)delta;
			CenterOfMass = new Vector3(0, 0, 0);
			Mass = 1;
		}
		else
		{
			timer_rolling = 0f;
		}
		if(timer_rolling >= WaitTime_until_the_player_stops_rolling)
		{
			LockRotation = true;
		}

		if(LockRotation)
		{
			Rotation = Rotation.Lerp(Vector3.Zero, (float)delta * 3);
		}
	}

}
