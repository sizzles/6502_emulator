namespace Cpu.Instructions
{
    //still faster to look up into a dictionary though I guess really

    //eg. Opcode 11 = 0xA1 in hex

    public enum AddressingMode
    {
        //13 Modes of addressing available
        Accum, //Accumulator
        IMM, //Immediate
        ABS, //Absolute
        ZP, //Zero Page
        ZPX, //Indexed Zero Page X
        ZPY, //Indexed Zero Page Y
        ABSX, //Indexed Absolute X
        ABSY, //Indexed Absolute Y
        Implied, //Implied
        Relative, //Relaitve
        INDX, //Indexed Indirect X
        INDY, //Indexed Indirect Y
        Indirect, //Absolute Indirect
    }

    public abstract class Instruction
    {
        public string mnemonic = String.Empty;
        public byte hexCode;
        public AddressingMode addressingMode;
        public int instructionBytes;
        public int machineCycles;
        public Cpu cpu;

        public Instruction(Cpu cpu) {
            this.cpu = cpu;
        }

        public Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles)
        {
            this.cpu = cpu;
            this.mnemonic = mnemonic;
            this.hexCode = hexCode;
            this.addressingMode = addressingMode;
            this.instructionBytes = instructionBytes;
            this.machineCycles = machineCycles;
        }

        public virtual string Description => "Not Implemented";
        public abstract void Execute(Cpu cpu);

        public virtual void HandleZeroFlag(byte result)
        {
            //Set zero flag
            if (result == 0)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Zero);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Zero);
            }
        }

        public virtual void HandleNegativeFlag(byte result)
        {
            //Set by result bit 7 --to do check this vs signed logic??
            bool isNegative = Convert.ToBoolean((result & 0b10000000) >> 7);
            cpu.SetProcessorStatusFlag(isNegative, StatusFlagName.Negative);
        }
    }
}
