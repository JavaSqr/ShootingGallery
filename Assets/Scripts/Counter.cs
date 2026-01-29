using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    [Space, Header("References")]
    [SerializeField] private Image numImage;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip popupAnimation;
    [SerializeField] private List<Sprite> numbersSprites;

    [Space, Header("Settings")]
    [SerializeField] private int countdown;
    [SerializeField] private float startDelay;
    [SerializeField] private bool isReversed;
    [SerializeField] private bool zeroIncluded;

    private Coroutine coroutine;

    private void OnEnable()
    {
        animator.enabled = false;
        numImage.enabled = false;
    }

    public void StartCountdown()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(sc());

        IEnumerator sc()
        {
            yield return new WaitForSecondsRealtime(startDelay);

            int border = zeroIncluded ? 0 : 1;

            animator.enabled = true;
            numImage.enabled = true;

            int step = isReversed ? -1 : 1;
            int start = isReversed ? countdown : border;
            int end = isReversed ? border : countdown;

            var wait = new WaitForSecondsRealtime(popupAnimation.length);

            for (int i = start; isReversed ? i >= end : i <= end; i += step)
            {
                numImage.sprite = numbersSprites[i];
                animator.SetTrigger("Popup");
                yield return wait;
            }

            animator.enabled = false;
            numImage.enabled = false;

            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }
}
