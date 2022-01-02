using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class STX_Instruction : Instruction
    {
        public STX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfers value of X register to addressed memory location.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            cpu.addressBus.WriteByte(cpu.X, fr.address);

            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
