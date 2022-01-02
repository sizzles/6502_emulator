using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class LDX_Instruction : Instruction
    {
        public LDX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }
        public override void Execute(Cpu cpu)
        {
            //fetch the operand
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte operand = fr.operand;

            cpu.X = operand;
            //Flags, Timing Control, Program Counter
            cpu.SetZeroFlagIfRequired(cpu.X);
            cpu.SetNegativeFlagIfRequired(cpu.X);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }

}