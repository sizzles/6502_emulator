using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class BRK_Implied_Instruction : Instruction
    {
        public BRK_Implied_Instruction(Cpu cpu) : base(cpu)
        {
            this.mnemonic = "BRK";
            this.hexCode = 0x00;
            this.addressingMode = AddressingMode.Implied;
            this.instructionBytes = 1;
            this.machineCycles = 7;
        }

        //Todo - Finish this - interupt vectors etc
        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC();
            cpu.IncrementPC(); //jump twice

            //store the program counter onto the stack
            //break up into little endian way of doing it I guess

            byte pcl = (byte)(cpu.PC << 8 >> 8);
            byte pch = (byte)(cpu.PC >> 8);

            cpu.PushStack(pch);
            cpu.PushStack(pcl);

            cpu.PushStack(cpu.P);
            cpu.SetProcessorStatusFlag(true, StatusFlagName.Brk); //Set break flag true and push processor status to the stack

            cpu.SetTimingControl(machineCycles);

        }

        public override string Description => "The break command causes the microprocessor to go through an interrupt sequence under program control.";
    }
}