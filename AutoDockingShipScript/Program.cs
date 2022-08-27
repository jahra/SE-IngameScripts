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
        readonly String BROADCAST_TAG_REQUEST = "DOCKINGLIST_REQUEST";
        readonly String UNICAST_TAG_RESPONSE = "DOCKINGLIST_RESPONSE";
        bool undock = false;
        MyWaypointInfo nextWp = MyWaypointInfo.Empty;

        List<IMyBatteryBlock> batts = new List<IMyBatteryBlock>();
        List<IMyGasTank> gasTanks = new List<IMyGasTank>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
        IMyShipConnector conn;
        IMyRemoteControl remc;
        IMyRadioAntenna ant;

        //IMyBroadcastListener myBroadcastListener;
        IMyUnicastListener myUnicastListener;

        private Logger _logger;
        private Helper _helper;

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
            _logger = new Logger(this);
            _helper = new Helper(this);

            batts = _helper.GetBlocks<IMyBatteryBlock>();
            gasTanks = _helper.GetBlocks<IMyGasTank>();
            thrusters = _helper.GetBlocks<IMyThrust>();
            conn = _helper.GetBlock<IMyShipConnector>();
            remc = _helper.GetBlock<IMyRemoteControl>();
            ant = _helper.GetBlock<IMyRadioAntenna>();
            lights = _helper.GetBlocks<IMyLightingBlock>();

            var s = Me.GetSurface(0);
            s.FontSize = 2;
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s = Me.GetSurface(1);
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s.WriteText("AUTO DOCKING");
            _logger.LogMessage("Running...");
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
                _logger.LogMessage("Requestign connectors coords.");
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
                _logger.LogMessage("IGC update");
                String data = "";
                while (myUnicastListener.HasPendingMessage)
                {
                    MyIGCMessage message = myUnicastListener.AcceptMessage();
                    _logger.LogMessage("Received message with tag: " + message.Tag + "\t\nfrom source: " + message.Source);
                    if (message.Tag == UNICAST_TAG_RESPONSE)
                        if (message.Data is String)
                            data += message.Data;
                }

                List<MyTuple<MyWaypointInfo, MyWaypointInfo>> connsList = GetConnsListFromString(data);

                if (connsList.Count <= 0)
                {
                    _logger.LogMessage("No connector was found.");
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
                    _logger.LogMessage("Autopilot disabled");
                }
                else
                {
                    remc.SetAutoPilotEnabled(false);
                    nextWp = MyWaypointInfo.Empty;
                }

                if (conn.Status == MyShipConnectorStatus.Connected)
                {
                    _logger.LogMessage("batts: " + batts.Count.ToString());
                    batts.ForEach(b => b.ChargeMode = undock ? ChargeMode.Auto : ChargeMode.Recharge);
                    gasTanks.ForEach(g => g.Enabled = undock);
                    thrusters.ForEach(t => t.Enabled = undock);
                    ant.Enabled = undock;
                    lights.ForEach(x => x.Enabled = undock);
                    _logger.LogMessage("thrusters: " + thrusters.Count.ToString());
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
            _logger.LogMessage(direction.ToString());


            _logger.LogMessage("remote: " + remc.GetPosition());
            _logger.LogMessage("conn: " + conn.GetPosition());
            coord = new MyWaypointInfo(coord.Name, (remc.GetPosition() - conn.GetPosition()) + coord.Coords);
            _logger.LogMessage("old coords" + (remc.GetPosition() - conn.GetPosition()) + coord.Coords + "\n");
            Vector3D rcoffset = remc.GetPosition() - conn.GetPosition();
            _logger.LogMessage("offset: " + rcoffset);
            Vector3D finalCoord = (rcoffset + remc.GetPosition()) + coord.Coords;
            //coord = new MyWaypointInfo(coord.Name, finalCoord);
            _logger.LogMessage("new coords: " + coord);

            remc.ClearWaypoints();
            remc.AddWaypoint(coord);

            remc.SetAutoPilotEnabled(true);
            _logger.LogMessage("Autopilot enabled.");
        }
    }
}
