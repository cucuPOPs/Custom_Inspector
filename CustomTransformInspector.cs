using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
    
[CustomEditor(typeof(Transform))]
public class CustomTransformInspector : Editor
{
    private VisualElement root;
    private Transform m_transform;

    private void OnEnable()
    {
        m_transform = (Transform) target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/CustomTransformInspector.uxml");
        root = visualTree.CloneTree();

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/CustomTransformInspector.uss");
        root.styleSheets.Add(styleSheet);

        Vector3Field localPosField = root.Q<Vector3Field>("Vector3Position");
        Vector3Field localRotField = root.Q<Vector3Field>("Vector3Rotation");
        Vector3Field localScaleField = root.Q<Vector3Field>("Vector3Scale");
        
        Vector3Field worldPosField =root.Q<Vector3Field>("Vector3WorldPosition");
        Vector3Field worldRotField =root.Q<Vector3Field>("Vector3WorldRotation");
        Vector3Field worldScaleField=root.Q<Vector3Field>("Vector3WorldScale");
        
        localPosField.value = m_transform.localPosition;
        localRotField.value = TransformUtils.GetInspectorRotation(m_transform);
        localScaleField.value = m_transform.localScale;
        
        worldPosField.value = m_transform.position;
        worldScaleField.value = m_transform.lossyScale;
        worldRotField.value = GetWorldRotation();
        
        worldPosField.SetEnabled(false);
        worldRotField.SetEnabled(false);
        worldScaleField.SetEnabled(false);
        

        localPosField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            m_transform.localPosition = localPosField.value;
        });
        localRotField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            TransformUtils.SetInspectorRotation(m_transform, localRotField.value);
        });
        localScaleField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            m_transform.localScale = localScaleField.value;
        });

        //전체 리셋
        root.Q<Button>("LocalReset").RegisterCallback<MouseUpEvent>(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            ResetPositionField(localPosField);
            ResetRotationField(localRotField);
            ResetScaleField(localScaleField);
        });

        //개별 리셋
        root.Q<Button>("RB").RegisterCallback<MouseUpEvent>(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            ResetPositionField(localPosField);
        });
        root.Q<Button>("GB").RegisterCallback<MouseUpEvent>(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            ResetRotationField(localRotField);
        });
        root.Q<Button>("BB").RegisterCallback<MouseUpEvent>(evt =>
        {
            Undo.RecordObject(m_transform, m_transform.name);
            ResetScaleField(localScaleField);
        });
        
        // 스케쥴러
        var scheduledAction = root.schedule.Execute(() =>
        {
            localPosField.value = m_transform.localPosition;
            localRotField.value = TransformUtils.GetInspectorRotation(m_transform);
            localScaleField.value = m_transform.localScale;
            
            worldPosField.value = m_transform.position;
            worldScaleField.value = m_transform.lossyScale;
            worldRotField.value = GetWorldRotation();
        });
        scheduledAction.Every(100);
        return root;
    }

    private void ResetPositionField(Vector3Field pos)
    {
        m_transform.localPosition = Vector3.zero;
        pos.value = Vector3.zero;
    }
    private void ResetRotationField(Vector3Field rot)
    {
        m_transform.localRotation = Quaternion.identity;
        rot.value = Vector3.zero;
    }
    private void ResetScaleField(Vector3Field scale)
    {
        m_transform.localScale = Vector3.one;
        scale.value = Vector3.one;
    }
    private Vector3 GetWorldRotation()
    {
        Transform temp = m_transform;
        Vector3 worldRotation = TransformUtils.GetInspectorRotation(temp);
        while (temp.parent != null)
        {
            var parent = temp.parent;
            worldRotation +=TransformUtils.GetInspectorRotation(parent);
            temp = parent;
        }

        return worldRotation;
    }
}


