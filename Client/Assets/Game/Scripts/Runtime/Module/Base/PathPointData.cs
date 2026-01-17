using UnityEngine;
using System.Collections.Generic;

public class PathPointData : ScriptableObject
{
    [System.Serializable]
    public class PathPoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 tangent;  // 曲线切线方向
        public float distanceFromStart;
        public float scale = 1f;  // 添加大小值
        public bool isCurvePoint = false; // 是否为曲线
        public bool followTangent = true;
        public Vector3 curveHandleForward = Vector3.zero;
        public Vector3 curveHandleBack = Vector3.zero;
        
        public PathPoint(Vector3 pos, Quaternion rot, Vector3 tan, float distance, bool isCurvePoint, float scale = 1f, bool followTangent = false)
        {
            this.position = pos;
            this.rotation = rot;
            this.tangent = tan;
            this.isCurvePoint = isCurvePoint;
            this.distanceFromStart = distance;
            this.scale = scale;
            this.followTangent = followTangent;
        }
    }
    
    public List<PathPoint> points = new List<PathPoint>();
    public float totalLength = 0f;
    public int segmentsPerCurve = 20;
    
    public void ClearPoints()
    {
        points.Clear();
        totalLength = 0f;
    }
    
    public void AddPoint(Vector3 pos, Quaternion rot, Vector3 tan, float distance, bool isCurvePoint, float scale = 1f)
    {
        points.Add(new PathPoint(pos, rot, tan, distance, isCurvePoint, scale));
    }
    
    public Vector3 GetPointPosition(int index)
    {
        if (index >= 0 && index < points.Count)
            return points[index].position;
        return Vector3.zero;
    }
    
    public Quaternion GetPointRotation(int index)
    {
        if (index >= 0 && index < points.Count)
            return points[index].rotation;
        return Quaternion.identity;
    }
    
    public Vector3 GetPointTangent(int index)
    {
        if (index >= 0 && index < points.Count)
            return points[index].tangent;
        return Vector3.forward;
    }
    
    public float GetPointScale(int index)
    {
        if (index >= 0 && index < points.Count)
            return points[index].scale;
        return 1f;
    }
    
    // 主要方法：返回当前位置的位置、旋转、大小
    public (Vector3 position, Quaternion rotation, float scale) GetPositionRotationScaleAtDistance(float distance)
    {
        distance = Mathf.Clamp(distance, 0, totalLength);
        
        if (points.Count == 0)
            return (Vector3.zero, Quaternion.identity, 1f);
        
        if (points.Count == 1)
            return (points[0].position, points[0].rotation, points[0].scale);
        
        // 找到距离所在的段
        for (int i = 0; i < points.Count - 1; i++)
        {
            float startDist = points[i].distanceFromStart;
            float endDist = points[i + 1].distanceFromStart;
            
            if (distance >= startDist && distance <= endDist)
            {
                float segmentLength = endDist - startDist;
                if (segmentLength <= Mathf.Epsilon)
                {
                    // 如果段长度为0，直接返回起点
                    return (points[i].position, points[i].rotation, points[i].scale);
                }
                
                float t = (distance - startDist) / segmentLength;
                
                // 插值计算位置
                Vector3 position = Vector3.Lerp(points[i].position, points[i + 1].position, t);
                
                // 插值计算旋转
                // Quaternion rotation = Quaternion.Slerp(points[i].rotation, points[i + 1].rotation, t);
                
                // 插值计算大小
                // float scale = Mathf.Lerp(points[i].scale, points[i + 1].scale, t);
                return (position, points[i].rotation, points[i].scale);
            }
        }
        
        // 如果距离超出范围，返回最后一个点
        var lastPoint = points[points.Count - 1];
        return (lastPoint.position, lastPoint.rotation, lastPoint.scale);
    }
    
    // 重载方法，只返回位置和旋转（保持向后兼容）
    public (Vector3 position, Quaternion rotation) GetPositionAndRotationAtDistance(float distance)
    {
        var result = GetPositionRotationScaleAtDistance(distance);
        return (result.position, result.rotation);
    }
    
    // 只返回位置
    public Vector3 GetPositionAtDistance(float distance)
    {
        var result = GetPositionRotationScaleAtDistance(distance);
        return result.position;
    }
    
    // 只返回旋转
    public Quaternion GetRotationAtDistance(float distance)
    {
        var result = GetPositionRotationScaleAtDistance(distance);
        return result.rotation;
    }
    
    // 只返回大小
    public float GetScaleAtDistance(float distance)
    {
        var result = GetPositionRotationScaleAtDistance(distance);
        return result.scale;
    }
    
    // 获取最近的点索引
    public int GetClosestPointIndex(Vector3 position)
    {
        if (points.Count == 0) return -1;
        
        int closestIndex = 0;
        float closestDistance = Vector3.Distance(position, points[0].position);
        
        for (int i = 1; i < points.Count; i++)
        {
            float distance = Vector3.Distance(position, points[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    // 获取最近点的距离
    public float GetClosestDistance(Vector3 position)
    {
        int index = GetClosestPointIndex(position);
        if (index >= 0 && index < points.Count)
            return points[index].distanceFromStart;
        return 0f;
    }
    
    // 获取总点数
    public int GetPointCount()
    {
        return points.Count;
    }
    
    // 检查是否有数据
    public bool HasData()
    {
        return points.Count > 0;
    }
    
    // 获取距离占总长度的百分比
    public float GetNormalizedDistance(float distance)
    {
        if (totalLength <= Mathf.Epsilon) return 0f;
        return Mathf.Clamp01(distance / totalLength);
    }
    
    // 根据百分比获取距离
    public float GetDistanceFromNormalized(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        return totalLength * normalized;
    }
    
    // 获取指定百分比的位置、旋转、大小
    public (Vector3 position, Quaternion rotation, float scale) GetPositionRotationScaleAtNormalized(float normalized)
    {
        float distance = GetDistanceFromNormalized(normalized);
        return GetPositionRotationScaleAtDistance(distance);
    }
    
    // 获取路径的方向（切线）在指定距离
    public Vector3 GetTangentAtDistance(float distance)
    {
        distance = Mathf.Clamp(distance, 0, totalLength);
        
        if (points.Count == 0)
            return Vector3.forward;
        
        if (points.Count == 1)
            return points[0].tangent;
        
        // 找到距离所在的段
        for (int i = 0; i < points.Count - 1; i++)
        {
            float startDist = points[i].distanceFromStart;
            float endDist = points[i + 1].distanceFromStart;
            
            if (distance >= startDist && distance <= endDist)
            {
                float segmentLength = endDist - startDist;
                if (segmentLength <= Mathf.Epsilon)
                    return points[i].tangent;
                
                float t = (distance - startDist) / segmentLength;
                return Vector3.Slerp(points[i].tangent, points[i + 1].tangent, t).normalized;
            }
        }
        
        return points[points.Count - 1].tangent;
    }
    
    // 在编辑器中绘制路径
    public void DrawGizmos(bool drawPoints = true, bool drawPath = true, bool drawTangents = false)
    {
        if (points.Count == 0) return;
        
        Color originalColor = Gizmos.color;
        
        // 绘制路径点
        if (drawPoints)
        {
            Gizmos.color = Color.red;
            foreach (var point in points)
            {
                Gizmos.DrawSphere(point.position, 0.1f);
            }
        }
        
        // 绘制路径连线
        if (drawPath && points.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
        
        // 绘制切线
        if (drawTangents)
        {
            Gizmos.color = Color.blue;
            foreach (var point in points)
            {
                Gizmos.DrawRay(point.position, point.tangent * 0.5f);
            }
        }
        
        Gizmos.color = originalColor;
    }
}