using System;
using System.Collections;
using System.Collections.Generic;
using SSpot.Robot;
using UnityEngine;

namespace SSpot.Ambient.ComputerCode
{
    [Serializable]
    public class CubeRunner
    {
        [SerializeField] private float timeBetweenCubes = .5f;

        public int CurrentIndex { get; private set; }

        public IEnumerator RunCubesCoroutine(List<Cube> cubes, RobotData robot, Action endCallback)
        {
            while (CurrentIndex < cubes.Count - 1)
            {
                var cube = cubes[CurrentIndex];

                if (cube.type == Cube.CubeType.If)
                {
                    // Resolved here, at the exact moment this cube is reached, not at compile time -
                    // the obstacle check depends on the robot's live position, which can differ every
                    // time this same compiled If is reached (e.g. once per loop iteration).
                    bool blocked = robot.Mover.IsBlockedAhead();

                    int thenStart = CurrentIndex + 1;
                    int thenEnd = thenStart + cube.ThenLength;
                    int elseEnd = thenEnd + cube.ElseLength;

                    int branchStart = blocked ? thenStart : thenEnd;
                    int branchEnd = blocked ? thenEnd : elseEnd;

                    for (int i = branchStart; i < branchEnd; i++)
                    {
                        yield return cubes[i].ExecuteCoroutine(robot);
                        yield return new WaitForSeconds(timeBetweenCubes);
                    }

                    CurrentIndex = elseEnd;
                    continue;
                }

                yield return cube.ExecuteCoroutine(robot);
                yield return new WaitForSeconds(timeBetweenCubes);
                CurrentIndex++;
            }

            endCallback?.Invoke();
        }

        public void Reset()
        {
            CurrentIndex = 0;
        }
    }
}