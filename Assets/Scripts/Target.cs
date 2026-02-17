using CustomInspector;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> targetSprites;
    [Space]
    [SerializeField] private SpriteRenderer lowerHolderSR;
    [SerializeField] private SpriteRenderer upperHolderSR;
    [Space]
    [SerializeField] private Animator animator;
    [SerializeField] private string shotDownTrigger = "shotDown";

    private TargetType targetType;
    private TargetState spawnType;
    private SpawnPosition spawnPosition;
    private LineRenderer pathLine;
    private Transform hangingPos;
    private float routeStartPos;
    private float routeEndPos;
    private float routeSpawnPos;

    [ShowIf("IsMoving")]    private float speed;
    [ShowIfNot("IsMoving")] private float lifeTime;

    private ObjectPool op;

    [Tooltip("Если true — движение от конца к началу, иначе от начала к концу")]
    private bool reverse = false;

    public void SetTargetPrefs(SpawnStep spawnStep, ObjectPool objectPool, bool startAction = true)
    {
        targetType = spawnStep.targetType;
        spawnType = spawnStep.spawnType;
        spawnPosition = spawnStep.spawnPosition;

        speed = spawnStep.speed;
        lifeTime = spawnStep.lifeTime;

        routeStartPos = spawnStep.routeStartPos;
        routeEndPos = spawnStep.routeEndPos;
        routeSpawnPos = spawnStep.routeSpawnPos;

        pathLine = StaticData.movingLines[(int)spawnPosition%3];
        hangingPos = StaticData.hangingSpawnPositions[(int)spawnPosition];

        op = objectPool;

        int orderLayer = 90;

        if (spawnType != TargetState.Hanging)
        {
            orderLayer = 30 + ((int)spawnPosition % 3) * 20;
        }

        SetSortingOrder(orderLayer);

        lowerHolderSR.gameObject.SetActive(spawnType != TargetState.Hanging);
        upperHolderSR.gameObject.SetActive(spawnType == TargetState.Hanging);

        spriteRenderer.flipX = spawnStep.routeStartPos > routeEndPos;

        if (startAction) { StartAction(); }
    }

    private void StartAction()
    {
        StopAllCoroutines();

        if (pathLine == null || pathLine.positionCount < 2)
            return;

        reverse = ((int)spawnPosition) > 2;

        switch(spawnType)
        {
            case TargetState.Moving:
                StartCoroutine(MoveAlongPath());
                break;
            case TargetState.Static:
                StartCoroutine(AppearAndDisappear(SpecialUtilits.GetPointOnLine(pathLine,routeSpawnPos)));
                break;
            case TargetState.Hanging:
                StartCoroutine(AppearAndDisappear(hangingPos.position, 10f));
                break;
            case TargetState.EphemeralMoving:
                StartCoroutine(MoveAlongRoute());
                break;
        }
    }

    private IEnumerator MoveAlongPath()
    {
        int index = reverse ? pathLine.positionCount - 1 : 0;
        int direction = reverse ? -1 : 1;

        // начальная позиция
        transform.position = pathLine.GetPosition(index) + pathLine.transform.position;
        index += direction;

        while (index >= 0 && index < pathLine.positionCount)
        {
            Vector3 targetPoint = pathLine.GetPosition(index);

            // пока не достигли точки
            while (Vector3.Distance(transform.position, targetPoint + pathLine.transform.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint + pathLine.transform.position, speed * Time.deltaTime);

                yield return null;
            }

            // следующая точка
            index += direction;
        }

        op.ReturnObject(gameObject);
    }

    private IEnumerator AppearAndDisappear(Vector3 targetPos, float appearYOffset = -2f, float appearSpeed = 10f)
    {
        Coroutine wandering = null;

        // Начальная позиция: выше или ниже targetPos в зависимости от знака offset
        Vector3 startPos = targetPos + Vector3.up * appearYOffset;
        transform.position = startPos;

        // Двигаемся в targetPos
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, appearSpeed * Time.deltaTime);
            yield return null;
        }

        // Если тип Hanging, запускаем блуждание
        if (spawnType == TargetState.Hanging)
        {
            wandering = StartCoroutine(Wander());
        }

        // Живём lifeTime секунд
        yield return new WaitForSeconds(lifeTime);

        // Останавливаем блуждание
        if (spawnType == TargetState.Hanging && wandering != null)
        {
            StopCoroutine(wandering);
        }

        // Возвращаемся обратно (туда, откуда пришли)
        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, appearSpeed * Time.deltaTime);
            yield return null;
        }

        op.ReturnObject(gameObject);
    }

    public IEnumerator MoveAlongRoute(float appearOffsetY = 3f, float appearDuration = 0.5f, bool rotateTowardsMovement = false)
    {
        if (pathLine == null || pathLine.positionCount < 2)
        {
            Debug.LogWarning("MoveAlongRouteOnce: pathLine не назначен или слишком короткий!");
            yield break;
        }

        // --- подготовка мировых точек линии ---
        int cnt = pathLine.positionCount;
        Vector3[] pts = new Vector3[cnt];
        bool worldSpace = pathLine.useWorldSpace;
        Transform lrT = pathLine.transform;
        for (int i = 0; i < cnt; i++)
        {
            Vector3 p = pathLine.GetPosition(i);
            pts[i] = worldSpace ? p : lrT.TransformPoint(p);
        }

        // --- длины сегментов и общая длина ---
        int segCount = cnt - 1;
        float[] segLen = new float[segCount];
        float totalLen = 0f;
        for (int i = 0; i < segCount; i++)
        {
            segLen[i] = Vector3.Distance(pts[i], pts[i + 1]);
            totalLen += segLen[i];
        }

        if (totalLen <= Mathf.Epsilon)
        {
            Debug.LogWarning("MoveAlongRouteOnce: длина линии ~0");
            yield break;
        }

        // --- вспомогательная функция: позиция на линии по t [0..1] ---
        Vector3 GetPointOnLine(float t)
        {
            t = Mathf.Clamp01(t);
            float targetDist = t * totalLen;
            float acc = 0f;
            for (int i = 0; i < segCount; i++)
            {
                if (acc + segLen[i] >= targetDist)
                {
                    float localT = Mathf.Clamp01((targetDist - acc) / segLen[i]);
                    return Vector3.Lerp(pts[i], pts[i + 1], localT);
                }
                acc += segLen[i];
            }
            return pts[cnt - 1];
        }

        // --- вспомогательная: найти индекс сегмента, где лежит t ---
        int FindSegmentIndex(float t)
        {
            t = Mathf.Clamp01(t);
            float targetDist = t * totalLen;
            float acc = 0f;
            for (int i = 0; i < segCount; i++)
            {
                if (acc + segLen[i] >= targetDist) return i;
                acc += segLen[i];
            }
            return segCount - 1;
        }

        // старт/конец в мировых координатах
        Vector3 startPoint = GetPointOnLine(routeStartPos);
        Vector3 endPoint = GetPointOnLine(routeEndPos);
        int startSeg = FindSegmentIndex(routeStartPos);
        int endSeg = FindSegmentIndex(routeEndPos);

        // --- собираем последовательность вершин, которые нужно пройти по порядку ---
        // pathNodes[0] = startPoint, последний = endPoint.
        List<Vector3> pathNodes = new List<Vector3>();
        pathNodes.Add(startPoint);

        if (startSeg == endSeg)
        {
            // на одном сегменте — лишь добавить конечную точку (если она не совпадает)
            if ((startPoint - endPoint).sqrMagnitude > 1e-6f)
                pathNodes.Add(endPoint);
        }
        else if (startSeg < endSeg)
        {
            // идём по увеличению индекса: добавляем вершины startSeg+1 ... endSeg, затем endPoint
            for (int i = startSeg + 1; i <= endSeg; i++)
                pathNodes.Add(pts[i]);
            pathNodes.Add(endPoint);
        }
        else // startSeg > endSeg (движение "назад" по массиву точек)
        {
            // добавляем вершины startSeg, startSeg-1, ..., endSeg+1  (в этом порядке)
            for (int i = startSeg; i > endSeg; i--)
                pathNodes.Add(pts[i]);
            pathNodes.Add(endPoint);
        }

        // --- Появление: из падения вниз к стартовой точке ---
        Vector3 appearFrom = startPoint + Vector3.down * appearOffsetY;
        transform.position = appearFrom;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(appearDuration, 0.0001f);
            transform.position = Vector3.Lerp(appearFrom, startPoint, Mathf.Clamp01(t));
            yield return null;
        }
        transform.position = startPoint; // жесткая установка, чтобы избежать погрешностей

        Coroutine floating = StartCoroutine(Float());

        // --- Движение по узлам pathNodes последовательно (только "туда") ---
        for (int i = 0; i < pathNodes.Count - 1; i++)
        {
            Vector3 from = pathNodes[i];
            Vector3 to = pathNodes[i + 1];

            // если мы уже немного не в from (например, из-за небольших погрешностей), корректируем
            // но не телепортируем, чтобы движение оставалось плавным
            // двигаем к 'to'
            while (Vector3.Distance(transform.position, to) > 0.01f)
            {
                // двигаться с фиксированной скоростью вдоль прямого сегмента (MoveTowards)
                transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);

                // вращение в сторону движения, если нужно
                if (rotateTowardsMovement)
                {
                    Vector3 dir = to - transform.position;
                    if (dir.sqrMagnitude > 1e-6f)
                        transform.rotation = Quaternion.LookRotation(dir);
                }

                yield return null;
            }

            // точная установка, чтобы не накапливались ошибки
            transform.position = to;
        }

        // --- опциональная пауза на lifeTime ---
        if (lifeTime > 0f)
            yield return new WaitForSeconds(lifeTime);

        StopCoroutine(floating);

        // --- Исчезновение: опускаемся вниз от текущей позиции и удаляем объект ---
        Vector3 disappearTo = transform.position + Vector3.down * appearOffsetY;
        t = 0f;
        Vector3 startDis = transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(appearDuration, 0.0001f);
            transform.position = Vector3.Lerp(startDis, disappearTo, Mathf.Clamp01(t));
            yield return null;
        }

        Return();
    }

    public void GetHitted()
    {
        StopAllCoroutines();
        animator.SetTrigger(shotDownTrigger);

        FindFirstObjectByType<ScoreManager>().AddScore(10);
    }

    public void GoBack()
    {
        float appearOffsetY = 3f; float appearDuration = 0.5f;

        StartCoroutine(gb());

        IEnumerator gb()
        {
            Vector3 dir = spawnType == TargetState.Hanging ? Vector3.up : Vector3.down;
            appearOffsetY = spawnType == TargetState.Hanging ? appearOffsetY * 3 : appearOffsetY;
            appearDuration = spawnType == TargetState.Hanging ? appearDuration * 3 : appearDuration;

            Vector3 disappearTo = transform.position + dir * appearOffsetY;
            float t = 0f;
            Vector3 startDis = transform.position;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(appearDuration, 0.0001f);
                transform.position = Vector3.Lerp(startDis, disappearTo, Mathf.Clamp01(t));
                yield return null;
            }

            Return();
        }
    }

    public void Return()
    {
        StopAllCoroutines();
        op.ReturnObject(gameObject);
    }

    private void SetSortingOrder(int layerNum, int holderLayerNum = -1)
    {

        spriteRenderer.sortingOrder = layerNum;
        lowerHolderSR.sortingOrder = layerNum + holderLayerNum;
        upperHolderSR.sortingOrder = layerNum + holderLayerNum;
    }

    private IEnumerator Wander(float wanderRadius = 1f, float wanderSpeed = 2f)
    {
        Vector3 origin = transform.position;

        while (true)
        {
            // выбираем случайную точку в радиусе
            Vector2 randCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 target = origin + new Vector3(randCircle.x, randCircle.y, 0);

            // плавно двигаемся к цели
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, wanderSpeed * Time.deltaTime);
                yield return null;
            }

            // чуть ждём перед следующим движением
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Float(float radius = 0.75f, float cycleTime = 2f, float angle = 90f)
    {
        float time = 0;

        while (true)
        {
            float theMove = radius * Mathf.Cos(time*2*Mathf.PI/cycleTime) * Time.fixedDeltaTime;

            transform.DOMoveX(transform.position.x + theMove * Mathf.Cos(Mathf.Deg2Rad * angle), 0);
            transform.DOMoveY(transform.position.y + theMove * Mathf.Sin(Mathf.Deg2Rad * angle), 0);

            time += Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }


    private bool IsMoving() => spawnType == TargetState.Moving;
}
