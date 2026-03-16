using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace D1
{
    public class Main : MonoBehaviour
    {
        void Start()
        {
            if (!TableDataMgr.Init())
            {
                return;
            }

            var controller = gameObject.AddComponent<DialogueController>().Init();
            if (!controller)
            {
                Debug.LogError("DialogueController init failed.");
                return;
            }

            controller.RenderNode(101);
            DontDestroyOnLoad(gameObject);
        }
        
    }
}
