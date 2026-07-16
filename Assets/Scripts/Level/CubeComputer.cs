using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Photon.Pun;
using SSpot.Ambient.ComputerCode;
using SSpot.Evaluators;
using SSpot.UI;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SSpot.Level
{
    public class CubeComputer : MonoBehaviourPun
    {
        [BoxGroup("Cells")]
        [SerializeField] private Transform cellsParent;

        [BoxGroup("Cells")] 
        [SerializeField] private LoopController.LoopSettings loopSettings;
        public LoopController.LoopSettings GlobalLoopSettings => loopSettings;

        [BoxGroup("Buttons")]
        [SerializeField] private PointerButton runButton;
        [BoxGroup("Buttons")]
        [SerializeField] private PointerButton resetButton;
        [BoxGroup("Buttons")]
        [SerializeField] private PointerButton clearButton;
        
        [BoxGroup("Sounds")]
        [SerializeField] private AudioSource audioSource;
        [BoxGroup("Sounds")]
        [SerializeField] private AudioClip successSound;
        [BoxGroup("Sounds")]
        [SerializeField] private AudioClip errorSound;

		private CodeEvaluator[] _myEvaluators;

        [BoxGroup("Custom Events")]
	    [SerializeField] private UnityEvent onLocalSuccess;


        [SerializeField] private ComputerRenderer[] renderers = Array.Empty<ComputerRenderer>();
        
        private CodingCell[] _cells = Array.Empty<CodingCell>();
        public IReadOnlyList<CodingCell> Cells => _cells;

        public int IndexOff(CodingCell cell) => _cells.IndexOf(cell);
        
        public int IndexOf(AttachingCube cube) => _cells.FindIndex(cell => cell.AttachingCube == cube);
        
        public int IndexOf(LoopController loop) => _cells.FindIndex(cell => cell.LoopController == loop);

        private void Awake()
        {
            _cells = transform.GetComponentsInChildren<CodingCell>();
			_myEvaluators = GetComponentsInChildren<CodeEvaluator>();

			for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].Init(i, this);
            }
        }

        private void OnEnable()
        {
            runButton.OnPointerClickEvent.AddListener(OnRunButtonPressed);
            resetButton.OnPointerClickEvent.AddListener(OnResetButtonPressed);
            clearButton.OnPointerClickEvent.AddListener(OnClearPressed);
            
            LevelManager.Instance.OnSuccess.AddListener(OnSuccess);
            LevelManager.Instance.OnLevelCompleted.AddListener(OnFinish);
			LevelManager.Instance.OnError.AddListener(OnError);
            LevelManager.Instance.OnReset.AddListener(OnReset);
        }
        
        private void OnDisable()
        {
            runButton.OnPointerClickEvent.RemoveListener(OnRunButtonPressed);
            resetButton.OnPointerClickEvent.RemoveListener(OnResetButtonPressed);
            clearButton.OnPointerClickEvent.RemoveListener(OnClearPressed);
            
            if (LevelManager.Instance)
            {
                LevelManager.Instance.OnSuccess.RemoveListener(OnSuccess);
                LevelManager.Instance.OnLevelCompleted.RemoveListener(OnFinish);
				LevelManager.Instance.OnError.RemoveListener(OnError);
                LevelManager.Instance.OnReset.RemoveListener(OnReset);
            }
        }

        private void OnValidate()
        {
            if (!cellsParent)
                cellsParent = transform; // Default to self if no parent set
        }

        public void ClearCells()
        {
            foreach (var cell in _cells)
                cell.Clear();
        }
        
        #region Button Callbacks
        
        
        private void OnRunButtonPressed() => LevelManager.Instance.Run(this);
        
        private void OnResetButtonPressed() => LevelManager.Instance.ResetLevel();
        
        private void OnClearPressed()
        {
            //OnResetButtonPressed();
            ClearCells();
        }
        
        #endregion

        #region  LevelManager Callbacks

        
        private void OnSuccess()
        {
            Debug.Log("Entrei no callback de sucesso");

			renderers.ForEach(r => r.SetMaterial(true));
		}

        private void OnFinish()
		{
			if (LevelManager.Instance.LastActiveComputer != this) return;

			Debug.Log("Entrei no callback de finalizacao");

			onLocalSuccess.Invoke();
		}

		private void OnError()
        {
            renderers.ForEach(r => r.SetMaterial(false));
            audioSource.PlayOneShot(errorSound);
        }
        
        private void OnReset() => renderers.ForEach(r => r.ResetMaterial());

		public void Evaluate(IReadOnlyList<CodingCell> cells)
		{
            Debug.Log("Computador avaliando o codigo...");
            Debug.Log(_myEvaluators.Length + " avaliadores encontrados");

			foreach (var evaluator in _myEvaluators)
			{
				evaluator.EvaluatePreCompilation(cells);
			}
		}

		public void EvaluateCompiled(IReadOnlyList<Cube> compiledCubes)
		{
			foreach (var evaluator in _myEvaluators)
			{
				evaluator.EvaluatePostCompilation(compiledCubes);
			}
		}

		#endregion
	}
}