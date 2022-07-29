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
        IMyCockpit c;
        public Program()
        {
            Echo("<==LanderCockpit==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Init();
        }

        void Init()
        {
            IdentifyDisplays();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            GetH2Info(c.GetSurface(0));
            ConnectorsInfo(c.GetSurface(1));
            ConnsAutoLock();
        }

        void ConnsAutoLock()
        {
            List<IMyShipConnector> conns = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(conns);
            conns = conns.Where(c => c.Enabled && c.Status == MyShipConnectorStatus.Connectable && c.PullStrength != 0).ToList();

            foreach (IMyShipConnector c in conns)
            {
                c.Connect();
            }
        }

        void ConnectorsInfo(IMyTextSurface ts)
        {
            List<IMyShipConnector> conns = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(conns);
            conns = conns.Where(x => x.CubeGrid == Me.CubeGrid).ToList();
            String s = "";

            foreach (IMyShipConnector c in conns)
            {
                s += String.Format("{0}: {1}", c.DisplayNameText, c.Status) + "\n";
            }

            ts.WriteText(s);
        }

        void GetBattsInfo(IMyTextSurface ts)
        {
            List<IMyBatteryBlock> batts = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts);
            batts = batts.Where(b => b.CubeGrid == Me.CubeGrid).ToList();
        }

        void SafetyFuse()
        {
            List<IMyShipConnector> conns = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(conns);
            conns = conns.Where(x => x.CubeGrid == Me.CubeGrid).ToList();

            //conns.Any(x=> x.IsConnected)

        }

        void GetH2Info(IMyTextSurface ts)
        {
            List<IMyGasTank> tanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
            tanks = tanks.Where(x => x.CubeGrid == Me.CubeGrid && x.BlockDefinition.SubtypeName == "LargeHydrogenTank").ToList();
            double totalRatio = 0;
            String s = "H2 stock pile: {0}";
            bool stockPile = true;
            foreach (IMyGasTank tank in tanks)
            {
                s += String.Format("\n       {0:f}%", tank.FilledRatio * 100.0);
                totalRatio += tank.FilledRatio;
                stockPile = stockPile & tank.Stockpile;
            }

            s = String.Format(s, stockPile);

            totalRatio = totalRatio / tanks.Count;
            if (totalRatio < 0.1)
                ts.FontColor = Color.Red;
            else
                ts.FontColor = Color.White;

            ts.WriteText(s);
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
