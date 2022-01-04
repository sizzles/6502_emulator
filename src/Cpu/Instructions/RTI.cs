using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class RTI_Instruction : Instruction
    {
        public RTI_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => base.Description;

        public override void Execute(Cpu cpu)
        {
            byte p = cpu.PopStack();
            cpu.P = p;

            byte pcl = cpu.PopStack();
            byte pch = cpu.PopStack();

            ushort pc = (ushort)((pch << 8) + pcl);
            cpu.SetPC(pc);

            cpu.SetTimingControl(machineCycles);
        }
    }
}