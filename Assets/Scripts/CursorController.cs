using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("Joystick Settings")]
    public FixedJoystick joystick; // —сылка на виртуальный джойстик
    public float moveSpeed = 5f;

    private void Awake()
    {
        StartCoroutine(RealtimeFixedUpdate());
    }

    private IEnumerator RealtimeFixedUpdate()
    {
        while (true)
        {
            if (!Application.isMobilePlatform && !StaticData.isMobileTesting)
            {
                transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
            else if (joystick != null)
            {
                Vector2 input = joystick.Direction;

                // ≈сли есть ввод Ч двигаем объект
                if (input.magnitude > 0.01f)
                {
                    Vector2 movement = input * moveSpeed * Time.fixedDeltaTime;
                    transform.Translate(movement, Space.World);
                }
            }

            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }
    }
}
