using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PShapes;


public class IntersectionAlgorithms : MonoBehaviour
{
    [SerializeField()]
    public Intersection_Type Interection_type;
    public Transform plane1_transform;
    public Transform plane2_transform;
    public Transform ray_transform;
    public Transform sphere_transform;
    public Transform line_segment_transform;
    public Transform triangle_transform;
    public Transform mesh1_transform;
    public Transform mesh2_transform;
    public GameObject intersection_point_gameobject;


    private GameObject[] intersection_point_gameobjects_pool = new GameObject[256];

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<intersection_point_gameobjects_pool.Length; i++)
        {
            intersection_point_gameobjects_pool[i] = Instantiate(intersection_point_gameobject,  Vector3.zero, Quaternion.identity, transform) as GameObject;
            intersection_point_gameobjects_pool[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // pre-visualization setup
        plane1_transform.gameObject.SetActive(false);
        plane2_transform.gameObject.SetActive(false);
        ray_transform.gameObject.SetActive(false);
        sphere_transform.gameObject.SetActive(false);
        triangle_transform.gameObject.SetActive(false);
        line_segment_transform.gameObject.SetActive(false);
        mesh1_transform.gameObject.SetActive(false);
        mesh2_transform.gameObject.SetActive(false);

        for (int i = 0; i < intersection_point_gameobjects_pool.Length; i++)
        {
            intersection_point_gameobjects_pool[i].SetActive(false);
        }

        // do intersection test
        List<Vector3> intersection_points = new List<Vector3>();
        switch (Interection_type)
        {
            case Intersection_Type.RAY_SPHERE:
                {
                    ray_transform.gameObject.SetActive(true);
                    sphere_transform.gameObject.SetActive(true);

                    PRay ray = new PRay();
                    ray.point = ray_transform.position;
                    ray.direction = ray_transform.up;

                    PSphere sphere = new PSphere();
                    sphere.point = sphere_transform.position;
                    sphere.radius = sphere_transform.localScale.x / 2f ;

                    intersection_points = IntersectionController.intersect(ray, sphere);
                }

                //shape_a = new 
                break;
            case Intersection_Type.RAY_PLANE:
                {
                    plane1_transform.gameObject.SetActive(true);
                    ray_transform.gameObject.SetActive(true);

                    PRay ray = new PRay();
                    ray.point = ray_transform.position;
                    ray.direction = ray_transform.up;

                    PPlane plane = new PPlane();
                    plane.point = plane1_transform.position;
                    plane.normal = -plane1_transform.forward;

                    Vector3[] vertices = plane1_transform.GetComponent<MeshFilter>().mesh.vertices;

                    // swap cause vertices order isn't correct for our algo
                    Vector3 temp = vertices[3];
                    vertices[3] = vertices[2];
                    vertices[2] = temp;

                    intersection_points = IntersectionController.intersect(ray, plane, vertices, plane1_transform);

                }
                break;
            case Intersection_Type.LINE_SEG_PLANE:
                {
                    plane1_transform.gameObject.SetActive(true);
                    line_segment_transform.gameObject.SetActive(true);
                    PPlane plane = new PPlane();
                    plane.point = plane1_transform.position;
                    plane.normal = -plane1_transform.forward;

                    PLine line = new PLine();
                    line.p0 = line_segment_transform.GetChild(0).position;
                    line.p1 = line_segment_transform.GetChild(1).position;

                    Vector3[] vertices = plane1_transform.GetComponent<MeshFilter>().mesh.vertices;
                    // swap cause vertices order isn't correct for our algo
                    Vector3 temp = vertices[3];
                    vertices[3] = vertices[2];
                    vertices[2] = temp;

                    intersection_points = IntersectionController.intersect(line, plane, vertices, plane1_transform);
                }
                break;
            case Intersection_Type.PLANE_PLANE:
                {
                    plane1_transform.gameObject.SetActive(true);
                    plane2_transform.gameObject.SetActive(true);

                    Vector3[] vertices_1 = plane1_transform.GetComponent<MeshFilter>().mesh.vertices;
                    // swap cause vertices order isn't correct for our algo
                    Vector3 temp = vertices_1[3];
                    vertices_1[3] = vertices_1[2];
                    vertices_1[2] = temp;

                    Vector3[] vertices_2 = plane2_transform.GetComponent<MeshFilter>().mesh.vertices;
                    // swap cause vertices order isn't correct for our algo
                    temp = vertices_2[3];
                    vertices_2[3] = vertices_2[2];
                    vertices_2[2] = temp;

                    PPlane p1 = new PPlane();
                    p1.point = plane1_transform.position;
                    p1.normal = -plane1_transform.forward;

                    PPlane p2 = new PPlane();
                    p2.point = plane2_transform.position;
                    p2.normal = -plane2_transform.forward;


                    intersection_points = IntersectionController.intersect(p1, p2, vertices_1, vertices_2, plane1_transform, plane2_transform);

                }
                break;
            case Intersection_Type.TRIANGLE_LINE:
                {
                    triangle_transform.gameObject.SetActive(true);
                    line_segment_transform.gameObject.SetActive(true);

                    PTriangle triangle = new PTriangle();
                    triangle.p0 = new Vector3(-1, 0, -1);
                    triangle.p1 = new Vector3( 1, 0, -1);
                    triangle.p2 = new Vector3( 0, 0, 1);

                    PLine line = new PLine();
                    line.p0 = line_segment_transform.GetChild(0).position;
                    line.p1 = line_segment_transform.GetChild(1).position;

                    intersection_points = IntersectionController.intersect(triangle, line, triangle_transform);
                }
                break;
            case Intersection_Type.MESH_LINE:
                {
                    line_segment_transform.gameObject.SetActive(true);
                    mesh1_transform.gameObject.SetActive(true);

                    PLine line = new PLine();
                    line.p0 = line_segment_transform.GetChild(0).position;
                    line.p1 = line_segment_transform.GetChild(1).position;

                    intersection_points = IntersectionController.intersect(mesh1_transform.GetComponent<MeshFilter>().mesh, line, mesh1_transform);
                }
                break;
            case Intersection_Type.MESH_MESH:
                {
                    mesh1_transform.gameObject.SetActive(true);
                    mesh2_transform.gameObject.SetActive(true);
                    intersection_points = IntersectionController.intersect(mesh1_transform.GetComponent<MeshFilter>().mesh, mesh2_transform.GetComponent<MeshFilter>().mesh,
                                                                            mesh1_transform, mesh2_transform);
                }
                break;
            default:
                break;
        }

        

        // setup contact points
        for(int i=0; i<intersection_points.Count; i++)
        {
            intersection_point_gameobjects_pool[i].SetActive(true);
            intersection_point_gameobjects_pool[i].transform.position = intersection_points[i];
        }

    }
}
