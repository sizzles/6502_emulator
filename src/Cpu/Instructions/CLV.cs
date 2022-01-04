using Cpu.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu
{
    public class CLV_Instruction : Instruction
    {
        public CLV_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "sets the overflow flag to a 0";

        public override void Execute(Cpu cpu)
        {
            var _ = cpu.Fetch(this.addressingMode);
            cpu.SetProcessorStatusFlag(false, StatusFlagName.Overflow);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
