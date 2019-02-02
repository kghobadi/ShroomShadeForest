using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    ForestGen forestGen;

    //timers and values for speed
    public float currentSpeed, walkSpeed, sprintSpeed;
    public float scrollSpeed = 2.0f;
    float sprintTimer = 0;
    public float sprintTimerMax = 1;

    float footStepTimer = 0;
    public float footStepTimerTotal = 0.5f;

    CharacterController player;
    camMouseLook mouseLook;
    Vector3 movement;

    //for footstep sounds
    public AudioClip[] currentFootsteps, indoorFootsteps, outsideFootsteps;
    AudioSource playerAudSource;

    //random ambience set at start
    public AudioSource ambientSource;
    public AudioClip[] ambientSounds;

    //moving bools
    public bool canMove;
    public bool moving;

    //shroom picking stuff
    public Transform shroomSpot;
    public bool pickingShroom, holdingShroom;
    public float shroomLerpSpeed;
    public GameObject currentShroom;

    public AudioClip[] eatingSounds;

    //jump bool
    public bool jumping;

    //for jump
    public int jumpFrameCounter, jumpFrameMax;
    public float jumpSpeed;

    //player bounds 
    public float xMin, xMax, zMin, zMax;

    

    void Start()
    {
        forestGen = GameObject.FindGameObjectWithTag("ForestGen").GetComponent<ForestGen>();
        player = GetComponent<CharacterController>();
        playerAudSource = GetComponent<AudioSource>();
        mouseLook = Camera.main.GetComponent<camMouseLook>();
        jumping = false;
        canMove = true;
    }

    void Update()
    {
        if (canMove)
        {
            //WASD controls
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                    Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                {
                    moving = true;
                    float moveForwardBackward = Input.GetAxis("Vertical") * currentSpeed;
                    float moveLeftRight = Input.GetAxis("Horizontal") * currentSpeed;
                    //float moveUpDown = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
                    if ((moveForwardBackward != 0 || moveLeftRight != 0) && !playerAudSource.isPlaying)
                    {
                        PlayFootStepAudio();
                    }

                    movement = new Vector3(moveLeftRight, 0, moveForwardBackward);

                    SprintSpeed();
                }
            //when not moving
                else
                {
                    moving = false;
                    movement = Vector3.zero;
                    currentSpeed = walkSpeed;
                }

            movement = transform.rotation * movement;
            player.Move(movement * Time.deltaTime);

            //JUMP AND FALL
                //jump input
                if (Input.GetKeyDown(KeyCode.Space) && !jumping)
                {
                    jumpFrameCounter = 0;
                    jumping = true;
                }

                //apply jump
                if (jumping)
                {
                    Debug.Log("jumping");
                    jumpFrameCounter++;
                    player.Move(new Vector3(0, jumpSpeed * (jumpFrameMax - jumpFrameCounter) * Time.deltaTime, 0));
                    if (jumpFrameCounter > jumpFrameMax)
                    {
                        jumping = false;
                    }
                }
                //fall + 'gravity'
                else
                {
                    player.Move(new Vector3(0, -0.5f, 0));
                }
        }

        if (pickingShroom)
        {
            canMove = false;

            currentShroom.transform.position = Vector3.Lerp(currentShroom.transform.position, shroomSpot.position, shroomLerpSpeed * Time.deltaTime);

            if (Vector3.Distance(currentShroom.transform.position, shroomSpot.position) < 0.25f)
            {
                pickingShroom = false;
                holdingShroom = true;
                canMove = true;
            }
        }

        if (holdingShroom)
        {
            currentShroom.transform.position = shroomSpot.position;

            //eat shroom
            if (Input.GetMouseButtonDown(0))
            {

                for (int i = 0; i < forestGen.trees.Count; i++)
                {
                    float randomTreeChance = Random.Range(0, 100);

                    if (randomTreeChance > 60)
                    {
                        forestGen.trees[i].GetComponent<MeshRenderer>().material = currentShroom.GetComponent<SetShroom>().myShroomShader;
                    }
                    
                    for(int t = 0; t < forestGen.trees[i].transform.childCount; t++)
                    {
                        float randomChangeChance = Random.Range(0, 100);

                        if(randomChangeChance > 60)
                        {
                            forestGen.trees[i].transform.GetChild(t).GetComponent<MeshRenderer>().material = currentShroom.GetComponent<SetShroom>().myShroomShader;
                        }
                    }
                }


                forestGen.allObjects.Remove(currentShroom);
                forestGen.mushrooms.Remove(currentShroom);

                playerAudSource.PlayOneShot(currentShroom.GetComponent<SetShroom>().eatingSound);

                Destroy(currentShroom.gameObject);

                holdingShroom = false;
                currentShroom = null;
            }

            //drop shroom
            if (Input.GetMouseButtonDown(1))
            {
                currentShroom.GetComponent<SetShroom>().returning = true;
                currentShroom = null;
                holdingShroom = false;
            }
        }

        //bounds player on map
        PlayerBounding();

          // quit, class ic!!!
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            // because when you restart you are essentially killing a version of the game that you will NEVER play AGAIN!!!
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                //0 is the only number ever to be loaded for it will always FOREVER be ONE SCENE. It is to be COUNTED as NOTHING!!!
                SceneManager.LoadScene(0);
            }

    }

    void PlayerBounding()
    {
        //above xmax
        if(transform.position.x > xMax)
        {
            transform.position = new Vector3(xMin + 15, transform.position.y, transform.position.z);
        }
        //below xmin
        if(transform.position.x < xMin)
        {
            transform.position = new Vector3(xMax - 15, transform.position.y, transform.position.z);
        }
        //above zmax
        if (transform.position.z > zMax)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zMin + 15);
        }
        //below zmin    
        if (transform.position.z < zMin)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zMax - 15);
        }
    }
    
    //increases move speed while player is moving over time
    public void SprintSpeed()
    {
        sprintTimer += Time.deltaTime;
        //while speed is less than sprint, autoAdd
        if (sprintTimer > sprintTimerMax && currentSpeed < sprintSpeed)
        {
            currentSpeed += Time.deltaTime;
        }
    }

    //called by triggers to change ambient sound
    public void RandomizeAmbience()
    {
        ambientSource.Stop();

        int randomAmbience = Random.Range(0, ambientSounds.Length);
        ambientSource.clip = ambientSounds[randomAmbience];

        ambientSource.Play();
    }
    

    void PlayEatingSounds()
    {
        int randomEat = Random.Range(0, eatingSounds.Length);
        playerAudSource.PlayOneShot(eatingSounds[randomEat]);
    }

    private void PlayFootStepAudio()
    {
        int n = Random.Range(1, currentFootsteps.Length);
        playerAudSource.clip = currentFootsteps[n];
        playerAudSource.PlayOneShot(playerAudSource.clip, 1f);
        // move picked sound to index 0 so it's not picked next time
        currentFootsteps[n] = currentFootsteps[0];
        currentFootsteps[0] = playerAudSource.clip;
    }
    
  

}
