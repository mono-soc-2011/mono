using System;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public struct Edge<TNode, TTag>
  {
    public TNode From;
    public TNode To;
    public TTag Tag;

    public Edge(TNode from, TNode to, TTag tag)
    {
      this.From = from;
      this.To = to;
      this.Tag = tag;
    }

    public override string ToString()
    {
      return string.Format ("({0} --'{1}'--> {2})", From, Tag, To);
    }

    public Edge<TNode, TTag> Reversed()
    {
      return new Edge<TNode, TTag> (To, From, Tag);
    }
  }

  [Flags]
  public enum EdgeTag : uint
  {
    None = 0,
    FallThroughReturn = 1,
    Branch = 1 << 1,
    Return = 1 << 2,
    EndSubroutine = 1 << 3,
    True = 1 << 4,
    False = 1 << 5,
    FallThrough = 1 << 6,
    Entry = 1 << 7,
    AfterNewObj = 1 << 8 | AfterMask,
    AfterCall = 1 << 9 | AfterMask,
    Exit = 1 << 10,
    Finally = 1 << 11,
    Inherited = 1 << 12 | InheritedMask,
    BeforeCall = 1 << 13 | BeforeMask,
    BeforeNewObj = 1 << 14 | BeforeMask,
    Requires = 1 << 15,
    Assume = 1 << 16,
    Assert = 1 << 17,
    Invariant = 1 << 18,

    BeforeMask = 1 << 20,
    AfterMask = 1 << 21,
    InheritedMask = 1 << 22,
    ExtraMask = 1 << 23,
    OldMask = 1 << 24,
  }

  public static class EdgeTagExtensions
  {
    public static bool StartsWith(this EdgeTag current, EdgeTag prefix)
    {
      return (current & prefix) != EdgeTag.None;
    }
  }
}