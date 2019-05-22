using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    class TreeNode
    {
        private TreeNode TopLeft;
        private TreeNode TopRight;
        private TreeNode BottomLeft;
        private TreeNode BottomRight;

        public int Level { get; }
        public int Width => (int)Math.Pow(2, Level + 1);

        private int Leafs;

        public bool IsLeafPopulated(LeafPosition position) => (Leafs & (int)position) > 0;
        public void SetLeafAlive(LeafPosition position) => Leafs |= (int)position;
        public void SetLeafDead(LeafPosition position) => Leafs &= ~(int)position;
        public void ToggleLeaf(LeafPosition position) => Leafs ^= (int)position;

        public enum LeafPosition { TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8 };

        private TreeNode(int level)
        {
            Level = level;
            if (level > 0)
            {
                TopLeft = new TreeNode(Level - 1);
                TopRight = new TreeNode(Level - 1);
                BottomLeft = new TreeNode(Level - 1);
                BottomRight = new TreeNode(Level - 1);
            }
        }

        private TreeNode(int level, TreeNode topLeft, TreeNode topRight, TreeNode bottomLeft, TreeNode bottomRight)
        {
            Level = level;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        private TreeNode GetNextGeneration()
        {
            void CalcForCell(int aliveNeighbourCount, bool cellAlive, LeafPosition position, Action<LeafPosition> setAlive, Action<LeafPosition> setDead)
            {
                if (cellAlive)
                {
                    if (aliveNeighbourCount > 1 && aliveNeighbourCount < 4) // alive cells with 2 or 3 neighbours live
                        setAlive(position);
                    else setDead(position); // alive cells with any other count of neighbours die
                }
                else if (aliveNeighbourCount == 3) // dead cells with 3 neighbours get resurrected
                    setAlive(position);
            }

            if (Level == 2)
            {
                TreeNode result = new TreeNode(0);

                // calc top left result
                {
                    int neighbours = new[] {
                        TopLeft.IsLeafPopulated(LeafPosition.TopLeft),
                        TopLeft.IsLeafPopulated(LeafPosition.TopRight),
                        TopRight.IsLeafPopulated(LeafPosition.TopLeft),
                        TopLeft.IsLeafPopulated(LeafPosition.BottomLeft),
                        TopRight.IsLeafPopulated(LeafPosition.BottomLeft),
                        BottomLeft.IsLeafPopulated(LeafPosition.TopLeft),
                        BottomLeft.IsLeafPopulated(LeafPosition.TopRight),
                        BottomRight.IsLeafPopulated(LeafPosition.TopLeft),
                    }.Count(b => b);
                    CalcForCell(neighbours, TopLeft.IsLeafPopulated(LeafPosition.BottomRight), LeafPosition.TopLeft, result.SetLeafAlive, result.SetLeafDead);
                }
                // calc top right result
                {
                    int neighbours = new[] {
                        TopLeft.IsLeafPopulated(LeafPosition.TopRight),
                        TopRight.IsLeafPopulated(LeafPosition.TopLeft),
                        TopRight.IsLeafPopulated(LeafPosition.TopRight),
                        TopLeft.IsLeafPopulated(LeafPosition.BottomRight),
                        TopRight.IsLeafPopulated(LeafPosition.BottomRight),
                        BottomLeft.IsLeafPopulated(LeafPosition.TopRight),
                        BottomRight.IsLeafPopulated(LeafPosition.TopLeft),
                        BottomRight.IsLeafPopulated(LeafPosition.TopRight),
                    }.Count(b => b);
                    CalcForCell(neighbours, TopRight.IsLeafPopulated(LeafPosition.BottomLeft), LeafPosition.TopRight, result.SetLeafAlive, result.SetLeafDead);
                }
                // calc bottom left result
                {
                    int neighbours = new[] {
                        TopLeft.IsLeafPopulated(LeafPosition.BottomLeft),
                        TopLeft.IsLeafPopulated(LeafPosition.BottomRight),
                        TopRight.IsLeafPopulated(LeafPosition.BottomLeft),
                        BottomLeft.IsLeafPopulated(LeafPosition.TopLeft),
                        BottomRight.IsLeafPopulated(LeafPosition.TopLeft),
                        BottomRight.IsLeafPopulated(LeafPosition.BottomLeft),
                        BottomRight.IsLeafPopulated(LeafPosition.BottomRight),
                        BottomLeft.IsLeafPopulated(LeafPosition.BottomLeft),
                    }.Count(b => b);
                    CalcForCell(neighbours, BottomLeft.IsLeafPopulated(LeafPosition.TopRight), LeafPosition.BottomLeft, result.SetLeafAlive, result.SetLeafDead);
                }
                // calc bottom right result
                {
                    int neighbours = new[] {
                        TopLeft.IsLeafPopulated(LeafPosition.BottomRight),
                        TopRight.IsLeafPopulated(LeafPosition.BottomLeft),
                        TopRight.IsLeafPopulated(LeafPosition.BottomRight),
                        BottomLeft.IsLeafPopulated(LeafPosition.TopRight),
                        BottomRight.IsLeafPopulated(LeafPosition.TopRight),
                        BottomLeft.IsLeafPopulated(LeafPosition.BottomRight),
                        BottomRight.IsLeafPopulated(LeafPosition.BottomLeft),
                        BottomRight.IsLeafPopulated(LeafPosition.BottomRight),
                    }.Count(b => b);
                    CalcForCell(neighbours, BottomRight.IsLeafPopulated(LeafPosition.TopLeft), LeafPosition.BottomRight, result.SetLeafAlive, result.SetLeafDead);
                }
                return result;
            }

            TreeNode n00 = TopLeft.CenteredSubnode();
            TreeNode n02 = TopRight.CenteredSubnode();
            TreeNode n20 = BottomLeft.CenteredSubnode();
            TreeNode n22 = BottomLeft.CenteredSubnode();
            TreeNode n01 = CenteredVertical(TopLeft, TopRight);
            TreeNode n21 = CenteredVertical(BottomLeft, BottomRight);
            TreeNode n10 = CenteredHorizontal(TopLeft, BottomLeft);
            TreeNode n12 = CenteredHorizontal(TopRight, BottomRight);
            TreeNode n11 = CenteredSubSubnode();

            return new TreeNode(
                Level - 1,
                new TreeNode(Level - 2, n00, n01, n10, n11).GetNextGeneration(),
                new TreeNode(Level - 2, n01, n02, n11, n12).GetNextGeneration(),
                new TreeNode(Level - 2, n10, n11, n20, n21).GetNextGeneration(),
                new TreeNode(Level - 2, n11, n12, n21, n22).GetNextGeneration()
            );
        }

        private TreeNode CenteredSubnode() => new TreeNode(Level - 1, TopLeft.BottomRight, TopRight.BottomLeft, BottomLeft.TopRight, BottomRight.TopLeft);
        private TreeNode CenteredHorizontal(TreeNode top, TreeNode bottom) => new TreeNode(Level - 1, top.BottomLeft.BottomRight, top.BottomRight.BottomLeft, bottom.TopLeft.TopRight, bottom.TopRight.TopLeft);
        private TreeNode CenteredVertical(TreeNode left, TreeNode right) => new TreeNode(Level - 1, left.TopRight.BottomRight, right.TopLeft.BottomLeft, left.BottomRight.TopRight, right.BottomLeft.TopLeft);
        private TreeNode CenteredSubSubnode() => new TreeNode(Level - 1, TopLeft.BottomRight.BottomRight, TopRight.BottomLeft.BottomLeft, BottomLeft.TopRight.TopRight, BottomRight.TopLeft.TopLeft);

        private void BirthCellAt(int x, int y)
        {
            if (Level == 0)
            {
                switch (y * 2 + x)
                {
                    case 0: SetLeafAlive(LeafPosition.TopLeft); break;
                    case 1: SetLeafAlive(LeafPosition.TopRight); break;
                    case 2: SetLeafAlive(LeafPosition.BottomLeft); break;
                    case 3: SetLeafAlive(LeafPosition.BottomRight); break;
                    default: throw new ArgumentException();
                }
                return;
            }

            int halfSize = Width / 2;
            int insertX = x < halfSize ? 0 : 1;
            int insertY = y < halfSize ? 0 : 1;
            switch (insertY * 2 + insertX)
            {
                case 0: TopLeft.BirthCellAt(x, y); break;
                case 1: TopRight.BirthCellAt(x - halfSize, y); break;
                case 2: BottomLeft.BirthCellAt(x, y - halfSize); break;
                case 3: BottomRight.BirthCellAt(x - halfSize, y - halfSize); break;
                default: throw new NotImplementedException();
            }
        }

        public static TreeNode GenerateNextGeneration(TreeNode node)
        {
            TreeNode wrapper = new TreeNode(
                node.Level + 2,
                new TreeNode(node.Level + 1),
                new TreeNode(node.Level + 1),
                new TreeNode(node.Level + 1),
                new TreeNode(
                    node.Level + 1,
                    node,
                    new TreeNode(node.Level),
                    new TreeNode(node.Level),
                    new TreeNode(node.Level)
                )
            );

            return wrapper.GetNextGeneration();
        }

        public static TreeNode BuildTree(bool[] array, int width, int height)
        {
            // external width to not always call Math.Pow()
            if (width * height != array.Length)
                throw new ArgumentException("array does not have the stated width and height");

            int largestDimensionSize = Math.Max(width, height);
            int nodeLevelsNeeded = (int)Math.Ceiling(Math.Log(largestDimensionSize, 2));
            TreeNode result = new TreeNode(nodeLevelsNeeded - 1);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (array[y * width + x])
                        result.BirthCellAt(x, y);
            return result;
        }

        private void Render(bool[] array, int x, int y, int arrayWidth)
        {
            if (Level == 0)
            {
                array[y * arrayWidth + x] = IsLeafPopulated(LeafPosition.TopLeft);
                array[y * arrayWidth + x + 1] = IsLeafPopulated(LeafPosition.TopRight);
                array[(y + 1) * arrayWidth + x] = IsLeafPopulated(LeafPosition.BottomLeft);
                array[(y + 1) * arrayWidth + x + 1] = IsLeafPopulated(LeafPosition.BottomRight);
            }
            else
            {
                int halfWidth = Width / 2;
                TopLeft.Render(array, x, y, arrayWidth);
                TopRight.Render(array, x + halfWidth, y, arrayWidth);
                BottomLeft.Render(array, x, y + halfWidth, arrayWidth);
                BottomRight.Render(array, x + halfWidth, y + halfWidth, arrayWidth);
            }
        }

        public bool[] Render()
        {
            int width = Width;
            bool[] result = new bool[width * width];
            Render(result, 0, 0, width);
            return result;
        }
    }
}
