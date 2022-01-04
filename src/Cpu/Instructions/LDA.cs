using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cpu.Instructions;

namespace Cpu.Instructions
{
    
    public class LDA_Instruction : Instruction
    {
        public LDA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, int instructionBytes, int machineCycles):base(cpu)
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

            cpu.A = operand;
            //Flags, Timing Control, Program Counter
            cpu.SetZeroFlagIfRequired(cpu.A);
            cpu.SetNegativeFlagIfRequired(cpu.A);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }
}
