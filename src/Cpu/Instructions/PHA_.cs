using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class PHA_Implied_Instruction : Instruction
    {
        public PHA_Implied_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
            this.mnemonic = "PHA";
            this.hexCode = 0x48;
            this.addressingMode = AddressingMode.Implied;
            this.instructionBytes = 1;
            this.machineCycles = 3;
        }

        public override string Description => "Transfers current accumulator value to the stack."
        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC(); //fetch opcode and discard
            cpu.PushStack(cpu.A);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
