using UnityEngine;
using System.Collections.Generic;

namespace PShapes
{
    public enum Intersection_Type
    {
        RAY_SPHERE = 0,
        RAY_PLANE = 1,
        LINE_SEG_PLANE = 2,
        PLANE_PLANE = 3,
        TRIANGLE_LINE = 4,
        MESH_LINE = 5,
        MESH_MESH = 6,
        COUNT = 7

    }

    public abstract class PShape
    {
        public Vector3 point;
    }

    public class PPlane : PShape
    {
        public Vector3 normal;
    }

    public class PRay : PShape
    {
        public Vector3 direction;
    }

    public class PLine : PShape
    {
        public Vector3 p0;
        public Vector3 p1;
    }

    public class PSphere : PShape
    {
        public float radius;
    }

    public class PTriangle
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
    }

    public class IntersectionController
    {

        private static void swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static bool is_point_in_shape(Vector3 point, Vector3[] vertices)
        {
            if (vertices.Length < 3)
                return false;

            Vector3 e1 = vertices[1] - vertices[0];
            Vector3 e2 = vertices[2] - vertices[1];
            Vector3 up = Vector3.Cross(e1, e2);

            for (int i=1; i < vertices.Length+1; i++)
            {
                int first = i % vertices.Length;
                int second = (i - 1) % vertices.Length;
                Vector3 v1 = vertices[first] - vertices[second];
                Vector3 v2 = point - vertices[second];
                Vector3 cross_res = Vector3.Cross( v1, v2);
                if (Vector3.Dot(up, cross_res) < 0)
                    return false;
            }

            return true;
        }


        static public List<Vector3> intersect(PLine l, PPlane p, Vector3[] vertices, Transform plane_transform)
        {
            return intersect(p, l, vertices, plane_transform);
        }

        static public List<Vector3> intersect(PPlane p, PLine l, Vector3[] vertices, Transform plane_transform)
        {
            // check if line passes through plane
            float d0 = Vector3.Dot(p.normal, l.p0 - p.point);
            float d1 = Vector3.Dot(p.normal, l.p1 - p.point);

            if (d0 > d1)
                swap<float>(ref d0, ref d1);

            if (d0 <= 0 && d1 >= 0)
            {
                PRay r = new PRay();
                r.point = l.p1;
                r.direction = (l.p0 - l.p1).normalized;
                return intersect(r, p, vertices, plane_transform);
            }   
            else
                return new List<Vector3>();
        }


        // ray - plane
        static public List<Vector3> intersect(PRay r, PPlane p, Vector3[] vertices, Transform plane_transform)
        {
            return intersect(p, r, vertices, plane_transform);
        }
        static public List<Vector3> intersect(PPlane p, PRay r, Vector3[] vertices, Transform plane_transform)
        {
            List<Vector3> intersection_points = new List<Vector3>();
            float r_dir_proj_in_dir_of_normal = -Vector3.Dot(p.normal, r.direction);
            float d = Vector3.Dot(p.normal, p.point);
            float r_pt_proj_in_dir_of_normal = Vector3.Dot(p.normal, r.point);
            float t = (r_pt_proj_in_dir_of_normal - d) / r_dir_proj_in_dir_of_normal;
            Vector3 point_in_plane = r.point + r.direction * t;
            
            intersection_points.Add(point_in_plane);

            Vector3 point_in_mesh_coordinate = (plane_transform.worldToLocalMatrix * (intersection_points[0] - plane_transform.position));

            if (!is_point_in_shape(point_in_mesh_coordinate, vertices))
                intersection_points.Clear();

            return intersection_points;
            //return point_in_plane;
        }

        // ray - circle
        static public List<Vector3> intersect(PRay r, PSphere c)
        {
            return intersect(c, r);
        }
        static public List<Vector3> intersect(PSphere s, PRay r)
        {
            List<Vector3> intersection_points = new List<Vector3>();
            Vector3 to_center = s.point - r.point;
            float proj = Vector3.Dot(to_center, r.direction.normalized);
            if (proj < 0)
                return intersection_points;

            float dist_to_ray_sqrd = to_center.sqrMagnitude - (proj * proj);
            float radius_sqrd = s.radius * s.radius;
            if (dist_to_ray_sqrd < radius_sqrd)
            {
                float half_chord = Mathf.Sqrt(radius_sqrd - dist_to_ray_sqrd);
                Vector3 p0 = r.point + r.direction * (proj - half_chord);
                Vector3 p1 = r.point + r.direction * (proj + half_chord);
                intersection_points.Add(p0);
                intersection_points.Add(p1);
                return intersection_points;

            }
            else
                return intersection_points;

        }


        private static List<PLine> get_lines_in_plane(Vector3[] vertices, Transform plane_transform)
        {
            PLine e1 = new PLine();
            e1.p0 = plane_transform.localToWorldMatrix * vertices[1];
            e1.p0 += plane_transform.position;
            e1.p1 = plane_transform.localToWorldMatrix * vertices[0];
            e1.p1 += plane_transform.position;
            
            PLine e2 = new PLine();
            e2.p0 = plane_transform.localToWorldMatrix * vertices[2];
            e2.p0 += plane_transform.position;
            e2.p1 = plane_transform.localToWorldMatrix * vertices[1];
            e2.p1 += plane_transform.position;

            PLine e3 = new PLine();
            e3.p0 = plane_transform.localToWorldMatrix * vertices[3];
            e3.p0 += plane_transform.position;
            e3.p1 = plane_transform.localToWorldMatrix * vertices[2];
            e3.p1 += plane_transform.position;

            PLine e4 = new PLine();
            e4.p0 = plane_transform.localToWorldMatrix * vertices[0];
            e4.p0 += plane_transform.position;
            e4.p1 = plane_transform.localToWorldMatrix * vertices[3];
            e4.p1 += plane_transform.position;

            Debug.DrawLine(e1.p0, e1.p1, Color.red);
            Debug.DrawLine(e2.p0, e2.p1, Color.red);
            Debug.DrawLine(e3.p0, e3.p1, Color.red);
            Debug.DrawLine(e4.p0, e4.p1, Color.red);


            List<PLine> lines = new List<PLine>();
            lines.Add(e1);
            lines.Add(e2);
            lines.Add(e3);
            lines.Add(e4);
            return lines;
        }

        private static List<PLine> get_lines_in_mesh(Mesh mesh, Transform mesh_transform)
        {
            List<PLine> lines = new List<PLine>();
            for(int i=0; i<mesh.triangles.Length; i+=3)
            {
                for(int j=1; j<4; j++)
                {
                    PLine line = new PLine();
                    line.p0 = mesh_transform.TransformPoint(mesh.vertices[mesh.triangles[i + j % 3] ]);
                    line.p1 = mesh_transform.TransformPoint(mesh.vertices[mesh.triangles[i + (j - 1) % 3]]);
                    lines.Add(line);
                }
            }

            return lines;
        }

        // plane - plane
        static public List<Vector3> intersect(PPlane p1, PPlane p2, Vector3[] vertices_p1, Vector3[] vertices_p2, Transform transform_p1, Transform transform_p2)
        {
            List<Vector3> intersection_points = new List<Vector3>() ;

            // check lines of p1 against plane p2
            List<PLine> lines_in_plane = get_lines_in_plane(vertices_p1, transform_p1);
            foreach(var l in lines_in_plane)
            {
                var points = intersect(l, p2, vertices_p2, transform_p2);
                if(points.Count == 1)
                {
                    intersection_points.Add(points[0]);
                }
            }

            // check lines of p2 against plane p1
            lines_in_plane = get_lines_in_plane(vertices_p2, transform_p2);
            foreach (var l in lines_in_plane)
            {
                var points = intersect(l, p1, vertices_p1, transform_p1);
                if (points.Count == 1)
                {
                    intersection_points.Add(points[0]);
                }
            }


            return intersection_points;
        }


        // Triangle - line
        static public List<Vector3> intersect(PTriangle triangle, PLine line, Transform triangle_transform)
        {
            
            //Debug.DrawLine(triangle_transform.TransformPoint(triangle.p0), triangle_transform.TransformPoint(triangle.p1));
            //Debug.DrawLine(triangle_transform.TransformPoint(triangle.p1), triangle_transform.TransformPoint(triangle.p2));
            //Debug.DrawLine(triangle_transform.TransformPoint(triangle.p2), triangle_transform.TransformPoint(triangle.p0));

            Vector3 normal = Vector3.Cross(triangle.p2 - triangle.p1, triangle.p1 - triangle.p0);
            PPlane p = new PPlane();
            p.normal = triangle_transform.localToWorldMatrix * normal;
            p.point = triangle_transform.TransformPoint(triangle.p0);

            //Debug.DrawRay(p.point, p.normal, Color.blue);

            Vector3[] vertices = { triangle.p0, triangle.p1, triangle.p2};
            return intersect(p, line, vertices, triangle_transform);
        }


        // mesh - line
        static public List<Vector3> intersect(PLine line, Mesh mesh, Transform mesh_transform)
        {
            return intersect(mesh, line, mesh_transform);
        }
        static public List<Vector3> intersect(Mesh mesh, PLine line, Transform mesh_transform)
        {
            PTriangle t = new PTriangle();
            List<Vector3> intersection_points = new List<Vector3>();
            for (int i=0; i<mesh.triangles.Length; i+=3)
            {
                t.p0 = mesh.vertices[mesh.triangles[i]];
                t.p1 = mesh.vertices[mesh.triangles[i+1]];
                t.p2 = mesh.vertices[mesh.triangles[i+2]];
                List<Vector3> triangle_intersections = intersect(t, line, mesh_transform);
                if(triangle_intersections.Count > 0)
                {
                    intersection_points.Add(triangle_intersections[0]);
                }
            }
            
            return intersection_points;
        }


        static public List<Vector3> intersect(Mesh mesh_1, Mesh mesh_2, Transform mesh_1_transform, Transform mesh_2_transform)
        {
            List<Vector3> intersection_points = new List<Vector3>();

            // get lines in mesh_1 and intersect them with mesh_2
            List<PLine> lines_in_mesh = get_lines_in_mesh(mesh_1, mesh_1_transform);
            //foreach(var line in lines_in_mesh)
            //{
            //    Debug.DrawLine(line.p0, line.p1, Color.white);
            //}

            foreach (var line in lines_in_mesh)
            {
                List<Vector3> intersection = intersect(mesh_2, line, mesh_2_transform);
                if (intersection.Count > 0)
                {
                    foreach (var p in intersection)
                        intersection_points.Add(p);
                }
            }

            // get lines in mesh_2 and intersect them with mesh_1
            lines_in_mesh = get_lines_in_mesh(mesh_2, mesh_2_transform);

            //foreach (var line in lines_in_mesh)
            //{
            //    Debug.DrawLine(line.p0, line.p1, Color.white);
            //}

            foreach (var line in lines_in_mesh)
            {
                List<Vector3> intersection = intersect(mesh_1, line, mesh_1_transform);
                if (intersection.Count > 0)
                {
                    foreach (var p in intersection)
                        intersection_points.Add(p);
                }
            }

            return intersection_points;
        }

    }
}