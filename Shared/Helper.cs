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
        public class Helper
        {
            Program _program;

            public Helper(Program program)
            {
                _program = program;
            }

            public void Echo(string text)
            {
                Echo(text);
            }

            public T GetBlock<T>(string tag = "") where T : class
            {
                List<T> blocks = new List<T>();
                _program.GridTerminalSystem.GetBlocksOfType(blocks);
                return blocks?.Where(x => _program.Me.CubeGrid == ((IMyCubeBlock)x).CubeGrid && ((IMyTerminalBlock)x).CustomName.Contains(tag)).FirstOrDefault();
                
            }

            public List<T> GetBlocks<T>(string tag = "") where T : class
            {
                List<T> blocks = new List<T>();
                _program.GridTerminalSystem.GetBlocksOfType(blocks);
                return blocks?.Where(x => _program.Me.CubeGrid == ((IMyCubeBlock)x).CubeGrid && ((IMyTerminalBlock)x).CustomName.Contains(tag)).ToList();
            }

            public void SetupDisplay(IMyTextSurface display, float fontSize = 1.5f)
            {
                display.ContentType = ContentType.TEXT_AND_IMAGE;
                display.FontSize = fontSize;
                
            }
        }
    }
}
