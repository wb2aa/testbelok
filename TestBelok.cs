using Belok.Common.Geometry;
using Belok.Common.Visualization.BaseGraphics;
using System.Drawing;

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
            WriteFullerene(x3dPath, points, connections);
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
        private void WriteFullerene(string path, Dictionary<int, Point_3> pointspoints, Dictionary<int, List<int>> connections)
        {
            double rCylinder = 0.1;
            double viewportZ = 15.0;
            X3D_Html_graphics x3d = new X3D_Html_graphics();
            x3d.open(path);
            x3d.SetColor(Color.Violet);
            Point_3 center = GetCenter(pointspoints);
            x3d.SetCenter(center);
            x3d.WriteHeader();
            double[] c = new double[3];
            c[0] = center.x;
            c[1] = center.y;
            c[2] = center.z;
            x3d.Viewpoint(c, viewportZ);
            foreach (KeyValuePair<int, List<int>> connection in connections)
            {
                Point_3 point1 = pointspoints[connection.Key];
                foreach(int p2 in connection.Value)
                {
                    Point_3 point2 = pointspoints[p2];
                    x3d.VRML_cylinder(rCylinder, point1, point2);
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
    }
}