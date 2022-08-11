using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    /*
     ***TODO***
     *DONE - close/open doors when landed/ lifted
     *DONE - one button - automaticaly set distant destination
     *DONE - trader mode on last waypoint - bool na zacatku scriptu
     *      - v BASE nevypnul Thrustry
     
     */

    partial class Program : MyGridProgram
    {
        readonly int LOG_HISTORY = 6;
        readonly int MAX_SPEED = 50;
        readonly bool TRADER_MODE = false;
        readonly int DELAY_BETWEEN_RUNS_SECONDS = 0;


        List<IMyBatteryBlock> batts = new List<IMyBatteryBlock>();
        List<IMyGasTank> gasTanks = new List<IMyGasTank>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        List<IMyDoor> doors = new List<IMyDoor>();
        IMyShipConnector conn;
        IMyRemoteControl remc;
        IMyRadioAntenna ant;

        List<String> log = new List<string>();
        List<MyWaypointInfo> waypoints = new List<MyWaypointInfo>();
        int _direction = 0;
        int _current = 0;
        DateTime landed = DateTime.MaxValue;
        bool addMode = false;


        public Program()
        {
            Echo("<==AutoShipPatrol==>");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Init();

            string[] positions = Me.CustomData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string pos in positions)
            {
                MyWaypointInfo wp;
                if (MyWaypointInfo.TryParse(pos, out wp))
                    waypoints.Add(wp);
                else
                    LogMessage($"Failed to parse wp");
            }
            LogMessage($"{waypoints.Count} wp loaded");


        }

        void Init()
        {
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts);
            batts = batts.Where(b => Me.CubeGrid == b.CubeGrid).ToList();

            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(gasTanks);
            gasTanks = gasTanks.Where(g => Me.CubeGrid == g.CubeGrid).ToList();

            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            thrusters = thrusters.Where(t => Me.CubeGrid == t.CubeGrid).ToList();

            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);
            doors = doors.Where(t => Me.CubeGrid == t.CubeGrid).ToList();

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

            List<IMyRadioAntenna> ants = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(ants);
            ants = ants.Where(r => Me.CubeGrid == r.CubeGrid).ToList();
            ant = ants.First();//Or setup by name

            var s = Me.GetSurface(0);
            s.FontSize = 2;
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s = Me.GetSurface(1);
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s.WriteText("AUTO DOCKING");
            log.Clear();
            LogMessage("Running...");

        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!String.IsNullOrWhiteSpace(argument))
                switch (argument.ToLower())
                {
                    case "stop":
                        remc.SetAutoPilotEnabled(false);
                        _direction = 0;
                        SwitchFlightSystems(true);
                        LogMessage("Stopped");
                        return;
                    case "go":
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        Go();
                        return;
                    case "undock":
                        addMode = true;
                        SwitchFlightSystems(true);
                        conn.Disconnect();
                        break;
                    case "add":
                        addMode = true;
                        SwitchFlightSystems(true);
                        AddGPSPosition();
                        break;
                    case "dock1":
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        RunDock1();
                        return;
                    case "dock2":
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        RunDock2();
                        return;
                    default:
                        LogMessage("Unknown arg");
                        break;
                }

            if (_direction != 0 && !remc.IsAutoPilotEnabled)
            {
                _current += _direction;
                if (_current > waypoints.Count - 1 || _current < 0)
                {
                    _direction = 0;
                    landed = DateTime.UtcNow;
                    doors?.ForEach(x => x.OpenDoor());
                    LogMessage("End");
                    return;
                }
                LogMessage($"GO: {waypoints[_current].Name}");
                SetupRemoteControl(waypoints[_current], _current == 0 || _current == (waypoints.Count - 1) ? 2 : MAX_SPEED, conn.Orientation.Forward);
                return;
            }

            if (!addMode && (_current > waypoints.Count - 1 || _current < 0))
            {
                conn.Connect();
                remc.SetAutoPilotEnabled(false);
            }


            if (!addMode && conn.Status == MyShipConnectorStatus.Connected)
                if (TRADER_MODE && _current > waypoints.Count - 1)
                    SwitchFlightSystems(true);
                else
                    SwitchFlightSystems(false);

            if (DELAY_BETWEEN_RUNS_SECONDS != 0 && (DateTime.UtcNow - landed).TotalSeconds > DELAY_BETWEEN_RUNS_SECONDS)
            {
                landed = DateTime.MaxValue;
                Go();
            }
        }

        private void RunDock2()
        {
            _direction = 1;
            _current = 0;
            _current += _direction;
            SwitchFlightSystems(true);
            conn.Disconnect();
            SetupRemoteControl(waypoints[_current]);
        }

        private void RunDock1()
        {
            _direction = -1;
            _current = waypoints.Count - 1;
            _current += _direction;
            SwitchFlightSystems(true);
            conn.Disconnect();
            SetupRemoteControl(waypoints[_current]);
        }

        private void Go()
        {
            double distDock1 = Vector3D.Distance(remc.GetPosition(), waypoints[0].Coords);
            double distDock2 = Vector3D.Distance(remc.GetPosition(), waypoints[waypoints.Count - 1].Coords);

            if (distDock1 > distDock2)
            {
                _direction = -1;
                _current = waypoints.Count - 1;
            }
            else
            {
                _direction = 1;
                _current = 0;
            }

            _current += _direction;
            SwitchFlightSystems(true);
            conn.Disconnect();
            SetupRemoteControl(waypoints[_current]);
        }

        private void SwitchFlightSystems(bool enabled)
        {
            batts.ForEach(b => b.ChargeMode = enabled ? ChargeMode.Auto : ChargeMode.Recharge);
            gasTanks.ForEach(g => g.Enabled = enabled);
            thrusters.ForEach(t => t.Enabled = enabled);
            ant.Enabled = enabled;
        }

        private void AddGPSPosition()
        {
            string[] positions = Me.CustomData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            MyWaypointInfo position = new MyWaypointInfo(positions.Length.ToString(), remc.GetPosition());
            Me.CustomData += position.ToString() + "\r\n";
            LogMessage($"WP added: {positions.Length}");
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

            try
            {
                var s = (GridTerminalSystem.GetBlockWithName("LCDLog") as IMyTextPanel);
                s.ContentType = ContentType.TEXT_AND_IMAGE;
                s.FontSize = 1.5f;
                s.WriteText(slog);
            }
            catch { }

            Echo(message);
        }

        void SetupRemoteControl(MyWaypointInfo coord, float speedLimit = 15, Base6Directions.Direction direction = Base6Directions.Direction.Forward)
        {
            remc.FlightMode = FlightMode.OneWay;
            remc.SpeedLimit = speedLimit;

            //remc.SetDockingMode(true);//todo: test it//precize mod?

            remc.Direction = Base6Directions.Direction.Forward;
            //LogMessage(direction.ToString());


            //LogMessage("remote: " + remc.GetPosition());
            //LogMessage("conn: " + conn.GetPosition());

            //coord = new MyWaypointInfo(coord.Name, (remc.GetPosition() - conn.GetPosition()) + coord.Coords);

            //LogMessage("new coords: " + coord);

            remc.ClearWaypoints();
            remc.AddWaypoint(coord);

            remc.SetAutoPilotEnabled(true);
            //LogMessage("Autopilot enabled.");
        }
    }
}
