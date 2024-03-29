﻿using Sandbox.Game.EntityComponents;
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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        IMyCockpit c;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            IdentifyDisplays();
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
        }

        void IdentifyDisplays()
        {
            List<IMyCockpit> cs = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cs);
            cs = cs.Where(x => x.CubeGrid == Me.CubeGrid).ToList();
            c = cs.FirstOrDefault();

            for (int i = 0; i < c.SurfaceCount; i++)
            {
                c.GetSurface(i).ContentType = ContentType.TEXT_AND_IMAGE;
                if (i == 1)
                    c.GetSurface(i).FontSize = 1.2f;
                c.GetSurface(i).FontSize = 2;
                c.GetSurface(i).WriteText("Display " + i);
            }
        }
    }
}
