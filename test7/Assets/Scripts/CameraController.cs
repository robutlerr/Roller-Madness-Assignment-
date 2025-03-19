using UnityEngine;

public class CameraController : MonoBehaviour
{
    // define coordinates
    public float xDistance = 5f, yDistance = 10f, zDistance = 40f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform target;
    void Start()
    {
        // start playing the music 
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 v1 = new Vector3(xDistance, 0, zDistance);
        // getting the camera to be behind the target (the player) in order to follow it
        Vector3 wantedPosition = target.position - v1;
        wantedPosition.y = yDistance;
        transform.position = wantedPosition;
        transform.LookAt(target);
    }
}
