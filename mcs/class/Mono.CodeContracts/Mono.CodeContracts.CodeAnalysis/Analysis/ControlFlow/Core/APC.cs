using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public struct APC : IEquatable<APC>
  {
    public static readonly APC Dummy = new APC(null, 0, null);

    public readonly CFGBlock Block;
    public readonly int Index;
    public readonly LispList<Edge<CFGBlock, EdgeTag>> SubroutineContext;

    public IEnumerable<APC> Successors { get { return this.Block.Subroutine.Successors (this); } }

    public APC(CFGBlock block, int index, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
    {
      this.Block = block;
      this.Index = index;
      this.SubroutineContext = subroutineContext;
    }

    public bool InsideContract
    {
      get
      {
        Subroutine sub = this.Block.Subroutine;
        return sub.IsContract || sub.IsOldValue;
      }
    }

    public bool InsideConstructor
    {
      get
      {
        var ctx = this.SubroutineContext;
        CFGBlock block = this.Block;
        while (block != null) {
          Subroutine subroutine = block.Subroutine;
          if (subroutine.IsConstructor)
            return true;
          if (subroutine.IsMethod)
            return false;
          if (ctx != null) {
            block = ctx.Head.From;
            ctx = ctx.Tail;
          } else block = null;
        }
        return false;
      }
    }

    public bool InsideEnsuresInMethod
    {
      get
      {
        if (!this.Block.Subroutine.IsEnsuresOrOldValue || this.SubroutineContext == null)
          return false;
        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable ()) {
          if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry || edge.Tag.StartsWith(EdgeTag.AfterMask))
            return true;
        }

        return false;
      }
    }

    public bool InsideRequiresAtCall
    {
      get
      {
        if (!this.Block.Subroutine.IsRequires || this.SubroutineContext == null)
          return false;
        
        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable ()) {
          if (edge.Tag == EdgeTag.Entry)
            return false;
          if (edge.Tag.StartsWith(EdgeTag.BeforeMask))
            return true;
        }

        return false;
      }
    }

    public bool InsideEnsuresAtCall
    {
      get
      {
        if (!this.Block.Subroutine.IsRequires || this.SubroutineContext == null)
          return false;

        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable ()) {
          if (edge.Tag == EdgeTag.Exit)
            return false;
          if (edge.Tag.StartsWith(EdgeTag.BeforeMask))
            return true;
        }

        return false;
      }
    }

    public bool InsideInvariantOnExit
    {
      get
      {
        if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
          return false;
        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable ()) {
          if (edge.Tag == EdgeTag.Exit)
            return true;
          if (edge.Tag == EdgeTag.Entry || edge.Tag.StartsWith(EdgeTag.AfterMask))
            return false;
        }

        return false;
      }
    }

    public bool InsideInvariantInMethod
    {
      get
      {
        if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
          return false;

        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable ()) {
          if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry || edge.Tag.StartsWith(EdgeTag.AfterMask))
            return true;
        }

        return false;
      }
    }

    public bool InsideInvariantAtCall
    {
      get
      {
        if (!this.Block.Subroutine.IsInvariant || this.SubroutineContext == null)
          return false;
        foreach (Edge<CFGBlock, EdgeTag> edge in this.SubroutineContext.AsEnumerable()) {
          if (edge.Tag == EdgeTag.Exit || edge.Tag == EdgeTag.Entry)
            return false;
          if (edge.Tag.StartsWith(EdgeTag.AfterMask))
            return true;
        }

        return false;
      }
    }

    public APC Next()
    {
      if (this.Index < this.Block.Count)
        return new APC(this.Block, this.Index + 1, this.SubroutineContext);

      return this;
    }

    public bool Equals(APC other)
    {
      return (this.Block == other.Block && this.Index == other.Index && this.SubroutineContext == other.SubroutineContext);
    }

    public static APC ForEnd(CFGBlock block, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
    {
      return new APC(block, block.Count, subroutineContext);
    }

    public static APC ForStart(CFGBlock block, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
    {
      return new APC(block, 0, subroutineContext);
    }

    public APC LastInBlock()
    {
      return APC.ForEnd (this.Block, this.SubroutineContext);
    }
  }
}