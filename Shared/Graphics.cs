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
        const string FILL = "■";
        const string BLANK = "‐";

        public class Graphics
        {
            public string GetProgressBar(float perc, string title = null, bool showPerc = false)
            {
                string res = "";
                for (float i = 0.1f; i <= 1; i+= 0.1f)
                    res += i <= perc ? FILL : BLANK;

                string t = String.IsNullOrWhiteSpace(title) ? "" : $"{title}\n";

                string percStr = showPerc ? $"{(perc * 100).ToString("F1")}%" : "";

                return $"{t}[{res}] {percStr}";
            }
        }
    }
}
