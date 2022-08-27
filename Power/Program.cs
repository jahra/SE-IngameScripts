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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        LCDHelper _lcd;
        public Program()
        {
            Echo("<==Power==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            _lcd = new LCDHelper(this);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //var lcd = GridTerminalSystem.GetBlockWithName("LCDBatBase") as IMyTextPanel;
            //var lcd1 = GridTerminalSystem.GetBlockWithName("LCDBatBase1") as IMyTextPanel;
            List<IMyBatteryBlock> bats = new List<IMyBatteryBlock>();
            List<IMySolarPanel> sps = new List<IMySolarPanel>();
            List<IMyPowerProducer> pps = new List<IMyPowerProducer>();
            List<IMyPowerProducer> wts = new List<IMyPowerProducer>();
            List<IMyPowerProducer> hes = new List<IMyPowerProducer>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(bats);
            GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(sps);
            GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(pps);
            List<string> fs = new List<string>();
            Dictionary<IMyCubeGrid, GridPowerSum> grids = new Dictionary<IMyCubeGrid, GridPowerSum>();

            foreach (var sp in pps)
            {
                if (sp.GetType().ToString().Split('.').Last() == "MyWindTurbine")
                    wts.Add(sp);
                if (sp.GetType().ToString().Split('.').Last() == "MyHydrogenEngine")
                    hes.Add(sp);
            }

            float output = 0;

            foreach (var b in bats)
            {
                if (!grids.ContainsKey(b.CubeGrid))
                    grids.Add(b.CubeGrid, new GridPowerSum());
                grids[b.CubeGrid].Current += b.CurrentStoredPower;
                grids[b.CubeGrid].Max += b.MaxStoredPower;

                output += b.CurrentOutput;
            }

            foreach (var g in grids.Keys)
            {
                fs.Add($"{g.CustomName}: {(grids[g].Current * 1000).ToString("0")} kWh / {(grids[g].Max * 1000).ToString("0")} kWh\t {grids[g].Perc.ToString("F1")}%");
            }
            //string s = String.Join("\n",fs);   
            //lcd.WriteText(s);
            Color c = new Color(100, 255, 255);
            _lcd.WriteToLcds(fs, "[BatStat]", 11, c, 1.5f);

            fs.Clear();

            float total = 0;

            total += AddSumPowerOutput(sps.Cast<IMyPowerProducer>().ToList(), fs, "Solar");
            total += AddSumPowerOutput(wts, fs, "Wind");
            total += AddSumPowerOutput(hes, fs, "Engine");
            fs.Add("Total Input: " + GetKW(total));
            fs.Add("Total Output: " + GetKW(output));
            fs.Add("Sum: " + GetKW(total - output));

            //s = String.Join("\n",fs);
            //lcd1.WriteText(s);
            _lcd.WriteToLcds(fs, "[BatStat1]", 11, c, 1.5f);
        }

        float AddSumPowerOutput(List<IMyPowerProducer> pps, List<string> fs, string name)
        {
            float spi = 0;

            foreach (var sp in pps)
            {
                spi += sp.CurrentOutput;
            }
            fs.Add(name + ": " + GetKW(spi));

            return spi;
        }

        string GetKW(float f)
        {
            return (f * 1000).ToString("0.##") + " kW";
        }

        private class GridPowerSum
        {
            public float Current = 0;

            public float Max = 0;
            public float Perc => (Current / Max) * 100f;
        }
    }
}

