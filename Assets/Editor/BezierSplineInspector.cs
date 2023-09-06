using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{
    private const int stepsPerCurve = 10;
    private const float directionScale = 1f;

    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int selectedPoint;
    private int selectedPosition;

    private bool showDirectionEnabled = true;

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < spline.ControlPointCount - 1; i++)
        {
            Vector3 startPoint = ShowPoint(i, 0);
            Vector3 endPoint = ShowPoint(i + 1, 0);
            Vector3 controlPoint1 = ShowPoint(i, 2);
            Vector3 controlPoint2 = ShowPoint(i + 1, 1);

            Handles.color = Color.gray;
            Handles.DrawAAPolyLine(startPoint, controlPoint1);
            Handles.DrawAAPolyLine(endPoint, controlPoint2);

            ShowDirections();
            Handles.color = Color.white;
            Handles.DrawBezier(startPoint, endPoint, controlPoint1, controlPoint2, Color.white, null, 2f);
        }
    }

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private Vector3 ShowPoint(int pointIndex, int positionIndex)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(pointIndex, positionIndex));
        float size = HandleUtility.GetHandleSize(point);
        Handles.color = modeColors[(int)spline.GetControlPointMode(pointIndex)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedPoint = pointIndex;
            selectedPosition = positionIndex;
            Repaint();
        }
        if (selectedPoint == pointIndex && selectedPosition == positionIndex)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(pointIndex, positionIndex, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }

    private void ShowDirections()
    {
        if (showDirectionEnabled == true)
        {
            Handles.color = Color.green;
            Vector3 point = spline.GetPoint(0f);
            Handles.DrawAAPolyLine(point, point + spline.GetDirection(0f) * directionScale);
            int steps = stepsPerCurve * spline.CurveCount;
            for (int i = 1; i <= steps; i++)
            {
                point = spline.GetPoint(i / (float)steps);
                Handles.DrawAAPolyLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        spline = target as BezierSpline;
        if (selectedPoint >= 0 && selectedPoint < spline.ControlPointCount)
        {
            if (selectedPosition >= 0 && selectedPosition < 3)
            {
                DrawSelectedPointInspector();
            }
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }
    
    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedPoint, selectedPosition));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedPoint, selectedPosition, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)
            EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedPoint));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedPoint, selectedPosition, mode);
            EditorUtility.SetDirty(spline);
        }
    }
}
