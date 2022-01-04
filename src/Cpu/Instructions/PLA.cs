using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{

    public class PLA_Instruction : Instruction
    {
        public PLA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Adds 1 to the current value of the stack pointer and uses it to address the stack and loads the contents of the stack into the A register";

        public override void Execute(Cpu cpu)
        { 
            //cpu.IncrementPC(); //fetch next op code and discard

            byte result = cpu.PopStack();

            cpu.A = result;

            //Set Zero flag if needed
            if (cpu.A == 0)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Zero);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Zero);
            }

            //Set Negative flag if needed
            if (cpu.IsByteNegative(result))
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Negative);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Negative);
            }

            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC(); //net opcode fetched and discarded

        }
    }
}
