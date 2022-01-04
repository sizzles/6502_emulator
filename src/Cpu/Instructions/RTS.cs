using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class RTS : Instruction
    {
        public RTS(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }
        public override string Description => "loads the program count low and program count high from the stack into the program counter and increments the program counter so that it points to the instruction following the JSR. The stack pointer is adjusted by incrementing it twice.";

        public override void Execute(Cpu cpu)
        {
            byte pcl = cpu.PopStack();
            byte pch = cpu.PopStack();

            ushort pc = (ushort)((pch << 8) + pcl);
            cpu.SetPC(pc);
            cpu.IncrementPC();

            cpu.SetTimingControl(machineCycles);            
        }
    }
}