using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ShowCallableFunclWindow : EditorWindow
{
    [SerializeField] private GameObject selectedGameObject;
    [SerializeField] public string DefaultGOName = "";
    private MonoBehaviour[] monoBehaviours;
    private Dictionary<MonoBehaviour, MethodInfo[]> methodCache = new Dictionary<MonoBehaviour, MethodInfo[]>();
    private Dictionary<string, bool> methodFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, bool> classFoldouts = new Dictionary<string, bool>();
    private GUIStyle methodNameStyle;
    private GUIStyle classNameStyle;

    private void Awake()
    {
        methodNameStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        classNameStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
    }

    [MenuItem("Tools/Callable Function Window")]
    public static void ShowWindow()
    {
        GetWindow<ShowCallableFunclWindow>("Callable Function Window");
    }

    private void OnGUI()
    {
        GameObject newSelectedGameObject =
            EditorGUILayout.ObjectField("父OBJ", selectedGameObject, typeof(GameObject), true) as GameObject;

        if (newSelectedGameObject != selectedGameObject)
        {
            selectedGameObject = newSelectedGameObject;
            if (selectedGameObject != null)
            {
                monoBehaviours = selectedGameObject.GetComponentsInChildren<MonoBehaviour>();
                // 更新方法缓存，仅当用户更换GameObject时才更新
                UpdateMethodCache();
            }
        }

        EditorGUI.BeginChangeCheck();
        var defaultGameObject = EditorGUILayout.TextField("默认OBJ名称", DefaultGOName);
        if (EditorGUI.EndChangeCheck())
        {
            DefaultGOName = defaultGameObject;
        }

        if (GUILayout.Button("刷新"))
        {
            selectedGameObject = GameObject.Find(DefaultGOName);
            if (selectedGameObject != null)
            {
                monoBehaviours = selectedGameObject.GetComponentsInChildren<MonoBehaviour>();
                // 更新方法缓存，仅当用户更换GameObject时才更新
                UpdateMethodCache();
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (selectedGameObject != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            ShowFields();
        }

        EditorGUILayout.EndScrollView();
    }

    private void UpdateMethodCache()
    {
        methodCache.Clear();
        foreach (var monoBehaviour in monoBehaviours)
        {
            Type monoBehaviourType = monoBehaviour.GetType();
            var displayFields = monoBehaviourType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.GetCustomAttribute<CallableFuncAttribute>() != null).ToArray();

            if (displayFields.Any())
            {
                methodCache.Add(monoBehaviour, displayFields);
            }
        }
    }

    private Vector2 scrollPosition;

    private void ShowFields()
    {
        foreach (var monoBehaviour in monoBehaviours)
        {
            classFoldouts.TryAdd(monoBehaviour.name, false);
            classFoldouts[monoBehaviour.name] = EditorGUILayout.Foldout(classFoldouts[monoBehaviour.name],
                monoBehaviour.GetType().Name, true, classNameStyle);
            if (!classFoldouts[monoBehaviour.name])
            {
                continue;
            }

            if (methodCache.TryGetValue(monoBehaviour, out var displayFields))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);

                foreach (var methodInfo in displayFields)
                {
                    DrawMethod(monoBehaviour, methodInfo);
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }
    }


    private Dictionary<string, object> parameterValues = new Dictionary<string, object>();

    public void DrawMethod(MonoBehaviour target, MethodInfo method)
    {
        var callableAttribute =
            Attribute.GetCustomAttribute(method, typeof(CallableFuncAttribute)) as CallableFuncAttribute;

        if (callableAttribute != null)
        {
            methodFoldouts.TryAdd(method.Name, false); // 默认为折叠状态

            EditorGUILayout.BeginHorizontal();
            ParameterInfo[] parameters = method.GetParameters();
            object[] parameterArray = new object[parameters.Length];

            methodFoldouts[method.Name] = EditorGUILayout.Foldout(methodFoldouts[method.Name], $"{method.Name}",
                true, methodNameStyle);
            if (GUILayout.Button("Invoke", GUILayout.ExpandWidth(true)))
            {
                method.Invoke(target, parameterArray);
            }

            EditorGUILayout.EndHorizontal();
            if (!methodFoldouts[method.Name]) return;

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                string parameterName = $"{method.Name}.{parameters[i].Name}";

                if (!parameterValues.ContainsKey(parameterName))
                {
                    parameterValues[parameterName] = GetDefaultValueOfType(parameterType);
                }

                EditorGUILayout.LabelField($"{parameters[i].Name}", GUILayout.MaxWidth(100));
                var value = FunctionCallerEditor.ShowAndGetParameterValueInput(parameterType,
                    parameterValues[parameterName]);
                parameterValues[parameterName] = value;

                parameterArray[i] = parameterValues[parameterName];

                parameterArray[i] = parameterValues[parameterName];
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private object GetDefaultValueOfType(Type t)
    {
        if (t.IsValueType)
        {
            return Activator.CreateInstance(t);
        }

        return null;
    }
}