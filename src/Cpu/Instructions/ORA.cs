using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister] 
    public class ORA_ABSY : Instruction
    {
        public ORA_ABSY():base()
        {
            this.mnemonic = "ORA";
            this.hexCode = 0x19;
            this.addressingMode = AddressingMode.ABSY;
            this.instructionBytes = 3;
            this.machineCycles = 4; // + 1 to N if page boundary is crossed
        }

        public override string Description => "Transfers the memory and the accumulator to the adder which performs a binary 'OR' on a bit-by-bit basis and stores the result in the accumulator.";
        public override void Execute(Cpu cpu)
        {
            //binary or on memory and accumulator - store in the accumulator
            //Add y register to 2nd and 3rd bytes of the instruction
            //then read that for the memory and compare with the accumulator
            //How many extra pages are there after that?

            cpu.IncrementPC(); //now on first byte of operand



        }
    }
}
