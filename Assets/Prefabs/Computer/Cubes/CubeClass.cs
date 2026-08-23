using System;
using NaughtyAttributes;
using static Cube;

[Serializable]
public class CubeClass
{
	public bool IsLoop => type == CubeType.Loop;
	public bool IsIf => type == CubeType.If;


	public CubeType type;

	public CubeClass(CubeType type)
	{
		this.type = type;
	}
}
