using GGJ.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGJ.UI
{
    public class StartButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private GameObject Menuroot;

        private void Start()
        {
            button.onClick.AddListener(() => Startlvl1());
        }

        private void Startlvl1()
        {
            GameManager.Instance.LevelManager.LoadFirstLevel();
            Menuroot.SetActive(false);
        }
    }
}
