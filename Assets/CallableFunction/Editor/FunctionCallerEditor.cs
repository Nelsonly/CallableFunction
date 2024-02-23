using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class FunctionCallerEditor : Editor
{
    private Dictionary<string, object> parameterValues = new Dictionary<string, object>();
    private Dictionary<string, bool> methodFoldouts = new Dictionary<string, bool>();
    private GUIStyle methodNameStyle;

    private void Awake()
    {
        methodNameStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                                           BindingFlags.NonPublic | BindingFlags.Static);
        foreach (var method in methods)
        {
            var callableAttribute =
                Attribute.GetCustomAttribute(method, typeof(CallableFuncAttribute)) as CallableFuncAttribute;
            if (callableAttribute != null)
            {
                methodFoldouts.TryAdd(method.Name, true); // 默认为折叠状态

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

                if (methodFoldouts[method.Name])
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        Type parameterType = parameters[i].ParameterType;
                        string parameterName = $"{method.Name}.{parameters[i].Name}";

                        if (!parameterValues.ContainsKey(parameterName))
                        {
                            parameterValues[parameterName] = GetDefaultValueOfType(parameterType);
                        }

                        EditorGUILayout.LabelField($"{parameters[i].Name}", GUILayout.MaxWidth(100));

                        var value = ShowAndGetParameterValueInput(parameterType, parameterValues[parameterName]);

                        parameterValues[parameterName] = value;

                        parameterArray[i] = parameterValues[parameterName];
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }
    }

    public static object ShowAndGetParameterValueInput(Type parameterType, object parameterValue)
    {
        if (parameterType == typeof(int))
        {
            return EditorGUILayout.IntField((int)parameterValue, GUILayout.ExpandWidth(true));
        }
        else if (parameterType == typeof(float))
        {
            return EditorGUILayout.FloatField((float)parameterValue, GUILayout.ExpandWidth(true));
        }
        else if (parameterType == typeof(string))
        {
            return EditorGUILayout.TextField((string)parameterValue, GUILayout.ExpandWidth(true));
        }
        else if (parameterType == typeof(bool))
        {
            return EditorGUILayout.Toggle((bool)parameterValue, GUILayout.ExpandWidth(true));
        }
        else if (parameterType == typeof(Vector2))
        {
            return new Vector2
            {
                x = EditorGUILayout.FloatField(((Vector2)parameterValue).x,
                    GUILayout.ExpandWidth(true)),
                y = EditorGUILayout.FloatField(((Vector2)parameterValue).y,
                    GUILayout.ExpandWidth(true))
            };
        }
        else if (parameterType == typeof(Vector3))
        {
            return new Vector3
            {
                x = EditorGUILayout.FloatField(((Vector3)parameterValue).x,
                    GUILayout.ExpandWidth(true)),
                y = EditorGUILayout.FloatField(((Vector3)parameterValue).y,
                    GUILayout.ExpandWidth(true)),
                z = EditorGUILayout.FloatField(((Vector3)parameterValue).z,
                    GUILayout.ExpandWidth(true))
            };
        }
        else if (parameterType == typeof(Action))
        {
            return 0;
        }
        else
        {
            EditorGUILayout.HelpBox("Unsupported parameter type: " + parameterType.Name, MessageType.Warning);

            return null;
        }
    }

    private static object GetDefaultValueOfType(Type t)
    {
        if (t.IsValueType)
        {
            return Activator.CreateInstance(t);
        }

        return null;
    }
}