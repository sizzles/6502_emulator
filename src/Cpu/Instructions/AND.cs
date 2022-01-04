using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class AND_Instruction : Instruction
    {
        public AND_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer the accumulator and memory to the adder which performs a bit-by-bit AND operation and stores result back in accumulator";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte result = (byte)(cpu.A & fr.operand);
            cpu.A = result;
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }
}