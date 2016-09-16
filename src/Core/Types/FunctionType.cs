#region License
/* 
 * Copyright (C) 1999-2016 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Reko.Core.Expressions;
using Reko.Core.Output;
using Reko.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reko.Core.Types
{
    /// <summary>
    /// Models a function type, including summarizing the effects of calling
    /// a procedure, as seen by the caller.
    /// </summary>
    /// <remarks>
    /// Calling a procedure affects a few things: the registers, the stack 
    /// depth, and in the case of the Intel x86 architecture the FPU stack 
    /// depth. These effects are summarized by the signature.
    /// <para>
    /// $TODO: There are CPU-specific items (like x86 FPU stack gunk). Move
    /// these into processor-specific subclasses. Also, some architectures 
    /// -- like the FORTH language -- have multiple stacks.
    /// </para>
    /// </remarks>
    public class FunctionType : DataType
	{
        public FunctionType()
        {
            this.ParametersValid = false;
            this.FpuStackArgumentMax = -1;
        }

        public FunctionType(
            string name,        //$REVIEW: what's the use? function types have no names
            Identifier returnValue,
            params Identifier [] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            this.ParametersValid = true;
            this.FpuStackArgumentMax = -1;
            if (returnValue == null)
                returnValue = new Identifier("", VoidType.Instance, null);
            this.ReturnValue = returnValue;
            this.Parameters = parameters;
        }

        public Identifier ReturnValue { get; private set; }
        public Identifier [] Parameters { get; private set; }
        public bool HasVoidReturn { get { return ReturnValue == null || ReturnValue.DataType is VoidType; } }
        public TypeVariable TypeVariable { get; set; }  //$REVIEW: belongs on the Procedure itself!

        public override void Accept(IDataTypeVisitor v)
        {
            v.VisitFunctionType(this);
        }

        public override T Accept<T>(IDataTypeVisitor<T> v)
        {
            return v.VisitFunctionType(this);
        }

		public override DataType Clone()
		{
            Identifier ret = new Identifier("", ReturnValue.DataType.Clone(), ReturnValue.Storage);
            Identifier[] parameters = this.Parameters
                .Select(p => new Identifier(p.Name, p.DataType.Clone(), p.Storage))
                .ToArray();
            var ft = new FunctionType(Name, ret, parameters);
            return ft;
		}

        public bool IsVarargs()
        {
            var last = Parameters != null ? Parameters.LastOrDefault() : null;
            return last != null && last.Name == "...";
        }

        public FunctionType ReplaceVarargs(params Identifier[] parameters)
        {
            if (!IsVarargs())
                throw new NotSupportedException(
                    "Signature should contain varargs");
            var sig = (FunctionType)Clone();
            sig.Parameters = Parameters.
                Where(a => a.Name != "...").
                Concat(parameters).
                ToArray();
            return sig;
        }

        /// <summary>
        /// The size of the return address if pushed on stack.
        /// </summary>
        /// <remarks>
        /// This low level detail is commonly implicit in ABI's.
        /// </remarks>
        public int ReturnAddressOnStack { get; set; }

        /// <summary>
        /// Number of slots by which the FPU stack grows or shrinks after the
        /// procedure is called. A positive number means that items are left
        /// on the stack, a negative number means items are removed from stack.
        /// </summary>
        /// <remarks>
        /// This is x86-specific.
        /// </remarks>
        public int FpuStackDelta { get; set; }

        /// <summary>
        /// Number of bytes to add to the stack pointer after returning from 
        /// the procedure. Note that this does include the return address 
        /// size, if the return address is passed on the stack. 
        /// </summary>
        public int StackDelta { get; set; }

        /// <summary>
        /// The index of the 'deepest' FPU stack argument used. -1 means no
        /// stack parameters are used.
        /// </summary>
        public int FpuStackArgumentMax { get; set; }

        /// <summary>
        /// The index of the 'deepest' FPU stack argument written. -1 means no
        /// stack parameters are written.
        /// </summary>
        public int FpuStackOutArgumentMax { get; set; }

        /// <summary>
        /// True if the medium-level arguments have been discovered. Otherwise,
        /// the signature just contains the net effect
        /// on the processor state.
        /// </summary>
        public bool ParametersValid { get; private set;  }

        /// <summary>
        /// True if this is an instance method of the EnclosingType.
        /// </summary>
        public bool IsInstanceMetod { get; set; }

        public override int Size
		{
			get { return 0; }
			set { ThrowBadSize(); }
		}

        #region Output methods
        public void Emit(string fnName, EmitFlags f, TextWriter writer)
        {
            Emit(fnName, f, new TextFormatter(writer));
        }

        public void Emit(string fnName, EmitFlags f, Formatter fmt)
        {
            Emit(fnName, f, fmt, new CodeFormatter(fmt), new TypeFormatter(fmt, true));
        }

        public void Emit(string fnName, EmitFlags f, Formatter fmt, CodeFormatter w, TypeFormatter t)
        {
            bool emitStorage = (f & EmitFlags.ArgumentKind) == EmitFlags.ArgumentKind;
            if (emitStorage)
            {
                if (HasVoidReturn)
                {
                    fmt.Write("void ");
                }
                else
                {
                    w.WriteFormalArgumentType(ReturnValue, emitStorage);
                    fmt.Write(" ");
                }
                fmt.Write("{0}(", fnName);
            }
            else
            {
                if (HasVoidReturn)
                {
                    fmt.Write("void {0}", fnName);
                }
                else
                {
                    t.Write(ReturnValue.DataType, fnName);           //$TODO: won't work with fn's that return pointers to functions or arrays.
                }
                fmt.Write("(");
            }
            var sep = "";
            if (Parameters != null)
            {
                IEnumerable<Identifier> parms = this.IsInstanceMetod
                    ? Parameters.Skip(1)
                    : Parameters;
                foreach (var p in parms)
                {
                    fmt.Write(sep);
                    sep = ", ";
                    w.WriteFormalArgument(p, emitStorage, t);
                }
            }
            fmt.Write(")");

            if ((f & EmitFlags.LowLevelInfo) == EmitFlags.LowLevelInfo)
            {
                fmt.WriteLine();
                fmt.Write("// stackDelta: {0}; fpuStackDelta: {1}; fpuMaxParam: {2}", StackDelta, FpuStackDelta, FpuStackArgumentMax);
                fmt.WriteLine();
            }
        }

        public string ToString(string name, EmitFlags flags = EmitFlags.ArgumentKind)
        {
            StringWriter sw = new StringWriter();
            TextFormatter f = new TextFormatter(sw);
            CodeFormatter cf = new CodeFormatter(f);
            TypeFormatter tf = new TypeFormatter(f, false);
            Emit(name, flags, f, cf, tf);
            return sw.ToString();
        }

        [Flags]
        public enum EmitFlags
        {
            None = 0,
            ArgumentKind = 1,
            LowLevelInfo = 2,
            AllDetails = ArgumentKind|LowLevelInfo,
        }
        #endregion
    }
}
