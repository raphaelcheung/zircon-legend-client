using Client.Scenes.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Node : IHeapItem<Node>, IComparable<Node>
    {
        public MapControl Map;
        public Point Location;
        public Node Parent;
        public int GCost;
        public int HCost;
        private int _heapIndex;

        public bool Walkable
        {
            get
            {
                return Map.EmptyCell(Location);
            }
        }

        public int FCost
        {
            get
            {
                return GCost + HCost;
            }
        }

        public int HeapIndex
        {
            get
            {
                return _heapIndex;
            }
            set
            {
                _heapIndex = value;
            }
        }

        public int CompareTo(Node nodeToCompare)
        {
            int num = FCost.CompareTo(nodeToCompare.FCost);
            if (num == 0)
                num = HCost.CompareTo(nodeToCompare.HCost);
            return -num;
        }

        public Node(MapControl map, int x, int y)
        {
            Map = map;
            Location = new Point(x, y);
        }
    }
}
