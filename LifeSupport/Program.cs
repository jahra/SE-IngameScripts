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
        public Program()
        {
            Echo("<==LifeSupport==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            List<IMyTextPanel> textPanels = GetMyGridBlocks<IMyTextPanel>();
            List<IMyGasTank> gasTanks = GetMyGridBlocks<IMyGasTank>().Where<IMyGasTank>(x => x.BlockDefinition.SubtypeName == "LargeHydrogenTank").ToList();
            List<IMyGasTank> o2Tanks = GetMyGridBlocks<IMyGasTank>().Where<IMyGasTank>(x => x.BlockDefinition.SubtypeName != "LargeHydrogenTank").ToList();

        }

        List<T> GetMyGridBlocks<T>() where T : class
        {
            List<T> blocks = new List<T>();
            GridTerminalSystem.GetBlocksOfType<T>(blocks);
            blocks = blocks.Where(x => ((IMyCubeBlock)x).CubeGrid == Me.CubeGrid).ToList();
            return blocks;
        }

        String GetCapacityStringInfo<T>(List<T> blocks) where T : IMyGasTank
        {
            double totalRatio = 0;
            foreach (IMyGasTank block in blocks)
                totalRatio += block.FilledRatio;
            return String.Format("{0}:{1}:{2}%", blocks.FirstOrDefault().BlockDefinition.SubtypeName, blocks.Count, totalRatio / blocks.Count);
        }

        String GetBatteryCapacityInfo()
        {
            List<IMyBatteryBlock> batts = GetMyGridBlocks<IMyBatteryBlock>();
            if (batts.Count <= 0)
                return null;
            float actualPerc = 0;
            float sum = 0;
            foreach (IMyBatteryBlock batt in batts)
            {
                actualPerc = (batt.CurrentStoredPower / batt.MaxStoredPower) * 100;
                sum = batt.CurrentInput - batt.CurrentOutput;
            }

            return String.Format("{0} {1}", actualPerc, sum);
        }
    }
}
