using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private volatile bool _cancelRequested = false;

        public void RequestCancel() => _cancelRequested = true;
        public void ClearCancel() => _cancelRequested = false;
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

            for (int i = 0; i < array.Length - 1 && !_cancelRequested; i++)
            {
                for (int j = 0; j < array.Length - 1 - i && !_cancelRequested; j++)
                {
                    if (_cancelRequested)
                    {
                        watch.Stop();
                        BubbleSortCompleted?.Invoke(null, -1, -1); // сигнал отмены
                        return;
                    }
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

            if (_cancelRequested)
            {
                watch.Stop();
                QuickSortCompleted?.Invoke(null, -1, -1);
                return;
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons)
        {
            if (_cancelRequested) return;
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
                if (_cancelRequested) return pivot;
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

            for (int i = 1; i < array.Length && !_cancelRequested; i++)
            {
                if (_cancelRequested)
                {
                    watch.Stop();
                    InsertionSortCompleted?.Invoke(null, -1, -1); // сигнал отмены
                    return;
                }

                int key = array[i];
                int j = i - 1;
                while (j >= 0 && array[j] > key && !_cancelRequested)
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

        // Событие завершения
        public event SortCompletedHandler HeapSortCompleted;

        // Публичный метод запуска
        public void HeapSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Построение кучи
            for (int i = array.Length / 2 - 1; i >= 0 && !_cancelRequested; i--)
            {


                if (_cancelRequested)
                {
                    watch.Stop();
                    HeapSortCompleted?.Invoke(null, -1, -1); // сигнал отмены
                    return;
                }

                Heapify(array, array.Length, i, ref comparisons);
            }


            // Извлечение элементов из кучи
            for (int i = array.Length - 1; i > 0 && !_cancelRequested; i--)
            {

                if (_cancelRequested)
                {
                    watch.Stop();
                    HeapSortCompleted?.Invoke(null, -1, -1); // сигнал отмены
                    return;
                }

                (array[0], array[i]) = (array[i], array[0]); // swap
                Heapify(array, i, 0, ref comparisons);
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            HeapSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        // Вспомогательный метод
        private void Heapify(int[] arr, int n, int i, ref long comparisons)
        {

            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && arr[left] > arr[largest])
            {
                comparisons++;
                largest = left;
            }

            if (right < n && arr[right] > arr[largest])
            {
                comparisons++;
                largest = right;
            }

            if (largest != i)
            {

                (arr[i], arr[largest]) = (arr[largest], arr[i]);
                Heapify(arr, n, largest, ref comparisons);
            }
        }
        public async Task<(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)> BubbleSortAsync(
    int[] originalArray,
    CancellationToken token = default,
    IProgress<double> progress = null)
        {
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int n = array.Length;

                for (int i = 0; i < n - 1; i++)
                {
                    token.ThrowIfCancellationRequested();

                    for (int j = 0; j < n - 1 - i; j++)
                    {
                        token.ThrowIfCancellationRequested();
                        comparisons++;

                        if (array[j] > array[j + 1])
                        {
                            (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        }
                    }

                    // Обновляем прогресс после каждой итерации внешнего цикла
                    double percent = ((double)(i + 1) / (n - 1)) * 100;
                    progress?.Report(percent);
                }

                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                progress?.Report(100);
                return (array, comparisons, watch.Elapsed.TotalMilliseconds);
            });
        }


        public async Task<(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)> QuickSortAsync(
    int[] originalArray,
    CancellationToken token = default,
    IProgress<double> progress = null)
        {
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = System.Diagnostics.Stopwatch.StartNew();

                QuickSortRecursiveWithCancellation(array, 0, array.Length - 1, ref comparisons, token, progress, array.Length);

                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                progress?.Report(100);
                return (array, comparisons, watch.Elapsed.TotalMilliseconds);
            }, token);
        }

        private void QuickSortRecursiveWithCancellation(
    int[] arr, int left, int right, ref long comparisons,
    CancellationToken token, IProgress<double> progress, int totalSize)
        {
            token.ThrowIfCancellationRequested();

            if (left < right)
            {
                int pivotIndex = PartitionWithCancellation(arr, left, right, ref comparisons, token);
                QuickSortRecursiveWithCancellation(arr, left, pivotIndex - 1, ref comparisons, token, progress, totalSize);
                QuickSortRecursiveWithCancellation(arr, pivotIndex + 1, right, ref comparisons, token, progress, totalSize);

                // Приблизительный прогресс
                double percent = ((double)(right - left) / totalSize) * 50;
                progress?.Report(percent);
            }
        }
        private int PartitionWithCancellation(int[] arr, int left, int right, ref long comparisons, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            int pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                token.ThrowIfCancellationRequested();
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

        public async Task<(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)> InsertionSortAsync(
    int[] originalArray,
    CancellationToken token = default,
    IProgress<double> progress = null)
        {
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int n = array.Length;

                for (int i = 1; i < n; i++)
                {
                    token.ThrowIfCancellationRequested();

                    int key = array[i];
                    int j = i - 1;
                    while (j >= 0 && array[j] > key)
                    {
                        token.ThrowIfCancellationRequested();
                        comparisons++;
                        array[j + 1] = array[j];
                        j--;
                    }
                    comparisons++;
                    array[j + 1] = key;

                    double percent = ((double)i / (n - 1)) * 100;
                    progress?.Report(percent);
                }
                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                progress?.Report(100);
                return (array, comparisons, watch.Elapsed.TotalMilliseconds);
            }, token);
        }
       
        
        //Асинхронная пирамидальная сортировка
        public async Task<(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)> HeapSortAsync(
     int[] originalArray,
     CancellationToken token = default,
     IProgress<double> progress = null)
        {
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int n = array.Length;

                for (int i = n / 2 - 1; i >= 0; i--)
                {
                    token.ThrowIfCancellationRequested();
                    HeapifyWithCancellation(array, n, i, ref comparisons, token);
                }

                for (int i = n - 1; i > 0; i--)
                {
                    token.ThrowIfCancellationRequested();
                    (array[0], array[i]) = (array[i], array[0]);
                    HeapifyWithCancellation(array, i, 0, ref comparisons, token);

                    // Обновляем прогресс
                    double percent = ((double)(n - i) / n) * 100;
                    progress?.Report(percent);
                }

                watch.Stop();
                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                progress?.Report(100);
                return (array, comparisons, watch.Elapsed.TotalMilliseconds);
            }, token);
        }
        private void HeapifyWithCancellation(int[] arr, int n, int i, ref long comparisons, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && arr[left] > arr[largest])
            {
                comparisons++;
                largest = left;
            }

            if (right < n && arr[right] > arr[largest])
            {
                comparisons++;
                largest = right;
            }

            if (largest != i)
            {
                token.ThrowIfCancellationRequested();
                (arr[i], arr[largest]) = (arr[largest], arr[i]);
                HeapifyWithCancellation(arr, n, largest, ref comparisons, token);
            }
        }

    }
}