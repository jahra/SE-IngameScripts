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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        readonly String BROADCAST_TAG_REQUEST = "DOCKINGLIST_REQUEST";
        readonly String UNICAST_TAG_RESPONSE = "DOCKINGLIST_RESPONSE";
        bool undock = false;
        MyWaypointInfo nextWp = MyWaypointInfo.Empty;

        List<IMyBatteryBlock> batts = new List<IMyBatteryBlock>();
        List<IMyGasTank> gasTanks = new List<IMyGasTank>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        IMyShipConnector conn;
        IMyRemoteControl remc;

        //IMyBroadcastListener myBroadcastListener;
        IMyUnicastListener myUnicastListener;

        readonly int LOG_HISTORY = 6;
        List<String> log = new List<string>();

        public Program()
        {
            Echo("<==AutoDockingShipScript==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            myUnicastListener = IGC.UnicastListener;
            myUnicastListener.SetMessageCallback(UNICAST_TAG_RESPONSE);
            Init();
        }

        void Init()
        {
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts);
            batts = batts.Where(b => Me.CubeGrid == b.CubeGrid).ToList();

            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(gasTanks);
            gasTanks = gasTanks.Where(g => Me.CubeGrid == g.CubeGrid).ToList();

            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            thrusters = thrusters.Where(t => Me.CubeGrid == t.CubeGrid).ToList();

            List<IMyShipConnector> conns = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(conns);
            conns = conns.Where(c => Me.CubeGrid == c.CubeGrid).ToList();
            conn = conns.First();//Or specify connector on next line
            //conn = GridTerminalSystem.GetBlockWithName("Connector 4") as IMyShipConnector;

            //Get RemoteControl
            List<IMyRemoteControl> rems = new List<IMyRemoteControl>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(rems);
            rems = rems.Where(r => Me.CubeGrid == r.CubeGrid).ToList();
            remc = rems.First();//Or setup by name
            //IMyRemoteControl remc = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;

            var s = Me.GetSurface(0);
            s.FontSize = 2;
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s = Me.GetSurface(1);
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s.WriteText("AUTO DOCKING");
            log.Clear();
            LogMessage("Running...");

        }

        //ARGUMENTS:
        //	DOCK: Initialize docking seq: asks for conns, docks on closest, when docked charge batt, disables thrusters,tanks
        //	UNDOCK: Initialize undocking seq: batt mode: Auto, Enables thrusters and tanks
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource == UpdateType.Terminal || updateSource == UpdateType.Once || updateSource == UpdateType.Trigger)//TODO: test Onse
                && (!String.IsNullOrWhiteSpace(argument) && argument.Trim() == "DOCK"))
            {
                undock = false;
                IGC.SendBroadcastMessage(BROADCAST_TAG_REQUEST, Me.CustomData);//Me.CubeGrid.DisplayName + " requests connectors coords.");
                LogMessage("Requestign connectors coords.");
                return;
            }

            if (!nextWp.IsEmpty() && !remc.IsAutoPilotEnabled)
            {
                SetupRemoteControl(nextWp, 2, conn.Orientation.Forward);
                nextWp = MyWaypointInfo.Empty;
                return;
            }

            if (updateSource == UpdateType.IGC && myUnicastListener.HasPendingMessage)
            {
                LogMessage("IGC update");
                String data = "";
                while (myUnicastListener.HasPendingMessage)
                {
                    MyIGCMessage message = myUnicastListener.AcceptMessage();
                    LogMessage("Received message with tag: " + message.Tag + "\t\nfrom source: " + message.Source);
                    if (message.Tag == UNICAST_TAG_RESPONSE)
                        if (message.Data is String)
                            data += message.Data;
                }

                List<MyTuple<MyWaypointInfo, MyWaypointInfo>> connsList = GetConnsListFromString(data);

                if (connsList.Count <= 0)
                {
                    LogMessage("No connector was found.");
                    return;
                }
                MyTuple<MyWaypointInfo, MyWaypointInfo> closestConnector = GetClosestConnector(connsList, new MyWaypointInfo("MyPosition", conn.GetPosition()));
                nextWp = closestConnector.Item2;
                SetupRemoteControl(closestConnector.Item1);

                return;
            }
            //String gps = "GPS:Connector1:27432.16:142587.03:-114471.32:";
            if (!String.IsNullOrWhiteSpace(argument))
                undock = argument.Trim() == "UNDOCK" ? true : undock;
            if (conn.Status == MyShipConnectorStatus.Connectable || undock)
            {
                if (!undock)
                {
                    conn.Connect();
                    remc.SetAutoPilotEnabled(false);
                    LogMessage("Autopilot disabled");
                }
                else
                {
                    remc.SetAutoPilotEnabled(false);
                    nextWp = MyWaypointInfo.Empty;
                }

                if (conn.Status == MyShipConnectorStatus.Connected)
                {
                    LogMessage("batts: " + batts.Count.ToString());
                    batts.ForEach(b => b.ChargeMode = undock ? ChargeMode.Auto : ChargeMode.Recharge);
                    gasTanks.ForEach(g => g.Enabled = undock);
                    thrusters.ForEach(t => t.Enabled = undock);
                    LogMessage("thrusters: " + thrusters.Count.ToString());
                    if (undock)
                        conn.Disconnect();
                }
                return;
            }
        }

        List<MyTuple<MyWaypointInfo, MyWaypointInfo>> GetConnsListFromString(string s)
        {
            List<MyTuple<MyWaypointInfo, MyWaypointInfo>> list = new List<MyTuple<MyWaypointInfo, MyWaypointInfo>>();
            List<String> lconns = s.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (String c in lconns)
            {
                List<String> coords = c.Split(';').ToList();
                MyWaypointInfo wp1 = new MyWaypointInfo();
                MyWaypointInfo wp2 = new MyWaypointInfo();

                MyWaypointInfo.TryParse(coords[0], out wp1);
                MyWaypointInfo.TryParse(coords[1], out wp2);

                list.Add(new MyTuple<MyWaypointInfo, MyWaypointInfo>(wp1, wp2));
            }
            return list;
        }

        MyTuple<MyWaypointInfo, MyWaypointInfo> GetClosestConnector(List<MyTuple<MyWaypointInfo, MyWaypointInfo>> wayPoints, MyWaypointInfo wp)
        {
            MyTuple<MyWaypointInfo, MyWaypointInfo> closestConn = wayPoints.FirstOrDefault();
            double shortestDistance = Vector3D.Distance(closestConn.Item1.Coords, wp.Coords);

            for (int i = 1; i < wayPoints.Count; i++)
            {
                double tmpDistance = Vector3D.Distance(wayPoints[i].Item1.Coords, wp.Coords);
                if (tmpDistance < shortestDistance)
                {
                    shortestDistance = tmpDistance;
                    closestConn = wayPoints[i];
                }
            }

            return closestConn;
        }

        void SetupRemoteControl(MyWaypointInfo coord, float speedLimit = 15, Base6Directions.Direction direction = Base6Directions.Direction.Forward)
        {
            remc.FlightMode = FlightMode.OneWay;
            remc.SpeedLimit = speedLimit;

            //remc.SetDockingMode(true);//todo: test it//precize mod?

            remc.Direction = Base6Directions.Direction.Forward;
            LogMessage(direction.ToString());


            LogMessage("remote: " + remc.GetPosition());
            LogMessage("conn: " + conn.GetPosition());
            coord = new MyWaypointInfo(coord.Name, (remc.GetPosition() - conn.GetPosition()) + coord.Coords);
            LogMessage("old coords" + (remc.GetPosition() - conn.GetPosition()) + coord.Coords + "\n");
            Vector3D rcoffset = remc.GetPosition() - conn.GetPosition();
            LogMessage("offset: " + rcoffset);
            Vector3D finalCoord = (rcoffset + remc.GetPosition()) + coord.Coords;
            //coord = new MyWaypointInfo(coord.Name, finalCoord);
            LogMessage("new coords: " + coord);

            remc.ClearWaypoints();
            remc.AddWaypoint(coord);

            remc.SetAutoPilotEnabled(true);
            LogMessage("Autopilot enabled.");
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
