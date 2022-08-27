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
        LCDHelper _lcd;


        public Program()
        {
            Echo("<==Factory==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            _lcd = new LCDHelper(this);
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
                _lcd.WriteToLcds(fs, "[Factory]", _linesPerDisplay, _c, _fontSize);
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


            _lcd.WriteToLcds(fs, "[Factory]", _linesPerDisplay, _c, _fontSize);
        }

        ComponentInfo[] GetComponentInfos()
        {
            List<ComponentInfo> lci = new List<ComponentInfo>();
            lci.Add(new ComponentInfo() { Name = "Steel:          ", TypeName = "MyObjectBuilder_Component/SteelPlate", Desired = 50000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SteelPlate" });
            lci.Add(new ComponentInfo() { Name = "Glass:          ", TypeName = "MyObjectBuilder_Component/BulletproofGlass", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/BulletproofGlass" });
            lci.Add(new ComponentInfo() { Name = "Detector:       ", TypeName = "MyObjectBuilder_Component/Detector", Desired = 10, BlueprintName = "MyObjectBuilder_BlueprintDefinition/DetectorComponent" });
            lci.Add(new ComponentInfo() { Name = "Girder:         ", TypeName = "MyObjectBuilder_Component/Girder", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/GirderComponent" });
            lci.Add(new ComponentInfo() { Name = "Display:        ", TypeName = "MyObjectBuilder_Component/Display", Desired = 3000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Display" });
            lci.Add(new ComponentInfo() { Name = "LargeTube:      ", TypeName = "MyObjectBuilder_Component/LargeTube", Desired = 500, BlueprintName = "MyObjectBuilder_BlueprintDefinition/LargeTube" });
            lci.Add(new ComponentInfo() { Name = "GravityG:       ", TypeName = "MyObjectBuilder_Component/GravityGenerator", Desired = 1, BlueprintName = "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent" });
            lci.Add(new ComponentInfo() { Name = "MetalGrid:      ", TypeName = "MyObjectBuilder_Component/MetalGrid", Desired = 4000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/MetalGrid" });
            lci.Add(new ComponentInfo() { Name = "Motor:          ", TypeName = "MyObjectBuilder_Component/Motor", Desired = 2000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/MotorComponent" });
            lci.Add(new ComponentInfo() { Name = "PowerCell:      ", TypeName = "MyObjectBuilder_Component/PowerCell", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/PowerCell" });
            lci.Add(new ComponentInfo() { Name = "RadioComm:      ", TypeName = "MyObjectBuilder_Component/RadioCommunication", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent" });
            lci.Add(new ComponentInfo() { Name = "Reactor:        ", TypeName = "MyObjectBuilder_Component/Reactor", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ReactorComponent" });
            lci.Add(new ComponentInfo() { Name = "SmallTube:      ", TypeName = "MyObjectBuilder_Component/SmallTube", Desired = 1000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SmallTube" });
            lci.Add(new ComponentInfo() { Name = "Supercond:      ", TypeName = "MyObjectBuilder_Component/Superconductor", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Superconductor" });
            lci.Add(new ComponentInfo() { Name = "SolarCell:      ", TypeName = "MyObjectBuilder_Component/SolarCell", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/SolarCell" });
            lci.Add(new ComponentInfo() { Name = "InterPlate:     ", TypeName = "MyObjectBuilder_Component/InteriorPlate", Desired = 15000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/InteriorPlate" });
            lci.Add(new ComponentInfo() { Name = "Computer:       ", TypeName = "MyObjectBuilder_Component/Computer", Desired = 2000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ComputerComponent" });
            lci.Add(new ComponentInfo() { Name = "ConstComp:      ", TypeName = "MyObjectBuilder_Component/Construction", Desired = 2000, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ConstructionComponent" });
            lci.Add(new ComponentInfo() { Name = "Ammo:           ", TypeName = "MyObjectBuilder_AmmoMagazine/NATO_25x184mm", Desired = 300, BlueprintName = "MyObjectBuilder_BlueprintDefinition/NATO_25x184mmMagazine" });
            lci.Add(new ComponentInfo() { Name = "Thruster comp:  ", TypeName = "MyObjectBuilder_Component/Thrust", Desired = 100, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ThrustComponent" });
            lci.Add(new ComponentInfo() { Name = "Explosives:  ", TypeName = "MyObjectBuilder_Component/Explosives", Desired = 200, BlueprintName = "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent" });
            lci.Add(new ComponentInfo() { Name = "Medical Comp:  ", TypeName = "MyObjectBuilder_BlueprintDefinition/MedicalComponent", Desired = 10, BlueprintName = "MyObjectBuilder_BlueprintDefinition/MedicalComponent" });

            lci.Add(new ComponentInfo() { Name = "Enhanced Welder:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/Welder2Item", Desired = 2, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Welder2" });
            lci.Add(new ComponentInfo() { Name = "Enhanced Grinder:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item", Desired = 2, BlueprintName = "MyObjectBuilder_BlueprintDefinition/AngleGrinder2" });
            lci.Add(new ComponentInfo() { Name = "Enhanced Hand Drill:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/HandDrill2Item", Desired = 2, BlueprintName = "MyObjectBuilder_BlueprintDefinition/HandDrill2" });

            lci.Add(new ComponentInfo() { Name = "Elite Welder:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/Welder3Item", Desired = 1, BlueprintName = "MyObjectBuilder_BlueprintDefinition/Welder3" });
            lci.Add(new ComponentInfo() { Name = "Elite Grinder:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item", Desired = 1, BlueprintName = "MyObjectBuilder_BlueprintDefinition/AngleGrinder3" });
            lci.Add(new ComponentInfo() { Name = "Elite Hand Drill:  ", TypeName = "MyObjectBuilder_PhysicalGunObject/HandDrill3Item", Desired = 1, BlueprintName = "MyObjectBuilder_BlueprintDefinition/HandDrill3" });
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
