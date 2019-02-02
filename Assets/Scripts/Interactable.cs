using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour {

    protected GameObject player;
    protected FirstPersonController fpc;
    protected GameObject cursorObj;
    protected SpriteRenderer cursorSprite;
    protected Sprite originalCursor;
    public Sprite interactCursor;
    public float withinClickDist = 10;

    protected bool changedSprites;

    void Awake () {
        player = GameObject.FindGameObjectWithTag("Player");
        fpc = player.GetComponent<FirstPersonController>();
        cursorObj = GameObject.FindGameObjectWithTag("Cursor");
        cursorSprite = cursorObj.GetComponent<SpriteRenderer>();
        originalCursor = cursorSprite.sprite;
        changedSprites = false;
    }

    protected virtual void OnMouseOver()
    {
        float dist = Vector3.Distance(transform.position, fpc.transform.position);
        if (dist < withinClickDist && !changedSprites)
        {
            cursorSprite.sprite = interactCursor;
            changedSprites = true;
        }
        Debug.Log("on mouse over");
    }

    protected virtual void OnMouseDown()
    {
        if (changedSprites)
        {
            Interact();
        }
    }

    protected virtual void OnMouseExit()
    {
        changedSprites = false;
        cursorSprite.sprite = originalCursor;
    }

    public virtual void Interact()
    {
        //do whatever the hell you want
    }
}
