﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
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

using System;
using System.Collections;
using System.Collections.Generic;
using Reko.Core;
using Reko.Core.Rtl;
using Reko.Core.Expressions;
using Reko.Core.Types;
using System.Diagnostics;
using System.Linq;
using Reko.Core.Machine;
using Reko.Core.Operators;

namespace Reko.Arch.Vax
{
    public partial class VaxRewriter : IEnumerable<RtlInstructionCluster>
    {
        private Frame frame;
        private IRewriterHost host;
        private ImageReader rdr;
        private ProcessorState state;
        private VaxArchitecture arch;
        private RtlInstructionCluster rtlc;
        private RtlEmitter emitter;
        private IEnumerator<VaxInstruction> dasm;

        public VaxRewriter(VaxArchitecture arch, ImageReader rdr, ProcessorState state, Frame frame, IRewriterHost host)
        {
            this.arch = arch;
            this.rdr = rdr;
            this.state = state;
            this.frame = frame;
            this.host = host;
            this.dasm = new VaxDisassembler(arch, rdr).GetEnumerator();
        }

        public IEnumerator<RtlInstructionCluster> GetEnumerator()
        {
            while (dasm.MoveNext())
            {
                rtlc = new RtlInstructionCluster(dasm.Current.Address, dasm.Current.Length);
                rtlc.Class = RtlClass.Linear;
                emitter = new RtlEmitter(rtlc.Instructions);
                switch (dasm.Current.Opcode)
                {
                default:
                    //EmitUnitTest();
                    //emitter.SideEffect(Constant.String(
                    //    dasm.Current.ToString(),
                    //    StringType.NullTerminated(PrimitiveType.Char)));
                    //break;
                    throw new AddressCorrelatedException(
                        dasm.Current.Address,
                        "Rewriting of VAX instruction {0} not implemented yet.",
                        dasm.Current.Opcode);
                case Opcode.acbb: RewriteAcbi(PrimitiveType.Byte); break;
                case Opcode.acbd: RewriteAcbf(PrimitiveType.Real64); break;
                case Opcode.acbf: RewriteAcbf(PrimitiveType.Real32); break;
                case Opcode.acbg: RewriteAcbf(PrimitiveType.Real64); break;
                case Opcode.acbh: RewriteAcbf(PrimitiveType.Real128); break;
                case Opcode.acbl: RewriteAcbi(PrimitiveType.Word32); break;
                case Opcode.acbw: RewriteAcbi(PrimitiveType.Word16); break;
                case Opcode.adawi: RewriteAlu2(PrimitiveType.Word16, Adawi, AllFlags); break;
                case Opcode.addb2: RewriteAlu2(PrimitiveType.Byte, emitter.IAdd, AllFlags); break;
                case Opcode.addb3: RewriteAlu3(PrimitiveType.Byte, emitter.IAdd, AllFlags); break;
                case Opcode.addd2: RewriteFpu2(PrimitiveType.Real64, emitter.FAdd, NZ00); break;
                case Opcode.addd3: RewriteFpu3(PrimitiveType.Real64, emitter.FAdd, NZ00); break;
                case Opcode.addf2: RewriteFpu2(PrimitiveType.Real32, emitter.FAdd, NZ00); break;
                case Opcode.addf3: RewriteFpu3(PrimitiveType.Real32, emitter.FAdd, NZ00); break;
                case Opcode.addl2: RewriteAlu2(PrimitiveType.Word32, emitter.IAdd, AllFlags); break;
                case Opcode.addl3: RewriteAlu3(PrimitiveType.Word32, emitter.IAdd, AllFlags); break;
                case Opcode.addp4: RewriteP4("vax_addp4"); break;
                case Opcode.addp6: RewriteP6("vax_addp6"); break;
                case Opcode.addw2: RewriteAlu2(PrimitiveType.Word16, emitter.IAdd, AllFlags); break;
                case Opcode.addw3: RewriteAlu3(PrimitiveType.Word16, emitter.IAdd, AllFlags); break;
                case Opcode.adwc: RewriteAdwc(); break;
                case Opcode.aobleq: RewriteAob(emitter.Le); break;
                case Opcode.aoblss: RewriteAob(emitter.Lt); break;
                case Opcode.ashl: RewriteAsh(PrimitiveType.Word32); break;
                case Opcode.ashp: RewriteAshp(); break;
                case Opcode.ashq: RewriteAsh(PrimitiveType.Word64); break;
                case Opcode.bbc: RewriteBb(false); break;
                case Opcode.bbs: RewriteBb(true); break;
                case Opcode.beql: RewriteBranch(ConditionCode.EQ, FlagM.ZF); break;
                case Opcode.bgeq: RewriteBranch(ConditionCode.GE, FlagM.NF); break;
                case Opcode.bgequ: RewriteBranch(ConditionCode.UGE, FlagM.CF); break;
                case Opcode.bgtr: RewriteBranch(ConditionCode.GT, FlagM.ZF | FlagM.NF); break;
                case Opcode.bgtru: RewriteBranch(ConditionCode.UGT, FlagM.ZF | FlagM.CF); break;
                case Opcode.bleq: RewriteBranch(ConditionCode.LE, FlagM.ZF | FlagM.NF); break;
                case Opcode.blequ: RewriteBranch(ConditionCode.ULE, FlagM.ZF | FlagM.CF); break;
                case Opcode.blss: RewriteBranch(ConditionCode.LT, FlagM.NF); break;
                case Opcode.blssu: RewriteBranch(ConditionCode.ULT, FlagM.CF); break;
                case Opcode.bneq: RewriteBranch(ConditionCode.NE, FlagM.ZF); break;
                case Opcode.bvc: RewriteBranch(ConditionCode.NO, FlagM.VF); break;
                case Opcode.bvs: RewriteBranch(ConditionCode.OV, FlagM.VF); break;
                case Opcode.bicb2: RewriteAlu2(PrimitiveType.Byte, Bic, NZ00); break;
                case Opcode.bicb3: RewriteAlu3(PrimitiveType.Byte, Bic, NZ00); break;
                case Opcode.bicl2: RewriteAlu2(PrimitiveType.Word32, Bic, NZ00); break;
                case Opcode.bicl3: RewriteAlu3(PrimitiveType.Word32, Bic, NZ00); break;
                case Opcode.bicpsw: goto default;
                case Opcode.bicw2: RewriteAlu2(PrimitiveType.Word16, Bic, NZ00); break;
                case Opcode.bicw3: RewriteAlu3(PrimitiveType.Word16, Bic, NZ00); break;
                case Opcode.bisb2: RewriteAlu2(PrimitiveType.Byte, emitter.Or, NZ00); break;
                case Opcode.bisb3: RewriteAlu3(PrimitiveType.Byte, emitter.Or, NZ00); break;
                case Opcode.bispsw: goto default;
                case Opcode.bisl2: RewriteAlu2(PrimitiveType.Word32, emitter.Or, NZ00); break;
                case Opcode.bisl3: RewriteAlu3(PrimitiveType.Word32, emitter.Or, NZ00); break;
                case Opcode.bisw2: RewriteAlu2(PrimitiveType.Word16, emitter.Or, NZ00); break;
                case Opcode.bisw3: RewriteAlu3(PrimitiveType.Word16, emitter.Or, NZ00); break;
                case Opcode.blbc: RewriteBlb(emitter.Eq0); break;
                case Opcode.blbs: RewriteBlb(emitter.Ne0); break;
                case Opcode.bpt: RewriteBpt(); break;
                case Opcode.brb: RewriteBranch(); break;
                case Opcode.brw: RewriteBranch(); break;
                case Opcode.bsbb: RewriteBsb(); break;
                case Opcode.bsbw: RewriteBsb(); break;

                case Opcode.chme: RewriteChme(); break;
                case Opcode.clrb: RewriteClr(PrimitiveType.Byte); break;
                case Opcode.clrh: RewriteClr(PrimitiveType.Word128); break;
                case Opcode.clrl: RewriteClr(PrimitiveType.Word32); break;
                case Opcode.clrq: RewriteClr(PrimitiveType.Word64); break;
                case Opcode.clrw: RewriteClr(PrimitiveType.Word16); break;
                case Opcode.cmpb: RewriteCmp(PrimitiveType.Byte); break;
                case Opcode.cmpd: RewriteCmp(PrimitiveType.Real64); break;
                case Opcode.cmpf: RewriteCmp(PrimitiveType.Real32); break;
                case Opcode.cmpg: RewriteCmp(PrimitiveType.Real64); break;
                case Opcode.cmph: RewriteCmp(PrimitiveType.Real128); break;
                case Opcode.cmpl: RewriteCmp(PrimitiveType.Word32); break;
                case Opcode.cmpw: RewriteCmp(PrimitiveType.Word16); break;
                case Opcode.cmpp3: RewriteCmpp3(); break;
                case Opcode.cmpp4: RewriteCmpp4(); break;
                case Opcode.cmpc3: goto default;
                case Opcode.cmpc5: goto default;
                case Opcode.cmpv: goto default;
                case Opcode.cmpzv: goto default;
                case Opcode.cvtbd: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Real64); break;
                case Opcode.cvtbf: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Real32); break;
                case Opcode.cvtbg: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Real64); break;
                case Opcode.cvtbh: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Real128); break;
                case Opcode.cvtbl: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Int32); break;
                case Opcode.cvtbw: RewriteCvt(PrimitiveType.SByte, PrimitiveType.Int16); break;
                case Opcode.cvtdb: RewriteCvt(PrimitiveType.Real64, PrimitiveType.SByte); break;
                case Opcode.cvtdf: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Real32); break;
                case Opcode.cvtdh: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Real128); break;
                case Opcode.cvtdl: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Int32; break;
                case Opcode.cvtdw: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Int16); break;
                case Opcode.cvtfb: RewriteCvt(PrimitiveType.Real32, PrimitiveType.SByte); break;
                case Opcode.cvtfd: RewriteCvt(PrimitiveType.Real32, PrimitiveType.Real64); break;
                case Opcode.cvtfg: RewriteCvt(PrimitiveType.Real32, PrimitiveType.Real64); break;
                case Opcode.cvtfh: RewriteCvt(PrimitiveType.Real32, PrimitiveType.Real128); break;
                case Opcode.cvtfl: RewriteCvt(PrimitiveType.Real32, PrimitiveType.Int32); break;
                case Opcode.cvtfw: RewriteCvt(PrimitiveType.Real32, PrimitiveType.Int16); break;
                case Opcode.cvtgb: RewriteCvt(PrimitiveType.Real64, PrimitiveType.SByte); break;
                case Opcode.cvtgf: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Real32); break;
                case Opcode.cvtgh: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Real128); break;
                case Opcode.cvtgl: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Int32); break;
                case Opcode.cvtgw: RewriteCvt(PrimitiveType.Real64, PrimitiveType.Int16); break; 
                case Opcode.cvthb: RewriteCvt(PrimitiveType.Real128, PrimitiveType.SByte); break;
                case Opcode.cvthd: RewriteCvt(PrimitiveType.Real128, PrimitiveType.Real64); break;
                case Opcode.cvthf: RewriteCvt(PrimitiveType.Real128, PrimitiveType.Real32); break;
                case Opcode.cvthg: RewriteCvt(PrimitiveType.Real128, PrimitiveType.Real64); break;
                case Opcode.cvthl: RewriteCvt(PrimitiveType.Real128, PrimitiveType.Int32); break;
                case Opcode.cvthw: RewriteCvt(PrimitiveType.Real128, PrimitiveType.Int16); break;
                case Opcode.cvtrdl: RewriteCvtr(PrimitiveType.Real64, PrimitiveType.Int32); break;
                case Opcode.cvtrfl: RewriteCvtr(PrimitiveType.Real32, PrimitiveType.Int32); break;
                case Opcode.cvtrgl: RewriteCvtr(PrimitiveType.Real64, PrimitiveType.Int32); break;
                case Opcode.cvtrhl: RewriteCvtr(PrimitiveType.Real128, PrimitiveType.Int32); break;
                case Opcode.cvtlb: RewriteCvt(PrimitiveType.Int32, PrimitiveType.SByte); break;
                case Opcode.cvtld: RewriteCvt(PrimitiveType.Int32, PrimitiveType.Real64); break;
                case Opcode.cvtlf: RewriteCvt(PrimitiveType.Int32, PrimitiveType.Real32); break;
                case Opcode.cvtlg: RewriteCvt(PrimitiveType.Int32, PrimitiveType.Real64); break;
                case Opcode.cvtlh: RewriteCvt(PrimitiveType.Int32, PrimitiveType.Real128); break;
                case Opcode.cvtlp: goto default;
                case Opcode.cvtlw: RewriteCvt(PrimitiveType.Int32, PrimitiveType.Int16); break;
                case Opcode.cvtpl: goto default;
                case Opcode.cvtps: goto default;
                case Opcode.cvtpt: goto default;
                case Opcode.cvtsp: goto default;
                case Opcode.cvttp: goto default;
                case Opcode.cvtwb: RewriteCvt(PrimitiveType.Int16, PrimitiveType.SByte); break;
                case Opcode.cvtwd: RewriteCvt(PrimitiveType.Int16, PrimitiveType.Real64); break;
                case Opcode.cvtwf: RewriteCvt(PrimitiveType.Int16, PrimitiveType.Real32); break;
                case Opcode.cvtwg: RewriteCvt(PrimitiveType.Int16, PrimitiveType.Real64); break;
                case Opcode.cvtwh: RewriteCvt(PrimitiveType.Int16, PrimitiveType.Real128); break;
                case Opcode.cvtwl: RewriteCvt(PrimitiveType.Int16, PrimitiveType.Int32; break; ;
                case Opcode.decb: RewriteIncDec(PrimitiveType.Byte, Dec); break;
                case Opcode.decl: RewriteIncDec(PrimitiveType.Word32, Dec); break;
                case Opcode.decw: RewriteIncDec(PrimitiveType.Word16, Dec); break;
                case Opcode.divb2: RewriteAlu2(PrimitiveType.Byte, emitter.SDiv, AllFlags); break;
                case Opcode.divb3: RewriteAlu3(PrimitiveType.Byte, emitter.SDiv, AllFlags); break;
                case Opcode.divd2: RewriteFpu2(PrimitiveType.Real64, emitter.FDiv, AllFlags); break;
                case Opcode.divd3: RewriteFpu3(PrimitiveType.Real64, emitter.FDiv, AllFlags); break;
                case Opcode.divf2: RewriteFpu2(PrimitiveType.Real32, emitter.FDiv, AllFlags); break;
                case Opcode.divf3: RewriteFpu3(PrimitiveType.Real32, emitter.FDiv, AllFlags); break;
                case Opcode.divl2: RewriteAlu2(PrimitiveType.Word32, emitter.SDiv, AllFlags); break;
                case Opcode.divl3: RewriteAlu3(PrimitiveType.Word32, emitter.SDiv, AllFlags); break;
                case Opcode.divp: RewriteDivp(); break;
                case Opcode.divw2: RewriteAlu2(PrimitiveType.Word16, emitter.SDiv, AllFlags); break;
                case Opcode.divw3: RewriteAlu3(PrimitiveType.Word16, emitter.SDiv, AllFlags); break;

                case Opcode.halt: RewriteHalt(); break;
                case Opcode.incb: RewriteIncDec(PrimitiveType.Byte, Inc); break;
                case Opcode.incl: RewriteIncDec(PrimitiveType.Word32, Inc); break;
                case Opcode.incw: RewriteIncDec(PrimitiveType.Word16, Inc); break;

                case Opcode.mcomb: RewriteAluUnary2(PrimitiveType.Byte, emitter.Comp, NZ00); break;
                case Opcode.mcoml: RewriteAluUnary2(PrimitiveType.Word32, emitter.Comp, NZ00); break;
                case Opcode.mcomw: RewriteAluUnary2(PrimitiveType.Word16, emitter.Comp, NZ00); break;
                case Opcode.mnegb: RewriteAluUnary2(PrimitiveType.Byte, emitter.Neg, AllFlags); break;
                case Opcode.mnegd: RewriteAluUnary2(PrimitiveType.Real64, emitter.FNeg, NZ00); break;
                case Opcode.mnegf: RewriteAluUnary2(PrimitiveType.Real32, emitter.FNeg, NZ00); break;
                case Opcode.mnegg: RewriteAluUnary2(PrimitiveType.Real64, emitter.FNeg, NZ00); break;
                case Opcode.mnegh: RewriteAluUnary2(PrimitiveType.Word128, emitter.FNeg, NZ00); break;
                case Opcode.mnegl: RewriteAluUnary2(PrimitiveType.Word32, emitter.Neg, AllFlags); break;
                case Opcode.mnegw: RewriteAluUnary2(PrimitiveType.Word16, emitter.Neg, AllFlags); break;

                case Opcode.movab:  RewriteAluUnary2(PrimitiveType.Word32, e => e, NZ00); break;
                case Opcode.movah:  RewriteAluUnary2(PrimitiveType.Word32, e => e, NZ00); break;
                case Opcode.moval:  RewriteAluUnary2(PrimitiveType.Word32, e => e, NZ00); break;
                case Opcode.movaq:  RewriteAluUnary2(PrimitiveType.Word32, e => e, NZ00); break;
                case Opcode.movaw:  RewriteAluUnary2(PrimitiveType.Word32, e => e, NZ00); break;
                    
                case Opcode.movb:   RewriteAluUnary2(PrimitiveType.Byte, Copy, NZ00); break;
                case Opcode.movc3:  goto default;
                case Opcode.movc5:  goto default;
                case Opcode.movd:   RewriteAluUnary2(PrimitiveType.Real64, Copy, NZ00); break;
                case Opcode.movf:   RewriteAluUnary2(PrimitiveType.Real32, Copy, NZ00); break;
                case Opcode.movg:   RewriteAluUnary2(PrimitiveType.Real64, Copy, NZ00); break;
                case Opcode.movh:   RewriteAluUnary2(PrimitiveType.Real128,Copy, NZ00); break;
                case Opcode.movl:   RewriteAluUnary2(PrimitiveType.Word32, Copy, NZ00); break;
                case Opcode.movo:   RewriteAluUnary2(PrimitiveType.Word128,Copy, NZ00); break;
                case Opcode.movp: goto default;
                case Opcode.movq:   RewriteAluUnary2(PrimitiveType.Word64, Copy, NZ00); break;
                case Opcode.movw:   RewriteAluUnary2(PrimitiveType.Word16, Copy, NZ00); break;
                case Opcode.movzbl: RewriteMovz(PrimitiveType.Byte, PrimitiveType.Word32); break;
                case Opcode.movzbw: RewriteMovz(PrimitiveType.Byte, PrimitiveType.Word16); break;
                case Opcode.movzwl: RewriteMovz(PrimitiveType.Word16, PrimitiveType.Word32); break;
                case Opcode.mulb2: RewriteAlu2(PrimitiveType.Byte, emitter.IMul, AllFlags); break;
                case Opcode.mulb3: RewriteAlu3(PrimitiveType.Byte, emitter.IMul, AllFlags); break;
                case Opcode.muld2: RewriteAlu2(PrimitiveType.Real64, emitter.FMul, AllFlags); break;
                case Opcode.muld3: RewriteAlu3(PrimitiveType.Real64, emitter.FMul, AllFlags); break;
                case Opcode.mulf2: RewriteAlu2(PrimitiveType.Real32, emitter.FMul, AllFlags); break;
                case Opcode.mulf3: RewriteAlu3(PrimitiveType.Real32, emitter.FMul, AllFlags); break;
                case Opcode.mulg2: RewriteAlu2(PrimitiveType.Real64, emitter.FMul, AllFlags); break;
                case Opcode.mulg3: RewriteAlu3(PrimitiveType.Real64, emitter.FMul, AllFlags); break;
                case Opcode.mulh2: RewriteAlu2(PrimitiveType.Real128, emitter.FMul, AllFlags); break;
                case Opcode.mulh3: RewriteAlu3(PrimitiveType.Real128, emitter.FMul, AllFlags); break;
                case Opcode.mull2: RewriteAlu2(PrimitiveType.Word32, emitter.IMul, AllFlags); break;
                case Opcode.mull3: RewriteAlu3(PrimitiveType.Word32, emitter.IMul, AllFlags); break;
                case Opcode.mulp: RewriteMulp(); break;
                case Opcode.mulw2: RewriteAlu2(PrimitiveType.Word16, emitter.IMul, AllFlags); break;
                case Opcode.mulw3: RewriteAlu3(PrimitiveType.Word16, emitter.IMul, AllFlags); break;
                case Opcode.nop: emitter.Nop(); break;

                case Opcode.polyd: RewritePoly(PrimitiveType.Real64); break;
                case Opcode.polyf: RewritePoly(PrimitiveType.Real32); break;
                case Opcode.polyg: RewritePoly(PrimitiveType.Real64); break;
                case Opcode.polyh: RewritePoly(PrimitiveType.Real128); break;

                case Opcode.popr:  goto default;
                case Opcode.prober: goto default;
                case Opcode.probew: goto default;
                case Opcode.pushr:  goto default;

                case Opcode.pushab: RewritePusha(); break;
                case Opcode.pushal: RewritePusha(); break;
                case Opcode.pushah: RewritePusha(); break;
                case Opcode.pushaw: RewritePusha(); break;
                case Opcode.pushaq: RewritePusha(); break;
                case Opcode.pushl: RewritePush(PrimitiveType.Word32); break;

                case Opcode.ret: RewriteRet(); break;
                case Opcode.rotl: RewriteAlu3(PrimitiveType.Word32, Rotl, NZ00); break;
                case Opcode.rsb: RewriteRsb(); break;

                case Opcode.sbwc: RewriteSbwc(); break;
                case Opcode.sobgeq: RewriteSob(emitter.Ge); break;
                case Opcode.sobgtr: RewriteSob(emitter.Gt); break;
                case Opcode.subb2: RewriteAlu2(PrimitiveType.Byte, emitter.ISub, AllFlags); break;
                case Opcode.subb3: RewriteAlu3(PrimitiveType.Byte, emitter.ISub, AllFlags); break;
                case Opcode.subd2: RewriteAlu2(PrimitiveType.Real64, emitter.FSub, NZ00); break;
                case Opcode.subd3: RewriteAlu3(PrimitiveType.Real64, emitter.FSub, NZ00); break;
                case Opcode.subf2: RewriteAlu2(PrimitiveType.Real32, emitter.FSub, NZ00); break;
                case Opcode.subf3: RewriteAlu3(PrimitiveType.Real32, emitter.FSub, NZ00); break;
                case Opcode.subg2: RewriteAlu2(PrimitiveType.Real64, emitter.FSub, NZ00); break;
                case Opcode.subg3: RewriteAlu3(PrimitiveType.Real64, emitter.FSub, NZ00); break;
                case Opcode.subh2: RewriteAlu2(PrimitiveType.Real128, emitter.FSub, NZ00); break;
                case Opcode.subh3: RewriteAlu3(PrimitiveType.Real128, emitter.FSub, NZ00); break;
                case Opcode.subl2: RewriteAlu2(PrimitiveType.Word32, emitter.ISub, AllFlags); break;
                case Opcode.subl3: RewriteAlu3(PrimitiveType.Word32, emitter.ISub, AllFlags); break;
                case Opcode.subp4: RewriteP4("vax_subp4"); break;
                case Opcode.subp6: RewriteP6("vax_subp6"); break;
                case Opcode.subw2: RewriteAlu2(PrimitiveType.Word16, emitter.ISub, AllFlags); break;
                case Opcode.subw3: RewriteAlu3(PrimitiveType.Word16, emitter.ISub, AllFlags); break;

                case Opcode.xorb2: RewriteAlu2(PrimitiveType.Byte, emitter.Xor, NZ00); break;
                case Opcode.xorb3: RewriteAlu3(PrimitiveType.Byte, emitter.Xor, NZ00); break;
                case Opcode.xorl2: RewriteAlu2(PrimitiveType.Word32, emitter.Xor, NZ00); break;
                case Opcode.xorl3: RewriteAlu3(PrimitiveType.Word32, emitter.Xor, NZ00); break;
                case Opcode.xorw2: RewriteAlu2(PrimitiveType.Word16, emitter.Xor, NZ00); break;
                case Opcode.xorw3: RewriteAlu3(PrimitiveType.Word16, emitter.Xor, NZ00); break;

                case Opcode.rei: goto default;
                case Opcode.ldpctx: goto default;
                case Opcode.svpctx: goto default;
                case Opcode.editpc: goto default;
                case Opcode.matchc: goto default;
                case Opcode.index: goto default;
                case Opcode.locc: goto default;
                case Opcode.crc: goto default;
                case Opcode.skpc: goto default;
                case Opcode.insque: goto default;
                case Opcode.remque: goto default;
                case Opcode.jsb: goto default;
                case Opcode.jmp: goto default;
                case Opcode.tstf: goto default;
                case Opcode.emodf: goto default;
                case Opcode.scanc: goto default;
                case Opcode.spanc: goto default;
                case Opcode.insqhi: goto default;
                case Opcode.insqti: goto default;
                case Opcode.movtc: goto default;
                case Opcode.remqhi: goto default;
                case Opcode.movtuc: goto default;
                case Opcode.remqti: goto default;
                case Opcode.tstb: goto default;

                case Opcode.bitb: goto default;
                case Opcode.tstd: goto default;
                case Opcode.emodd: goto default;

                case Opcode.emul: goto default;
                case Opcode.ediv: goto default;
                case Opcode.casew: goto default;
                case Opcode.bitw: goto default;
                case Opcode.tstw: goto default;
                case Opcode.chmk: goto default;
                case Opcode.chms: goto default;
                case Opcode.caseb: goto default;
                case Opcode.chmu: goto default;
                case Opcode.bbss: goto default;
                case Opcode.bbcs: goto default;
                case Opcode.bbsc: goto default;
                case Opcode.bbcc: goto default;
                case Opcode.bbssi: goto default;
                case Opcode.bbcci: goto default;
                case Opcode.ffs: goto default;
                case Opcode.ffc: goto default;
                case Opcode.extv: goto default;
                case Opcode.casel: goto default;
                case Opcode.extzv: goto default;
                case Opcode.insv: goto default;

                case Opcode.bitl: goto default;
                case Opcode.tstl: goto default;
                case Opcode.mtpr: goto default;
                case Opcode.callg: goto default;
                case Opcode.mfpr: goto default;
                case Opcode.calls: goto default;
                case Opcode.movpsl: goto default;
                case Opcode.xfc: goto default;
                case Opcode.mfvp: goto default;
                case Opcode.vldl: goto default;
                case Opcode.vgathl: goto default;
                case Opcode.vldq: goto default;
                case Opcode.vgathq: goto default;
                case Opcode.addg2: goto default;
                case Opcode.addg3: goto default;
                case Opcode.divg2: goto default;
                case Opcode.divg3: goto default;
                case Opcode.tstg: goto default;
                case Opcode.emodg: goto default;
                case Opcode.addh2: goto default;
                case Opcode.addh3: goto default;
                case Opcode.divh2: goto default;
                case Opcode.divh3: goto default;

                case Opcode.vstl: goto default;
                case Opcode.vscatl: goto default;
                case Opcode.vstq: goto default;
                case Opcode.vscatq: goto default;
                case Opcode.vvmull: goto default;
                case Opcode.vsmull: goto default;
                case Opcode.vvmulg: goto default;
                case Opcode.tsth: goto default;
                case Opcode.vsmulg: goto default;
                case Opcode.emodh: goto default;
                case Opcode.vvmulf: goto default;
                case Opcode.vsmulf: goto default;
                case Opcode.vvmuld: goto default;
                case Opcode.vsmuld: goto default;
                case Opcode.vsync: goto default;
                case Opcode.mtvp: goto default;
                case Opcode.vvdivg: goto default;
                case Opcode.vsdivg: goto default;
                case Opcode.vvdivf: goto default;
                case Opcode.vsdivf: goto default;
                case Opcode.vvdivd: goto default;
                case Opcode.vsdivd: goto default;
                case Opcode.vvaddl: goto default;
                case Opcode.vsaddl: goto default;
                case Opcode.vvaddg: goto default;
                case Opcode.vsaddg: goto default;
                case Opcode.vvaddf: goto default;
                case Opcode.vsaddf: goto default;
                case Opcode.vvaddd: goto default;
                case Opcode.vsaddd: goto default;
                case Opcode.vvsubl: goto default;
                case Opcode.vssubl: goto default;
                case Opcode.vvsubg: goto default;
                case Opcode.vssubg: goto default;
                case Opcode.vvsubf: goto default;
                case Opcode.vssubf: goto default;
                case Opcode.vvsubd: goto default;
                case Opcode.vssubd: goto default;
                case Opcode.vvcmpl: goto default;
                case Opcode.vvsrll: goto default;
                case Opcode.vscmpl: goto default;
                case Opcode.vssrll: goto default;
                case Opcode.vvcmpg: goto default;
                case Opcode.vscmpg: goto default;
                case Opcode.vvcmpf: goto default;
                case Opcode.vvslll: goto default;
                case Opcode.vscmpf: goto default;
                case Opcode.vsslll: goto default;
                case Opcode.vvcmpd: goto default;
                case Opcode.vscmpd: goto default;
                case Opcode.vvbisl: goto default;
                case Opcode.vvxorl: goto default;
                case Opcode.vsbisl: goto default;
                case Opcode.vsxorl: goto default;
                case Opcode.vvbicl: goto default;
                case Opcode.vvcvt: goto default;
                case Opcode.vsbicl: goto default;
                case Opcode.iota: goto default;
                case Opcode.vvmerge: goto default;
                case Opcode.vsmerge: goto default;

                case Opcode.bugl: goto default;
                case Opcode.bugw: goto default;
                }
                yield return rtlc;
            }
        }

        private static HashSet<Opcode> seen = new HashSet<Opcode>();

        [Conditional("DEBUG")]
        private void EmitUnitTest()
        {
            if (seen.Contains(dasm.Current.Opcode))
                return;
            seen.Add(dasm.Current.Opcode);

            var r2 = rdr.Clone();
            r2.Offset -= dasm.Current.Length;
            var bytes = r2.ReadBytes(dasm.Current.Length);
            Debug.WriteLine("        [Test]");
            Debug.WriteLine("        public void VaxRw_" + dasm.Current.Opcode + "()");
            Debug.WriteLine("        {");
            Debug.Write("            BuildTest(");
            Debug.Write(string.Join(
                ", ",
                bytes.Select(b => string.Format("0x{0:X2}", (int)b))));
            Debug.WriteLine(");\t// " + dasm.Current.ToString());
            Debug.WriteLine("            AssertCode(");
            Debug.WriteLine("                \"0|L--|00100000(2): 1 instructions\",");
            Debug.WriteLine("                \"1|L--|@@@\");");
            Debug.WriteLine("        }");
            Debug.WriteLine("");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //$REVIEW: push PseudoProc into the RewriterHost interface"
        public Expression PseudoProc(string name, DataType retType, params Expression[] args)
        {
            var ppp = host.EnsurePseudoProcedure(name, retType, args.Length);
            return PseudoProc(ppp, retType, args);
        }

        public Expression PseudoProc(PseudoProcedure ppp, DataType retType, params Expression[] args)
        {
            if (args.Length != ppp.Arity)
                throw new ArgumentOutOfRangeException(
                    string.Format("Pseudoprocedure {0} expected {1} arguments, but was passed {2}.",
                    ppp.Name,
                    ppp.Arity,
                    args.Length));

            return emitter.Fn(new ProcedureConstant(arch.PointerType, ppp), retType, args);
        }

        private Expression RewriteSrcOp(int iOp, PrimitiveType width)
        {
            var op = dasm.Current.Operands[iOp];
            var regOp = op as RegisterOperand;
            if (regOp != null)
            {
                var reg = frame.EnsureRegister(regOp.Register);
                if (reg.DataType.Size <= width.Size)
                {
                    return reg;
                } else
                {
                    return emitter.Cast(width, reg);
                }
            }
            var immOp = op as ImmediateOperand;
            if (immOp != null)
            {
                return immOp.Value;
            }
            var memOp = op as MemoryOperand;
            if (memOp != null)
            {
                Expression ea;
                if (memOp.Base != null)
                {
                    var reg = frame.EnsureRegister(memOp.Base);
                    if (memOp.AutoDecrement)
                    {
                        emitter.Assign(reg, emitter.ISub(reg, width.Size));
                    } else if (memOp.AutoIncrement)
                    {
                        var tmp = frame.CreateTemporary(reg.DataType);
                        emitter.Assign(tmp, reg);
                        reg = tmp;
                    }
                    ea = reg;
                    if (memOp.Offset != null)
                    {
                        ea = emitter.IAdd(ea, memOp.Offset);
                    }
                    if (memOp.Index != null)
                    {
                        Expression idx = frame.EnsureRegister(memOp.Index);
                        if (width.Size != 1)
                            idx = emitter.IMul(idx, Constant.Int32(width.Size));
                        ea = emitter.IAdd(ea, idx);
                    }
                    Expression load;
                    if (memOp.Deferred)
                        load = emitter.Load(width, emitter.LoadDw(ea));
                    else
                        load = emitter.Load(width, ea);
                    if (memOp.AutoIncrement)
                    {
                        if (memOp.AutoIncrement)
                        {
                            reg = frame.EnsureRegister(memOp.Base);
                            int inc = (memOp.Deferred) ? 4 : width.Size;
                            emitter.Assign(reg, emitter.IAdd(reg, inc));
                        }
                    }
                    return load;
                }
                else
                {
                }
            }
            throw new NotImplementedException(op.GetType().Name);
        }

        private Identifier RewriteDstOp(int iOp, PrimitiveType width, Func<Expression, Expression> fn)
        {
            var op = dasm.Current.Operands[iOp];
            var regOp = op as RegisterOperand;
            if (regOp != null)
            {
                var reg = frame.EnsureRegister(regOp.Register);
                if (reg.DataType.Size > width.Size)
                {
                    var tmp = frame.CreateTemporary(width);
                    emitter.Assign(tmp, fn(emitter.Cast(width, reg)));
                    emitter.Assign(reg, emitter.Dpb(reg, tmp, 0));
                    return tmp;
                }
                else
                {
                    emitter.Assign(reg, fn(reg));
                    return reg;
                }
            }
            if( op is ImmediateOperand)
            {
                throw new AddressCorrelatedException(
                    dasm.Current.Address,
                    "Instruction {0} is attempting to assign to an immediate value.",
                    dasm.Current);
            }
            var memOp = op as MemoryOperand;
            if (memOp != null)
            {
                Expression ea;
                if (memOp.Base != null)
                {
                    var reg = frame.EnsureRegister(memOp.Base);
                    if (memOp.AutoDecrement)
                    {
                        emitter.Assign(reg, emitter.ISub(reg, width.Size));
                    }
                    ea = reg;
                    if (memOp.Offset != null)
                    {
                        ea = emitter.IAdd(ea, memOp.Offset);
                    }
                    var tmp = frame.CreateTemporary(width);
                    emitter.Assign(tmp, fn(emitter.Load(width, ea)));
                    Expression load; 
                    if (memOp.Deferred)
                        load = emitter.Load(width, emitter.LoadDw(ea));
                    else
                        load = emitter.Load(width, ea);
                    emitter.Assign(load, tmp);

                    if (memOp.AutoIncrement)
                    {
                        int inc = (memOp.Deferred) ? 4 : width.Size;
                        emitter.Assign(reg, emitter.IAdd(reg, inc));
                    }
                    return tmp;
                }
                else
                {
                }
            }
            throw new NotImplementedException(op.GetType().Name);
        }

        private Identifier FlagGroup(FlagM flags)
        {
            return frame.EnsureFlagGroup(Registers.psw, (uint)flags, arch.GrfToString((uint)flags), PrimitiveType.Byte);
        }

        private void AllFlags(Expression dst)
        {
            var grf = FlagGroup(FlagM.NZVC);
            emitter.Assign(grf, emitter.Cond(dst));
        }

        private void NZ00(Expression dst)
        {
            var grf = FlagGroup(FlagM.NF| FlagM.ZF);
            emitter.Assign(grf, emitter.Cond(dst));
            var c = FlagGroup(FlagM.CF);
            var v = FlagGroup(FlagM.VF);
            emitter.Assign(c, Constant.False());
            emitter.Assign(v, Constant.False());
        }

        private void NZV(Expression dst)
        {
            var grf = FlagGroup(FlagM.NF | FlagM.ZF | FlagM.VF);
            emitter.Assign(grf, emitter.Cond(dst));
        }

        private void NZV0(Expression dst)
        {
            var grf = FlagGroup(FlagM.NF | FlagM.ZF | FlagM.VF);
            emitter.Assign(grf, emitter.Cond(dst));
            var c = FlagGroup(FlagM.CF);
            emitter.Assign(c, Constant.False());
        }
    }
}
