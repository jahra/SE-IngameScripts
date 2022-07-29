using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private List<IMyTerminalBlock> bpList = new List<IMyTerminalBlock>();
        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            GridTerminalSystem.GetBlocksOfType<IMyButtonPanel>(bpList);
            bpList = bpList.Where(x => x.BlockDefinition.SubtypeName == "LargeSciFiButtonPanel").ToList();
            List<ITerminalAction> actions = new List<ITerminalAction>();
            foreach (var item in bpList)
            {
                
            }
            
            IMyTextSurfaceProvider i = (IMyTextSurfaceProvider)bpList.First();
            i.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            i.GetSurface(0).FontSize = 4;
            i.GetSurface(0).WriteText("Rot lights");
        }
    }
}
