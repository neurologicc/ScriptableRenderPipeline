using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Experimental.Rendering.ModularSRP;
using System.Reflection;

[CustomPropertyDrawer(typeof(RenderSetup), true)]
public class RenderPassInfoDrawer : PropertyDrawer
{
    private ReorderableList m_ReorderableList;
    private RenderPipelineAsset m_RenderPipelineAsset;
    private Texture m_ErrorIcon;
    private Dictionary<string, bool> FoldoutStates;

    private void Init(SerializedProperty property)
    {
        if (m_ReorderableList != null)
            return;

        SerializedProperty array = property.FindPropertyRelative("m_RenderPassList");
        m_RenderPipelineAsset = (RenderPipelineAsset)property.FindPropertyRelative("m_RenderPipelineAsset").objectReferenceValue;

        m_ReorderableList = new ReorderableList(property.serializedObject, array);
        m_ReorderableList.drawElementCallback = DrawOptionData;
        m_ReorderableList.drawHeaderCallback = DrawHeader;
        m_ReorderableList.onAddDropdownCallback = DrawDropdown;
        m_ReorderableList.onRemoveCallback = RemoveItem;


        if (m_ErrorIcon == null)
            m_ErrorIcon = EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;

        FoldoutStates = new Dictionary<string, bool>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Init(property);
        RenderSetup renderSetup = fieldInfo.GetValue(property.serializedObject.targetObject) as RenderSetup;
        renderSetup.CheckForErrors();
        m_ReorderableList.DoList(position);
        DrawSetupData();
    }


    public void RemoveItem(ReorderableList reorderableList)
    {
        var element = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index);
        ScriptableRenderPass renderPass = (ScriptableRenderPass)element.FindPropertyRelative("passObject").objectReferenceValue;
        if (renderPass != null)
        {
            AssetDatabase.RemoveObjectFromAsset(renderPass);
            AssetDatabase.SaveAssets();
        }

        reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
    }

    private void DrawSetupData()
    {
        if (FoldoutStates == null)
            return;

        Dictionary<string, List<Tuple<string,FieldInfo,ScriptableRenderPass>>> groupData = new Dictionary<string, List<Tuple<string, FieldInfo, ScriptableRenderPass>>>();
 
        // First lets sort all our data into groups. Any item with "" as a group with not be grouped
        int arraySize = m_ReorderableList.serializedProperty.arraySize;
        for (int index = 0; index < arraySize; index++)
        {
            var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            ScriptableRenderPass renderPass = (ScriptableRenderPass)element.FindPropertyRelative("passObject").objectReferenceValue;

            if (renderPass != null)
            {
                FieldInfo[] allFieldInfo = renderPass.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (allFieldInfo.Count() > 0)
                {
                    foreach (FieldInfo info in allFieldInfo)
                    {
                        RenderingDataGroup renderingDataGroup = info.GetCustomAttribute<RenderingDataGroup>();
                        string groupName = "";
                        if (renderingDataGroup != null)
                        {
                            groupName = renderingDataGroup.Group;
                            if (groupName == null)
                                groupName = "";
                        }

                        List<Tuple<string, FieldInfo, ScriptableRenderPass>> listToAddTo;
                        if (!groupData.ContainsKey(groupName))
                        {
                            listToAddTo = new List<Tuple<string, FieldInfo, ScriptableRenderPass>>();
                            groupData.Add(groupName, listToAddTo);
                        }
                        else
                            listToAddTo = groupData[groupName];

                        listToAddTo.Add(new Tuple<string, FieldInfo, ScriptableRenderPass>(info.Name, info, renderPass));
                    }
                }
            }
        }

        // Now draw our grouped data
        List<string> keys = groupData.Keys.ToList<string>();
        keys.Sort((str1, str2) => string.Compare(str1, str2, true));  // sort our groups alphabetically first
        foreach (string key in keys)
        {
            bool showContents = true;
            bool useFoldout = key != "";
            if (useFoldout)
            {                 // Handle our foldout
                if (!FoldoutStates.ContainsKey(key))
                    FoldoutStates.Add(key, true);

                showContents = FoldoutStates[key];
                showContents = EditorGUILayout.Foldout(showContents, key);
                FoldoutStates[key] = showContents;
            }

            if (showContents)
            {
                if(useFoldout)
                    EditorGUI.indentLevel++;

                List<Tuple<string, FieldInfo, ScriptableRenderPass>> data = groupData[key];
                for (int i = 0; i < data.Count; i++)
                {
                    string name = data[i].Item1;
                    object obj = ((FieldInfo)data[i].Item2).GetValue(data[i].Item3);
                    FieldInfo fieldInfo = (FieldInfo)data[i].Item2;

                    SerializedObject serializedObject = new UnityEditor.SerializedObject(data[i].Item3);
                    SerializedProperty serializedProperty = serializedObject.FindProperty(fieldInfo.Name);
                    if(serializedProperty != null)
                        EditorGUILayout.PropertyField(serializedProperty);
                    serializedObject.ApplyModifiedProperties();
                }

                if (useFoldout)
                    EditorGUI.indentLevel--;
            }
        }
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

        Type pass;
        RenderPassReflectionUtilities.GetTypeFromClassAndAssembly(rpInfo.className, rpInfo.assemblyName, out pass);
        var passObject = (ScriptableRenderPass)Activator.CreateInstance(pass);
        passObject.name = pass.ToString();

        AssetDatabase.AddObjectToAsset(passObject, m_RenderPipelineAsset);
        AssetDatabase.SaveAssets();
        element.FindPropertyRelative("passObject").objectReferenceValue = passObject;


        m_ReorderableList.index = index;
        m_ReorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
    }


    static GUIContent s_TextImage = new GUIContent();
    static GUIContent TempContent(string text, string tooltip, Texture i)
    {
        s_TextImage.image = i;
        s_TextImage.text = text;
        s_TextImage.tooltip = tooltip;
        return s_TextImage;
    }

    private void DrawOptionData(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty itemData = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty itemText = itemData.FindPropertyRelative("className");
        SerializedProperty errorText = itemData.FindPropertyRelative("errorMessage");


        string message = TypeToName(itemText.stringValue);
        if (String.IsNullOrEmpty(errorText.stringValue))
            EditorGUI.LabelField(rect, message);
        else
        {
            EditorGUI.LabelField(rect, TempContent(message, errorText.stringValue, m_ErrorIcon), EditorStyles.label);
        }

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
