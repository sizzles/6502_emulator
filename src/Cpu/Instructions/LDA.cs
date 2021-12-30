using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cpu.Instructions;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class LDA_INDX_Instruction : Instruction
    {
        public LDA_INDX_Instruction(Cpu cpu) : base(cpu)
        {
            this.mnemonic = "LDX";
            this.hexCode = 0xA1;
            this.addressingMode = AddressingMode.INDX;
            this.instructionBytes = 2;
            this.machineCycles = 6;
        }

        public override string Description => "Loads Into Accumulator Indexed Indirect X";
        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC();
            //Operand added to the x register
            //result is 0 page address = lsb
            byte operand = cpu.addressBus.ReadByte(cpu.PC); //At the program counter

            unchecked
            {
                operand += cpu.X; //Add the X register address
            }

            //Fetch target address - must be in the zero page!
            ushort targetAddress = (ushort)(0 << 8 + operand);

            byte result = cpu.addressBus.ReadByte(targetAddress);
            cpu.A = result; //set the Accumulator register

            //Flags, Timing Control, Program Counter
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
