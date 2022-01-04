using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class SBC_Instruction : Instruction
    {
        public SBC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "subtracts the value of memory and borrow from the value of the accumulator";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);


            //Subtracts memory and borrow from the value of the accumulator
            // instead of 35 - 7 do 35 + -7

            //twos complement - find ones complement and add one
            //ones complement = invert all the bits = dor an XOR

            // 00110100 ^ 11111111 = 11001001
            // 11001011 

            //uint inverted = (uint)(~fr.operand + 1);

            byte carry = Convert.ToByte(cpu.GetProcessorStatusFlag(StatusFlagName.Carry));

            byte inverted = (byte)~fr.operand;

            uint result16 = (uint)(cpu.A + inverted + ~carry);

            //first 8 bits are the result
            byte accumulatorResult = (byte)result16;
            cpu.A = accumulatorResult;

            sbyte resultSigned = (sbyte)(accumulatorResult);

            //Carry flag
            if (cpu.A >= 0)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
            }

            //over flow flag
            if (resultSigned > 127 || resultSigned < -127)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Overflow);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Overflow);
            }

            //Set zero flag
            cpu.SetZeroFlagIfRequired(cpu.A);

            //negative flag
            cpu.SetNegativeFlagIfRequired(cpu.A);

            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }
}
