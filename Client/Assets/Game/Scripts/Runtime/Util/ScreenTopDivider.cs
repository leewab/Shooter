using UnityEngine;
using System.Collections.Generic;

public static class ScreenTopDivider
{
    private static Camera targetCamera;

    public static Camera TargetCamera
    {
        get
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            return targetCamera;
        }
    }


    // 获取分割点的世界坐标列表
    public static List<Vector2> GetCoverageAreaPositions(int horizontalDivisions, int verticalDivisions, float downwardUnits = 100f, float yOffset = 0f)
    {
        List<Vector2> positions = new List<Vector2>();

        if (TargetCamera == null)
        {
            Debug.LogError("No camera assigned!");
            return positions;
        }

        // 获取屏幕尺寸
        float screenWidth = TargetCamera.pixelWidth;
        float screenHeight = TargetCamera.pixelHeight;

        // 计算每个分割的宽度
        float horDivisionWidth = screenWidth / horizontalDivisions;
        // 包含起点和终点
        for (int i = 0; i <= horizontalDivisions; i++) 
        {
            // 计算屏幕坐标
            float screenX = i * horDivisionWidth;
            // 屏幕最上方
            float screenY = screenHeight - yOffset; 

            Vector2 screenPoint = new Vector2(screenX, screenY);

            // 转换为世界坐标
            Vector2 worldPoint = ScreenToWorld2D(screenPoint);

            positions.Add(worldPoint);
        }

        // 计算每个分割的宽度
        float verDivisionWidth = screenWidth / verticalDivisions;
        for (int i = 0; i < verticalDivisions; i++)
        {
            float screenY = screenHeight - yOffset - (i * verDivisionWidth);
            float screenLeftX = 0; 
            float screenRightX = screenWidth; 

            Vector2 screenLeftPoint = new Vector2(screenLeftX, screenY);
            Vector2 screenRightPoint = new Vector2(screenRightX, screenY);

            // 转换为世界坐标
            Vector2 worldLeftPoint = ScreenToWorld2D(screenLeftPoint);
            Vector2 worldRightPoint = ScreenToWorld2D(screenRightPoint);

            positions.Add(worldLeftPoint);
            positions.Add(worldRightPoint);
        }

        return positions;
    }


    // 获取分割点的世界坐标列表（从屏幕中心计算）
    public static List<Vector2> GetDividedTopPositionsFromCenter(int divisionCount, float yOffset = 0f)
    {
        List<Vector2> positions = new List<Vector2>();

        if (TargetCamera == null)
        {
            Debug.LogError("No camera assigned!");
            return positions;
        }

        // 计算每个分割的宽度
        float divisionWidth = TargetCamera.pixelWidth / divisionCount;

        for (int i = 0; i <= divisionCount; i++)
        {
            // 计算相对于屏幕中心的位置
            float screenX = i * divisionWidth - (TargetCamera.pixelWidth / 2f);
            float screenY = (TargetCamera.pixelHeight / 2f) - yOffset; // 屏幕最上方，相对于中心

            Vector2 screenPoint = new Vector2(screenX, screenY);

            // 转换为世界坐标
            Vector2 worldPoint = ScreenToWorld2D(screenPoint, false);

            positions.Add(worldPoint);
        }

        return positions;
    }

    // 获取分割点的世界坐标列表（使用视口坐标）
    public static List<Vector2> GetDividedTopPositionsUsingViewport(int divisionCount, float yOffset = 0f)
    {
        List<Vector2> positions = new List<Vector2>();

        if (TargetCamera == null)
        {
            Debug.LogError("No camera assigned!");
            return positions;
        }

        // 计算每个分割的视口宽度
        float divisionWidth = 1f / divisionCount;

        for (int i = 0; i <= divisionCount; i++)
        {
            // 计算视口坐标
            float viewportX = i * divisionWidth;
            float viewportY = 1f; // 视口最上方

            Vector2 viewportPoint = new Vector2(viewportX, viewportY);

            // 转换为世界坐标
            Vector2 worldPoint =
                TargetCamera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y,
                    TargetCamera.nearClipPlane));

            positions.Add(worldPoint);
        }

        return positions;
    }

    // 获取屏幕边界的世界坐标
    public static List<Vector2> GetScreenCornersWorld2D()
    {
        List<Vector2> corners = new List<Vector2>();

        if (TargetCamera == null)
        {
            Debug.LogError("No camera assigned!");
            return corners;
        }

        // 左下角
        Vector3 bottomLeft = TargetCamera.ViewportToWorldPoint(new Vector3(0, 0, TargetCamera.nearClipPlane));
        // 右上角
        Vector3 topRight = TargetCamera.ViewportToWorldPoint(new Vector3(1, 1, TargetCamera.nearClipPlane));
        // 左上角
        Vector3 topLeft = TargetCamera.ViewportToWorldPoint(new Vector3(0, 1, TargetCamera.nearClipPlane));
        // 右下角
        Vector3 bottomRight = TargetCamera.ViewportToWorldPoint(new Vector3(1, 0, TargetCamera.nearClipPlane));

        corners.Add(bottomLeft);
        corners.Add(topLeft);
        corners.Add(topRight);
        corners.Add(bottomRight);

        return corners;
    }

    // 屏幕坐标转世界坐标（2D）
    private static Vector2 ScreenToWorld2D(Vector2 screenPosition, bool absoluteScreenPos = true)
    {
        if (absoluteScreenPos)
        {
            // 绝对屏幕坐标
            Vector3 worldPoint =
                TargetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y,
                    TargetCamera.nearClipPlane));
            return new Vector2(worldPoint.x, worldPoint.y);
        }
        else
        {
            // 相对于屏幕中心的坐标
            Vector3 screenCenter = new Vector3(TargetCamera.pixelWidth / 2f, TargetCamera.pixelHeight / 2f,
                TargetCamera.nearClipPlane);
            Vector3 worldCenter = TargetCamera.ScreenToWorldPoint(screenCenter);
            Vector3 worldPoint = TargetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x + screenCenter.x,
                screenPosition.y + screenCenter.y, TargetCamera.nearClipPlane));
            return new Vector2(worldPoint.x, worldPoint.y);
        }
    }
}