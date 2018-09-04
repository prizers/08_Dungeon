using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon
{
    public class BfsTask
    {
        private static IEnumerable<Point> AllSteps(Point from)
        {
            yield return new Point(from.X - 1, from.Y);
            yield return new Point(from.X, from.Y - 1);
            yield return new Point(from.X + 1, from.Y);
            yield return new Point(from.X, from.Y + 1);
        }

        private static IEnumerable<Point> ValidSteps(Map map, Point pos) =>
            AllSteps(pos).Where(pt => map.InBounds(pt) &&
                                      map.Dungeon[pt.X, pt.Y] == MapCell.Empty);

        public static IEnumerable<SinglyLinkedList<Point>> FindPaths(Map map, Point start, Point[] chests)
        {
            var chestsSet = new HashSet<Point>();
            var marks = new HashSet<Point>();
            var wave = new Queue<SinglyLinkedList<Point>>();
            foreach (var chest in chests)
                chestsSet.Add(chest);
            marks.Add(start);
            wave.Enqueue(new SinglyLinkedList<Point>(start));
            while (0 < wave.Count)
            {
                var from = wave.Dequeue();
                foreach (var step in ValidSteps(map, from.Value))
                {
                    if (marks.Contains(step)) continue;
                    var tail = new SinglyLinkedList<Point>(step, from);
                    marks.Add(step);
                    wave.Enqueue(tail);
                    if (chestsSet.Contains(step))
                        yield return tail;
                }
            }
            yield break;
        }
    }
}
