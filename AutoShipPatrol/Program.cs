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
        readonly int MAX_SPEED = 50;
        readonly bool TRADER_MODE = false;
        readonly int DELAY_BETWEEN_RUNS_SECONDS = 0;
        readonly Base6Directions.Direction GO_DIRECTION = Base6Directions.Direction.Forward;


        List<IMyBatteryBlock> batts = new List<IMyBatteryBlock>();
        List<IMyGasTank> gasTanks = new List<IMyGasTank>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        List<IMyDoor> doors = new List<IMyDoor>();
        IMyShipConnector conn;
        IMyRemoteControl remc;
        IMyRadioAntenna ant;

        List<MyWaypointInfo> waypoints = new List<MyWaypointInfo>();
        int _direction = 0;
        int _current = 0;
        bool _isStopped = true;
        DateTime landed = DateTime.MaxValue;
        bool addMode = false;
        private Logger _logger;
        private Helper _helper;

        public Program()
        {
            Echo("<==AutoShipPatrol==>");

            _helper = new Helper(this);
            _logger = new Logger(this);

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Init();

            string[] positions = Me.CustomData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string pos in positions)
            {
                MyWaypointInfo wp;
                if (MyWaypointInfo.TryParse(pos, out wp))
                    waypoints.Add(wp);
                else
                    _logger.LogMessage($"Failed to parse wp");
            }
            _logger.LogMessage($"{waypoints.Count} wp loaded");
        }

        void Init()
        {
            batts = _helper.GetBlocks<IMyBatteryBlock>();
            gasTanks = _helper.GetBlocks<IMyGasTank>();
            thrusters = _helper.GetBlocks<IMyThrust>();
            doors = _helper.GetBlocks<IMyDoor>();
            conn = _helper.GetBlock<IMyShipConnector>();
            remc = _helper.GetBlock<IMyRemoteControl>();
            ant = _helper.GetBlock<IMyRadioAntenna>();

            var s = Me.GetSurface(0);
            s.FontSize = 2;
            s.ContentType = ContentType.TEXT_AND_IMAGE;
            s = Me.GetSurface(1);
            s.ContentType = ContentType.TEXT_AND_IMAGE;

            _logger.Clear();
            _logger.LogMessage("Running...");

        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!String.IsNullOrWhiteSpace(argument))
                switch (argument.ToLower())
                {
                    case "stop":
                        _isStopped = true;
                        remc.SetAutoPilotEnabled(false);
                        _direction = 0;
                        SwitchFlightSystems(true);
                        _logger.LogMessage("Stopped");
                        return;
                    case "go":
                        _isStopped = false;
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        Go();
                        return;
                    case "dock":
                        _isStopped = true;
                        doors?.ForEach(x => x.OpenDoor());
                        addMode = false;
                        conn.Connect();
                        SwitchFlightSystems(!(conn.Status == MyShipConnectorStatus.Connected));
                        return;
                    case "undock":
                        _isStopped = true;
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
                        _isStopped = true;
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        RunDock1();
                        return;
                    case "dock2":
                        _isStopped = true;
                        doors?.ForEach(x => x.CloseDoor());
                        addMode = false;
                        RunDock2();
                        return;
                    default:
                        _logger.LogMessage("Unknown arg");
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
                    _logger.LogMessage("End");
                    return;
                }
                _logger.LogMessage($"GO: {waypoints[_current].Name}");
                SetupRemoteControl(waypoints[_current], _current == 0 || _current == (waypoints.Count - 1) ? 2 : MAX_SPEED);
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

            if (!_isStopped && DELAY_BETWEEN_RUNS_SECONDS != 0 && (DateTime.UtcNow - landed).TotalSeconds > DELAY_BETWEEN_RUNS_SECONDS)
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
            SetupRemoteControl(waypoints[_current], 15, GO_DIRECTION);
        }

        private void SwitchFlightSystems(bool enabled)
        {
            batts.ForEach(b => b.ChargeMode = enabled ? ChargeMode.Auto : ChargeMode.Recharge);
            gasTanks.ForEach(g => g.Enabled = enabled);
            thrusters.ForEach(t => t.Enabled = enabled);
            if (ant != null)
                ant.Enabled = enabled;
        }

        private void AddGPSPosition()
        {
            string[] positions = Me.CustomData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            MyWaypointInfo position = new MyWaypointInfo(positions.Length.ToString(), remc.GetPosition());
            Me.CustomData += position.ToString() + "\r\n";
            _logger.LogMessage($"WP added: {positions.Length}");
        }

        void SetupRemoteControl(MyWaypointInfo coord, float speedLimit = 15, Base6Directions.Direction direction = Base6Directions.Direction.Forward)
        {
            remc.FlightMode = FlightMode.OneWay;
            remc.SpeedLimit = speedLimit;

            //remc.SetDockingMode(true);//todo: test it//precize mod?

            remc.Direction = direction;
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
