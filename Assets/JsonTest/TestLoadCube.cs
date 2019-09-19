using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       GameObject tt= ObjectManager.Instance.InstantiateObject("Assets/JsonTest/CubeTest.prefab");
    }

}
