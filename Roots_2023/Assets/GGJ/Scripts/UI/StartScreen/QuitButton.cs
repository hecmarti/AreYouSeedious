using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private void Start()
    {
        button.onClick.AddListener(() => Exitgame());
    }

    private void Exitgame()
    {
        Application.Quit();
    }
}

