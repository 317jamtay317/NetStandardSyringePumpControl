﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SyringePumpControlCore.Models;
using SyringePumpControlCore.Models.Commands;
using SyringePumpControlCore.Services;

namespace SyringePumpControlNetStandard.Services
{
    public class PumpService:IPumpService, IDisposable
    {
        private readonly PumpChain _pumps;
        private readonly ISyringePumpPort _syringePumpPort;

        public PumpService(GetPumps getPumps)
        {
            _pumps = getPumps();
            _syringePumpPort = new SyringePumpPort();
            BuzzDelay = 50;
        }

        public int BuzzDelay { get; set; }
        
        public IEnumerable<string> PortNames => _syringePumpPort.GetPortNames();
        
        public void Start(int pumpAddress, float rate)
        {
            BuzzPump(pumpAddress);
            var pump = GetPump(pumpAddress);
            pump.Rate = rate;

            var startRequest = new StartCommand(pumpAddress);
            pump.SetCurrentValues();
            pump.Send(startRequest);
        }

        public void Start(int pumpAddress)
        {
            var pump = GetPump(pumpAddress);
            Start(pumpAddress, pump.Rate);
        }

        public void Stop(int pumpAddress)
        {
            var pump = GetPump(pumpAddress);
            var stopRequest = new StopCommand(pumpAddress);
            pump.Send(stopRequest);
        }

        public IPump GetPump(int pumpAddress)
        {
            if (_pumps.ContainsPump(pumpAddress) == false)
            {
                throw new ArgumentException($"The Pump:{pumpAddress} doesnt exist in the chain");
            }
            return _pumps.GetPump(pumpAddress);
        }

        public void BuzzPump(int pumpAddress)
        {
            var request = new BuzzCommand(pumpAddress, Switch.On);
            SendCommand(request, pumpAddress);

            Task.Delay(BuzzDelay);
            request = new BuzzCommand(pumpAddress, Switch.Off);
            SendCommand(request, pumpAddress);

          }

        public void UpdateSinglePumpValues(int pumpAddress, PumpValues values)
        {
            var pump = GetPump(pumpAddress);
            pump.UpdateValues(values);
        }

        private void TimerCallback(IPumpCommand pumpCommand)
        {
            SendCommand(pumpCommand, pumpCommand.Address);
        }

        private void SendCommand(IPumpCommand pumpCommand, int address)
        {
            var pump = GetPump(address);
            pump.Send(pumpCommand);
        }

        public void Dispose()
        {
            _syringePumpPort?.Dispose();
        }
    }

    public delegate PumpChain GetPumps();

}