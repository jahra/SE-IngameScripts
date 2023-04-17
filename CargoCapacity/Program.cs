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
    partial class Program : MyGridProgram
    {
        const bool SHOW_INVENTORY_ITEMS = true;
        const bool SHOW_ONLY_MY_GRID = true;
        const string LCD_TAG = "LCDCargo";
        const int COCKPIT_DISPLAY_INDEX = -1;
        readonly bool CARGO_CONTAINER_ONLY = false;

        public void Save() { }

        List<IMyTerminalBlock> cargos;
        IMyTextPanel lcd;
        IMyTextSurface cockpit;

        Helper _helper;
        Graphics _graphics = new Graphics();

        public Program()
        {
            _helper = new Helper(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            cargos = new List<IMyTerminalBlock>();

            List<IMyTextPanel> lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(lcds);
            lcd = lcds?.Where(x => Me.CubeGrid == x.CubeGrid && x.CustomName.Contains(LCD_TAG)).FirstOrDefault();
            if(lcd != null)
			{
                lcd.Font = "Monospace";
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
			}

            GridTerminalSystem.GetBlocksOfType<IMyEntity>(cargos);
            var cargosSubGrid = new List<IMyTerminalBlock>();

            if (COCKPIT_DISPLAY_INDEX >= 0)
            {
                List<IMyCockpit> cockpits = new List<IMyCockpit>();
                GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
                cockpit = cockpits.Where(x => Me.CubeGrid == x.CubeGrid && x.SurfaceCount > 0).FirstOrDefault()?.GetSurface(COCKPIT_DISPLAY_INDEX);
                cockpit.ContentType = ContentType.TEXT_AND_IMAGE;
                cockpit.Font = "Monospace";
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (CARGO_CONTAINER_ONLY)
            {
                cargos = _helper.GetBlocks<IMyCargoContainer>().ToList<IMyTerminalBlock>();
            }
            else
            {
                if (SHOW_ONLY_MY_GRID)
                    cargos = cargos.Where(c => Me.CubeGrid == c.CubeGrid && c.HasInventory).ToList();
                else
                    cargos = cargos.Where(c => c.HasInventory).ToList();
            }

            float used = 0.0f;
            float max = 0.0f;

            float currMass = 0.0f;

            string displayText = "";


            foreach (var c in cargos)
            {
                used += (float)c.GetInventory(0).CurrentVolume;
                max += (float)c.GetInventory(0).MaxVolume;
                //currMass += (float)c.GetInventory(0).CurrentMass;
            }

            float usedPerc = (100 * used) / max;
            displayText = $"{used.ToString("### ### ### ##0.##")} /{max.ToString("### ### ### ##0.##")}\nUsed: {_graphics.GetProgressBar(usedPerc / 100)}{usedPerc.ToString("### ### ### ##0.##")}%\n";


            displayText += SHOW_INVENTORY_ITEMS ? GetInventoryItems(cargos) : "";

            lcd?.WriteText(displayText);
            cockpit?.WriteText(displayText);
        }

        private static string GetInventoryItems(List<IMyTerminalBlock> cargos)
        {
            List<MyInventoryItem> inv = new List<MyInventoryItem>();
            foreach (IMyEntity entity in cargos)
            {
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                entity.GetInventory().GetItems(items);
                inv.AddRange(items);
            }


            Dictionary<string, MyFixedPoint> allItems = new Dictionary<string, MyFixedPoint>();
            foreach (MyInventoryItem item in inv)
            {
                string type = item.Type.ToString().Split('/')[1];
                if (!allItems.ContainsKey(type))
                    allItems.Add(type, item.Amount);
                else
                    allItems[type] += item.Amount;
            }

            string invString = "";

            foreach (var i in allItems)
            {
                invString += $"{i.Key} : {((double)i.Value).ToString("### ### ### ###.##")}\r\n";
            }

            return invString;
        }
    }
}
