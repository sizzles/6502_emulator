using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class TAX_Instruction : Instruction
    {
        public TAX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer Accumulator To Index X";

        public override void Execute(Cpu cpu)
        {
            FetchResult _ = cpu.Fetch(this.addressingMode);
            cpu.X = cpu.A;
            cpu.SetNegativeFlagIfRequired(cpu.X);
            cpu.SetZeroFlagIfRequired(cpu.X);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
