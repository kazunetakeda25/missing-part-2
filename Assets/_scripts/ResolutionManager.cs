using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour {

    private void Start()
    {
        Screen.SetResolution(1024, 768, true);
    }
}
