using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class SEI_Instruction : Instruction
    {
        public SEI_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => " initializes the interrupt disable flag to a 1";

        public override void Execute(Cpu cpu)
        {
            cpu.SetProcessorStatusFlag(true, StatusFlagName.IRQDisable);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
