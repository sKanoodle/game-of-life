using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class TreeNode
    {
        private TreeNode TopLeft;
        private TreeNode TopRight;
        private TreeNode BottomLeft;
        private TreeNode BottomRight;

        /// <summary>
        /// Level 0 is a node without subnodes but containing leaves, every other level has subnodes
        /// </summary>
        public int Level { get; }
        public int Width => (int)Math.Pow(2, Level + 1);

        private int Leafs;

        private bool IsLeafPopulated(LeafPosition position) => (Leafs & (int)position) > 0;
        private void SetLeafAlive(LeafPosition position) => Leafs |= (int)position;
        private void SetLeafDead(LeafPosition position) => Leafs &= ~(int)position;
        private void ToggleLeaf(LeafPosition position) => Leafs ^= (int)position;

        private enum LeafPosition { TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8 };

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

        private TreeNode(TreeNode topLeft, TreeNode topRight, TreeNode bottomLeft, TreeNode bottomRight)
        {
            Level = topLeft.Level + 1;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        private TreeNode(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            Level = 0;
            if (topLeft) SetLeafAlive(LeafPosition.TopLeft);
            if (topRight) SetLeafAlive(LeafPosition.TopRight);
            if (bottomLeft) SetLeafAlive(LeafPosition.BottomLeft);
            if (bottomRight) SetLeafAlive(LeafPosition.BottomRight);
        }

        public TreeNode GetNextGeneration()
        {
            void CalcForCell(IEnumerable<bool> neighbours, bool cellAlive, LeafPosition position, Action<LeafPosition> setAlive, Action<LeafPosition> setDead)
            {
                int aliveNeighbourCount = neighbours.Count(b => b);
                if (cellAlive)
                {
                    if (aliveNeighbourCount > 1 && aliveNeighbourCount < 4) // alive cells with 2 or 3 neighbours live
                        setAlive(position);
                    else setDead(position); // alive cells with any other count of neighbours die
                }
                else if (aliveNeighbourCount == 3) // dead cells with 3 neighbours get resurrected
                    setAlive(position);
            }

            IEnumerable<bool> topLeftNeighbours() => new[] {
                TopLeft.IsLeafPopulated(LeafPosition.TopLeft), TopLeft.IsLeafPopulated(LeafPosition.TopRight), TopRight.IsLeafPopulated(LeafPosition.TopLeft),
                TopLeft.IsLeafPopulated(LeafPosition.BottomLeft), TopRight.IsLeafPopulated(LeafPosition.BottomLeft),
                BottomLeft.IsLeafPopulated(LeafPosition.TopLeft), BottomLeft.IsLeafPopulated(LeafPosition.TopRight), BottomRight.IsLeafPopulated(LeafPosition.TopLeft),
            };
            IEnumerable<bool> topRightNeighbours() => new[] {
                TopLeft.IsLeafPopulated(LeafPosition.TopRight), TopRight.IsLeafPopulated(LeafPosition.TopLeft), TopRight.IsLeafPopulated(LeafPosition.TopRight),
                TopLeft.IsLeafPopulated(LeafPosition.BottomRight), TopRight.IsLeafPopulated(LeafPosition.BottomRight),
                BottomLeft.IsLeafPopulated(LeafPosition.TopRight), BottomRight.IsLeafPopulated(LeafPosition.TopLeft), BottomRight.IsLeafPopulated(LeafPosition.TopRight),
            };
            IEnumerable<bool> bottomLeftNeighbours() => new[] {
                TopLeft.IsLeafPopulated(LeafPosition.BottomLeft), TopLeft.IsLeafPopulated(LeafPosition.BottomRight), TopRight.IsLeafPopulated(LeafPosition.BottomLeft),
                BottomLeft.IsLeafPopulated(LeafPosition.TopLeft), BottomRight.IsLeafPopulated(LeafPosition.TopLeft),
                BottomRight.IsLeafPopulated(LeafPosition.BottomLeft), BottomRight.IsLeafPopulated(LeafPosition.BottomRight), BottomLeft.IsLeafPopulated(LeafPosition.BottomLeft),
            };
            IEnumerable<bool> bottomRightNeighbours() => new[] {
                TopLeft.IsLeafPopulated(LeafPosition.BottomRight), TopRight.IsLeafPopulated(LeafPosition.BottomLeft), TopRight.IsLeafPopulated(LeafPosition.BottomRight),
                BottomLeft.IsLeafPopulated(LeafPosition.TopRight), BottomRight.IsLeafPopulated(LeafPosition.TopRight),
                BottomLeft.IsLeafPopulated(LeafPosition.BottomRight), BottomRight.IsLeafPopulated(LeafPosition.BottomLeft), BottomRight.IsLeafPopulated(LeafPosition.BottomRight),
            };

            if (Level == 1)
            {
                TreeNode result = new TreeNode(0);
                CalcForCell(topLeftNeighbours(), TopLeft.IsLeafPopulated(LeafPosition.BottomRight), LeafPosition.TopLeft, result.SetLeafAlive, result.SetLeafDead);
                CalcForCell(topRightNeighbours(), TopRight.IsLeafPopulated(LeafPosition.BottomLeft), LeafPosition.TopRight, result.SetLeafAlive, result.SetLeafDead);
                CalcForCell(bottomLeftNeighbours(), BottomLeft.IsLeafPopulated(LeafPosition.TopRight), LeafPosition.BottomLeft, result.SetLeafAlive, result.SetLeafDead);
                CalcForCell(bottomRightNeighbours(), BottomRight.IsLeafPopulated(LeafPosition.TopLeft), LeafPosition.BottomRight, result.SetLeafAlive, result.SetLeafDead);
                return result;
            }

            TreeNode topLeft = TopLeft.CenteredSubnode();
            TreeNode top = CenteredHorizontal(TopLeft, TopRight);
            TreeNode topRight = TopRight.CenteredSubnode();
            TreeNode left = CenteredVertical(TopLeft, BottomLeft);
            TreeNode center = CenteredSubSubnode();
            TreeNode right = CenteredVertical(TopRight, BottomRight);
            TreeNode bottomLeft = BottomLeft.CenteredSubnode();
            TreeNode bottom = CenteredHorizontal(BottomLeft, BottomRight);
            TreeNode bottomRight = BottomRight.CenteredSubnode();

            return new TreeNode(
                new TreeNode(topLeft, top, left, center).GetNextGeneration(),
                new TreeNode(top, topRight, center, right).GetNextGeneration(),
                new TreeNode(left, center, bottomLeft, bottom).GetNextGeneration(),
                new TreeNode(center, right, bottom, bottomRight).GetNextGeneration()
            );
        }

        private TreeNode CenteredSubnode()
        {
            if (Level > 1)
                return new TreeNode(TopLeft.BottomRight, TopRight.BottomLeft, BottomLeft.TopRight, BottomRight.TopLeft);
            return new TreeNode(TopLeft.IsLeafPopulated(LeafPosition.BottomRight), TopRight.IsLeafPopulated(LeafPosition.BottomLeft), BottomLeft.IsLeafPopulated(LeafPosition.TopRight), BottomRight.IsLeafPopulated(LeafPosition.TopLeft));
        }
        private TreeNode CenteredVertical(TreeNode top, TreeNode bottom)
        {
            if (Level > 2)
                return new TreeNode(top.BottomLeft.BottomRight, top.BottomRight.BottomLeft, bottom.TopLeft.TopRight, bottom.TopRight.TopLeft);
            return new TreeNode(top.BottomLeft.IsLeafPopulated(LeafPosition.BottomRight), top.BottomRight.IsLeafPopulated(LeafPosition.BottomLeft), bottom.TopLeft.IsLeafPopulated(LeafPosition.TopRight), bottom.TopRight.IsLeafPopulated(LeafPosition.TopLeft));
        }
        private TreeNode CenteredHorizontal(TreeNode left, TreeNode right)
        {
            if (Level > 2)
                return new TreeNode(left.TopRight.BottomRight, right.TopLeft.BottomLeft, left.BottomRight.TopRight, right.BottomLeft.TopLeft);
            return new TreeNode(left.TopRight.IsLeafPopulated(LeafPosition.BottomRight), right.TopLeft.IsLeafPopulated(LeafPosition.BottomLeft), left.BottomRight.IsLeafPopulated(LeafPosition.TopRight), right.BottomLeft.IsLeafPopulated(LeafPosition.TopLeft));
        }
        private TreeNode CenteredSubSubnode()
        {
            if (Level > 2)
                return new TreeNode(TopLeft.BottomRight.BottomRight, TopRight.BottomLeft.BottomLeft, BottomLeft.TopRight.TopRight, BottomRight.TopLeft.TopLeft);
            return new TreeNode(TopLeft.BottomRight.IsLeafPopulated(LeafPosition.BottomRight), TopRight.BottomLeft.IsLeafPopulated(LeafPosition.BottomLeft), BottomLeft.TopRight.IsLeafPopulated(LeafPosition.TopRight), BottomRight.TopLeft.IsLeafPopulated(LeafPosition.TopLeft));
        }

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
            // grows too big too fast
            //TreeNode wrapper = new TreeNode(
            //    new TreeNode(node.Level + 1),
            //    new TreeNode(node.Level + 1),
            //    new TreeNode(node.Level + 1),
            //    new TreeNode(
            //        node,
            //        new TreeNode(node.Level),
            //        new TreeNode(node.Level),
            //        new TreeNode(node.Level)
            //    )
            //);

            int level = node.Level - 1;
            TreeNode wrapper = new TreeNode(
                new TreeNode(new TreeNode(level), new TreeNode(level), new TreeNode(level), node.TopLeft),
                new TreeNode(new TreeNode(level), new TreeNode(level), node.TopRight, new TreeNode(level)),
                new TreeNode(new TreeNode(level), node.BottomLeft, new TreeNode(level), new TreeNode(level)),
                new TreeNode(node.BottomRight, new TreeNode(level), new TreeNode(level), new TreeNode(level))
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
