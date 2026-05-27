using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_rab_2._0_KhasanovaNG_BPI_23_01.Model;

namespace Lab_rab_2._0_KhasanovaNG_BPI_23_01.ViewModel
{

    public partial class MainViewModel_Task : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private int[] _originalArray;
        private CancellationTokenSource _cts = new CancellationTokenSource();

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
        private string _heapSortResult;

        [ObservableProperty]
        private string _totalComparisons = "Общее число сравнений: 0";

        [ObservableProperty]
        private double _bubbleProgress;

        [ObservableProperty]
        private double _quickProgress;

        [ObservableProperty]
        private double _insertionProgress;

        [ObservableProperty]
        private double _heapProgress;

        [ObservableProperty]
        private string _comparisonResults;

        

        // Команды 
        public IAsyncRelayCommand GenerateArrayCommand { get; }
        public IAsyncRelayCommand BubbleSortCommand { get; }
        public IAsyncRelayCommand QuickSortCommand { get; }
        public IAsyncRelayCommand InsertionSortCommand { get; }
        public IAsyncRelayCommand HeapSortCommand { get; }
        public IAsyncRelayCommand CancelAllCommand { get; }
        public IAsyncRelayCommand RunComparisonCommand { get; }

        public MainViewModel_Task()
        {
            _sorter = new ArraySorter();

            // Инициализация команд 
            GenerateArrayCommand = new AsyncRelayCommand(GenerateArrayAsync, CanGenerateArray);
            BubbleSortCommand = new AsyncRelayCommand(BubbleSortAsync, CanSortBubble);
            QuickSortCommand = new AsyncRelayCommand(QuickSortAsync, CanSortQuick);
            InsertionSortCommand = new AsyncRelayCommand(InsertionSortAsync, CanSortInsertion);
            HeapSortCommand = new AsyncRelayCommand(HeapSortAsync, CanSortHeap);
            CancelAllCommand = new AsyncRelayCommand(CancelAllAsync);

            _bubbleProgressReporter = new Progress<double>(p => BubbleProgress = p);
            _quickProgressReporter = new Progress<double>(p => QuickProgress = p);
            _insertionProgressReporter = new Progress<double>(p => InsertionProgress = p);
            _heapProgressReporter = new Progress<double>(p => HeapProgress = p);

            RunComparisonCommand = new AsyncRelayCommand(RunComparisonAsync);
            ComparisonResults = "Результаты сравнения будут здесь";
        }

        // Условия выполнения команд 
        private bool CanGenerateArray() => !GenerateArrayCommand.IsRunning;
        private bool CanSortBubble() => _originalArray != null && !BubbleSortCommand.IsRunning;
        private bool CanSortQuick() => _originalArray != null && !QuickSortCommand.IsRunning;
        private bool CanSortInsertion() => _originalArray != null && !InsertionSortCommand.IsRunning;
        private bool CanSortHeap() => _originalArray != null && !HeapSortCommand.IsRunning;

        private Progress<double> _bubbleProgressReporter;
        private Progress<double> _quickProgressReporter;
        private Progress<double> _insertionProgressReporter;
        private Progress<double> _heapProgressReporter;

        // Асинхронные методы команд 
        private async Task GenerateArrayAsync()
        {
            // Имитация небольшой задержки (можно убрать) 
            await Task.Delay(100);
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            OriginalArrayString = "Исходный массив: " +
    string.Join(", ", _originalArray.Take(Math.Min(20, _originalArray.Length))) +
    (_originalArray.Length > 20 ? "..." : "");
            // Сброс результатов 
            BubbleSortResult = QuickSortResult = InsertionSortResult = HeapSortResult = null;
            TotalComparisons = "Общее число сравнений: 0";
            // Обновляем состояние других команд 
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            HeapSortCommand.NotifyCanExecuteChanged();
        }

        private async Task BubbleSortAsync()
        {
            BubbleSortResult = "Сортируется...";
            BubbleProgress = 0;
            try
            {
                var result = await _sorter.BubbleSortAsync(_originalArray, _cts.Token, _bubbleProgressReporter);
                BubbleSortResult = $"Пузырьковая: {FormatArray(result.SortedArray)}, время: {result.ElapsedMilliseconds:F2} мс, сравнений: {result.Comparisons}";
                UpdateTotalComparisons();
            }
            catch (OperationCanceledException)
            {
                BubbleSortResult = "Пузырьковая: отменено";
            }
            finally
            {
                BubbleProgress = 0;
            }
        }

