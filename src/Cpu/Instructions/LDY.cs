using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class LDY_IMM_Instruction : Instruction
    {
        public LDY_IMM_Instruction() : base()
        {
            this.mnemonic = "LDY";
            this.hexCode = 0xA0;
            this.addressingMode = AddressingMode.IMM;
            this.instructionBytes = 2;
            this.machineCycles = 2;
        }

        public override string Description => "Loads the immediate next byte in memory into the Y register.";

        //Need to pass the CPU so we can actually execute on the registers adn the ram
        public override void Execute(Cpu cpu)
        {
            //Increment the program counter
            cpu.IncrementPC();
            byte operand = cpu.addressBus.ReadByte(cpu.PC); //Read the operand
            //Load this value into the Y register
            cpu.Y = operand;
            cpu.SetZeroFlagIfRequired(operand);
            cpu.SetNegativeFlagIfRequired(operand);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
