using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class TAY_Instruction : Instruction
    {
        public TAY_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer Accumulator To Index Y";

        public override void Execute(Cpu cpu)
        {
            FetchResult _ = cpu.Fetch(this.addressingMode);
            cpu.Y = cpu.A;
            cpu.SetNegativeFlagIfRequired(cpu.Y);
            cpu.SetZeroFlagIfRequired(cpu.Y);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}