        private async Task QuickSortAsync()
        {
            QuickSortResult = "Сортируется...";
            QuickProgress = 0;
            try
            {
                var result = await _sorter.QuickSortAsync(_originalArray, _cts.Token, _quickProgressReporter);
                QuickSortResult = $"Быстрая: {FormatArray(result.SortedArray)}, время: {result.ElapsedMilliseconds:F2} мс, сравнений: {result.Comparisons}";
                UpdateTotalComparisons();
            }
            catch (OperationCanceledException)
            {
                QuickSortResult = "Быстрая: отменено";
            }
            finally
            {
                QuickProgress = 0;
            }
        }

        private async Task InsertionSortAsync()
        {
            InsertionSortResult = "Сортируется...";
            InsertionProgress = 0;
            try
            {
                var result = await _sorter.InsertionSortAsync(_originalArray, _cts.Token, _insertionProgressReporter);
                InsertionSortResult = $"Вставками: {FormatArray(result.SortedArray)}, время: {result.ElapsedMilliseconds:F2} мс, сравнений: {result.Comparisons}";
                UpdateTotalComparisons();
            }
            catch (OperationCanceledException)
            {
                InsertionSortResult = "Вставками: отменено";
            }
            finally
            {
                InsertionProgress = 0;
            }
        }

        private async Task HeapSortAsync()
        {
            HeapSortResult = "Сортируется...";
            HeapProgress = 0;
            try
            {
                var result = await _sorter.HeapSortAsync(_originalArray, _cts.Token, _heapProgressReporter);
                HeapSortResult = $"Пирамидальная: {FormatArray(result.SortedArray)}, время: {result.ElapsedMilliseconds:F2} мс, сравнений: {result.Comparisons}";
                UpdateTotalComparisons();
            }
            catch (OperationCanceledException)
            {
                HeapSortResult = "Пирамидальная: отменено";
            }
            finally
            {
                HeapProgress = 0;
            }
        }

        private async Task CancelAllAsync()
        {
            // Отменяем все текущие задачи
            _cts.Cancel();

            // Создаём новый токен для будущих запусков
            _cts = new CancellationTokenSource();

            // Сбрасываем результаты
            BubbleSortResult = "Отменено";
            QuickSortResult = "Отменено";
            InsertionSortResult = "Отменено";
            HeapSortResult = "Отменено";

            // Сбрасываем прогресс
            BubbleProgress = 0;
            QuickProgress = 0;
            InsertionProgress = 0;
            HeapProgress = 0;

            // Обновляем состояние команд
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            HeapSortCommand.NotifyCanExecuteChanged();

            await Task.Delay(10); // Небольшая задержка для обновления UI
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
                return string.Join(", ", arr.Take(5)) + "...";
        }

        private async Task RunComparisonAsync()
        {
            ComparisonResults = "Запуск сравнения...\n\n";

            // Проверяем, есть ли массив
            if (_originalArray == null)
            {
                ComparisonResults = "Сначала сгенерируйте массив!";
                return;
            }

            int size = _originalArray.Length;
            ComparisonResults += $"=== Размер массива: {size} ===\n\n";

            // Сбрасываем прогресс
            BubbleProgress = QuickProgress = InsertionProgress = HeapProgress = 0;

            // Запускаем последовательно НА ОДНОМ И ТОМ ЖЕ МАССИВЕ
            ComparisonResults += "Пузырьковая: ";
            await RunSingleSort("Пузырьковая", () =>
                _sorter.BubbleSortAsync(_originalArray, _cts.Token, _bubbleProgressReporter));

            ComparisonResults += "Быстрая: ";
            await RunSingleSort("Быстрая", () =>
                _sorter.QuickSortAsync(_originalArray, _cts.Token, _quickProgressReporter));

            ComparisonResults += "Вставками: ";
            await RunSingleSort("Вставками", () =>
                _sorter.InsertionSortAsync(_originalArray, _cts.Token, _insertionProgressReporter));

            ComparisonResults += "Пирамидальная: ";
            await RunSingleSort("Пирамидальная", () =>
                _sorter.HeapSortAsync(_originalArray, _cts.Token, _heapProgressReporter));

            ComparisonResults += $"\nОбщее число сравнений: {_sorter.TotalComparisons}\n";
        }

        private async Task<double> RunSingleSort(string name, Func<Task<(int[] SortedArray, long Comparisons, double ElapsedMilliseconds)>> sortFunc)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var result = await sortFunc();
                sw.Stop();
                ComparisonResults += $"{name}: {result.ElapsedMilliseconds:F2} мс, сравнений: {result.Comparisons}\n";
                return result.ElapsedMilliseconds;
            }
            catch (OperationCanceledException)
            {
                ComparisonResults += $"{name}: отменено\n";
                return -1;
            }
        }
    } 
}