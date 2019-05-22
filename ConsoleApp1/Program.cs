using System;
using GameOfLife;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 8;
            int height = 8;
            bool[] area = new bool[width * height];

            //area[16] = true;
            //area[17] = true;
            //area[18] = true;
            //area[19] = true;
            //area[20] = true;
            //area[21] = true;

            area[3] = true;
            area[11] = true;
            area[19] = true;
            area[27] = true;
            area[35] = true;

            var tree = TreeNode.BuildTree(area, width, height);
            Print(tree);

            while (true)
            {
                Console.ReadKey();
                tree = TreeNode.GenerateNextGeneration(tree);
                Print(tree);
            }
        }

        static void Print(TreeNode tree)
        {
            //Console.Clear();

            int width = tree.Width;
            var result = tree.Render();
            for (int y = 0; y * width < result.Length; y++)
            {
                for (int x = 0; x < width; x++)
                    Console.Write(result[y * width + x] ? "O" : "I");
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
