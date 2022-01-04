using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class CPX_Instruction : Instruction
    {
        public CPX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "subtracts the value of the addressed memory location from the content of index register X"; 

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            var result = (byte)(cpu.X - fr.operand);

            if (cpu.X >= fr.operand)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
            }

            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }
}
