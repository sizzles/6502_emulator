using NUnit.Framework;
using Cpu;
using System;

namespace NesEmTests
{
    public class Tests
    {
        Cpu.Cpu cpu;

        [SetUp]
        public void Setup()
        {
            cpu = new Cpu.Cpu();

            //0xFFFCwhat is used in the reset as starting point for the program counter
            cpu.addressBus.WriteByte(0X00, 0xFFFC);  //low byte 
            cpu.addressBus.WriteByte(0x08, 0xFFFD); //high byte

            cpu.Reset();
        }

        [Test]
        public void ShouldRead16()
        {
            cpu.addressBus.WriteByte(0x10, 0x0001);
            cpu.addressBus.WriteByte(0x12, 0x0002);
            cpu.SetPC(0x0001);

            ushort result = cpu.Read16();

            Assert.AreEqual(0x1210, result);
            Assert.AreEqual(0x0002, cpu.PC);
        }

        [Test]
        public void ShouldSetFlags()
        {
            //P is initialised to 0x34
            cpu.SetProcessorStatusFlag(true, StatusFlagName.Negative);
            Assert.AreEqual(0b10110100, cpu.P);
            cpu.SetProcessorStatusFlag(false, StatusFlagName.Negative);
            Assert.AreEqual(0b00110100, cpu.P);
        }

        [Test]
        public void ShouldExecute_ADC_ZP_Instruction()
        {
            void TestValues(byte accumulator, byte val, bool carry, byte accumulatorResult, bool carryFlagResult)
            {
                cpu.A = accumulator; //Set zero manually in the accumulator

                cpu.addressBus.WriteByte(0x65, 1024); //PC instruction
                cpu.addressBus.WriteByte(64, 1025); //what we want to look up at the zero page
                cpu.addressBus.WriteByte(val, 64); //put 10 into position 0 on the zero page

                cpu.SetProcessorStatusFlag(carry, StatusFlagName.Carry);

                cpu.SetPC(1024);

                cpu.Tick(); //tick the cpu 

                //result in the accumulator should be 10
                Assert.AreEqual(accumulatorResult, cpu.A);
                Assert.AreEqual(carryFlagResult, cpu.GetProcessorStatusFlag(StatusFlagName.Carry));
                Assert.AreEqual(1026, cpu.PC); //Program counter has been incremented
            }

            //test this progrma is actually correct

            //Set program counter to 1024 - with the value

            TestValues(0, 10, false, 10, false);
            TestValues(11, 10, false, 21,false);
            TestValues(11, 10, true, 22, false);
            TestValues(1, 255, false, 0, true);
            TestValues(255, 255, true, 255, true);

        }

        [Test]
        public void ShouldDoSubtraction()
        {
            //test 100 - 107
            string testAsm = "A9 64 18 E9 6B 8D 00 02 EA";

            ushort startAdr = cpu.PC; //starting point

            foreach (string hexCode in testAsm.Split(" "))
            {
                var v = Convert.ToByte($"0x{hexCode}", 16);
                cpu.addressBus.WriteByte(v, startAdr);
                startAdr += 1;
            }

            //then start the program

            try
            {
                for (int i = 0; i <= 30; i++)
                {

                    cpu.Tick();
                }
            }

            catch (Exception e)
            {
                //Doesnt really matter as just want to test the memory really
            }

            Assert.AreEqual(248, cpu.addressBus.ReadByte(0x0200));
        }

        [Test]
        public void ShouldDoMultiply()
        {

            string testAsm = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA EA EA";
            //run 

            //load that into memory

            ushort startAdr = cpu.PC; //starting point

            foreach(string hexCode in testAsm.Split(" "))
            {
                var v = Convert.ToByte($"0x{hexCode}", 16);
                cpu.addressBus.WriteByte(v, startAdr);
                startAdr += 1;
            }

            //then start the program

            try
            {
                for (int i = 0; i <= 100; i++)
                {

                    cpu.Tick();
                }
            }

            catch(Exception e)
            {
                //Doesnt really matter as just want to test the memory really
            }

            Assert.AreEqual(30, Convert.ToInt32(cpu.addressBus.ReadByte(0x0002)));

            //Multiplies 10 x 3 - using addition as 6502 doesnt support it otherwise
            // Load Program (assembled at https://www.masswerk.at/6502/assembler.html)
            /*
                *=$8000
                LDX #10
                STX $0000
                LDX #3
                STX $0001
                LDY $0000
                LDA #0
                CLC
                loop
                ADC $0001
                DEY
                BNE loop
                STA $0002
                NOP
                NOP
                NOP
            */


        }
    }
}