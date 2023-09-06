using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BezierSpline : MonoBehaviour
{
    [SerializeField]
    private List<Point> points = new List<Point>();

    public int ControlPointCount
    {
        get
        {
            return points.Count;
        }
    }

    public Vector3 GetControlPoint(int pointIndex, int positionIndex)
    {
        return points[pointIndex].positions[positionIndex];
    }

    public void SetControlPoint(int pointIndex, int positionIndex, Vector3 point)
    {
        if (positionIndex == 0)
        {
            Vector3 delta = point - points[pointIndex].positions[positionIndex];
            if (pointIndex > 0)
            {
                points[pointIndex].positions[1] += delta;
            }
            if (pointIndex + 1 < points.Count) { }
            {
                points[pointIndex].positions[2] += delta;
            }
        }
        points[pointIndex].positions[positionIndex] = point;
        EnforceMode(pointIndex, positionIndex);
    }

    public void SetControlPointMode(int pointIndex, int positionIndex, BezierControlPointMode mode)
    {
        points[pointIndex].controlPointMode = mode;
        EnforceMode(pointIndex, positionIndex);
    }

    private void EnforceMode(int pointIndex, int positionIndex)
    {
        BezierControlPointMode mode = points[pointIndex].controlPointMode;
        if (mode == BezierControlPointMode.Free)
        {
            return;
        }

        int fixedIndex;
        int enforcedIndex;

        if (positionIndex == 0 || positionIndex == 1)
        {
            fixedIndex = 1;
            enforcedIndex = 2;
        }

        else
        {
            fixedIndex = 2;
            enforcedIndex = 1;
        }

        Vector3 middle = points[pointIndex].positions[0];
        Vector3 enforcedTangent = middle - points[pointIndex].positions[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[pointIndex].positions[enforcedIndex]);
        }
        points[pointIndex].positions[enforcedIndex] = middle + enforcedTangent;


    }

    [Serializable]
    public class Point
    {
        public Vector3[] positions = new Vector3[3];
        public BezierControlPointMode controlPointMode;
    }

    public int CurveCount
    {
        get
        {
            return (points.Count - 1);
        }
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
        }
        return transform.TransformPoint(Bezier.GetPoint(points[i].positions[0], points[i].positions[2], points[i + 1].positions[1], points[i + 1].positions[0], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i].positions[0], points[i].positions[2], points[i + 1].positions[1], points[i + 1].positions[0], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return points[index].controlPointMode;
    }

    public void AddCurve()
    {
        Point newPoint = new Point();
        Vector3 lastPoint = points[ControlPointCount - 1].positions[2];

        newPoint.positions[0] = new Vector3(lastPoint.x + 8.0f, lastPoint.y, lastPoint.z);
        newPoint.positions[1] = new Vector3(lastPoint.x + 4.0f, lastPoint.y, lastPoint.z);
        newPoint.positions[2] = new Vector3(lastPoint.x + 12.0f, lastPoint.y, lastPoint.z);

        points.Add(newPoint);
        EnforceMode(points.Count - 2, 2);
    }

    private void Reset()
    {
        Point point1 = new Point();
        Point point2 = new Point();

        point1.positions[0] = new Vector3(8.0f, 0.0f, 0.0f);
        point1.positions[1] = new Vector3(4.0f, 0.0f, 0.0f);
        point1.positions[2] = new Vector3(12.0f, 0.0f, 0.0f);

        point2.positions[0] = new Vector3(20.0f, 0.0f, 0.0f);
        point2.positions[1] = new Vector3(16.0f, 0.0f, 0.0f);
        point2.positions[2] = new Vector3(24.0f, 0.0f, 0.0f);

        point1.controlPointMode = BezierControlPointMode.Free;
        point2.controlPointMode = BezierControlPointMode.Free;

        points.Add(point1);
        points.Add(point2);
    }
}


