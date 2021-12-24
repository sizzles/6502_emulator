using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class JMP_ABS_Instruction : Instruction
    {
        public JMP_ABS_Instruction() : base()
        {
            this.mnemonic = "JMP";
            this.hexCode = 0x4C;
            this.addressingMode = AddressingMode.ABS;
            this.instructionBytes = 3;
            this.machineCycles = 3;
        }

        public override string Description => "JMP set program counter to absolute address.";

        public override void Execute(Cpu cpu)
        {
            //Increment PC
            cpu.IncrementPC();

            ushort jmpToAddress = cpu.Read16();
            //set the program counter to the jmpAddress
            cpu.SetPC(jmpToAddress);
            cpu.SetTimingControl(machineCycles);

        }
    }
}
