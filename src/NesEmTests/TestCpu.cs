using NUnit.Framework;
using Cpu;

namespace NesEmTests
{
    public class Tests
    {
        Cpu.Cpu cpu;

        [SetUp]
        public void Setup()
        {
            cpu = new Cpu.Cpu();
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
    }
}