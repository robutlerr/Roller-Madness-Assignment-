using System;
using UnityEngine;

public class Chase : MonoBehaviour
{
    // the target to be chased
    public Transform target;
    public float speed = 2.5f;
    public float minDistance = 1f;
    // tracking the time for despawning so that pausing can occur
    public float despawnTime = 10f;
    // bool to see if the enemy is a Level 2 enemy
    public bool isCactusEnemy = false;
    // how much faster the level 2 enemy will be
    public float speedMultiplier = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // sets the target automatically (w/ tags)
        if (target == null) {
            if (GameObject.FindWithTag("Player") != null) {
                // sets the object with the player tag as the target
                target = GameObject.FindWithTag("Player").GetComponent<Transform>();
            }
        }
        // increases the speed if the enemy is a Level 2 enemy
        if (isCactusEnemy) {
            speed *= speedMultiplier;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.gm.pauseGame == false) {
            despawnTime -= Time.deltaTime;
            if (despawnTime <= 0) {
                Destroy(gameObject);
            }
        }

        // case checking
        if (target == null) {
            Debug.Log("Target is not assigned");
        }
        // LookAt rotates the z axis to face towards the target
        transform.LookAt(target);
        // compute the distance of the object from the target
        // transform.position = position of the enemy (the one the script is attached to)
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > minDistance) {
            // moves the object forward
            transform.position += transform.forward * speed * Time.deltaTime;
        }

    }
}
