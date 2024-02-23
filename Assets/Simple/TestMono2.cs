using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMono2 : MonoBehaviour
{
    [CallableFunc]
    private void StringTest(string s)
    {
        Debug.Log("StringTest" + s);
    }

    [CallableFunc]
    private void BoolTest(bool b)
    {
        Debug.Log("BoolTest" + b);
    }
}