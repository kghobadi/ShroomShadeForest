using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShroom : Interactable
{
    Vector3 originalSize, largeSize, originalPosition;

    //lerp bools
    bool lerpingUp, lerpingDown;
    public float lerpSpeed, breatheDistance;

    //audio for shroom
    AudioSource shroomSource;
    public AudioClip[] breathIn, breathOut;

    public AudioClip eatingSound;

    //for player to take!!
    public MeshRenderer shroomMR;
    public Material myShroomShader;

    //player interactin bool
    public bool playerHolding, returning;

    //layermasks
    public LayerMask ground;

    void Start()
    {
        shroomMR = GetComponent<MeshRenderer>();
        myShroomShader = shroomMR.material;

        float randomScale = Random.Range(0.75f, 1.5f);

        transform.localScale *= randomScale;

        originalSize = transform.localScale;

        largeSize = originalSize * 2;

        float randomRotate = Random.Range(0, 360);

        transform.Rotate(0, randomRotate, 0);

        lerpSpeed = Random.Range(0.5f, 2f);

        shroomSource = GetComponent<AudioSource>();

        breatheDistance = 15;
    }

    void OnEnable()
    {
        StartCoroutine(BreatheIn());
    }

    void Update()
    {
        if(Vector3.Distance(transform.position, player.transform.position) < breatheDistance && !returning && !playerHolding)
        {
            Debug.Log("player near");
            if (lerpingUp)
            {
                if (!shroomSource.isPlaying)
                {
                    PlaySound(breathIn);
                }

                transform.localScale = Vector3.Lerp(transform.localScale, largeSize, lerpSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.localScale, largeSize) < 0.1f)
                {
                    StartCoroutine(BreatheOut());
                }
                
            }

            if (lerpingDown)
            {
                if (!shroomSource.isPlaying)
                {
                    PlaySound(breathOut);
                }

                transform.localScale = Vector3.Lerp(transform.localScale, originalSize, lerpSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.localScale, originalSize) < 0.1f)
                {
                    StartCoroutine(BreatheIn());
                }
                
            }
        }

        //after player puts back down
        if (returning)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, lerpSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, originalPosition) < 0.1f)
            {
                returning = false;
                playerHolding = false;

                StartCoroutine(BreatheIn());
            }
        }
        
    }

    IEnumerator BreatheIn()
    {
        lerpingDown = false;

        float randomWait = Random.Range(0.1f, 1);
        yield return new WaitForSeconds(randomWait);

        lerpingUp = true;
    }

    IEnumerator BreatheOut()
    {
        lerpingUp = false;

        float randomWait = Random.Range(0.1f, 1);
        yield return new WaitForSeconds(randomWait);

        lerpingDown = true;
    }

    public void PlaySound(AudioClip[] sounds)
    {
        int randomSound = Random.Range(0, sounds.Length);
        shroomSource.PlayOneShot(sounds[randomSound]);
    }

    public void SetYPos()
    {

        //adjust trees height
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, ground))
        {
            transform.position = hit.point;

            transform.position = new Vector3(transform.position.x,
               transform.position.y + (transform.localScale.y / 2),
               transform.position.z);
        }
    }

    //player picks up if not holding another shroom
    public override void Interact()
    {
        if(fpc.currentShroom == null && !fpc.pickingShroom)
        {
            base.Interact();
            originalPosition = transform.position;
            lerpingDown = false;
            lerpingUp = false;
            transform.localScale = originalSize;
            fpc.currentShroom = gameObject;
            fpc.pickingShroom = true;
            playerHolding = true;
        }
    }

}
