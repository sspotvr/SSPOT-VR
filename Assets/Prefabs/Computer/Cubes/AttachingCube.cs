using JetBrains.Annotations;
using UnityEngine;
using Photon.Pun;
using SSpot.Ambient.ComputerCode;

public class AttachingCube : MonoBehaviourPun
{
    public CodingCell ParentCell { get; set; }
    
    public CubeClass CurrentCube { get; private set; }

    // Coding cell
    public GameObject cubeHolder;       // Coding cell cube holder

    // Audio
    public AudioClip selectingCube;     // Cube select audio
    public AudioClip releasingCube;     // Cube release audio

    // Audio source
    [SerializeField] private AudioSource audioSource;    // Audio source

    /// <summary>
    /// When player click on this object, it attaches a cube in the coding cell
    /// </summary>
    [UsedImplicitly]
    public void OnPointerClick()
    {
        GameObject hand = PlayerSetup.Local.Hand;
        AttachingHandler(hand);
    }

    /// <summary>
    /// Attach a cube to a coding cell.
    /// 
    /// <para>If there is a cube in player hand and the coding cell cube holder is free, then attach it to the clicked cube holder.</para>
    /// <para>If there is a cube in the coding cell but not in the player hand, remove the cube from coding cell.</para>
    /// </summary>
    private void AttachingHandler(GameObject hand)
    {
        if (hand.transform.childCount == 1)
        {
            var cube = hand.transform.GetChild(0).GetComponent<CloningCube>();
            AttachCube(cube);
        }
        else if (cubeHolder.transform.childCount == 1)
        {
            ClearCell();
        }
    }

    private void AttachCube(CloningCube selectedCube)
    {
        if(selectedCube.Cube.IsLoop)
        {
            ParentCell.SetLoop(true);
            PlayerSetup.Local.DestroyCubeOnHand();
        }
        else if(selectedCube.Cube.IsIf)
        {
            // The If block occupies its own slot, like a movement/Begin/End cube - it can't share a
            // slot with one of those (but it CAN share a slot with a Loop, which wraps it instead of
            // competing with it).
            if (CurrentCube != null || ParentCell.HasCondition || ParentCell.HasSenao) return;

            ParentCell.SetCondition(true);
            PlayerSetup.Local.DestroyCubeOnHand();
        }
        else if (selectedCube.Cube.IsElse)
        {
            // Senão occupies its own slot too, and can only ever go immediately after the then-body of
            // some If - never standalone.
            if (CurrentCube != null || ParentCell.HasCondition || ParentCell.HasSenao) return;

            var owningIf = FindOwningIf();
            if (owningIf == null) return;

            // Link both ways BEFORE activating: activation can synchronously discover there's no room
            // for the Senão's own body (e.g. it landed on the last cell) and self-deactivate right away,
            // and its own cleanup (ResetRpc) only clears the link on the If's side if it can already see it.
            ParentCell.ElseController.OwningIf = owningIf;
            owningIf.AttachedSenao = ParentCell.ElseController;

            ParentCell.SetSenao(true);
            owningIf.RefreshLimits(); // hides the If's own Range buttons now that AttachedSenao is set

            PlayerSetup.Local.DestroyCubeOnHand();
        }
        else
        {
            // A movement/Begin/End cube can't share a slot with an If or a Senão.
            if (ParentCell.HasCondition || ParentCell.HasSenao) return;

            photonView.RPC(nameof(SetCubeRPC), RpcTarget.AllBuffered, selectedCube.photonView.ViewID);
        }
    }

    /// <summary>
    /// Finds the If whose then-body ends exactly at this cell, scanning backward from it. Returns null
    /// if the nearest earlier block is anything else (a Loop, another Senão) or the index doesn't line
    /// up - i.e. this cell isn't immediately after some If's then-body.
    /// </summary>
    private ConditionController FindOwningIf()
    {
        var cells = ParentCell.Computer.Cells;
        for (int i = ParentCell.Index - 1; i >= 0; i--)
        {
            var cell = cells[i];
            if (cell.HasCondition)
            {
                var ifController = cell.ConditionController;
                return i + 1 + ifController.Range == ParentCell.Index ? ifController : null;
            }

            if (cell.HasLoop || cell.HasSenao) return null;
        }

        return null;
    }

    [PunRPC]
    private void SetCubeRPC(int cubeId)
    {
        if (CurrentCube != null)
            ClearCellRPC();
        
        var selectedCube = PhotonView.Find(cubeId);
        if (!selectedCube)
        {
            Debug.LogError($"Failed to find cube with id {cubeId}");
            return;
        }
        
        // Attach the selected cube to cubeHolder
        selectedCube.transform.SetParent(cubeHolder.transform);

        // Disable BoxCollider and EvenTrigger from selected cube
        selectedCube.GetComponent<BoxCollider>().enabled = false;

        // Set selected cube transform
        selectedCube.transform.localPosition = Vector3.zero;
        selectedCube.transform.rotation = cubeHolder.transform.rotation;
        selectedCube.transform.localScale = Vector3.one;

        // Play select cube sound
        audioSource.clip = selectingCube;
        audioSource.Play();
        
        CurrentCube = selectedCube.GetComponent<CloningCube>().Cube;
    }

    public void ClearCell() => photonView.RPC(nameof(ClearCellRPC), RpcTarget.AllBuffered);

    [PunRPC]
    private void ClearCellRPC()
    {
        if (CurrentCube == null)
            return;
        
        // Destroy cube from cube holder
        Destroy(cubeHolder.transform.GetChild(0).gameObject);

        // Play release sound
        audioSource.clip = releasingCube;
        audioSource.Play();

        CurrentCube = null;
    }
}
