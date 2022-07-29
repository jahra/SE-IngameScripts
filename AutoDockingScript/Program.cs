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
        //CONNECTOR PROVIDER

        readonly String BROADCAST_TAG_REQUEST = "DOCKINGLIST_REQUEST";
        readonly String UNICAST_TAG_RESPONSE = "DOCKINGLIST_RESPONSE";
        IMyBroadcastListener myBroadcastListener;

        readonly int LOG_HISTORY = 20;
        List<String> log = new List<string>();

        public Program()
        {
            Echo("<==AutoDockingScript==>");
            //Runtime.UpdateFrequency = UpdateFrequency.Update100;//Will be called only by IGC
            myBroadcastListener = IGC.RegisterBroadcastListener(BROADCAST_TAG_REQUEST);
            myBroadcastListener.SetMessageCallback(BROADCAST_TAG_REQUEST);
            Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            Me.GetSurface(0).WriteText("Docking Script running...");
        }

        public void Save()
        {
        }

        void Init()
        {

            var s = Me.GetSurface(0);
            s.FontSize = 2;
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s = Me.GetSurface(1);
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s.WriteText("AUTO DOCKING");
            log.Clear();
            LogMessage("Running...");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource == UpdateType.IGC && myBroadcastListener.HasPendingMessage)
            {
                //Connectors
                List<IMyShipConnector> conns = new List<IMyShipConnector>();
                GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(conns);
                conns = conns.Where(c => c.Enabled && c.Status == MyShipConnectorStatus.Unconnected).ToList();

                MyIGCMessage message = myBroadcastListener.AcceptMessage();
                LogMessage("Received message with tag: " + message.Tag + "from source: " + message.Source + "with data: " + message.Data);
                if (message.Tag == BROADCAST_TAG_REQUEST)
                {
                    String connsList = GetConnectorsPositions(conns.Where(c => c.CustomName.Contains(message.Data.ToString())).ToList());
                    bool res = IGC.SendUnicastMessage(message.Source, UNICAST_TAG_RESPONSE, connsList);
                    if (res)
                    {
                        LogMessage("Connectors list send successfully");
                        LogMessage(connsList);
                    }
                    else
                        LogMessage("Connectors list cannot be send, given endpoint is unreachable");
                }
            }
        }

        String GetConnectorsPositions(List<IMyShipConnector> conns)
        {
            String connectors = "";
            foreach (IMyShipConnector c in conns)
            {
                var cRotation = c.WorldMatrix.Forward;

                double x = c.GetPosition().X + 15 * cRotation.X;
                double y = c.GetPosition().Y + 15 * cRotation.Y;
                double z = c.GetPosition().Z + 15 * cRotation.Z;

                MyWaypointInfo wp1 = new MyWaypointInfo("Above", x, y, z);
                x = c.GetPosition().X + 1.5 * cRotation.X;
                y = c.GetPosition().Y + 1.5 * cRotation.Y;
                z = c.GetPosition().Z + 1.5 * cRotation.Z;
                MyWaypointInfo wp2 = new MyWaypointInfo("Sit", x, y, z);
                String connPosition = String.Format("{0};{1}", wp1.ToString(), wp2.ToString());
                connectors += connectors + "|" + connPosition;
            }
            return connectors;
        }

        void LogMessage(String message)
        {
            if (log.Count > LOG_HISTORY)
                log.Remove(log.LastOrDefault());

            log.Add(message);

            string slog = "";
            log.ForEach(x => slog += x + "\n");

            Me.GetSurface(0).WriteText(slog);
            try
            {
                var s = (GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit).GetSurface(0);
                s.ContentType = ContentType.TEXT_AND_IMAGE;
                s.FontSize = 2;
                s.WriteText(slog);
            }
            catch { }
            Echo(message);
        }
    }
}
