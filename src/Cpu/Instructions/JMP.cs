using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class JMP_Instruction : Instruction
    {
        public JMP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "JMP set program counter to absolute address.";

        public override void Execute(Cpu cpu)
        {
            //Increment PC
            cpu.IncrementPC();

            if (this.addressingMode == AddressingMode.ABS)
            {
                ushort jmpToAddress = cpu.Read16();
                cpu.SetPC(jmpToAddress);
                cpu.SetTimingControl(machineCycles);
            }
            else if (this.addressingMode == AddressingMode.Indirect)
            {
                ushort jmpToAddress = cpu.Read16();
                byte lsb = cpu.addressBus.ReadByte(jmpToAddress);
                byte hsb = cpu.addressBus.ReadByte((ushort)(jmpToAddress + 1));
                ushort indJmpToAddress = (ushort)((hsb << 8) + lsb); //Indirect address
                cpu.SetPC(indJmpToAddress);
                cpu.SetTimingControl(machineCycles);
            }
        }
    }
}
//    [InstructionRegister]
//    public class JMP_ABS_Instruction : Instruction
//    {


//        public JMP_ABS_Instruction(Cpu cpu) : base(cpu)
//        {
//            this.mnemonic = "JMP";
//            this.hexCode = 0x4C;
//            this.addressingMode = AddressingMode.ABS;
//            this.instructionBytes = 3;
//            this.machineCycles = 3;
//        }

//        public override string Description => "JMP set program counter to absolute address.";

//        public override void Execute(Cpu cpu)
//        {
//            //Increment PC
//            cpu.IncrementPC();

//            ushort jmpToAddress = cpu.Read16();
//            //set the program counter to the jmpAddress
//            cpu.SetPC(jmpToAddress);
//            cpu.SetTimingControl(machineCycles);

//        }
//    }
//}
