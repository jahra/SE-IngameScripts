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
        string AssemblerName = "AssemblerBase";

        Color _c = new Color(0, 255, 255);
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
            Echo("<==Factory==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var ass = GridTerminalSystem.GetBlockWithName(AssemblerName) as IMyAssembler;
            List<string> fs = new List<string>();
            if (ass == null)
            {
                fs.Add(AssemblerName + " not found!");
                WriteToLcds(fs, "[Factory]", _linesPerDisplay, _c, _fontSize);
                return;
            }

            List<IMyEntity> ents = new List<IMyEntity>();
            GridTerminalSystem.GetBlocksOfType<IMyEntity>(ents);
            List<MyInventoryItem> iis = new List<MyInventoryItem>();

            ComponentInfo[] ingots = GetComponentInfos();
            List<MyProductionItem> queued = new List<MyProductionItem>();

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

            ass.GetQueue(queued);

            for (int i = 0; i < ingots.Length; i++)
            {
                fs.Add(ingots[i].Text);
                if (ingots[i].Needed > 0)
                {
                    MyDefinitionId m;
                    bool b = MyDefinitionId.TryParse(ingots[i].BlueprintName, out m);


                    if (b && !queued.Any(x => x.BlueprintId.Equals(m)))
                        ass.AddQueueItem(m, ingots[i].Needed);
                }
            }


            WriteToLcds(fs, "[Factory]", _linesPerDisplay, _c, _fontSize);
        }

        ComponentInfo[] GetComponentInfos()
        {
            List<ComponentInfo> lci = new List<ComponentInfo>();
            lci.Add(new ComponentInfo() { Name = "Steel:          ", TypeName = "MyObjectBuilder_Component/SteelPlate", Desired = 5000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SteelPlate" });
            lci.Add(new ComponentInfo() { Name = "Glass:          ", TypeName = "MyObjectBuilder_Component/BulletproofGlass", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/BulletproofGlass" });
            lci.Add(new ComponentInfo() { Name = "Detector:       ", TypeName = "MyObjectBuilder_Component/Detector", Desired = 25, BlueprintName = "MyObjectBuilder_BlueprintDefinition/DetectorComponent" });
            lci.Add(new ComponentInfo() { Name = "Girder:         ", TypeName = "MyObjectBuilder_Component/Girder", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/GirderComponent" });
            lci.Add(new ComponentInfo() { Name = "Display:        ", TypeName = "MyObjectBuilder_Component/Display", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Display" });
            lci.Add(new ComponentInfo() { Name = "LargeTube:      ", TypeName = "MyObjectBuilder_Component/LargeTube", Desired = 500, BlueprintName = "MyObjectBuilder_BlueprintDefinition/LargeTube" });
            lci.Add(new ComponentInfo() { Name = "GravityG:       ", TypeName = "MyObjectBuilder_Component/GravityGenerator", Desired = 1, BlueprintName = "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent" });
            lci.Add(new ComponentInfo() { Name = "MetalGrid:      ", TypeName = "MyObjectBuilder_Component/MetalGrid", Desired = 500, BlueprintName = "MyObjectBuilder_BlueprintDefinition/MetalGrid" });
            lci.Add(new ComponentInfo() { Name = "Motor:          ", TypeName = "MyObjectBuilder_Component/Motor", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/MotorComponent" });
            lci.Add(new ComponentInfo() { Name = "PowerCell:      ", TypeName = "MyObjectBuilder_Component/PowerCell", Desired = 200, BlueprintName = "MyObjectBuilder_BlueprintDefinition/PowerCell" });
            lci.Add(new ComponentInfo() { Name = "RadioComm:      ", TypeName = "MyObjectBuilder_Component/RadioCommunication", Desired = 15, BlueprintName = "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent" });
            lci.Add(new ComponentInfo() { Name = "Reactor:        ", TypeName = "MyObjectBuilder_Component/Reactor", Desired = 5, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ReactorComponent" });
            lci.Add(new ComponentInfo() { Name = "SmallTube:      ", TypeName = "MyObjectBuilder_Component/SmallTube", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SmallTube" });
            lci.Add(new ComponentInfo() { Name = "Supercond:      ", TypeName = "MyObjectBuilder_Component/Superconductor", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Superconductor" });
            lci.Add(new ComponentInfo() { Name = "SolarCell:      ", TypeName = "MyObjectBuilder_Component/SolarCell", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SolarCell" });
            lci.Add(new ComponentInfo() { Name = "InterPlate:     ", TypeName = "MyObjectBuilder_Component/InteriorPlate", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/InteriorPlate" });
            lci.Add(new ComponentInfo() { Name = "Computer:       ", TypeName = "MyObjectBuilder_Component/Computer", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ComputerComponent" });
            lci.Add(new ComponentInfo() { Name = "ConstComp:      ", TypeName = "MyObjectBuilder_Component/Construction", Desired = 2000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ConstructionComponent" });
            lci.Add(new ComponentInfo() { Name = "Ammo:           ", TypeName = "MyObjectBuilder_AmmoMagazine/NATO_25x184mm", Desired = 300, BlueprintName = "MyObjectBuilder_BlueprintDefinition/NATO_25x184mmMagazine" });
            lci.Add(new ComponentInfo() { Name = "Thruster comp:  ", TypeName = "MyObjectBuilder_Component/Thrust", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ThrustComponent" });
            return lci.ToArray();
        }

        class ComponentInfo
        {
            public string Name { get; set; }
            public string TypeName { get; set; }
            public string BlueprintName { get; set; }
            public long Amount { get; set; }
            public long Desired { get; set; }
            public double Needed { get { return Desired - Amount / 1000000; } }

            public string Text { get { return Name + Amount / 1000000 + "/" + Desired; } }
        }

    }
}
