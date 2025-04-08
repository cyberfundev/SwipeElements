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

                return matchesCounter.Any(x => x.Value > 2);
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

            void AddNeighbors(int x, int y)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (Math.Abs(dr) + Math.Abs(dc) == 1)
                        {
                            int nr = x + dr, nc = y + dc;
                            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                            {
                                if (Props[nr, nc]?.PropId == Props[x, y]?.PropId)
                                {
                                    destroyables.Add(Props[nr, nc]);
                                }
                            }
                        }
                    }
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols - 2; c++)
                {
                    if (Props[r, c]?.PropId == Props[r, c + 1]?.PropId && Props[r, c]?.PropId == Props[r, c + 2]?.PropId)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            destroyables.Add(Props[r, c + i]);
                        }

                        AddNeighbors(r, c);
                        AddNeighbors(r, c + 1);
                        AddNeighbors(r, c + 2);
                    }
                }
            }

            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows - 2; r++)
                {
                    if (Props[r, c] == null)
                    {
                        continue;
                    }
                    
                    if (Props[r, c]?.PropId == Props[r + 1, c]?.PropId && Props[r, c]?.PropId == Props[r + 2, c]?.PropId)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            destroyables.Add(Props[r + i, c]);
                        }

                        AddNeighbors(r, c);
                        AddNeighbors(r + 1, c);
                        AddNeighbors(r + 2, c);
                    }
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

            if (targetPos.x < 0 || targetPos.x >= Props.Length || targetPos.y < 0 || targetPos.y >= Props.Length)
            {
                return false;
            }

            return targetPos.y <= swipedPropPosition.y || Props[targetPos.x, targetPos.y] != null;
        }
    }
}