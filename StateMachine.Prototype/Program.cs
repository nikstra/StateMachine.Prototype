using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateMachine.Prototype
{
    class Program
    {
        static void Main(string[] args)
        {
            var state = State.Outside;
            var previousState = state;
            var previousIdentifier = string.Empty;
            var startTime = default(DateTime);

            var overhead = new Dictionary<int, List<InputLine>>();
            var matches = new Dictionary<string, List<InputLine>>();
            var maybeOverhead = new LinkedList<InputLine>();
            var overheadKey = 0;

            var lines = Seed();
            foreach (var line in lines)
            {
                state = line.Inside
                    ? State.Inside
                    : previousState == State.Overhead
                        ? State.Overhead
                        : State.Outside;

                // Transition from Outside back to Inside
                if (state == State.Inside && previousState == State.Outside && line.Identifier == previousIdentifier)
                {
                    if (maybeOverhead.Any())
                    {
                        matches[line.Identifier].AddRange(maybeOverhead);
                        maybeOverhead = new LinkedList<InputLine>();
                    }

                    previousIdentifier = line.Identifier;
                    matches[line.Identifier].Add(line);
                }
                // Transition from any state to Inside
                else if (state == State.Inside && previousState != State.Inside)
                {
                    if (!matches.ContainsKey(line.Identifier))
                    {
                        matches[line.Identifier] = new List<InputLine>();
                    }
                    previousIdentifier = line.Identifier;
                    matches[line.Identifier].Add(line);
                }
                // Inside
                else if (state == State.Inside && previousState == State.Inside)
                {
                    previousIdentifier = line.Identifier;
                    matches[line.Identifier].Add(line);
                }
                // Transition from Inside to Outside
                else if (state == State.Outside && previousState == State.Inside)
                {
                    startTime = line.Timestamp;
                    maybeOverhead.AddLast(line);
                }
                // Outside
                else if (state == State.Outside && previousState == State.Outside)
                {
                    if ((line.Timestamp - startTime).TotalSeconds > 5)
                    {
                        maybeOverhead.AddLast(line);
                        overheadKey++;
                        overhead[overheadKey] = maybeOverhead.ToList();
                        maybeOverhead = new LinkedList<InputLine>();
                        state = State.Overhead;
                    }
                    else
                    {
                        maybeOverhead.AddLast(line);
                    }
                }
                else if (state == State.Overhead && previousState == State.Overhead)
                {
                    overhead[overheadKey].Add(line);
                }
                else
                {
                    Debug.WriteLine("*** State is unhandled: " + state);
                }

                previousState = state;
            }

            if (maybeOverhead.Any())
            {
                Debug.WriteLine("*** Adding remaining overhead.");
                overheadKey++;
                overhead[overheadKey] = maybeOverhead.ToList();
            }

            Console.WriteLine("Matches:");
            foreach (var matchKvp in matches)
            {
                Console.WriteLine($"{matchKvp.Key}");
                foreach (var line in matchKvp.Value)
                {
                    Console.WriteLine($"{line.Identifier} {line.Timestamp}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Overhead:");
            foreach (var overheadKvp in overhead)
            {
                Console.WriteLine($"{overheadKvp.Key}");
                foreach (var line in overheadKvp.Value)
                {
                    Console.WriteLine($"{line.Identifier} {line.Timestamp}");
                }
            }

            Console.WriteLine("Hello World!");
        }
        static IEnumerable<InputLine> Seed()
        {
            const int count = 100;
            var seedList = new List<InputLine>(count);
            var startTime = DateTime.Now;

            for (var i = 0; i < count; i++)
            {
                seedList.Add(new InputLine
                {
                    Identifier = "zone" + i / 10,
                    Inside = i / 10 % 2 == 0,
                    Timestamp = startTime.AddSeconds(i)
                });
            }

            seedList[6].Identifier = "removed";
            seedList[6].Inside = false;
            return seedList;
        }

    }

    [DebuggerDisplay("Timestamp = {Timestamp}, Inside = {Inside}, Identifier = {Identifier}")]
    class InputLine
    {
        public DateTime Timestamp { get; set; }
        public bool Inside { get; set; }
        public string Identifier { get; set; }
    }

    enum State
    {
        Unknown = 0,
        Inside,
        Outside,
        Overhead
    }
}

