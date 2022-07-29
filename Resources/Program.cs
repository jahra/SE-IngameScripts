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
        Color _c = new Color(255, 255, 0);
        float _fontSize = 2.5f;
        int _linesPerDisplay = 7;

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
            Echo("<==Resources==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            ShowIngotResources("LCDIngots");
            ShowOreResources("LCDOres");
        }
        void ShowIngotResources(String LcdName)
        {
            List<IMyEntity> ents = new List<IMyEntity>();
            GridTerminalSystem.GetBlocksOfType<IMyEntity>(ents);
            List<MyInventoryItem> iis = new List<MyInventoryItem>();
            List<string> fs = new List<string>();
            IngotInfo[] ingots = GetIngotInfos();

            fs.Add("Ingots:");

            foreach (var e in ents)
            {
                for (int i = 0; i < e.InventoryCount; i++)
                {
                    e.GetInventory(i).GetItems(iis);
                }
            }

            foreach (var item in iis)
            {
                for (int i = 0; i < ingots.Length; i++)
                {
                    if (item.Type.ToString() == ingots[i].TypeName)
                        ingots[i].Amount += item.Amount.RawValue;
                }
            }

            for (int i = 0; i < ingots.Length; i++)
            {
                fs.Add(ingots[i].Text);
            }


            WriteToLcds(fs, "[Ingots]", _linesPerDisplay, _c, _fontSize);
        }

        void ShowOreResources(String LcdName)
        {
            List<IMyEntity> ents = new List<IMyEntity>();
            GridTerminalSystem.GetBlocksOfType<IMyEntity>(ents);
            List<MyInventoryItem> iis = new List<MyInventoryItem>();
            List<string> fs = new List<string>();
            IngotInfo[] ingots = GetOreInfos();

            fs.Add("Ores:");

            foreach (var e in ents)
            {
                for (int i = 0; i < e.InventoryCount; i++)
                {
                    e.GetInventory(i).GetItems(iis);
                }
            }

            foreach (var item in iis)
            {
                for (int i = 0; i < ingots.Length; i++)
                {
                    if (item.Type.ToString() == ingots[i].TypeName)
                        ingots[i].Amount += item.Amount.RawValue;
                }
            }

            for (int i = 0; i < ingots.Length; i++)
            {
                fs.Add(ingots[i].Text);
            }

            WriteToLcds(fs, "[Ores]", _linesPerDisplay, _c, _fontSize);
        }

        IngotInfo[] GetIngotInfos()
        {
            List<IngotInfo> lii = new List<IngotInfo>();
            lii.Add(new IngotInfo() { Name = "Fe", TypeName = "MyObjectBuilder_Ingot/Iron" });
            lii.Add(new IngotInfo() { Name = "Ni", TypeName = "MyObjectBuilder_Ingot/Nickel" });
            lii.Add(new IngotInfo() { Name = "Si", TypeName = "MyObjectBuilder_Ingot/Silicon" });
            lii.Add(new IngotInfo() { Name = "Co", TypeName = "MyObjectBuilder_Ingot/Cobalt" });
            lii.Add(new IngotInfo() { Name = "Ag", TypeName = "MyObjectBuilder_Ingot/Silver" });
            lii.Add(new IngotInfo() { Name = "Mg", TypeName = "MyObjectBuilder_Ingot/Magnesium" });
            lii.Add(new IngotInfo() { Name = "Au", TypeName = "MyObjectBuilder_Ingot/Gold" });
            lii.Add(new IngotInfo() { Name = "Ice", TypeName = "MyObjectBuilder_Ore/Ice" });
            return lii.ToArray();
        }

        IngotInfo[] GetOreInfos()
        {
            List<IngotInfo> lii = new List<IngotInfo>();
            lii.Add(new IngotInfo() { Name = "Fe", TypeName = "MyObjectBuilder_Ore/Iron" });
            lii.Add(new IngotInfo() { Name = "Ni", TypeName = "MyObjectBuilder_Ore/Nickel" });
            lii.Add(new IngotInfo() { Name = "Si", TypeName = "MyObjectBuilder_Ore/Silicon" });
            lii.Add(new IngotInfo() { Name = "Co", TypeName = "MyObjectBuilder_Ore/Cobalt" });
            lii.Add(new IngotInfo() { Name = "Ag", TypeName = "MyObjectBuilder_Ore/Silver" });
            lii.Add(new IngotInfo() { Name = "Mg", TypeName = "MyObjectBuilder_Ore/Magnesium" });
            lii.Add(new IngotInfo() { Name = "Au", TypeName = "MyObjectBuilder_Ore/Gold" });
            lii.Add(new IngotInfo() { Name = "Ice", TypeName = "MyObjectBuilder_Ore/Ice" });
            lii.Add(new IngotInfo() { Name = "Stone", TypeName = "MyObjectBuilder_Ore/Stone" });
            return lii.ToArray();
        }

        class IngotInfo
        {
            public string Name { get; set; }
            public string TypeName { get; set; }
            public long Amount { get; set; }

            public string Text { get { return Name + ": " + Amount / 1000000 + "kg"; } }
        }
    }
}
