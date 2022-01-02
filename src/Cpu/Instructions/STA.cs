using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class STA_Instruction : Instruction
    {
        public STA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "transfers the contents of the accumulator to memory.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            cpu.addressBus.WriteByte(cpu.A, fr.address);

            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
