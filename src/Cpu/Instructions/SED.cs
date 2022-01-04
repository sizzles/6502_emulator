using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class SED_Instruction : Instruction
    {
        public SED_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "sets the decimal mode flag D to a 1";

        public override void Execute(Cpu cpu)
        {
            cpu.SetProcessorStatusFlag(true, StatusFlagName.DecimalMode);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
