using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InstructionRegisterAttribute : Attribute
    {
    }
}
