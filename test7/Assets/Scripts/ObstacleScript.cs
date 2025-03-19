using UnityEngine;

public class TreeScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision) {
        // if the collision is with the ground, just return and ignore
        // using a tag on a game object to specify which object to manipulate
        if (collision.gameObject.tag == "AddHealth" || collision.gameObject.tag == "SpeedBoost"
            || collision.gameObject.tag == "KillEnemy") {
            // Ignore the collision and make the object pass through
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}
