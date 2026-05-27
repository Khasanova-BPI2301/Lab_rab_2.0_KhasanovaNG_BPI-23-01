using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_rab_2._0_KhasanovaNG_BPI_23_01.Model;

namespace Lab_rab_2._0_KhasanovaNG_BPI_23_01.ViewModel
{
    public partial class MainViewModel_Thread : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private readonly SynchronizationContext _uiContext;
        private int[] _originalArray;

        // Наблюдаемые свойства 
        [ObservableProperty]
        private int _arraySize = 1000;

        [ObservableProperty]
        private string _originalArrayString;

        [ObservableProperty]
        private string _bubbleSortResult;

        [ObservableProperty]
        private string _quickSortResult;

        [ObservableProperty]
        private string _insertionSortResult;

        [ObservableProperty]
        private string _totalComparisons = "Общее число сравнений: 0";

        [ObservableProperty]
        private bool _canGenerate = true;

        [ObservableProperty]
        private string _heapSortResult = "";

        public MainViewModel_Thread()
        {
            _sorter = new ArraySorter();
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
           
            // Подписка на события завершения сортировки 
            _sorter.BubbleSortCompleted += OnBubbleSortCompleted;
            _sorter.QuickSortCompleted += OnQuickSortCompleted;
            _sorter.InsertionSortCompleted += OnInsertionSortCompleted;
            _sorter.HeapSortCompleted += OnHeapSortCompleted;
        }

        // Команда генерации массива 
        [RelayCommand(CanExecute = nameof(CanGenerateArray))]
        private void GenerateArray()
        {
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            // Отображаем первые 20 элементов 
            OriginalArrayString = "Исходный массив: " +
    string.Join(", ", _originalArray.Take(Math.Min(20, _originalArray.Length))) +
    (_originalArray.Length > 20 ? "..." : "");
            // Сбрасываем предыдущие результаты 
            BubbleSortResult = QuickSortResult = InsertionSortResult = HeapSortResult = null;
            TotalComparisons = "Общее число сравнений: 0";
            // Обновляем состояние команд сортировок 
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            HeapSortCommand.NotifyCanExecuteChanged();
            CancelAllCommand.NotifyCanExecuteChanged();
        }

        private bool CanGenerateArray() => CanGenerate;

        // Пузырьковая сортировка 
        private bool CanSortBubble() => _originalArray != null && BubbleSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortBubble))]
        private void BubbleSort()
        {
            BubbleSortResult = "Сортируется...";
            BubbleSortCommand.NotifyCanExecuteChanged();
            Thread thread = new Thread(() => _sorter.BubbleSort(_originalArray));
            thread.Start();
        }

        // Быстрая сортировка 
        private bool CanSortQuick() => _originalArray != null && QuickSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortQuick))]
        private void QuickSort()
        {
            QuickSortResult = "Сортируется...";
            QuickSortCommand.NotifyCanExecuteChanged();
            Thread thread = new Thread(() => _sorter.QuickSort(_originalArray));
            thread.Start();
        }

        // Сортировка вставками 
        private bool CanSortInsertion() => _originalArray != null && InsertionSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortInsertion))]
        private void InsertionSort()
        {
            InsertionSortResult = "Сортируется...";
            InsertionSortCommand.NotifyCanExecuteChanged();
            Thread thread = new Thread(() => _sorter.InsertionSort(_originalArray));
            thread.Start();
        }

        // Обработчики событий (вызываются из фоновых потоков) 
        private void OnBubbleSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                BubbleSortResult = $"Пузырьковая: {string.Join(", ", sortedArray.Take(5))}, время: {elapsedMs:F2} мс, сравнений: { comparisons}"; 
                UpdateTotalComparisons();
                BubbleSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnQuickSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                QuickSortResult = $"Быстрая: {string.Join(", ", sortedArray.Take(5))}, время: {elapsedMs:F2} мс, сравнений: { comparisons}"; 
                UpdateTotalComparisons();
                QuickSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnInsertionSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                InsertionSortResult = $"Вставками: {string.Join(", ", sortedArray.Take(5))}, время: {elapsedMs:F2} мс, сравнений: { comparisons} "; 
                UpdateTotalComparisons();
                InsertionSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void UpdateTotalComparisons()
        {
            TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";
        }

        private string FormatArray(int[] arr)
        {
            if (arr.Length <= 10)
                return string.Join(", ", arr);
            else
                return string.Join(", ", arr, 0, 5) + "...";
        }
        private void OnHeapSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                if (comparisons == -1)
                    HeapSortResult = "Пирамидальная: отменено";
                else
                    HeapSortResult = $"Пирамидальная:  {string.Join(", ", sortedArray.Take(5))}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";

                UpdateTotalComparisons();
                HeapSortCommand.NotifyCanExecuteChanged();
            }, null);
        }
        private bool CanSortHeap() => _originalArray != null && HeapSortResult != "Сортируется...";

        [RelayCommand(CanExecute = nameof(CanSortHeap))]
        private void HeapSort()
        {
            _sorter.ClearCancel();
            HeapSortResult = "Сортируется...";
            HeapSortCommand.NotifyCanExecuteChanged();
            Thread thread = new Thread(() => _sorter.HeapSort(_originalArray));
            thread.IsBackground = true;
            thread.Start();
        }
        [RelayCommand]
        private void CancelAll()
        {
            _sorter.RequestCancel();
            BubbleSortResult = QuickSortResult = InsertionSortResult = HeapSortResult = "Отменено";
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            HeapSortCommand.NotifyCanExecuteChanged();
        }
    }
}
