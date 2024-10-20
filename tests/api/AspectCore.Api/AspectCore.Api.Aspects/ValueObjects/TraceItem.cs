﻿using AspectSharp.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspectCore.Api.Trace.ValueObjects
{
    public sealed class TraceItem
    {
        private readonly ConcurrentBag<TraceItem> _childrens = new ConcurrentBag<TraceItem>();
        private readonly AspectContext _context;

        [JsonIgnore]
        public MemberTypes MemberType { get; set; }
        [JsonIgnore]
        public string TargetName { get; set; }
        
        public string Name { get; set; }

        [JsonIgnore]
        public object[] Parameters { get; set; }
        [JsonIgnore]
        public Dictionary<int, string> ParameterNames { get; set; }
        [JsonIgnore]
        public object Return { get; private set; }
        public IEnumerable<TraceItem> Childrens => _childrens;

        public DateTime StartedAt { get; private set; }
        public TimeSpan Duration { get; private set; }

        public TraceItem(AspectContext context)
        {
            _context = context;
            MemberType = _context.MemberType;
            TargetName = _context.TargetType.Name;
            Parameters = _context.Parameters;

            ParameterNames = _context.TargetMethod.GetParameters().Select((pi, idx) => (pi, idx)).ToDictionary(tuple => tuple.idx, tuple => tuple.pi.Name!);

            Name = string.Format("{0} {1}.{2}", MemberType, TargetName, _context.TargetMethod.Name);
        }

        public void AddChildren(TraceItem children)
        {
            if (!ReferenceEquals(this, children))
                _childrens.Add(children ?? throw new ArgumentNullException(nameof(children)));
        }

        public IDisposable Start()
            => new StopwatchDisposable(this);

        public string ToString(int depth)
        {
            var padLeft = new string(' ', depth);
            if (_childrens.Any())
            {
                var parents = string.Join("\r\n", _childrens.Select(item => item.ToString(depth + 4)));
                return string.Format("{0}{1}: {2}\r\n{3}{{\r\n{4}\r\n{5}}}", padLeft, Name, Duration.ToString(), padLeft, parents, padLeft);
            }
            return string.Format("{0}{1}({2}): {3}", padLeft, Name, string.Join(", ", Parameters.Select(
                (x, idx) =>
                {
                    try
                    {
                        return string.Format("{0}: {1}", ParameterNames[idx], JsonSerializer.Serialize(x));
                    }
                    catch
                    {
                        return ParameterNames[idx];
                    }
                })),
                Duration.ToString()
            );
        }

        private sealed class StopwatchDisposable : IDisposable
        {
            private readonly TraceItem _parent;
            private readonly long _startTimestamp;

            public StopwatchDisposable(TraceItem parent)
            {
                _parent = parent;
                _parent.StartedAt = DateTime.UtcNow;
                _startTimestamp = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                _parent.Duration = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - _startTimestamp);
                _parent.Return = _parent._context.ReturnValue;
            }
        }
    }
}
