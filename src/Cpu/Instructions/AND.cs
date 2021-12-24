using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class AND_ZP_Instruction : Instruction
    {
        public AND_ZP_Instruction() : base()
        {
            this.mnemonic = "AND";
            this.hexCode = 0x25;
            this.addressingMode = AddressingMode.ZP; //Zero Page Mode
            this.instructionBytes = 2;
            this.machineCycles = 3;
        }

        public override string Description => "Transfer the accumulator and memory to the adder which performs a bit-by-bit AND operation and stores result back in accumulator. Zero Page.";

        public override void Execute(Cpu cpu)
        {
            //increment the PC - read the page
            cpu.IncrementPC();
            byte operand = cpu.addressBus.ReadByte(cpu.PC);

            //do a bitwise and with what is in the Accumulator
            byte result = (byte)(cpu.A & operand);

            //store this on the accumulator
            cpu.A = result;

            cpu.IncrementPC();
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles);
        }
    }
}