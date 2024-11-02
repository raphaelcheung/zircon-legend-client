using Client.Scenes.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Client.Models
{
    public class PathFinder
    {
        private Node[,] Grid;
        public MapControl Map;

        public int MaxSize
        {
            get
            {
                return Map.Width * Map.Height;
            }
        }

        public PathFinder(MapControl map)
        {
            Map = map;
            CreateGrid();
        }

        private void CreateGrid()
        {
            Grid = new Node[Map.Width, Map.Height];
            for (int x = 0; x < Map.Width; ++x)
            {
                for (int y = 0; y < Map.Height; ++y)
                    Grid[x, y] = new Node(Map, x, y);
            }
        }

        public List<Node> FindPath(Point start, Point target)
        {
            Node node1 = GetNode(start);
            Node node2 = GetNode(target);
            Heap<Node> heap = new Heap<Node>(MaxSize);
            HashSet<Node> nodeSet = new HashSet<Node>();
            heap.Add(node1);
            while (heap.Count > 0)
            {
                Node node3 = heap.RemoveFirst();
                nodeSet.Add(node3);
                if (node3 == node2)
                    return RetracePath(node1, node2);
                foreach (Node neighbour in GetNeighbours(node3))
                {
                    if (neighbour.Walkable && !nodeSet.Contains(neighbour))
                    {
                        int num = node3.GCost + GetDistance(node3, neighbour);
                        if (num < neighbour.GCost || !heap.Contains(neighbour))
                        {
                            neighbour.GCost = num;
                            neighbour.HCost = GetDistance(neighbour, node2);
                            neighbour.Parent = node3;
                            if (!heap.Contains(neighbour))
                                heap.Add(neighbour);
                            else
                                heap.UpdateItem(neighbour);
                        }
                    }
                }
            }
            return (List<Node>)null;
        }

        public List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> nodeList = new List<Node>();
            for (Node node = endNode; node != startNode; node = node.Parent)
                nodeList.Add(node);
            nodeList.Add(startNode);
            nodeList.Reverse();
            return nodeList;
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int num1 = Math.Abs(nodeA.Location.X - nodeB.Location.X);
            int num2 = Math.Abs(nodeA.Location.Y - nodeB.Location.Y);
            if (num1 > num2)
                return 14 * num2 + 10 * (num1 - num2);
            return 14 * num1 + 10 * (num2 - num1);
        }

        private Node GetNode(Point location)
        {
            var x = location.X >= Map.Width ? Map.Width - 1 : location.X;
            var y = location.Y >= Map.Height ? Map.Height - 1 : location.Y;

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            return Grid[x, y];
        }

        private List<Node> GetNeighbours(Node node)
        {
            List<Node> nodeList = new List<Node>();
            for (int index1 = -1; index1 <= 1; ++index1)
            {
                for (int index2 = -1; index2 <= 1; ++index2)
                {
                    if (index1 != 0 || index2 != 0)
                    {
                        int index3 = node.Location.X + index1;
                        int index4 = node.Location.Y + index2;
                        if (index3 >= 0 && index3 < Grid.GetLength(0) && index4 >= 0 && index4 < Grid.GetLength(1))
                            nodeList.Add(Grid[index3, index4]);
                    }
                }
            }
            return nodeList;
        }
    }
}
