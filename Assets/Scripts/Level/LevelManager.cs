using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using SSpot.Ambient.ComputerCode;
using SSPot.Level;
using SSpot.Evaluators;
using SSpot.Robot;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using SSPot;

namespace SSpot.Level
{
    public class LevelManager : NetworkedSingleton<LevelManager>
    {
        #region Serialized Properties
        
        [SerializeField] private CubeCompiler compiler;
        
        [SerializeField] private CubeRunner runner;

        [Tooltip("If true, whenever an error is encountered, the robot will be reset before allowing to play again.")]
        [SerializeField] private bool resetOnExecutionError = true;

        [Tooltip("If true, robot can continue walking from the ending position" +
                 " if the code was already executed but didnt achieve success or errored out.")]
        [SerializeField] private bool allowConsecutiveRuns;

		[SerializeField] private bool disableRobotMovement = false;
		public CubeComputer LastActiveComputer { get; private set; }

		#endregion

		#region Events

		//Consider using event bus ScriptableObjects

		[field: Header("Events"), SerializeField]
        public UnityEvent OnReset { get; private set; } = new();

        [field: SerializeField]
        public UnityEvent OnError { get; private set; } = new();
        
        [field: SerializeField]
        public UnityEvent OnSuccess { get; private set; } = new();
        
        [field: SerializeField]
        public UnityEvent OnStartRunning { get; private set; } = new();
        
        [field: SerializeField]
        public UnityEvent OnFinishRunning { get; private set; } = new();
        
        [field: SerializeField]
        public UnityEvent OnLevelCompleted { get; private set; } = new();

        #endregion

        public RobotData _robot;

        public RobotData Robot
        {
            get
            {
                if (!_robot)
                    _robot = FindFirstObjectByType<RobotData>();
                
                if (!_robot)
                    Debug.LogError($"{nameof(RobotData)} not found in scene", gameObject);
                
                return _robot;
            }
        }

        public LevelResult CurrentResult { get; private set; } = LevelResult.None();
    
        
        public enum Stage { None, Running, AwaitingResult, End }

        public Stage CurrentStage { get; private set; }
        
        private Coroutine _currentCoroutine;

        private void KillCurrentCoroutine()
        {
            CurrentStage = Stage.None;
            if (_currentCoroutine == null) return;
            
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
            runner.Reset();
        }

        #region Run Methods
        
        /// <inheritdoc cref="RunRpc"/>
        public void Run(CubeComputer computer)
        {
            photonView.RPC(nameof(RunRpc), RpcTarget.AllBuffered, computer.photonView.ViewID);
        }

        /// <summary>
        /// Compiles and executes the code represented by the cells provided.
        /// <br/>
        /// If already running or the level already ended, does nothing.
        /// </summary>
        [PunRPC]
        private void RunRpc(int computerId)
        {
            var computerView = PhotonView.Find(computerId);
            if (!computerView || !computerView.TryGetComponent(out CubeComputer computer))
            {
                Debug.LogError($"Can't find computer with id {computerId}");
                return;
            }

			LastActiveComputer = computer;
			var cells = computer.Cells;

			Debug.Log("Rodando o botão de run");

			if (CurrentStage is Stage.Running or Stage.End)
                return;
            
            // If already executed once, either resets everything or stops awaiting to keep playing.
            if (CurrentStage == Stage.AwaitingResult || CurrentResult.Type != LevelResult.ResultType.None)
            {
                if (allowConsecutiveRuns)
                    KillCurrentCoroutine();
                else
                    ResetRpc();
            }

            CurrentStage = Stage.None;
            CurrentResult = LevelResult.None();
            
            // Attempt compilation
            var compilation = compiler.Compile(cells);
            if (compilation.IsError)
            {
                var compilationError = LevelResult.Error(compilation.Error, compilation.ErrorIndex);
                ReportResult(compilationError);
                return;
            }

			// Evaluate raw and compiled code
			computer.Evaluate(computer.Cells);
			if (CurrentResult.Type == LevelResult.ResultType.Error)
                return;

			computer.EvaluateCompiled(compilation.Result);
			if (CurrentResult.Type == LevelResult.ResultType.Error)
                return;

            // OPICIONAL - Desativa o robo
			if (disableRobotMovement)
			{
				CurrentStage = Stage.End;
				OnLevelCompleted.Invoke();
                Debug.Log("Movimento Desativado para o Robo");
				return;
			}

			// Run and wait for result
			CurrentStage = Stage.Running;
            OnStartRunning.Invoke();
            _currentCoroutine = StartCoroutine(runner.RunCubesCoroutine(compilation.Result, Robot, () =>
            {
                OnFinishRunning.Invoke();
                _currentCoroutine = StartCoroutine(AwaitResultCoroutine());
            }));
        }

        /// <summary>
        /// Waits until a result is reported and then ends or resets the level.
        /// </summary>
        private IEnumerator AwaitResultCoroutine()
        {
            CurrentStage = Stage.AwaitingResult;
            yield return new WaitUntil(() => CurrentResult.Type != LevelResult.ResultType.None);

            if (CurrentResult.Type == LevelResult.ResultType.Success)
            {
                CurrentStage = Stage.End;
                OnLevelCompleted.Invoke();
                yield break;
            }

            CurrentStage = Stage.None;
        }
        
        #endregion
        
        #region Reset Methods
        
        /// <inheritdoc cref="ResetRpc"/>
        public void ResetLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Stops execution of code and resets the robot to its original position.
        /// <br/>
        /// Does nothing if level is already over.
        /// </summary>
        [PunRPC]
        private void ResetRpc()
        {
            if (CurrentStage == Stage.End) return;
            
            KillCurrentCoroutine();

            if (!disableRobotMovement)
            {
                if (Robot == null) Debug.Log($"No {nameof(RobotData)} found in scene, can't reset robot");

				Robot.ResetRobot();
            }
            
            OnReset.Invoke();
        }
        
        #endregion

        #region Result methods
        
        public void ReportResult(LevelResult result)
        {
            CurrentResult = result;
            switch (result.Type)
            {
                case LevelResult.ResultType.Error:
                    Error();
                    break;
                case LevelResult.ResultType.Success:
                    Success();
                    break;
                case LevelResult.ResultType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Error()
        {
            if (resetOnExecutionError)
                ResetRpc();
            else
                KillCurrentCoroutine();
            OnError.Invoke();
        }
        
        private void Success()
        {
            OnSuccess.Invoke();
        }
        
        #endregion

        public void activateRobotMovement()
        {
            disableRobotMovement = false;
        }

        public void resetStage()
        {
            CurrentStage = Stage.None;
		}
	}
}