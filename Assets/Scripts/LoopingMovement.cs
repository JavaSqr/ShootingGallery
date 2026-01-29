using UnityEngine;

public class LoopingMovement : MonoBehaviour
{
    public enum MoveType { LeftToRight, RightToLeft, TopToDown, DownToTop, Shaking }

    [Header("General Settings")]
    [SerializeField] private MoveType moveType = MoveType.LeftToRight;
    [SerializeField] private float speed = 1f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private bool playOnAwake = true;

    [Header("Loop Settings")]
    [Tooltip("Дистанция одного цикла в единицах мира")]
    [SerializeField] private float distance = 3f;

    [Tooltip("Амплитуда тряски (для Shaking)")]
    [SerializeField] private float shakeAmplitude = 0.5f;

    private Vector3 startPos;
    private float cycleTime; // длительность цикла при базовой скорости
    private float timer;
    private bool isPlaying;

    private void Awake()
    {
        startPos = transform.localPosition;
        cycleTime = distance / Mathf.Max(speed, 0.01f); // время прохождения distance при базовой скорости

        if (playOnAwake)
            Play();
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float t = (timer % cycleTime) / cycleTime; // нормализованный таймер [0..1]
        float curveSpeed = speedCurve.Evaluate(t);

        Vector3 offset = Vector3.zero;

        switch (moveType)
        {
            case MoveType.LeftToRight:
                offset = Vector3.right * (Mathf.PingPong(timer * speed, distance) - distance / 2) * curveSpeed;
                break;
            case MoveType.RightToLeft:
                offset = Vector3.left * (Mathf.PingPong(timer * speed, distance) - distance / 2) * curveSpeed;
                break;
            case MoveType.TopToDown:
                offset = Vector3.down * (Mathf.PingPong(timer * speed, distance) - distance / 2) * curveSpeed;
                break;
            case MoveType.DownToTop:
                offset = Vector3.up * (Mathf.PingPong(timer * speed, distance) - distance / 2) * curveSpeed;
                break;
            case MoveType.Shaking:
                offset = new Vector3(
                    Mathf.Sin(timer * speed) * shakeAmplitude,
                    Mathf.Cos(timer * speed) * shakeAmplitude,
                    0f
                ) * curveSpeed;
                break;
        }

        transform.localPosition = startPos + offset;
    }

    public void Play()
    {
        isPlaying = true;
        timer = 0f;
    }

    public void Stop()
    {
        isPlaying = false;
        transform.localPosition = startPos;
    }
}
