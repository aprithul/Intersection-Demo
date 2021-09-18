using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMaker : MonoBehaviour
{
    public Transform p0;
    public Transform p1;
    private LineRenderer line_renderer;

    // Start is called before the first frame update
    void Awake()
    {
        line_renderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        line_renderer.SetPosition(0, p0.position);
        line_renderer.SetPosition(1, p1.position);
    }
}
