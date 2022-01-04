using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class SEC_Instruction : Instruction
    {
        public SEC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => " initializes the carry flag to a 1";

        public override void Execute(Cpu cpu)
        {
            cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
