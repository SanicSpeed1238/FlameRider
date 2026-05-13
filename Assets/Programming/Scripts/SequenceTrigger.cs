using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class SequenceTrigger : MonoBehaviour
{
    private GameObject rootGameObject;
    private PlayableDirector levelSequence;

    private GameObject mainCamera;
    private GameObject sequenceCamera;
    
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;

    private void Awake()
    {
        rootGameObject = gameObject.transform.parent.gameObject;
        levelSequence = rootGameObject.GetComponent<PlayableDirector>();

        mainCamera = Camera.main.gameObject;        

        sequenceCamera = rootGameObject.transform.Find("SequenceCamera").gameObject;
        if (sequenceCamera != null) sequenceCamera.SetActive(false);
        else Debug.LogWarning("'SequenceCamera' for " + levelSequence.gameObject.name + " not found.");
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player && player.CanInput())
        {
            playerController = player;
            playerController.SetAutomatedState(true);

            playerAnimator = player.GetComponentInChildren<PlayerAnimator>();
            playerAnimator.AnimateHair(false);

            mainCamera.GetComponent<CinemachineBrain>().UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            sequenceCamera.SetActive(true);

            levelSequence.stopped += OnSequenceFinished;
            levelSequence.Play();            
        }
    }

    private void OnSequenceFinished(PlayableDirector director)
    {
        playerController.SetAutomatedState(false);
        playerAnimator.AnimateHair(true);

        sequenceCamera.SetActive(false);
        mainCamera.GetComponent<CinemachineBrain>().UpdateMethod = CinemachineBrain.UpdateMethods.FixedUpdate;
        levelSequence.stopped -= OnSequenceFinished;
    }
}