using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLayerController : MonoBehaviour
{
    // Reference transform
    Transform t;

    public static bool inWater;
    public static bool isSwimming;
    // if not in water, walk
    // if in water and not swimming, float
    // if in water and swimming, swim

    public LayerMask waterMask;

    [Header("Player Rotation")]
    public float sensitivity = 1;

    //clamp variables
    public float rotationMin;
    public float rotationMax;

    //mouse input values
    float rotationX;
    float rotationY;

    [Header("Player Movement")]
    public float speed = 1;
    float moveX;
    float moveY;
    float moveZ;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        t = this.transform;

        Cursor.lockState = CursorLockMode.Locked;

        inWater = false;
    }

    private void FixedUpdate()
    {
        SwimmingOrFloating();
        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        SwitchMovement();
    }

    void SwitchMovement()
    {
        //toggle inWater
        inWater = !inWater;

        //change the rigidbody accordingly
        rb.useGravity = !rb.useGravity;

    }
    
    void SwimmingOrFloating()
    {
        bool swimCheck = false;

        if (inWater)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(t.position.x, t.position.y + 0.5f, t.position.z), Vector3.down, out hit, Mathf.Infinity, waterMask))
            {
                if(hit.distance < 0.1f)
                {
                    swimCheck = true;
                }
            }
            else
            {
                swimCheck = true;
            }
        }

        isSwimming = swimCheck;
        Debug.Log("isSwimming = " + isSwimming);
    }
    // Update is called once per frame
    void Update()
    {
        LookAround();

        //debug function to unlock cursor
        if(Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void LookAround()
    {
        //get the mouse input
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;

        //clamp the y rotation
        rotationY = Mathf.Clamp(rotationY, rotationMin, rotationMax);

        //setting the rotation value every update
        t.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);
    }

    void Move()
    {
        //get the movement input
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");
        moveZ = Input.GetAxis("Forward");

        //check if the player is standing still
        if (inWater) //if in water, velocity = 0
        {
            rb.velocity = new Vector2(0,0);
        }
        else
        {
            if (moveX == 0 && moveZ == 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        
        if (!inWater)
        {
            //move the character (land ver)
            t.Translate(new Quaternion(0, t.rotation.y, 0, t.rotation.w) * new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed, Space.World);
        }
        else
        {
            //check if the player is swimming underwater or floating
            if (!isSwimming)
            {
                //move player floating
                //clamp the moveY value, so they cannot use space or shift to move vertically
                moveY = Mathf.Min(moveY, 0);

                //convert the local direction vector into a worldspace vector
                Vector3 clampedDirection = t.TransformDirection(new Vector3(moveX, moveY, moveZ));

                //clamp the values of this worldspace vector
                clampedDirection = new Vector3(clampedDirection.x, Mathf.Min(clampedDirection.y, 0), clampedDirection.z);

                t.Translate(clampedDirection * Time.deltaTime * speed, Space.World);
            }
            else
            {
                //move the character (swimming vertically)
                t.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed);
                t.Translate(new Vector3(0, moveY, 0) * Time.deltaTime * speed, Space.World);
            }
        }
        
    }
}
