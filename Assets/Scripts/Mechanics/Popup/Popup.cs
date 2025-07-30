using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject[] _uiObjects;

    // Function to get UI object.
    public GameObject GetUI(string objectName)
    {
        for (int i = 0; i < _uiObjects.Length; i++)
        {
            if (_uiObjects[i].name.Equals(objectName))
            {
                return _uiObjects[i];
            }
        }

        return null;
    }
}
