using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] items;
        private int currentItemCount;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            ++currentItemCount;
        }

        public T RemoveFirst()
        {
            T obj = items[0];
            --currentItemCount;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return obj;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }

        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int index1 = item.HeapIndex * 2 + 1;
                int index2 = item.HeapIndex * 2 + 2;
                if (index1 < currentItemCount)
                {
                    int index3 = index1;
                    if (index2 < currentItemCount && items[index1].CompareTo(items[index2]) < 0)
                        index3 = index2;
                    if (item.CompareTo(items[index3]) < 0)
                        Swap(item, items[index3]);
                    else
                        goto label_6;
                }
                else
                    break;
            }
            return;
        label_6:;
        }

        private void SortUp(T item)
        {
            int index = (item.HeapIndex - 1) / 2;
            while (true)
            {
                T obj = items[index];
                if (item.CompareTo(obj) > 0)
                {
                    Swap(item, obj);
                    index = (item.HeapIndex - 1) / 2;
                }
                else
                    break;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int heapIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = heapIndex;
        }
    }
}
