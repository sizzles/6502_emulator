using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class CLC_Instruction : Instruction
    {
        public CLC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Clears carry flag";

        public override void Execute(Cpu cpu)
        {
            var _ = cpu.Fetch(this.addressingMode);
            cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();

        }
    }
}
