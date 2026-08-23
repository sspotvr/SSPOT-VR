using System;
using System.Collections;
using NaughtyAttributes;
using SSpot.Robot;

[Serializable]
public class Cube
{
	public enum CubeType
	{
		Null = 0,
		Begin = 1,
		End = 2,
		Left = 3,
		Right = 4,
		Forward = 5,
		Loop = 6,
		If = 7
	}

	[BoxGroup("Cube General Type")]
	public CubeType type;

	/// <summary>
	/// Number of compiled cubes belonging to the "then" branch, immediately following this cube. Only used when type is If.
	/// </summary>
	public int ThenLength;

	/// <summary>
	/// Number of compiled cubes belonging to the "else" branch, immediately following the "then" branch. Only used when type is If.
	/// </summary>
	public int ElseLength;

	public Cube(CubeType type)
	{
		this.type = type;
	}

	//Consider using polymorphism
	public IEnumerator ExecuteCoroutine(RobotData robot)
	{
		switch (type)
		{
			case CubeType.Begin:
				if (robot.Animator.IsBroken)
					yield return robot.Animator.SetBrokenCoroutine(false);
				break;
			case CubeType.Left:
				yield return robot.Mover.TurnLeftCoroutine();
				break;
			case CubeType.Right:
				yield return robot.Mover.TurnRightCoroutine();
				break;
			case CubeType.Forward:
				yield return robot.Mover.MoveForwardCoroutine();
				break;
		}
	}
}
