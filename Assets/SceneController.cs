using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PShapes;

public class SceneController : MonoBehaviour
{

    Text instruction;
    public IntersectionAlgorithms intersection_algo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int mode = (int)intersection_algo.Interection_type;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            mode = (mode - 1) % (int)Intersection_Type.COUNT;
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            mode = (mode + 1) % (int)Intersection_Type.COUNT;
        }

        intersection_algo.Interection_type = (Intersection_Type)mode;

    }
}
