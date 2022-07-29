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
using Sandbox.Game.Entities.Blocks;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        IMyCockpit _cockpit;
        private float _fontSize;
        private Color _color;
        private int _linesPerDisplay;

        public Program()
        {
            Echo("<==MinerCockpit==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            List<IMyCockpit> cs = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cs);
            cs = cs.Where(x => x.CubeGrid == Me.CubeGrid).ToList();
            _cockpit = cs.FirstOrDefault();
        }

        public void Save()
        {
        }
        public void Main(string argument, UpdateType updateSource)
        {

            for (int i = 0; i < _cockpit.SurfaceCount; i++)
            {
                switch (i)
                {

                    case 0:
                        ConfigureOresDisplay(i);
                        _cockpit.GetSurface(i).WriteText(GetOreResources(), false);
                        break;
                    case 1:
                        //ConfigureDisplay(i);
                        //_cockpit.GetSurface(i).WriteText(String.Format("{0:f}", _cockpit.GetShipSpeed()));
                        _cockpit.GetSurface(i).ContentType = ContentType.SCRIPT;
                        _cockpit.GetSurface(i).Script = "TSS_ArtificialHorizon";
                        break;
                    case 2:
                        //c.GetSurface(i).WriteText("2", false);
                        ConfigureDisplay(i);
                        _cockpit.GetSurface(i).WriteText(GetCargoCapacity());
                        break;
                    case 3:
                        //c.GetSurface(i).WriteText("3", false);
                        break;

                    default:
                        break;
                }

            }
        }

        public string GetInventoryItems()
        {
            List<IMyTerminalBlock> ents = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyEntity>(ents);
            ents = ents.Where(x => x.HasInventory && x.GetInventory().ItemCount > 0).ToList();
            foreach (var item in ents)
            {

            }

            return null;
        }

        public void ConfigureOresDisplay(int i)
        {
            _cockpit.GetSurface(i).Alignment = TextAlignment.LEFT;
            _cockpit.GetSurface(i).FontSize = 2;
            _cockpit.GetSurface(i).ContentType = ContentType.TEXT_AND_IMAGE;
        }

        public void ConfigureDisplay(int i)
        {
            _cockpit.GetSurface(i).Alignment = TextAlignment.CENTER;
            _cockpit.GetSurface(i).FontSize = 3;
            _cockpit.GetSurface(i).ContentType = ContentType.TEXT_AND_IMAGE;
        }

        public string GetCargoCapacity()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> inventory = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(blocks);
            inventory.AddList(blocks);
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks);
            inventory.AddList(blocks);
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks);
            IMyShipConnector conn = (IMyShipConnector)blocks.Where(x => x.CubeGrid == Me.CubeGrid).FirstOrDefault();
            if (conn == null)
                return "No connector found";
            inventory.AddList(blocks);

            inventory = inventory.Where(x => x.HasInventory && Me.CubeGrid == x.CubeGrid && x.GetInventory().IsConnectedTo(conn.GetInventory())).ToList();

            double current = 0;
            double max = 0;
            foreach (var i in inventory)
            {
                current += (double)i.GetInventory().CurrentVolume;
                max += (double)i.GetInventory().MaxVolume;
            }
            Echo(String.Format("Total cargo capacity: {0:f}/{1:f}  {2:f}%", current, max, (current / max) * 100));
            return String.Format("{0:f}%", (current / max) * 100);
        }

        string GetOreResources()
        {
            List<IMyEntity> ents = new List<IMyEntity>();
            GridTerminalSystem.GetBlocksOfType<IMyEntity>(ents);
            ents = ents.Where(b => ((IMyCubeBlock)b).CubeGrid == Me.CubeGrid).ToList();
            List<MyInventoryItem> iis = new List<MyInventoryItem>();
            List<string> fs = new List<string>();
            IngotInfo[] ingots = GetOreInfos();

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
                if (ingots[i].Amount > 0)
                    fs.Add(ingots[i].Text);
            }

            //WriteToDisplay(fs, 1, _linesPerDisplay, _color, _fontSize);
            return String.Join("\r\n", fs);
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
