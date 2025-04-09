using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Levels;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class GridAnalyser
    {
        public Prop[,] Props;

        public int PropsCount
        {
            get
            {
                int propsCount = 0;
                for (int i = 0; i < Props.GetLength(0); i++)
                {
                    for (int j = 0; j < Props.GetLength(1); j++)
                    {
                        if (HasProp(i, j))
                        {
                            propsCount++;
                        }
                    }
                }

                return propsCount;
            }
        }

        public bool NoMatchesLeft
        {
            get
            {
                Dictionary<int, int> matchesCounter = new Dictionary<int, int>();
                for (int i = 0; i < Props.GetLength(0); i++)
                {
                    for (int j = 0; j < Props.GetLength(1); j++)
                    {
                        if (HasProp(i, j))
                        {
                            matchesCounter.TryAdd(Props[i, j].PropId, 0);
                            matchesCounter[Props[i, j].PropId]++;
                        }
                    }
                }

                return !matchesCounter.Any(x => x.Value > 2);
            }
        }

        public void Initialize(Prop[,] props)
        {
            Props = props;
        }

        public Prop ProcessSwipe(Vector2Int position, Vector2Int direction)
        {
            Vector2Int swipePairPos = position + direction;

            Prop swipedProp = Props[position.x, position.y];
            Prop swipedPair = Props[swipePairPos.x, swipePairPos.y];

            Props[position.x, position.y] = swipedPair;
            Props[swipePairPos.x, swipePairPos.y] = swipedProp;

            swipedProp.Position = swipePairPos;
            if (swipedPair != null)
            {
                swipedPair.Position = position;
            }

            return swipedPair;
        }

        private bool HasProp(int i, int j)
        {
            return Props[i, j] != null;
        }

        public List<Prop> AnalyseMerges()
        {
            int rows = Props.GetLength(0);
            int cols = Props.GetLength(1);
            List<Prop> destroyables = new List<Prop>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols - 2; c++)
                {
                    CompareProps(destroyables, new Vector2Int[] {new(r, c), new(r, c + 1), new(r, c + 2), }, rows, cols);
                }
            }

            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows - 2; r++)
                {
                    CompareProps(destroyables, new Vector2Int[] {new(r, c), new(r + 1, c), new(r + 2, c), }, rows, cols);
                }
            }

            destroyables = destroyables.Distinct().ToList();

            for (int i = 0; i < destroyables.Count; i++)
            {
                Props[destroyables[i].Position.x, destroyables[i].Position.y] = null;
            }

            return destroyables;

            
        }

        public List<Prop> UpdatePositions()
        {
            List<Prop> updatedProps = new List<Prop>();
            for (int x = 0; x < Props.GetLength(0); x++)
            {
                int spaceAmount = 0;
                for (int y = 0; y < Props.GetLength(1); y++)
                {
                    if (Props[x, y] == null)
                    {
                        spaceAmount++;
                    }
                    else if(spaceAmount > 0)
                    {
                        Props[x, y - spaceAmount] = Props[x, y];
                        Props[x, y] = null;
                        Props[x, y - spaceAmount].Position = new Vector2Int(x, y - spaceAmount);
                        updatedProps.Add(Props[x, y - spaceAmount]);
                    }
                }
            }

            return updatedProps;
        }

        public bool CanSwipe(Vector2Int swipedPropPosition, Vector2Int direction)
        {
            var targetPos = swipedPropPosition + direction;

            if (targetPos.x < 0 || targetPos.x >= Props.GetLength(0) || targetPos.y < 0 || targetPos.y >= Props.GetLength(1))
            {
                return false;
            }

            return targetPos.y <= swipedPropPosition.y || Props[targetPos.x, targetPos.y] != null;
        }

        private void CompareProps(List<Prop> destroyables, Vector2Int[] positions, int rows, int cols)
        {
            if (Props[positions[0].x, positions[0].y] == null)
            {
                return;
            }

            foreach (var pos in positions)
            {
                if (Props[positions[0].x, positions[0].y]?.PropId != Props[pos.x, pos.y]?.PropId)
                {
                    return;
                }
            }

            foreach (var pos in positions)
            {
                destroyables.Add(Props[pos.x, pos.y]);
                AddNeighbors(pos.x, pos.y, rows, cols, destroyables);
            }
        }
        
        private void AddNeighbors(int x, int y, int rows, int cols, List<Prop> destroyables)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (Math.Abs(dr) + Math.Abs(dc) != 1) continue;

                    int analyzedRow = x + dr;
                    int analyzedCol = y + dc;
                    if (analyzedRow >= 0 && analyzedRow < rows && analyzedCol >= 0 && analyzedCol < cols)
                    {
                        if (Props[analyzedRow, analyzedCol]?.PropId == Props[x, y]?.PropId)
                        {
                            destroyables.Add(Props[analyzedRow, analyzedCol]);
                        }
                    }
                }
            }
        }
    }
}