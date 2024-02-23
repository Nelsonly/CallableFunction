using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMono1 : MonoBehaviour
{
    [CallableFunc]
    private void IntTest(int a)
    {
        Debug.Log("IntTest" + a);
    }

    [CallableFunc]
    private void FloatTest(float a)
    {
        Debug.Log("FloatTest" + a);
    }

    [CallableFunc]
    private void VectorTest(int a, Vector2 b)
    {
        Debug.Log("IntVector2Test" + a + b);
    }
}