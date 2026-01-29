using UnityEngine;

public static class SpecialUtilits
{
    public static Vector3 GetPointOnLine(LineRenderer line, float t)
    {
        t = Mathf.Clamp01(t);

        int count = line.positionCount;
        if (count < 2)
            return line.transform.position;

        Vector3[] points = new Vector3[count];
        line.GetPositions(points);

        // 1. Общая длина линии
        float totalLength = 0f;
        float[] segmentLengths = new float[count - 1];

        for (int i = 0; i < count - 1; i++)
        {
            float length = Vector3.Distance(points[i], points[i + 1]);
            segmentLengths[i] = length;
            totalLength += length;
        }

        // 2. Целевая длина
        float targetLength = totalLength * t;

        // 3. Поиск сегмента
        float currentLength = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (currentLength + segmentLengths[i] >= targetLength)
            {
                float segmentT = (targetLength - currentLength) / segmentLengths[i];
                return Vector3.Lerp(points[i], points[i + 1], segmentT);
            }

            currentLength += segmentLengths[i];
        }

        // На случай погрешностей
        return points[count - 1];
    }
}
