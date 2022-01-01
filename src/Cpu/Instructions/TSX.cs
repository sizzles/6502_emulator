using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class TSX_Instruction : Instruction
    {
        public TSX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer Stack Pointer to X Register.";

        public override void Execute(Cpu cpu)
        {
            FetchResult _ = cpu.Fetch(this.addressingMode);
            byte sp = cpu.PopStack();
            cpu.X = sp;
            cpu.SetNegativeFlagIfRequired(cpu.X);
            cpu.SetZeroFlagIfRequired(cpu.X);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
