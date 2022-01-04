using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class PHA_Instruction : Instruction
    {
        public PHA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfers current accumulator value to the stack.";
        public override void Execute(Cpu cpu)
        {
            //cpu.IncrementPC(); //fetch opcode and discard
            cpu.PushStack(cpu.A);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
