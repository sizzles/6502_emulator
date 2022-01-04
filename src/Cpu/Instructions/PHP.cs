using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class PHP_Instruction : Instruction
    {
        public PHP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => base.Description;

        public override void Execute(Cpu cpu)
        {
            //cpu.IncrementPC(); //fetch next opode and discard?
            cpu.PushStack(cpu.P);
            cpu.SetTimingControl(this.machineCycles);
            cpu.IncrementPC();

        }
    }
}
