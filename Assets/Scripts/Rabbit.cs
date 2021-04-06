using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rabbit : MonoBehaviour
{
    public enum State
    {
        Sitting,
        Jumping,
    }
    private State state = State.Jumping;
    private const float MIN_SITTING_TIMEOUT = 4.0f;
    private const float MAX_SITTING_TIMEOUT = 8.0f;
    private const float JUMPING_TIMEOUT = 1.0f;
    private const float SPEED = 1.0f;
    private float stateChangeTimeout = -1;
    private new Rigidbody2D rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
    }

    // Update is called once per frame
    void Update()
    {
        if (stateChangeTimeout < 0.0f) {
            if (state == State.Jumping) {
                rigidbody.velocity = new Vector2(0, 0);
                state = State.Sitting;
                setTimeout();
            } else {
                randomRotate();
                rigidbody.velocity = transform.right.normalized * SPEED;
                stateChangeTimeout = JUMPING_TIMEOUT;
                state = State.Jumping;
            }
        }
        stateChangeTimeout -= Time.deltaTime;
    }

    private void setTimeout()
    {
        stateChangeTimeout = Random.Range(MIN_SITTING_TIMEOUT, MAX_SITTING_TIMEOUT);
    }

    private void randomRotate()
    {
        var angle = Random.Range(-45, 45);
        var rotation = Vector3.forward * angle;
        transform.Rotate(rotation, Space.World);
    }
}
