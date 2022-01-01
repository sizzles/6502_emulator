using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{

    public class LDY_Instruction : Instruction
    {
        public LDY_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, int instructionBytes, int machineCycles) : base(cpu)
        {
            this.mnemonic = mnemonic;
            this.hexCode = hexCode;
            this.addressingMode = addressingMode;
            this.instructionBytes = instructionBytes;
            this.machineCycles = machineCycles;
        }
        public override void Execute(Cpu cpu)
        {
            //fetch the operand
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte operand = fr.operand;

            cpu.Y = operand;
            //Flags, Timing Control, Program Counter
            cpu.SetZeroFlagIfRequired(cpu.Y);
            cpu.SetNegativeFlagIfRequired(cpu.Y);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }



    //[InstructionRegister]
    //public class LDY_IMM_Instruction : Instruction
    //{
    //    public LDY_IMM_Instruction(Cpu cpu) : base(cpu)
    //    {
    //        this.mnemonic = "LDY";
    //        this.hexCode = 0xA0;
    //        this.addressingMode = AddressingMode.IMM;
    //        this.instructionBytes = 2;
    //        this.machineCycles = 2;
    //    }

    //    public override string Description => "Loads the immediate next byte in memory into the Y register.";

    //    //Need to pass the CPU so we can actually execute on the registers adn the ram
    //    public override void Execute(Cpu cpu)
    //    {
    //        //Increment the program counter
    //        cpu.IncrementPC();
    //        byte operand = cpu.addressBus.ReadByte(cpu.PC); //Read the operand
    //        //Load this value into the Y register
    //        cpu.Y = operand;
    //        HandleZeroFlag(cpu.Y);
    //        HandleNegativeFlag(cpu.Y);
    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();
    //    }
    //}

    //[InstructionRegister]
    //public class LDY_ZP_Instruction : Instruction { 
    //    public LDY_ZP_Instruction(Cpu cpu) :base(cpu)
    //    {
    //        this.mnemonic = "LDY";
    //        this.hexCode = 0xA4;
    //        this.addressingMode = AddressingMode.ZP;
    //        this.instructionBytes = 2;
    //        this.machineCycles = 3;
    //    }

    //    public override string Description => "Loads byte in memory from zero page into the Y register.";

    //    public override void Execute(Cpu cpu)
    //    {
    //        cpu.IncrementPC();
    //        byte lb = cpu.addressBus.ReadByte(cpu.PC); //Read the operand
    //        byte m = cpu.addressBus.ReadByte((ushort)(lb));

    //        cpu.Y = m;

    //        HandleZeroFlag(cpu.Y);
    //        HandleNegativeFlag(cpu.Y);
    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();
    //    }
    //}

    //[InstructionRegister]
    //public class LDY_ABS_Instruction : Instruction
    //{
    //    public LDY_ABS_Instruction(Cpu cpu) : base(cpu)
    //    {
    //        this.mnemonic = "LDY";
    //        this.hexCode = 0xAC;
    //        this.addressingMode = AddressingMode.ABS;
    //        this.instructionBytes = 3;
    //        this.machineCycles = 4;
    //    }

    //    public override void Execute(Cpu cpu)
    //    {
    //        cpu.IncrementPC();
    //        ushort address = cpu.Read16();
    //        byte m = cpu.addressBus.ReadByte(address);

    //        cpu.Y = m;

    //        HandleZeroFlag(cpu.Y);
    //        HandleNegativeFlag(cpu.Y);
    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();
    //    }
    //}

}
