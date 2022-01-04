using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class DEC_Instruction : Instruction
    {
        public DEC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Decrements the value in the specified byte in memory by one.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            byte result = 0;

            unchecked
            {
                result = (byte)(fr.operand - (byte)1);
            }

            cpu.addressBus.WriteByte(result, fr.address);
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }
}