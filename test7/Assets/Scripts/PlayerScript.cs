using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;  // Required for IEnumerator
// library for TextMeshPro
using TMPro;
public class PlayerScript : MonoBehaviour
{
    Rigidbody rb;
    InputSystem_Actions inputSystem_Actions;
    Vector3 movement = Vector3.zero;    // (0,0,0)
    public float moveSpeed = 12f;       // typically 15
    public int itemCounter = 0;
    // text variable
    public TMP_Text scoreText;

    // for the particle system (explosion)
    public GameObject explosionPlayer;
    public GameObject explosionCoin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputSystem_Actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        //GameManager.gm.score += 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnPause() {
        GameManager.gm.pauseGame = !(GameManager.gm.pauseGame);
        GameManager.gm.pause();
    }
    
    void OnCollisionEnter(Collision collision) {
        // if the collision is with the ground, just return and ignore
        // using a tag on a game object to specify which object to manipulate
        if (collision.gameObject.tag == "Point") {
            GameManager.gm.add_score(1);
            Destroy(collision.gameObject); // <- destroys the collectable

        } else if (collision.gameObject.tag == "PointDouble") {
            GameManager.gm.add_score(2);
            Destroy(collision.gameObject); // <- destroys the collectable
        } else if (collision.gameObject.tag == "AddHealth") {
            GameManager.gm.incHealth();
            Destroy(collision.gameObject); // <- destroys the collectable
        } else if (collision.gameObject.tag == "SpeedBoost") {
            // 5 second speed boost
            StartCoroutine(SpeedBoost(5f));
            Destroy(collision.gameObject); // <- destroys the collectable
        } else if (collision.gameObject.tag == "KillEnemy") {
            // find an enemy and destroy it
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length > 0) {
                Destroy(enemies[0]); // destroy the first enemy found
                GameManager.gm.playSound(2);
            }
            Destroy(collision.gameObject); // <- destroys the collectable
        }
        
        // if the enemy collides with the player or
        // if the player collides with the invisible walls around the ground (aka going out-of-bounds)
        if (collision.gameObject.tag == "Enemy") {
            // decrease the health of the player
            GameManager.gm.decHealth("Enemy");
        } else if (collision.gameObject.tag == "InvisWall" || collision.gameObject.tag == "Obstacle") {
            GameManager.gm.decHealth("Wall");
        }
    }

    // activates a speed boost effect for n seconds
    IEnumerator SpeedBoost(float time) {
        itemCounter++;  // increment the item count
        float originalSpeed = moveSpeed; // Save the original speed
        if (itemCounter <= 1) {
            moveSpeed *= 2; // Double the player's speed
            GameManager.gm.playSound(1);
            yield return new WaitForSeconds(time); // Keep it boosted for n seconds

            moveSpeed = originalSpeed; // Reset speed back to normal
            itemCounter--;             // item is used so reset the counter
        } else {
            itemCounter--;
        } 
    }

    void FixedUpdate() {
        if (!rb.isKinematic) { // Only apply velocity when not paused
            rb.linearVelocity = movement; // Use linearVelocity instead of velocity
        }
    }

    void OnMove(InputValue inputValue) {
        if (GameManager.gm.pauseGame) return;
        if (GameManager.gm.pauseGame) {
            movement = Vector3.zero;
            return;
        }
        // getting the input for moving (for any device)
        // the action in unity's folder returns Vector2
        // this value is between 0-1, 0 being not pressing, 1 being holding down the button
        Vector2 b = inputValue.Get<Vector2>();
        movement = new Vector3(b.x,0,b.y) * moveSpeed;
    }

}
