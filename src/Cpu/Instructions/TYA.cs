using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class TYA_Instruction : Instruction
    {
        public TYA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer Y Register to Accumulator.";

        public override void Execute(Cpu cpu)
        {
            FetchResult _ = cpu.Fetch(this.addressingMode);
            cpu.A = cpu.Y;
            cpu.SetNegativeFlagIfRequired(cpu.A);
            cpu.SetZeroFlagIfRequired(cpu.A);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
