﻿using Sandbox.Game.EntityComponents;
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
        public class LCDHelper
        {
            Program _program;

            public LCDHelper(Program program)
            {
                _program = program;
            }

            private Dictionary<string, List<IMyTextPanel>> GetLcds(string tag)
            {
                List<IMyTextPanel> lcds = new List<IMyTextPanel>();
                _program.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds);
                lcds = lcds.Where(x => x.CustomName.Contains(tag)).ToList();
                Dictionary<string, List<IMyTextPanel>> dlcds = new Dictionary<string, List<IMyTextPanel>>();
                foreach (IMyTextPanel lcd in lcds)
                {
                    List<IMyTextPanel> l;
                    if (dlcds.ContainsKey(lcd.CustomName))
                        l = dlcds[lcd.CustomName];
                    else
                    {
                        l = new List<IMyTextPanel>();
                        dlcds.Add(lcd.CustomName, l);
                    }
                    l.Add(lcd);
                }

                return dlcds;
            }

            public void WriteToLcds(List<string> fs, string tag, int lines, Color color, float size)
            {
                Dictionary<string, List<IMyTextPanel>> dlcds = GetLcds(tag);
                List<string> names = dlcds.Keys.OrderBy(x => x).ToList();

                int t = 0;
                int c = 0;
                int i = 0;

                while (fs.Count > t)
                {
                    c = lines < fs.Count - t ? lines : (fs.Count - t);
                    List<string> part = fs.GetRange(t, c);
                    if (i >= names.Count)
                        return;
                    foreach (var lcd in dlcds[names[i]])
                    {
                        lcd.FontSize = size;
                        lcd.FontColor = color;
                        lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                        lcd.Font = "Monospace";
                        lcd.WriteText(String.Join("\n", part));
                    }

                    t += c;
                    i++;
                }

                while (i < names.Count)
                {
                    foreach (var lcd in dlcds[names[i]])
                    {
                        lcd.WriteText("");
                    }
                    i++;
                }
            }

        }
    }
}
