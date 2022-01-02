using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class ASL_Instruction : Instruction
    {
        public override string Description => "shifts either the accumulator or the address memory location 1 bit to the left,";

        public ASL_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            byte operand = fr.operand;

            byte result = (byte)(operand << 1);

            cpu.SetNegativeFlagIfRequired(result);

            cpu.SetZeroFlagIfRequired(result);

            bool carryFlag = Convert.ToBoolean(operand << 1 >> 7);
            cpu.SetProcessorStatusFlag(carryFlag, StatusFlagName.Carry);

            if (this.addressingMode == AddressingMode.Accum)
            {
                cpu.A = result;
            }
            else
            {
                cpu.addressBus.WriteByte(result, fr.address);
            }
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC(); 
        }
    }
}
