using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{

    public class LDY_Instruction : Instruction
    {
        public LDY_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, int instructionBytes, int machineCycles) : base(cpu)
        {
            this.mnemonic = mnemonic;
            this.hexCode = hexCode;
            this.addressingMode = addressingMode;
            this.instructionBytes = instructionBytes;
            this.machineCycles = machineCycles;
        }
        public override void Execute(Cpu cpu)
        {
            //fetch the operand
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte operand = fr.operand;

            cpu.Y = operand;
            //Flags, Timing Control, Program Counter
            cpu.SetZeroFlagIfRequired(cpu.Y);
            cpu.SetNegativeFlagIfRequired(cpu.Y);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }
}
