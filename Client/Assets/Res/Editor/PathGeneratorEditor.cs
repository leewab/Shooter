#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PathGeneratorEditor : EditorWindow
{
    [System.Serializable]
    public class KeyPoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool followTangent = true;  // true=跟随切线，false=自定义旋转
        public Vector3 tangent;            // 切线方向
        public Vector3 handleForward;      // 贝塞尔曲线前向手柄
        public Vector3 handleBack;         // 贝塞尔曲线后向手柄
        public float scale = 1f;
        public bool isCurvePoint = false;  // 是否为曲线模式
        
        public KeyPoint(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            tangent = Vector3.right;  // 2D模式默认向右
            handleForward = pos + Vector3.forward;
            handleBack = pos - Vector3.forward;
        }
        
        public void ToggleCurveMode()
        {
            isCurvePoint = !isCurvePoint;
        }
    }
    
    public enum PathMode { Mode2D, Mode3D }
    public enum Plane2D { XY, XZ, YZ }
    
    private List<KeyPoint> keyPoints = new List<KeyPoint>();
    private Vector2 scrollPosition = Vector2.zero;
    private int selectedPoint = -1;
    private EditTarget currentEditTarget = EditTarget.Position;
    private PathMode currentMode = PathMode.Mode2D;
    private Plane2D currentPlane = Plane2D.XY;
    private PathPointData currentPathData;
    
    // 显示设置
    private bool showTangents = true;
    private bool showCurve = true;
    private Color keyPointColor = Color.red;
    private Color curveColor = Color.green;
    private Color tangentColor = Color.blue;
    private Color handleColor = Color.cyan;
    private float tangentLength = 1f;
    private int segmentsPerCurve = 20;
    
    public enum EditTarget { Position, Rotation, Scale }
    
    [MenuItem("Tools/Custom Path Generator")]
    public static void ShowWindow()
    {
        GetWindow<PathGeneratorEditor>("Custom Path Generator");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (keyPoints.Count == 0)
        {
            InitializeDefaultPoints();
        }
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void InitializeDefaultPoints()
    {
        keyPoints.Clear();
        
        // 根据当前模式初始化
        if (currentMode == PathMode.Mode2D)
        {
            switch (currentPlane)
            {
                case Plane2D.XY:
                    keyPoints.Add(new KeyPoint(new Vector3(0, 0, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(3, 0, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(6, 3, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(9, 0, 0)));
                    break;
                    
                case Plane2D.XZ:
                    keyPoints.Add(new KeyPoint(new Vector3(0, 0, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(3, 0, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(6, 0, 3)));
                    keyPoints.Add(new KeyPoint(new Vector3(9, 0, 0)));
                    break;
                    
                case Plane2D.YZ:
                    keyPoints.Add(new KeyPoint(new Vector3(0, 0, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(0, 3, 0)));
                    keyPoints.Add(new KeyPoint(new Vector3(0, 6, 3)));
                    keyPoints.Add(new KeyPoint(new Vector3(0, 9, 0)));
                    break;
            }
        }
        else
        {
            // 3D模式
            keyPoints.Add(new KeyPoint(new Vector3(0, 0, 0)));
            keyPoints.Add(new KeyPoint(new Vector3(3, 1, 1)));
            keyPoints.Add(new KeyPoint(new Vector3(6, 0, 3)));
            keyPoints.Add(new KeyPoint(new Vector3(9, 2, 1)));
        }
        
        selectedPoint = 0;
        RecalculateTangents();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 1. 模式选择
        EditorGUILayout.Space();
        GUILayout.Label("Path Settings", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        currentMode = (PathMode)EditorGUILayout.EnumPopup("Path Mode", currentMode);
        if (currentMode == PathMode.Mode2D)
        {
            currentPlane = (Plane2D)EditorGUILayout.EnumPopup("2D Plane", currentPlane);
        }
        if (EditorGUI.EndChangeCheck())
        {
            InitializeDefaultPoints();
        }
        
        // 2. 路径数据
        EditorGUILayout.Space();
        GUILayout.Label("Path Data", EditorStyles.boldLabel);
        currentPathData = (PathPointData)EditorGUILayout.ObjectField("Path Data", currentPathData, typeof(PathPointData), false);
        
        if (GUILayout.Button("Create New Path Data"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Path Data", "NewPathData", "asset", "Save Path Data");
            if (!string.IsNullOrEmpty(path))
            {
                PathPointData data = CreateInstance<PathPointData>();
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
                currentPathData = data;
            }
        }
        
        if (GUILayout.Button("加载数据"))
        {
            LoadPathData();
        }
        
        if (GUILayout.Button("生成路径"))
        {
            GeneratePathData();
        }
        
        // 3. 编辑目标
        EditorGUILayout.Space();
        GUILayout.Label("Edit Target", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Position", currentEditTarget == EditTarget.Position ? "Button" : "Button")) 
            currentEditTarget = EditTarget.Position;
        if (GUILayout.Button("Rotation", currentEditTarget == EditTarget.Rotation ? "Button" : "Button")) 
            currentEditTarget = EditTarget.Rotation;
        if (GUILayout.Button("Scale", currentEditTarget == EditTarget.Scale ? "Button" : "Button")) 
            currentEditTarget = EditTarget.Scale;
        EditorGUILayout.EndHorizontal();
        
        // 4. 显示设置
        EditorGUILayout.Space();
        GUILayout.Label("Display Settings", EditorStyles.boldLabel);
        showTangents = EditorGUILayout.Toggle("Show Tangents", showTangents);
        showCurve = EditorGUILayout.Toggle("Show Curve", showCurve);
        tangentLength = EditorGUILayout.FloatField("Tangent Length", tangentLength);
        segmentsPerCurve = EditorGUILayout.IntSlider("Curve Segments", segmentsPerCurve, 5, 50);
        
        keyPointColor = EditorGUILayout.ColorField("Key Point Color", keyPointColor);
        curveColor = EditorGUILayout.ColorField("Curve Color", curveColor);
        tangentColor = EditorGUILayout.ColorField("Tangent Color", tangentColor);
        handleColor = EditorGUILayout.ColorField("Handle Color", handleColor);
        
        // 5. 关键点控制
        EditorGUILayout.Space();
        GUILayout.Label("Key Points Control", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Key Points: {keyPoints.Count}", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Point")) AddKeyPoint();
        if (GUILayout.Button("Remove Selected") && selectedPoint >= 0) RemoveKeyPoint(selectedPoint);
        if (GUILayout.Button("Clear All")) { keyPoints.Clear(); selectedPoint = -1; }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Recalc Tangents")) RecalculateTangents();
        if (GUILayout.Button("Apply Tangent to Following")) ApplyTangentToFollowingPoints();
        EditorGUILayout.EndHorizontal();
        
        // 6. 关键点列表
        EditorGUILayout.Space();
        GUILayout.Label("Key Points List", EditorStyles.boldLabel);
        
        for (int i = 0; i < keyPoints.Count; i++)
        {
            DisplayKeyPoint(i);
        }
        
        EditorGUILayout.EndScrollView();
        
        if (GUI.changed)
            SceneView.RepaintAll();
    }
    
    private void DisplayKeyPoint(int index)
    {
        KeyPoint point = keyPoints[index];
        bool isSelected = index == selectedPoint;
        
        EditorGUILayout.BeginHorizontal();
        
        // 选择按钮
        if (GUILayout.Toggle(isSelected, $"P{index}", "Button", GUILayout.Width(40)))
            selectedPoint = index;
        
        // 旋转模式状态
        string modeText = point.followTangent ? "Follow" : "Custom";
        Color modeColor = point.followTangent ? Color.green : Color.yellow;
        Color originalColor = GUI.contentColor;
        GUI.contentColor = modeColor;
        GUILayout.Label(modeText, GUILayout.Width(50));
        GUI.contentColor = originalColor;
        
        // 曲线模式状态
        string curveText = point.isCurvePoint ? "Curve" : "Line";
        GUI.contentColor = point.isCurvePoint ? Color.green : Color.gray;
        GUILayout.Label(curveText, GUILayout.Width(50));
        GUI.contentColor = originalColor;
        
        // 位置编辑
        EditorGUI.BeginChangeCheck();
        Vector3 newPos = EditorGUILayout.Vector3Field("", point.position);
        if (EditorGUI.EndChangeCheck() && currentEditTarget == EditTarget.Position)
        {
            point.position = ConstrainToPlane(newPos);
            keyPoints[index] = point;
            RecalculateTangents();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 选中点的详细设置
        if (isSelected)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 旋转模式切换
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Follow Tangent:", GUILayout.Width(100));
            bool newFollow = EditorGUILayout.Toggle(point.followTangent);
            if (newFollow != point.followTangent)
            {
                point.followTangent = newFollow;
                if (newFollow) // 切换到跟随切线，立即应用
                    ApplyTangentToPoint(index);
            }
            EditorGUILayout.EndHorizontal();
            
            // 曲线模式切换
            if (GUILayout.Button(point.isCurvePoint ? "Switch to Line" : "Switch to Curve"))
            {
                point.ToggleCurveMode();
                RecalculateTangents();
            }
            
            // 旋转编辑（自定义模式下可用）
            EditorGUI.BeginDisabledGroup(point.followTangent);
            EditorGUI.BeginChangeCheck();
            Vector3 euler = point.rotation.eulerAngles;
            Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", euler);
            if (EditorGUI.EndChangeCheck() && currentEditTarget == EditTarget.Rotation)
            {
                point.rotation = Quaternion.Euler(newEuler);
            }
            EditorGUI.EndDisabledGroup();
            
            // 大小编辑
            EditorGUI.BeginChangeCheck();
            float newScale = EditorGUILayout.FloatField("Scale", point.scale);
            if (EditorGUI.EndChangeCheck() && currentEditTarget == EditTarget.Scale)
            {
                point.scale = Mathf.Max(0.1f, newScale);
            }
            
            // 切线信息
            EditorGUILayout.LabelField("Tangent:", point.tangent.ToString("F3"));
            
            // 旋转信息
            Vector3 rotationEuler = point.rotation.eulerAngles;
            EditorGUILayout.LabelField($"Rotation: ({rotationEuler.x:F1}, {rotationEuler.y:F1}, {rotationEuler.z:F1})");
            
            EditorGUILayout.EndVertical();
            
            keyPoints[index] = point;
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        // 绘制曲线
        if (showCurve)
        {
            DrawCurves();
        }
        else
        {
            DrawLines();
        }
        
        // 绘制关键点
        DrawKeyPoints();
    }
    
    private void DrawLines()
    {
        if (keyPoints.Count < 2) return;
        
        Handles.color = Color.gray;
        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Handles.DrawLine(keyPoints[i].position, keyPoints[i + 1].position);
        }
    }
    
    private void DrawCurves()
    {
        if (keyPoints.Count < 2) return;
        
        Handles.color = curveColor;
        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            KeyPoint start = keyPoints[i];
            KeyPoint end = keyPoints[i + 1];
            
            if (start.isCurvePoint && end.isCurvePoint)
            {
                // 绘制贝塞尔曲线
                Handles.DrawBezier(
                    start.position, 
                    end.position, 
                    start.handleForward, 
                    end.handleBack, 
                    curveColor, 
                    null, 
                    2f
                );
                
                // 绘制手柄线
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                Handles.DrawDottedLine(start.position, start.handleForward, 2f);
                Handles.DrawDottedLine(end.position, end.handleBack, 2f);
                Handles.color = curveColor;
            }
            else
            {
                // 绘制直线
                Handles.DrawLine(start.position, end.position);
            }
        }
    }
    
    private void DrawKeyPoints()
    {
        for (int i = 0; i < keyPoints.Count; i++)
        {
            KeyPoint point = keyPoints[i];
            bool isSelected = i == selectedPoint;
            
            // 绘制关键点
            DrawKeyPoint(point, isSelected, i);
            
            // 绘制手柄（如果是曲线点）
            if (point.isCurvePoint)
            {
                DrawHandles(point, isSelected, i);
            }
        }
    }
    
    private void DrawKeyPoint(KeyPoint point, bool isSelected, int index)
    {
        // 设置颜色
        Color pointColor = point.followTangent ? 
            (isSelected ? Color.yellow : Color.green) : 
            (isSelected ? Color.yellow : keyPointColor);
        
        Handles.color = pointColor;
        
        // 计算大小
        float handleSize = HandleUtility.GetHandleSize(point.position) * 0.3f * point.scale;
        
        // 绘制关键点
        if (point.isCurvePoint)
        {
            Handles.SphereHandleCap(0, point.position, Quaternion.identity, handleSize, EventType.Repaint);
        }
        else
        {
            Handles.CubeHandleCap(0, point.position, Quaternion.identity, handleSize * 0.8f, EventType.Repaint);
        }
        
        // 位置手柄
        if (currentEditTarget == EditTarget.Position && isSelected)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(point.position, point.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                newPos = ConstrainToPlane(newPos);
                point.position = newPos;
                keyPoints[index] = point;
                RecalculateTangents();
            }
        }
        
        // 旋转手柄（仅自定义模式）
        if (currentEditTarget == EditTarget.Rotation && isSelected && !point.followTangent)
        {
            EditorGUI.BeginChangeCheck();
            Quaternion newRot = Handles.RotationHandle(point.rotation, point.position);
            if (EditorGUI.EndChangeCheck())
            {
                point.rotation = newRot;
                keyPoints[index] = point;
            }
        }
        
        // 显示切线
        if (showTangents)
        {
            Handles.color = tangentColor;
            float tanLength = HandleUtility.GetHandleSize(point.position) * tangentLength;
            Handles.ArrowHandleCap(0, point.position, 
                Quaternion.LookRotation(point.tangent), 
                tanLength, EventType.Repaint);
            
            // 绘制切线线
            Handles.DrawDottedLine(point.position, point.position + point.tangent * tanLength, 2f);
        }
        
        // 显示坐标轴
        if (isSelected)
        {
            float axisSize = HandleUtility.GetHandleSize(point.position) * 0.5f;
            Handles.color = Color.red;
            Handles.DrawLine(point.position, point.position + point.rotation * Vector3.right * axisSize);
            Handles.color = Color.green;
            Handles.DrawLine(point.position, point.position + point.rotation * Vector3.up * axisSize);
            Handles.color = Color.blue;
            Handles.DrawLine(point.position, point.position + point.rotation * Vector3.forward * axisSize);
        }
        
        // 显示信息标签
        string info = $"P{index}";
        info += $"\n{(point.followTangent ? "Follow" : "Custom")}";
        info += $"\n{(point.isCurvePoint ? "Curve" : "Line")}";
        Vector3 euler = point.rotation.eulerAngles;
        info += $"\n({euler.x:F0}, {euler.y:F0}, {euler.z:F0})";
        
        Handles.Label(point.position + Vector3.up * 0.5f, info, EditorStyles.whiteLabel);
        
        // 显示切线方向文本
        if (showTangents)
        {
            Handles.Label(point.position - Vector3.up * 0.3f, 
                $"Tan: {point.tangent.ToString("F2")}", 
                EditorStyles.whiteMiniLabel);
        }
    }
    
    private void DrawHandles(KeyPoint point, bool isSelected, int index)
    {
        Handles.color = handleColor;
        
        // 绘制前向手柄
        EditorGUI.BeginChangeCheck();
        Vector3 newForward = Handles.PositionHandle(point.handleForward, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            point.handleForward = newForward;
            keyPoints[index] = point;
        }
        
        Handles.DrawLine(point.position, point.handleForward);
        Handles.SphereHandleCap(0, point.handleForward, Quaternion.identity, 
            HandleUtility.GetHandleSize(point.handleForward) * 0.15f, EventType.Repaint);
        
        // 绘制后向手柄
        EditorGUI.BeginChangeCheck();
        Vector3 newBack = Handles.PositionHandle(point.handleBack, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            point.handleBack = newBack;
            keyPoints[index] = point;
        }
        
        Handles.DrawLine(point.position, point.handleBack);
        Handles.SphereHandleCap(0, point.handleBack, Quaternion.identity, 
            HandleUtility.GetHandleSize(point.handleBack) * 0.15f, EventType.Repaint);
    }
    
    #region 辅助方法
    private Vector3 ConstrainToPlane(Vector3 position)
    {
        if (currentMode == PathMode.Mode2D)
        {
            switch (currentPlane)
            {
                case Plane2D.XY: return new Vector3(position.x, position.y, 0);
                case Plane2D.XZ: return new Vector3(position.x, 0, position.z);
                case Plane2D.YZ: return new Vector3(0, position.y, position.z);
            }
        }
        return position;
    }
    
    private void AddKeyPoint()
    {
        Vector3 newPos = Vector3.zero;
        if (keyPoints.Count > 0)
        {
            KeyPoint last = keyPoints.Last();
            // 根据平面选择添加方向
            if (currentMode == PathMode.Mode2D)
            {
                switch (currentPlane)
                {
                    case Plane2D.XY: newPos = last.position + Vector3.right * 2f; break;
                    case Plane2D.XZ: newPos = last.position + Vector3.right * 2f; break;
                    case Plane2D.YZ: newPos = last.position + Vector3.up * 2f; break;
                }
            }
            else
            {
                newPos = last.position + Vector3.right * 2f;
            }
        }
        newPos = ConstrainToPlane(newPos);
        
        keyPoints.Add(new KeyPoint(newPos));
        selectedPoint = keyPoints.Count - 1;
        RecalculateTangents();
    }
    
    private void RemoveKeyPoint(int index)
    {
        keyPoints.RemoveAt(index);
        selectedPoint = Mathf.Clamp(selectedPoint, 0, keyPoints.Count - 1);
        RecalculateTangents();
    }
    
    private void RecalculateTangents()
    {
        for (int i = 0; i < keyPoints.Count; i++)
        {
            CalculateTangent(i);
        }
    }
    
    private void CalculateTangent(int index)
    {
        if (keyPoints.Count < 2) return;
        
        KeyPoint point = keyPoints[index];
        
        // 计算切线方向（指向下一个点）
        if (index < keyPoints.Count - 1)
        {
            Vector3 toNext = keyPoints[index + 1].position - point.position;
            point.tangent = toNext.normalized;
        }
        else
        {
            // 最后一个点使用前一个方向
            Vector3 fromPrev = point.position - keyPoints[index - 1].position;
            point.tangent = fromPrev.normalized;
        }
        
        // 计算手柄位置（如果是曲线点）
        if (point.isCurvePoint)
        {
            float handleDistance = 1.5f;
            if (index == 0)
            {
                point.handleForward = point.position + point.tangent * handleDistance;
                point.handleBack = point.position - point.tangent * handleDistance;
            }
            else if (index == keyPoints.Count - 1)
            {
                point.handleForward = point.position + point.tangent * handleDistance;
                point.handleBack = point.position - point.tangent * handleDistance;
            }
            else
            {
                Vector3 toPrev = (keyPoints[index - 1].position - point.position).normalized;
                point.handleBack = point.position - toPrev * handleDistance;
                point.handleForward = point.position + point.tangent * handleDistance;
            }
        }
        
        keyPoints[index] = point;
        
        // 如果跟随切线，应用旋转
        if (point.followTangent)
            ApplyTangentToPoint(index);
    }
    
    private void ApplyTangentToPoint(int index)
    {
        KeyPoint point = keyPoints[index];
        
        if (point.tangent.magnitude < 0.001f)
        {
            point.rotation = Quaternion.identity;
        }
        else
        {
            if (currentMode == PathMode.Mode2D)
            {
                // 2D模式：只绕一个轴旋转
                point.rotation = Calculate2DRotation(point.tangent);
            }
            else
            {
                // 3D模式：使用LookRotation
                point.rotation = Quaternion.LookRotation(point.tangent);
            }
        }
        
        keyPoints[index] = point;
    }
    
    private Quaternion Calculate2DRotation(Vector3 tangent)
    {
        switch (currentPlane)
        {
            case Plane2D.XY:
                // XY平面：绕Z轴旋转
                // 物体的前向应该是切线方向
                // 计算从X轴（右）到切线的角度
                float angleXY = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                return Quaternion.Euler(0, 0, angleXY);
                
            case Plane2D.XZ:
                // XZ平面：绕Y轴旋转
                // 计算从Z轴（前）到切线的角度
                float angleXZ = Mathf.Atan2(tangent.x, tangent.z) * Mathf.Rad2Deg;
                return Quaternion.Euler(0, angleXZ, 0);
                
            case Plane2D.YZ:
                // YZ平面：绕X轴旋转
                // 计算从Z轴（前）到切线的角度
                float angleYZ = Mathf.Atan2(tangent.y, tangent.z) * Mathf.Rad2Deg;
                return Quaternion.Euler(-angleYZ, 0, 0);
                
            default:
                return Quaternion.identity;
        }
    }
    
    private void ApplyTangentToFollowingPoints()
    {
        for (int i = 0; i < keyPoints.Count; i++)
        {
            if (keyPoints[i].followTangent)
                ApplyTangentToPoint(i);
        }
    }
    
    private void LoadPathData()
    {
        if (currentPathData == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Path Data asset first!", "OK");
            return;
        }
    
        if (currentPathData.points == null || currentPathData.points.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "Selected Path Data is empty!", "OK");
            return;
        }
    
        // 清空当前的关键点
        keyPoints.Clear();
    
        // 根据路径数据判断模式
        DeterminePathMode(currentPathData);
    
        // 从PathPointData中加载关键点
        foreach (var pointData in currentPathData.points)
        {
            KeyPoint newPoint = new KeyPoint(pointData.position)
            {
                rotation = pointData.rotation,
                tangent = pointData.tangent,
                scale = pointData.scale,
                followTangent = pointData.followTangent
            };
        
            // 设置曲线模式
            newPoint.isCurvePoint = pointData.isCurvePoint;
        
            // 设置手柄位置
            if (pointData.isCurvePoint)
            {
                newPoint.handleForward = pointData.curveHandleForward;
                newPoint.handleBack = pointData.curveHandleBack;
            }
            else
            {
                // 为直线点计算手柄位置
                newPoint.handleForward = newPoint.position + newPoint.tangent;
                newPoint.handleBack = newPoint.position - newPoint.tangent;
            }
        
            keyPoints.Add(newPoint);
        }
    
        // 重新计算切线
        RecalculateTangents();
    
        // 选择第一个点
        selectedPoint = 0;
    
        // 显示成功消息
        EditorUtility.DisplayDialog("Success", $"Loaded {keyPoints.Count} points from {currentPathData.name}", "OK");
    
        // 重绘场景视图
        SceneView.RepaintAll();
    }
    
    private void DeterminePathMode(PathPointData data)
    {
        if (data.points.Count < 2) return;
    
        // 检查所有点的Z坐标是否相同
        float firstZ = data.points[0].position.z;
        bool is2D = data.points.All(p => Mathf.Approximately(p.position.z, firstZ));
    
        if (is2D)
        {
            currentMode = PathMode.Mode2D;
        
            // 检查是哪个平面
            float firstY = data.points[0].position.y;
            float firstX = data.points[0].position.x;
        
            bool isXY = data.points.All(p => Mathf.Approximately(p.position.z, 0));
            bool isXZ = data.points.All(p => Mathf.Approximately(p.position.y, 0));
            bool isYZ = data.points.All(p => Mathf.Approximately(p.position.x, 0));
        
            if (isXY) currentPlane = Plane2D.XY;
            else if (isXZ) currentPlane = Plane2D.XZ;
            else if (isYZ) currentPlane = Plane2D.YZ;
            else
            {
                // 自动选择最接近的平面
                Vector3 avgPos = Vector3.zero;
                foreach (var point in data.points)
                {
                    avgPos += point.position;
                }
                avgPos /= data.points.Count;
            
                if (Mathf.Abs(avgPos.x) < Mathf.Abs(avgPos.y) && Mathf.Abs(avgPos.x) < Mathf.Abs(avgPos.z))
                    currentPlane = Plane2D.YZ;
                else if (Mathf.Abs(avgPos.y) < Mathf.Abs(avgPos.z))
                    currentPlane = Plane2D.XZ;
                else
                    currentPlane = Plane2D.XY;
            }
        }
        else
        {
            currentMode = PathMode.Mode3D;
        }
    }
    
    private void GeneratePathData()
    {
        if (currentPathData == null)
        {
            EditorUtility.DisplayDialog("Error", "Please create or select a Path Data asset first!", "OK");
            return;
        }
        
        if (keyPoints.Count < 2)
        {
            EditorUtility.DisplayDialog("Error", "Need at least 2 key points!", "OK");
            return;
        }
        
        Undo.RecordObject(currentPathData, "Generate Path");
        currentPathData.ClearPoints();
        
        float totalDistance = 0f;
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        
        // 判断是否有曲线点
        bool hasCurvePoint = keyPoints.Exists(p => p.isCurvePoint);
        
        if (hasCurvePoint)
        {
            // 生成贝塞尔曲线
            for (int i = 0; i < keyPoints.Count - 1; i++)
            {
                KeyPoint start = keyPoints[i];
                KeyPoint end = keyPoints[i + 1];
                
                if (start.isCurvePoint && end.isCurvePoint)
                {
                    for (int j = 0; j <= segmentsPerCurve; j++)
                    {
                        float t = (float)j / segmentsPerCurve;
                        Vector3 pos = CalculateBezierPoint(t, start.position, start.handleForward, end.handleBack, end.position);
                        Vector3 tan = CalculateBezierTangent(t, start.position, start.handleForward, end.handleBack, end.position).normalized;
                        
                        positions.Add(pos);
                        tangents.Add(tan);
                        rotations.Add(Quaternion.LookRotation(tan));
                        
                        if (j > 0) 
                        {
                            totalDistance += Vector3.Distance(positions[positions.Count - 2], pos);
                        }
                    }
                }
                else
                {
                    // 直线段
                    positions.Add(start.position);
                    tangents.Add(start.tangent);
                    rotations.Add(start.rotation);
                    
                    if (i > 0)
                    {
                        totalDistance += Vector3.Distance(positions[positions.Count - 2], start.position);
                    }
                }
            }
            
            // 添加最后一个点
            KeyPoint last = keyPoints.Last();
            positions.Add(last.position);
            tangents.Add(last.tangent);
            rotations.Add(last.rotation);
            totalDistance += Vector3.Distance(positions[positions.Count - 2], last.position);
        }
        else
        {
            // 所有都是直线
            for (int i = 0; i < keyPoints.Count; i++)
            {
                KeyPoint point = keyPoints[i];
                positions.Add(point.position);
                tangents.Add(point.tangent);
                rotations.Add(point.rotation);
                
                if (i > 0)
                {
                    totalDistance += Vector3.Distance(positions[i - 1], point.position);
                }
            }
        }
        
        // 添加到路径数据
        for (int i = 0; i < positions.Count; i++)
        {
            float distance = 0f;
            for (int j = 1; j <= i; j++)
            {
                distance += Vector3.Distance(positions[j - 1], positions[j]);
            }
            
            currentPathData.AddPoint(positions[i], rotations[i],tangents[i], distance, hasCurvePoint);
        }
        
        currentPathData.totalLength = totalDistance;
        EditorUtility.SetDirty(currentPathData);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", $"Path generated!\nPoints: {positions.Count}\nLength: {totalDistance:F2}", "OK");
    }
    
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;
        
        return p;
    }
    
    private Vector3 CalculateBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        
        Vector3 tangent = 3 * u * u * (p1 - p0) +
                          6 * u * t * (p2 - p1) +
                          3 * t * t * (p3 - p2);
        
        return tangent;
    }
    #endregion
}
#endif