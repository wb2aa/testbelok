using Belok.Common.Geometry;
using Belok.Common.PDB;
using Belok.Common.Quaternion;
using Belok.Common.Visualization.BaseGraphics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;

namespace TestBelok
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Fullerene()
        {
            string TestDataPath = "../../../TestData";
            string pdbPath = Path.Combine(TestDataPath, "pdb5hkn.ent");
            string x3dPath = Path.Combine(TestDataPath, "pdb5hkn.html");
            string[] lines = File.ReadAllLines(pdbPath);
            Dictionary<int, Point_3> points = ExtractHetatmPoints(lines);
            Dictionary<int, List<int>> connections = ExtractConnections(lines);
            WriteFrame(x3dPath, points, connections, false);
            Assert.Pass();
        }
        [Test]
        public void DodecahedronX3D()
        {
            string TestDataPath = "../../../TestData";
            string x3dPath = Path.Combine(TestDataPath, "dodecahedron.html");
            Dictionary<int, Point_3> points = Dodecahedron.GetPoints();
            Dictionary<int, List<int>> connections = Dodecahedron.GetLines();
            WriteFrame(x3dPath, points, connections, true);
            Assert.Pass();
        }
        [Test]
        public void Alignment()
        {
            string TestDataPath = "../../../TestData";
            string x3dPath = Path.Combine(TestDataPath, "alignment.html");
            Dictionary<int, Point_3> D1points = Dodecahedron.GetPoints();
            Dictionary<int, List<int>> D1connections = Dodecahedron.GetLines();
            Dictionary<int, Point_3> D2points = Dodecahedron.GetPoints();
            Dictionary<int, List<int>> D2connections = Dodecahedron.GetLines();
            const int size = 3;
            int[] points1 = new int[size] { 1, 9, 10};
            double[,] x1 = PointsToDouble(D1points, points1);
            int[] points2 = new int[size] { 10, 9, 1 };
            double[,] x2 = PointsToDouble(D2points, points2);
            Quaternion quaternion = new Quaternion();
            Transform4x4 transform4x4 = quaternion.Execute_res_list(x1, x2, size);
            Write2Frames(x3dPath, D1points, D1connections, D2points, D2connections, transform4x4);
            Assert.Pass();
        }
        //HETATM  449  C1  60C B 101      -0.317 -11.262  -3.737  1.00 28.18           C  
        private Dictionary<int, Point_3> ExtractHetatmPoints(string[] lines)
        {
            Dictionary<int, Point_3> points = new Dictionary<int, Point_3>();
            foreach (string line in lines)
            {
                if(line.StartsWith("HETATM"))
                {
                    string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (fields[3] != "60C") //ligand name
                        continue;
                    int key = int.Parse(fields[1]);
                    double x1 = double.Parse(fields[6]);
                    double x2 = double.Parse(fields[7]);
                    double x3 = double.Parse(fields[8]);
                    Point_3 point = new Point_3(x1, x2, x3);
                    points.Add(key, point);
                }
            }
            return points;
        }

        //CONECT  449  450  455  495
        private Dictionary<int,List<int>> ExtractConnections(string[] lines)
        {
            Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();
            foreach (string line in lines)
            {
                if (line.StartsWith("CONECT"))
                {
                    string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int key = int.Parse(fields[1]);
                    List<int> connect = new List<int>();
                    for(int i = 2; i < fields.Length; i++)
                    {
                        int pointNumber = int.Parse(fields[i]);
                        connect.Add(pointNumber);
                    }
                    connections.Add(key, connect);
                }
            }
            return connections;
        }
        private void WriteFrame(string path, Dictionary<int, Point_3> points, Dictionary<int, List<int>> connections, bool label)
        {
            double rCylinder = 0.1;
            double viewportZ = 15.0;
            X3D_Html_graphics x3d = new X3D_Html_graphics();
            x3d.open(path);
            x3d.SetColor(Color.Violet);
            Point_3 center = GetCenter(points);
            x3d.SetCenter(center);
            x3d.WriteHeader();
            double[] c = new double[3];
            c[0] = center.x;
            c[1] = center.y;
            c[2] = center.z;
            x3d.Viewpoint(c, viewportZ);
            foreach (KeyValuePair<int, List<int>> connection in connections)
            {
                Point_3 point1 = points[connection.Key];
                foreach(int p2 in connection.Value)
                {
                    Point_3 point2 = points[p2];
                    x3d.VRML_cylinder(rCylinder, point1, point2);
                }
            }
            if(label)
            {
                foreach (KeyValuePair<int, Point_3> kvp in points)
                {
                    double[] x = new double[3];
                    kvp.Value.CopyTo(x);
                    string text = kvp.Key.ToString();
                    x3d.VRML_text(text, x);
                }
            }
            x3d.close();
        }
        public void Write2Frames(string path, Dictionary<int, Point_3> points1, Dictionary<int, List<int>> connections1,
                                               Dictionary<int, Point_3> points2, Dictionary<int, List<int>> connections2,
                                               Transform4x4 transform4x4)
        {
            double rCylinder = 0.1;
            double viewportZ = 15.0;
            X3D_Html_graphics x3d = new X3D_Html_graphics();
            x3d.open(path);
            x3d.SetColor(Color.Violet);
            Point_3 center = GetCenter(points1);
            x3d.SetCenter(center);
            x3d.WriteHeader();
            double[] c = new double[3];
            c[0] = center.x;
            c[1] = center.y;
            c[2] = center.z;
            x3d.Viewpoint(c, viewportZ);
            foreach (KeyValuePair<int, List<int>> connection in connections1)
            {
                Point_3 point1 = points1[connection.Key];
                foreach (int p2 in connection.Value)
                {
                    Point_3 point2 = points1[p2];
                    x3d.VRML_cylinder(rCylinder, point1, point2);
                }
            }
            x3d.SetColor(Color.Yellow);
            Rotate rot = new Rotate();
            foreach (KeyValuePair<int, List<int>> connection in connections2)
            {
                Point_3 point1 = points2[connection.Key];
                Point_3 point1new = rot.TransformPTP(point1, transform4x4);
                foreach (int p2 in connection.Value)
                {
                    Point_3 point2 = points2[p2];
                    Point_3 point2new = rot.TransformPTP(point2, transform4x4);
                    x3d.VRML_cylinder(rCylinder, point1new, point2new);
                }
            }
            x3d.close();
        }

        Point_3 GetCenter(Dictionary<int, Point_3> pointspointspointspoints)
        {
            Point_3 center = new Point_3(0,0,0);
            if (pointspointspointspoints != null && pointspointspointspoints.Count > 0)
            {
                Point_3 p1 = pointspointspointspoints.First().Value;
                double xMin = p1.x;
                double xMax = p1.x;
                double yMin = p1.y;
                double yMax = p1.y;
                double zMin = p1.z;
                double zMax = p1.z;
                foreach(KeyValuePair<int, Point_3> kvp in pointspointspointspoints)
                {
                    Point_3 p = kvp.Value;
                    if (p.x < xMin) xMin = p.x;
                    if (p.x > xMax) xMax = p.x;
                    if (p.y < yMin) yMin = p.y;
                    if (p.y > yMax) yMax = p.y;
                    if (p.z < zMin) zMin = p.z;
                    if (p.z > zMax) zMax = p.z;
                }
                center = new Point_3((xMax + xMin)/2.0, (yMax + yMin) / 2.0, (zMax + zMin) / 2.0); 
            }
            return center;
        }
        private double[,] PointsToDouble(Dictionary<int, Point_3> dpoints, int[] points)
        {
            int size = points.Length;
            double[,] x = new double[3, size];
            for(int i = 0; i < size; i++)
            {
                Point_3 point = dpoints[points[i]];
                x[0,i]= point.x;
                x[1,i]= point.y;
                x[2,i]= point.z;
            }
            return x;
        }
        public static class Dodecahedron
        {
            public static Dictionary<int, Point_3> GetPoints()
            {
                Dictionary<int, Point_3> points = new Dictionary<int, Point_3>();
                double goldenRatio = (1.0 + Math.Sqrt(5.0)) / 2.0;
                double goldenRatio1 = 1.0 / goldenRatio;
                int count = 1;

                points.Add(count, new Point_3( 1.0,  1.0,  1.0)); count++;
                points.Add(count, new Point_3( 1.0,  1.0, -1.0)); count++;
                points.Add(count, new Point_3( 1.0, -1.0,  1.0)); count++;
                points.Add(count, new Point_3( 1.0, -1.0, -1.0)); count++;

                points.Add(count, new Point_3(-1.0,  1.0,  1.0)); count++;
                points.Add(count, new Point_3(-1.0,  1.0, -1.0)); count++;
                points.Add(count, new Point_3(-1.0, -1.0,  1.0)); count++;
                points.Add(count, new Point_3(-1.0, -1.0, -1.0)); count++;

                points.Add(count, new Point_3( 0.0,  goldenRatio,  goldenRatio1)); count++;
                points.Add(count, new Point_3( 0.0,  goldenRatio, -goldenRatio1)); count++;
                points.Add(count, new Point_3( 0.0, -goldenRatio,  goldenRatio1)); count++;
                points.Add(count, new Point_3( 0.0, -goldenRatio, -goldenRatio1)); count++;

                points.Add(count, new Point_3( goldenRatio1, 0.0,  goldenRatio)); count++;
                points.Add(count, new Point_3( goldenRatio1, 0.0, -goldenRatio)); count++;
                points.Add(count, new Point_3(-goldenRatio1, 0.0,  goldenRatio)); count++;
                points.Add(count, new Point_3(-goldenRatio1, 0.0, -goldenRatio)); count++;

                points.Add(count, new Point_3( goldenRatio,  goldenRatio1, 0.0)); count++;
                points.Add(count, new Point_3( goldenRatio, -goldenRatio1, 0.0)); count++;
                points.Add(count, new Point_3(-goldenRatio,  goldenRatio1, 0.0)); count++;
                points.Add(count, new Point_3(-goldenRatio, -goldenRatio1, 0.0)); count++;

                return points;
            }
            public static Dictionary<int, List<int>> GetLines()
            {
                Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();
                connections.Add(1, new List<int>() {  9, 13, 17 });
                connections.Add(2, new List<int>() { 10, 14, 17 });
                connections.Add(3, new List<int>() { 11, 13, 18 });
                connections.Add(4, new List<int>() { 12, 18, 14 });
                connections.Add(5, new List<int>() { 15, 19,  9 });
                connections.Add(6, new List<int>() { 16, 10, 19 });
                connections.Add(7, new List<int>() { 20, 11, 15 });
                connections.Add(8, new List<int>() { 12, 16, 20 });

                connections.Add( 9, new List<int>() { 10 });
                connections.Add(11, new List<int>() { 12 });
                connections.Add(13, new List<int>() { 15 });
                connections.Add(14, new List<int>() { 16 });
                connections.Add(17, new List<int>() { 18 });
                connections.Add(19, new List<int>() { 20 });
                return connections;
            }
        }
    }
}