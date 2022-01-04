using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class EOR_Instruction : Instruction
    {
        public EOR_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => " transfers the memory and the accumulator to the adder which performs a binary EXCLUSIVE OR";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            byte result = (byte)(fr.operand ^ cpu.A);
            cpu.A = result;

            cpu.SetZeroFlagIfRequired(cpu.A);
            cpu.SetNegativeFlagIfRequired(cpu.A);

            cpu.SetTimingControl(this.machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }
}
