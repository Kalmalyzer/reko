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

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Reko.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System;
using Reko.Gui.Windows.Controls;
using Reko.Core.Output;

namespace Reko.Gui.Windows
{
    public class CfgGraphGenerator
    {
        private Graph graph;
        private HashSet<Block> visited;
        private Graphics g;

        public CfgGraphGenerator(Graph graph, Graphics g)
        {
            this.graph = graph;
            this.g = g;
            this.visited = new HashSet<Block>();
        }

        public static Graph Generate(Procedure proc, Graphics g)
        {
            Graph graph = new Graph();
            var cfgGen = new CfgGraphGenerator(graph, g);
            cfgGen.Traverse(proc.EntryBlock.Succ[0]);
            graph.Attr.LayerDirection = LayerDirection.TB;
            return graph;
        }

        public void Traverse(Block block)
        {
            var q = new Queue<Block>();
            q.Enqueue(block);
            while (q.Count > 0)
            {
                var b = q.Dequeue();
                if (visited.Contains(b))
                    continue;
                visited.Add(b);
                Debug.Print("Node {0}", b.Name);
                visited.Add(b);
                var n = CreateGraphNode(b);
                foreach (var pred in b.Pred.Where(p => p != block.Procedure.EntryBlock))
                {
                    Debug.Print("Edge {0} - {1}", pred.Name, b.Name);
                    graph.AddEdge(pred.Name, b.Name);
                }
                foreach (var succ in b.Succ)
                {
                    q.Enqueue(succ);
                }
            }
        }

        private Node CreateGraphNode(Block b)
        {
            var nl = "\n    ";
            var blockNode = new CfgBlockNode {
                Block = b,
                TextModel = GenerateTextModel(b)
            };
            var node = graph.AddNode(b.Name);
            node.Attr.LabelMargin = 5;
            node.UserData = blockNode;
            if (nl.Length > 0)
            {
                node.Attr.Shape = Shape.DrawFromGeometry;
                node.DrawNodeDelegate = blockNode.DrawNode;
                node.NodeBoundaryDelegate = blockNode.GetNodeBoundary;
            }
            else
            {
                node.Label.FontName = "Lucida Console";
                node.Label.FontSize = 10f;
                node.LabelText =
                    b.Name + nl +
                    string.Join(nl, b.Statements.Select(s => s.Instruction));
            }
            return node;
        }

        private TextViewModel GenerateTextModel(Block b)
        {
            var tsf = new TextSpanFormatter();
            var fmt = new AbsynCodeFormatter(tsf);
            var procf = new ProcedureFormatter(b.Procedure, fmt);
            fmt.InnerFormatter.UseTabs = false;
            procf.WriteBlock(b, fmt);
            return tsf.GetModel();
        }
    }
}