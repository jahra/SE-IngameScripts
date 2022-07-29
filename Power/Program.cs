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
        private Dictionary<string, List<IMyTextPanel>> GetLcds(string tag)
        {
            List<IMyTextPanel> lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds);
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

        private void WriteToLcds(List<string> fs, string tag, int lines, Color color, float size)
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

        public Program()
        {
            Echo("<==Power==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
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
            Dictionary<IMyCubeGrid, float> grids = new Dictionary<IMyCubeGrid, float>();

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
                    grids.Add(b.CubeGrid, 0);
                grids[b.CubeGrid] += b.CurrentStoredPower;
                output += b.CurrentOutput;
            }

            foreach (var g in grids.Keys)
            {
                fs.Add(g.CustomName + ": " + (grids[g] * 1000).ToString("0") + " kWh");
            }
            //string s = String.Join("\n",fs);   
            //lcd.WriteText(s);
            Color c = new Color(100, 255, 255);
            WriteToLcds(fs, "[BatStat]", 11, c, 1.5f);

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
            WriteToLcds(fs, "[BatStat1]", 11, c, 1.5f);
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

    }
}

