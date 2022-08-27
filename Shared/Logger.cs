using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Logger
        {
            public float FontSize = 2;

            readonly int _logHistory;
            private Program _program;
            private List<String> _log = new List<string>();
            private List<IMyTextSurface> _textSurfaces = new List<IMyTextSurface>();

            public Logger(Program program, int log_history = 20)
            {
                _logHistory = log_history;
                _program = program;
            }

            public void LogMessage(String message)
            {
                if (_log.Count > _logHistory)
                    _log.Remove(_log.LastOrDefault());

                _log.Add(message);

                string slog = "";
                _log.ForEach(x => slog += x + "\n");

                _program.Me.GetSurface(0).WriteText(slog);
                foreach (IMyTextSurface surface in _textSurfaces)
                {
                    surface.WriteText(slog);
                }
                _program.Echo(message);
            }

            public void Clear()
            {
                _log.Clear();
            }

            public void AddSurface(IMyTextSurface ts)
            {
                _textSurfaces.Add(ts);
            }
        }
    }
}