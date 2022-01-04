using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class ROR_Instruction : Instruction
    {
        public ROR_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Shifts either the accumulator or addressed memory right 1 bit.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte operand = fr.operand;
            
            //Old carry goes to bit 7
            byte oldCarry = Convert.ToByte(cpu.GetProcessorStatusFlag(StatusFlagName.Carry));
            //Original bit 0 into the carry
            bool newCarry = Convert.ToBoolean((byte)(operand & 0b00000001));
            cpu.SetProcessorStatusFlag(newCarry, StatusFlagName.Carry);

            byte result = (byte)((oldCarry << 7) + (byte)(operand >> 1));

            cpu.SetZeroFlagIfRequired(result);
            cpu.SetProcessorStatusFlag(Convert.ToBoolean(oldCarry), StatusFlagName.Negative);


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
