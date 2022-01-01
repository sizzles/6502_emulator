using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class PLP_Instruction : Instruction
    {
        public PLP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        //public PLP_Implied_Instruction(Cpu cpu):base(cpu)
        //{
        //    this.mnemonic = "PLP";
        //    this.hexCode = 0x28;
        //    this.addressingMode = AddressingMode.Implied;
        //    this.instructionBytes = 1;
        //    this.machineCycles = 4;
        //}

        public override string Description => "Transfers the next value on the stack to the Processor Status register.";

        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC(); //fetch opcode and discard
            byte stackValue = cpu.PopStack();
            cpu.P = stackValue;
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}