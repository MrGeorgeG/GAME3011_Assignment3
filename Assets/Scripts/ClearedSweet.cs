using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearedSweet : MonoBehaviour {

    public AnimationClip clearAnimation;

    private bool isClearing = false;

    public AudioClip destoryAudio;

    public bool IsClearing
    {
        get { return isClearing; }
    }

    protected GameSweet sweet;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    public virtual void Clear()
    {
        sweet.GM.level.OnSweetCleared(sweet);
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play(clearAnimation.name);
            AudioSource.PlayClipAtPoint(destoryAudio, transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);

        }
    }
}