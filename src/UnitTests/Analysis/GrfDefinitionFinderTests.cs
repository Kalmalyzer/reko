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

using Reko.Analysis;
using Reko.Core;
using NUnit.Framework;
using Reko.UnitTests.Mocks;
using System;
using System.IO;
using Reko.Core.Expressions;
using Rhino.Mocks;
using System.Collections.Generic;

namespace Reko.UnitTests.Analysis
{
	[TestFixture]
	public class GrfDefinitionFinderTests : AnalysisTestBase
	{
		protected override void RunTest(Program program, TextWriter writer)
		{
            var importResolver = MockRepository.GenerateStub<IImportResolver>();
            importResolver.Replay();
            var dfa = new DataFlowAnalysis(program, importResolver, new FakeDecompilerEventListener());
			var ssts = dfa.UntangleProcedures();
			foreach (var sst in ssts)
			{
				var ssa = sst.SsaState;
				var grfd = new GrfDefinitionFinder(ssa.Identifiers);
				foreach (SsaIdentifier sid in ssa.Identifiers)
				{
                    var id = sid.OriginalIdentifier;
					if (id == null || !(id.Storage is FlagGroupStorage) || sid.Uses.Count == 0)
						continue;
					writer.Write("{0}: ", sid.DefStatement.Instruction);
					grfd.FindDefiningExpression(sid);
					string fmt = grfd.IsNegated ? "!{0};" : "{0}";
					writer.WriteLine(fmt, grfd.DefiningExpression);
				}
			}
		}

		[Test]
        [Ignore(Categories.AnalysisDevelopment)]
        [Category(Categories.AnalysisDevelopment)]
        public void GrfdAdcMock()
		{
			RunFileTest(new AdcMock(), "Analysis/GrfdAdcMock.txt");
		}

		[Test]
        [Ignore(Categories.AnalysisDevelopment)]
        [Category(Categories.AnalysisDevelopment)]
        public void GrfdAddSubCarries()
		{
			RunFileTest_x86_real("Fragments/addsubcarries.asm", "Analysis/GrfdAddSubCarries.txt");
		}

		[Test]
        [Ignore(Categories.AnalysisDevelopment)]
        [Category(Categories.AnalysisDevelopment)]
        public void GrfdCmpMock()
		{
			RunFileTest(new CmpMock(), "Analysis/GrfdCmpMock.txt");
		}
	}
}
