using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon
{
    using Track = SinglyLinkedList<Point>;
    using Steps = List<Point>;

    public static class Helpers
    {
        public static IEnumerable<MoveDirection> AsPathWalk(this Steps steps) =>
            steps.Zip(steps.Skip(1),
                      (a, b) => Walker.ConvertOffsetToDirection(new Size(b) - new Size(a)));

        public static IEnumerable<MoveDirection> AsPathWalk(this Track track, bool revert)
        {
            var steps = track.ToList();
            if (revert) steps.Reverse();
            return steps.AsPathWalk();
        }
    }

    public class TwoPartedTrack : IComparable<TwoPartedTrack>
    {
        public readonly Track FromStart;
        public readonly Track FromExit;

        public TwoPartedTrack(Track part1, Track part2)
        {
            FromStart = part1;
            FromExit = part2;
        }

        public int Length { get => FromStart.Length + FromExit.Length - 1; }

        public int CompareTo(TwoPartedTrack that) =>
            Length.CompareTo(that.Length);

        public IEnumerable<MoveDirection> AsPathWalk() =>
            FromStart.AsPathWalk(true).Concat(FromExit.AsPathWalk(false));
    }

    public class DungeonTask
    {
        private class Pathfinder
        {
            private Map map;
            private Track trackFromStartToExit;
            private List<Track> tracksFromStartToChests;
            private List<Track> tracksFromExitToChests;
            private List<TwoPartedTrack> completeTracks;
            TwoPartedTrack theShortestCompleteTrack;

            private void PrepareTracksFromStart()
            {
                var targets = map.Chests.Concat(new Point[] { map.Exit }).ToArray();
                var tracesToTargets = BfsTask.FindPaths(map, map.InitialPosition, targets)
                                             .ToList();
                tracksFromStartToChests = tracesToTargets.Where(x =>
                {
                    if (x.Value != map.Exit)
                        return true;
                    trackFromStartToExit = x;
                    return false;
                }).ToList();
            }

            private void PrepareTracksFromExit()
            {
                tracksFromExitToChests = BfsTask.FindPaths(map, map.Exit, map.Chests)
                                                .ToList();
            }

            private void MergeTracks()
            {
                completeTracks = tracksFromStartToChests.Join(tracksFromExitToChests,
                    track1 => track1.Value,
                    track2 => track2.Value,
                    (track1, track2) => new TwoPartedTrack(track1, track2)).ToList();
            }

            private void SearchForShortestCompleteTrack()
            {
                theShortestCompleteTrack = completeTracks.Any() ? completeTracks.Min()
                                                                : null;
            }

            private MoveDirection[] GetPathFromCompleteTrack() =>
                theShortestCompleteTrack.AsPathWalk().ToArray();

            private MoveDirection[] GetFallbackPath() =>
                trackFromStartToExit.AsPathWalk(true).ToArray();

            private MoveDirection[] GetEmptyPath() =>
                new MoveDirection[0];

            public Pathfinder(Map map)
            {
                this.map = map;
            }

            public MoveDirection[] FindPath()
            {
                PrepareTracksFromStart();
                if (null == trackFromStartToExit)
                    return GetEmptyPath();
                PrepareTracksFromExit();
                MergeTracks();
                SearchForShortestCompleteTrack();
                if (null != theShortestCompleteTrack)
                    return GetPathFromCompleteTrack();
                return GetFallbackPath();
            }
        }

        public static MoveDirection[] FindShortestPath(Map map) =>
            new Pathfinder(map).FindPath();
    }
}
