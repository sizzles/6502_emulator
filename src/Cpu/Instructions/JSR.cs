using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [InstructionRegister]
    public class JSR_ABS_Instruction : Instruction
    {
        public JSR_ABS_Instruction(Cpu cpu) : base(cpu)
        {
            this.mnemonic = "JSR";
            this.hexCode = 0x02;
            this.addressingMode = AddressingMode.ABS;
            this.instructionBytes = 3;
            this.machineCycles = 6;
        }

        public override string Description => "Jump to sub routine.";

        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC();
            byte adl = cpu.addressBus.ReadByte(cpu.PC);
            cpu.IncrementPC();
            byte adh = cpu.addressBus.ReadByte(cpu.PC);
            cpu.IncrementPC();

            cpu.PushStack(adh);
            cpu.PushStack(adl);

            ushort pc = cpu.Read16();
            cpu.SetPC(pc);

            cpu.SetTimingControl(machineCycles);
        }
    }
}
