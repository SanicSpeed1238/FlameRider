using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float inputAccel;
    Rigidbody playerRB;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        playerRB.AddForce(60 * inputAccel * transform.forward);
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKey(KeyCode.W))
        {
            inputAccel = 1;
        }
        else
        {
            inputAccel = 0;
        }
        Debug.Log(inputAccel);
    }
}