using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_rab_2._0_KhasanovaNG_BPI_23_01.Model
{
    public class ArraySorter
    {
        // Общий счётчик сравнений (разделяемый ресурс)
        private long _totalComparisons;
        private readonly object _locker = new object();

        // События завершения сортировки
        public delegate void SortCompletedHandler(int[] sortedArray, long comparisons, double elapsedMs);
        public event SortCompletedHandler BubbleSortCompleted;
        public event SortCompletedHandler QuickSortCompleted;
        public event SortCompletedHandler InsertionSortCompleted;

        public long TotalComparisons => _totalComparisons;

        public int[] GenerateRandomArray(int size)
        {
            Random rand = new Random();
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(1000);
            return array;
        }

        private int[] CopyArray(int[] source)
        {
            int[] copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        // Пузырьковая сортировка
        public void BubbleSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - 1 - i; j++)
                {
                    comparisons++;
                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    }
                }
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        // Быстрая сортировка
        public void QuickSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            QuickSortRecursive(array, 0, array.Length - 1, ref comparisons);
            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons)
        {
            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right, ref comparisons);
                QuickSortRecursive(arr, left, pivotIndex - 1, ref comparisons);
                QuickSortRecursive(arr, pivotIndex + 1, right, ref comparisons);
            }
        }

        private int Partition(int[] arr, int left, int right, ref long comparisons)
        {
            int pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                comparisons++;
                if (arr[j] < pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            return i + 1;
        }

        // Сортировка вставками
        public void InsertionSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;
                while (j >= 0 && array[j] > key)
                {
                    comparisons++;
                    array[j + 1] = array[j];
                    j--;
                }
                comparisons++; // последнее сравнение
                array[j + 1] = key;
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            InsertionSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }
    }

}
