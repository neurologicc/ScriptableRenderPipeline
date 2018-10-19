using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.Experimental.Rendering.ModularSRP;

[CustomPropertyDrawer(typeof(RenderSetup), true)]
public class RenderPassInfoDrawer : PropertyDrawer
{
    private ReorderableList m_ReorderableList;

    private void Init(SerializedProperty property)
    {
        if (m_ReorderableList != null)
            return;

        SerializedProperty array = property.FindPropertyRelative("m_RenderPassList");

        m_ReorderableList = new ReorderableList(property.serializedObject, array);
        m_ReorderableList.drawElementCallback = DrawOptionData;
        m_ReorderableList.drawHeaderCallback = DrawHeader;
        m_ReorderableList.onAddDropdownCallback = DrawDropdown;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Init(property);
        m_ReorderableList.DoList(position);
    }

    private void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Render Pass Setup");
    }

    private void DrawDropdown(Rect buttonRect, ReorderableList list)
    {
        RenderPassInfo[] passClasses = RenderPassReflectionUtilities.QueryRenderPasses();

        var menu = new GenericMenu();
        if (passClasses != null)
        {
            for (int i = 0; i < passClasses.Length; i++)
            {
                string path = "";

                Type classType;
                RenderPassReflectionUtilities.GetTypeFromClassAndAssembly(passClasses[i].className, passClasses[i].assemblyName, out classType);

                RenderPassGroup renderPassGroup = classType.GetCustomAttributes(typeof(RenderPassGroup), true).FirstOrDefault() as RenderPassGroup;
                if (renderPassGroup != null)
                {
                    path = renderPassGroup.Group + "/";
                }
                
                menu.AddItem(new GUIContent(path + TypeToName(passClasses[i].className)), false, DropDownClickHandler, passClasses[i]);
            }
        }

        menu.ShowAsContext();
    }

    private void DropDownClickHandler(object target)
    {
        RenderPassInfo rpInfo = (RenderPassInfo)target;
        m_ReorderableList.serializedProperty.serializedObject.Update();
        int index = m_ReorderableList.serializedProperty.arraySize;
        m_ReorderableList.serializedProperty.arraySize++;
        var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("className").stringValue = rpInfo.className;
        element.FindPropertyRelative("assemblyName").stringValue = rpInfo.assemblyName;
        m_ReorderableList.index = index;
        m_ReorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    private void DrawOptionData(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty itemData = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty itemText = itemData.FindPropertyRelative("className");

        Rect labelRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        EditorGUI.LabelField(labelRect, new GUIContent(TypeToName(itemText.stringValue)));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Init(property);
        return m_ReorderableList.GetHeight();
    }

    static string TypeToName(string typeString)
    {
        if (String.IsNullOrEmpty(typeString))
            return "None";

        int startPos = typeString.Length;
        while (startPos > 0 && typeString[startPos-1] != '.')
            startPos--;

        typeString = typeString.Substring(startPos);

        string retName = "";
        for (int i = 0; i < typeString.Length; i++)
        {
            if (Char.IsUpper(typeString[i]))
                retName = retName + " ";

            retName = retName + typeString[i];
        }

        return retName;
    }
}
