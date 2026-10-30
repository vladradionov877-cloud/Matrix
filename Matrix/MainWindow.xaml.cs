using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Matrix // Простір імен
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Random random = new Random(); // Статичний метод для рандому

        public MainWindow()
        {
            InitializeComponent();
            Resize(GridA, 3, 3, false); // Сітка А
            Resize(GridB, 3, 3, false); // Сітка B
            Resize(GridC, 3, 3, true);  // Сітка C
        }

        /// <summary>
        /// Метод, який малює матрицю будь-якого розміру
        /// </summary>

        // Метод для створення або або зміни розміру матриці
        private static void Resize(UniformGrid grid, int rows, int cols, bool readOnly)
        {
            // Збереження старих комірок
            var old = new Dictionary<(int, int), string>();
            if (grid.Columns > 0 && grid.Children.Count > 0)
            {
                for (int k = 0; k < grid.Children.Count; k++)
                {
                    int r = k / grid.Columns;
                    int c = k % grid.Columns;
                    if (grid.Children[k] is TextBox tb)
                    {
                        old[(r, c)] = tb.Text;
                    }
                }
            }

            // Очищення сітки
            grid.Children.Clear();
            grid.Rows = rows;
            grid.Columns = cols;

            // Генерація нових текстових полів
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var tb = new TextBox
                    {
                        Width = 60,
                        Height = 28,
                        Margin = new Thickness(2),
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        IsReadOnly = readOnly,
                        // Відновлюємо старе значення або 0 по замовчуванню
                        Text = old.TryGetValue((i, j), out var t) ? t : "0"
                    };
                    var normal = readOnly ? Brushes.WhiteSmoke : Brushes.White;
                    tb.Background = normal;
                    // некоректне число — червоний фон
                    tb.TextChanged += (s, e) => tb.Background = TryParse(tb.Text, out _) ? normal : Brushes.MistyRose;
                    grid.Children.Add(tb);
                }
            }
        }

        // Парсинг рядка
        private static bool TryParse(string s, out double v)
            => double.TryParse(s?.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v)
                && !double.IsNaN(v) && !double.IsInfinity(v);

        // Метод, який читає значення матриці із UniformGrid
        private static double[,] Read(UniformGrid grid, string name)
        {
            int rows = grid.Rows; // Рядки
            int cols = grid.Columns; // Колонки
            var m = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    if (index >= grid.Children.Count || grid.Children[index] is not TextBox tb)
                    {
                        throw new InvalidOperationException($"Помилка читання матриці {name}");
                    }

                    // Перевірка, чи є текст числом
                    if (!TryParse(tb.Text, out m[i, j]))
                    {
                        tb.Background = Brushes.MistyRose;
                        throw new InvalidOperationException($"Помилка у матриці {name}: некоректне значення у комірці ({i + 1}, {j + 1})");
                    }
                }
            }
            return m;
        }

        // Форматування числа
        private static string Format(double v) => Math.Round(v, 6).ToString("G7", CultureInfo.InvariantCulture);

        // Виправлено вихід за межі для прямокутних матриць (використовувалось cols замість rows)
        private static void Fill(UniformGrid grid, double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            Resize(grid, rows, cols, true);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    if (index < grid.Children.Count && grid.Children[index] is TextBox tb)
                    {
                        tb.Text = Format(m[i, j]);
                    }
                }
            }
        }
        // Обробка кнопок "Застосувати", "Випадково", "Одинична", "Очистити"
        private void MatrixAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string matrixName = button.Tag?.ToString();
                string action = button.Content?.ToString();

                if (matrixName == "A")
                {
                    HandleMatrixAction(action, RowsA, ColsA, GridA);
                }
                else if (matrixName == "B")
                {
                    HandleMatrixAction(action, RowsB, ColsB, GridB);
                }
            }
        }

        // Логіка виконання дій для кнопок керування
        private void HandleMatrixAction(string action, TextBox rowsBox, TextBox colsBox, UniformGrid grid)
        {
            if (!int.TryParse(rowsBox.Text, out int rows) || rows < 1 || rows > 10) rows = 3;
            if (!int.TryParse(colsBox.Text, out int cols) || cols < 1 || cols > 10) cols = 3;

            rowsBox.Text = rows.ToString();
            colsBox.Text = cols.ToString();

            switch (action)
            {
                case "Застосувати":
                    Resize(grid, rows, cols, false);
                    break;

                case "Випадково":
                    Resize(grid, rows, cols, false);
                    for (int i = 0; i < grid.Children.Count; i++)
                    {
                        if (grid.Children[i] is TextBox tb)
                        {
                            // Випадкові числа від -10 до 10
                            double val = Math.Round(random.NextDouble() * 20 - 10, 2);
                            tb.Text = val.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    break;

                case "Одинична":
                    // Одинична матриця можлива лише для квадратних матриць
                    if (rows != cols)
                    {
                        MessageBox.Show("Одинична матриця може бути тільки квадратною (кількість рядків має дорівнювати кількості стовпців)!",
                            "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    Resize(grid, rows, cols, false);
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            int index = i * cols + j;
                            if (grid.Children[index] is TextBox tb)
                            {
                                tb.Text = (i == j) ? "1" : "0";
                            }
                        }
                    }
                    break;

                case "Очистити":
                    Resize(grid, rows, cols, false);
                    for (int i = 0; i < grid.Children.Count; i++)
                    {
                        if (grid.Children[i] is TextBox tb)
                        {
                            tb.Text = "0";
                        }
                    }
                    break;
            }
        }
        // Операції: додавання, віднімання, множення і ділення
        
        // Додавання
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Читання даних
                var a = Read(GridA, "A");
                var b = Read(GridB, "B");

                // Якщо у матриць не однакові розміри
                if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                {
                    MessageBox.Show("Для додавання матриці повинні мати однакові розміри!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ширина рядків і стовпців
                int rows = a.GetLength(0);
                int cols = a.GetLength(1);
                double[,] result = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[i, j] = a[i, j] + b[i, j];

                // Результат записуємо у матрицю C
                Fill(GridC, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Віднімання
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Читання даних
                var a = Read(GridA, "A");
                var b = Read(GridB, "B");

                // Якщо у матриць не однакові розміри
                if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                {
                    MessageBox.Show("Для віднімання матриці повинні мати однакові розміри!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ширина рядків і стовпців
                int rows = a.GetLength(0);
                int cols = a.GetLength(1);
                double[,] result = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        result[i, j] = a[i, j] - b[i, j];
                    }
                }
                // Результат записуємо у матрицю C
                Fill(GridC, result);
            }
            // Якщо виникне якась інша помилка, не передбачена в коді
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Множення
        private void btnMultiply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Читання даних
                var a = Read(GridA, "A");
                var b = Read(GridB, "B");

                // Ширина рядків і стовпців
                int rowsA = a.GetLength(0);
                int colsA = a.GetLength(1);
                int rowsB = b.GetLength(0);
                int colsB = b.GetLength(1);

                // Якщо у матриць не однакові розміри
                if (colsA != rowsB)
                {
                    MessageBox.Show("Для множення матриці повинні мати однакові розміри!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double[,] result = new double[rowsA, colsB];

                for (int i = 0; i < rowsA; i++)
                {
                    for (int j = 0; j < colsB; j++)
                    {
                        result[i, j] = 0;
                        for (int k = 0; k < colsA; k++)
                        {
                            result[i, j] += a[i, k] * b[k, j];
                        }
                    }
                }

                Fill(GridC, result);
            }
            // Якщо виникне якась інша помилка, не передбачена в коді
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ділення
        private void btnDivide_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Читання даних
                var a = Read(GridA, "A");
                var b = Read(GridB, "B");

                if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                {
                    MessageBox.Show("Для ділення матриці повинні мати однакові розміри!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Ширина рядків і стовпців
                int rows = a.GetLength(0);
                int cols = a.GetLength(1);
                double[,] result = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        // На нуль ділити не можна
                        if (b[i, j] == 0)
                        {
                            MessageBox.Show("Помилка: ділення на нуль неможливе!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        result[i, j] = a[i, j] / b[i, j];
                    }
                }

                Fill(GridC, result);
            }
            // Якщо виникне якась інша помилка, не передбачена в коді
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}