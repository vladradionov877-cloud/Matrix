namespace Matrix;

public static class MatrixMath
{
    public static double[,] Add(double[,] a, double[,] b)
    {
        int r = a.GetLength(0);
        int c = a.GetLength(1);

        if (r != b.GetLength(0) || c != b.GetLength(1))
            throw new InvalidOperationException("Не співпадають матриці по виміру");
        var res = new double[r, c];
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                res[i, j] = a[i, j] + b[i, j];
        return res;
    }
}
