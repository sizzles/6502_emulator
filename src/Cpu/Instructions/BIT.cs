using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BIT_Instruction : Instruction
    {
        public BIT_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => " performs an AND between a memory location and the accumulator but does not store the result of the AND into the accumulator.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            byte result = (byte)(fr.operand & cpu.A);

            cpu.SetNegativeFlagIfRequired(result);

            //Overflow set to bit 6 of memory being tested
            bool overflowResult = Convert.ToBoolean((byte)((fr.operand & 0b01000000) >> 6));
            cpu.SetProcessorStatusFlag(overflowResult, StatusFlagName.Overflow);

            cpu.SetZeroFlagIfRequired(result);

            cpu.SetTimingControl(this.machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }
}
