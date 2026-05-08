using UnityEngine;
using UnityEngine.Playables;

public class SequenceTrigger : MonoBehaviour
{
    public GameObject sequenceCamera;

    private PlayableDirector levelSequence;
    private PlayerController animatedPlayer;

    private void Awake()
    {
        levelSequence = GetComponent<PlayableDirector>();
        sequenceCamera.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player)
        {
            animatedPlayer = player;
            animatedPlayer.SetAutomatedState(true);
            sequenceCamera.SetActive(true);

            levelSequence.stopped += OnSequenceFinished;
            levelSequence.Play();            
        }
    }

    private void OnSequenceFinished(PlayableDirector director)
    {
        animatedPlayer.SetAutomatedState(false);
        sequenceCamera.SetActive(false);

        levelSequence.stopped -= OnSequenceFinished;
    }
}