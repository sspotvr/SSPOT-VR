using System;
using System.Collections;
using SSpot.Grids;
using SSpot.Level;
using SSPot.Level;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace SSPot.Ambient.Labyrinth
{
    public class LabyrinthManager : MonoBehaviour
    {
        [Serializable]
        private struct Section
        {
            public GridObject endMarker;
            public GameObject coveringRoof;
        }
        
        [Header("Environment")]
        [SerializeField] private GameObject projector;
        [SerializeField] private GameObject invisibleWall;
        
        [Header("Objectives")]
        [SerializeField] private GridObject objective;
        [SerializeField] private Section[] sections = Array.Empty<Section>();
        [SerializeField, TextArea] private string wrongMoveError = "Caminho errado!";

        [Header("Narration")]
        [Tooltip("Narration keys played when a section is completed and the players swap roles.")]
        [SerializeField] private string[] switchRolesClips = { "Labirinto_swap.1" };

        [Header("Players")]
        [SerializeField] private LabyrinthPlayer player1;
        [SerializeField] private LabyrinthPlayer player2;
        
        private LabyrinthPlayer _currentWatcher;
        private LabyrinthPlayer _currentCoder;

        private int _currentSectionIndex = -1;
        private int CurrentSectionIndex
        {
            get => _currentSectionIndex;
            set
            {
                // Clears listener for old section
                if (_currentSectionIndex >= 0)
                {
                    sections[_currentSectionIndex].endMarker.SteppedOnEvent.RemoveListener(OnSectionEndReached);
                }
                
                // Uncovers and listens to new section
                if (value >= 0 && value < sections.Length)
                {
                    sections[value].endMarker.SteppedOnEvent.AddListener(OnSectionEndReached);
                    sections[value].coveringRoof.SetActive(false);
                }
                
                _currentSectionIndex = value;
            }
        }
        
        private void ReportWrongMove() => LevelManager.Instance.ReportResult(LevelResult.Error(wrongMoveError));
        
        private static void ReportSuccess() => LevelManager.Instance.ReportResult(LevelResult.Success());


        private void SwitchPlayers()
        {
            //TODO ideally instructions should be stored in the manager itself
            var watcherInstructions = _currentWatcher.InstructionsText.StringReference;
            var coderInstructions = _currentCoder.InstructionsText.StringReference;
            _currentWatcher.SetInstructions(new LocalizedString(coderInstructions.TableReference, coderInstructions.TableEntryReference));
            _currentCoder.SetInstructions(new LocalizedString(watcherInstructions.TableReference, watcherInstructions.TableEntryReference));
            
            _currentWatcher.SetCoder(true);
            _currentCoder.SetCoder(false);
            (_currentWatcher, _currentCoder) = (_currentCoder, _currentWatcher);
        }

        private void OnSectionEndReached()
        {
            CurrentSectionIndex++;

            // The last section's end marker is the objective itself - the level is over, nobody swaps.
            if (CurrentSectionIndex >= sections.Length) return;

            SwitchPlayers();
            StartCoroutine(StartNextSectionCoroutine());
        }

        /// <summary>
        /// Ends the previous coder's run right at the section end and places Mark there already facing the
        /// next section's path, so the new coder plans from the pose the code will actually start from
        /// (otherwise Mark keeps the facing he arrived with until the next Run resets him).
        /// </summary>
        private IEnumerator StartNextSectionCoroutine()
        {
            // Wait a frame so the Checkpoint under Mark finishes storing itself as the new start position.
            yield return null;
            LevelManager.Instance.ResetRobotLocal();

            if (Voice.instance != null && switchRolesClips.Length > 0)
                _ = Voice.instance.Speak(switchRolesClips);
        }
        
        private void Start()
        {
            // Initialize players
            int localIndex = PlayerSetup.Local.PlayerIndex;
            player1.Init(0, localIndex);
            player2.Init(1, localIndex);

            player1.SetCoder(true);
            _currentCoder = player1;
        
            player2.SetCoder(false);
            _currentWatcher = player2;
            
            // Cover all sections and begin listening to the first
            sections.ForEach(s => s.coveringRoof.SetActive(true));
            CurrentSectionIndex = 0;
            
            // Listen to objective
            objective.SteppedOnEvent.AddListener(ReportSuccess);
            PlayerSetup.Local.isUp = true;
        }
        
        private void OnEnable()
        {
            LevelManager.Instance.OnSuccess.AddListener(LabyrinthSuccess);
            LevelManager.Instance.Robot.Mover.OnFailedToMove += ReportWrongMove;
        }

        private void OnDisable()
        {
            if (!LevelManager.Instance) return;
            LevelManager.Instance.OnSuccess.RemoveListener(LabyrinthSuccess);
            
            if (!LevelManager.Instance.Robot) return;
            LevelManager.Instance.Robot.Mover.OnFailedToMove -= ReportWrongMove;
        }

        private void LabyrinthSuccess()
        {
            // Deactivate objects for both players
            player1.SetObjectsActive(false);
            player2.SetObjectsActive(false);
            
            // Activate tv for both players
            player1.TV.SetActive(true);
            player2.TV.SetActive(true);

            // Activate projector
            projector.SetActive(true);

            // Destroy invisible wall
            Destroy(invisibleWall);
        }
    }
}