using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public class EdgeMap<Tag> : IEnumerable<Edge<CFGBlock, Tag>>, IGraph<CFGBlock, Tag>
  {
    private readonly List<Edge<CFGBlock, Tag>> edges;

    public EdgeMap(List<Edge<CFGBlock, Tag>> edges)
    {
      this.edges = edges;
      Resort ();
    }

    #region IEnumerable<Pair<CFGBlock,Pair<Tag,CFGBlock>>> Members
    public IEnumerator<Edge<CFGBlock, Tag>> GetEnumerator()
    {
      return this.edges.GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator ();
    }
    #endregion

    #region IGraph<CFGBlock,Tag> Members
    IEnumerable<CFGBlock> IGraph<CFGBlock, Tag>.Nodes { get { throw new NotImplementedException (); } }

    IEnumerable<Pair<Tag, CFGBlock>> IGraph<CFGBlock, Tag>.Successors(CFGBlock node)
    {
      return this[node];
    }

    public EdgeMap<Tag> Reverse()
    {
      var newEdges = new List<Edge<CFGBlock, Tag>>(this.edges.Count);
      
      newEdges.AddRange (this.edges.Select (edge => edge.Reversed()));

      return new EdgeMap<Tag> (newEdges);
    }

    public ICollection<Pair<Tag, CFGBlock>> this[CFGBlock node]
    {
      get { return new Successors (this, this.FindStartIndex (node)); }
    }
    #endregion

    private struct Successors : ICollection<Pair<Tag, CFGBlock>>
    {
      private readonly EdgeMap<Tag> underlying;
      private readonly int start_index;

      public Successors(EdgeMap<Tag> underlying, int startIndex)
      {
        this.underlying = underlying;
        this.start_index = startIndex;
      }

      public IEnumerator<Pair<Tag, CFGBlock>> GetEnumerator()
      {
        var edges = this.underlying.edges;
        if (this.start_index < edges.Count) {
          int index = this.start_index;
          int blockIndex = edges[index].From.Index;
          do {
            yield return new Pair<Tag, CFGBlock> (edges[index].Tag, edges[index].To);
            ++index;
          } while (index < edges.Count && edges[index].From.Index == blockIndex);
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator ();
      }

      public void Add(Pair<Tag, CFGBlock> item)
      {
        throw new InvalidOperationException();
      }

      public void Clear()
      {
        throw new InvalidOperationException();
      }

      public bool Contains(Pair<Tag, CFGBlock> item)
      {
        throw new NotImplementedException ();
      }

      public void CopyTo(Pair<Tag, CFGBlock>[] array, int arrayIndex)
      {
        throw new NotImplementedException ();
      }

      public bool Remove(Pair<Tag, CFGBlock> item)
      {
        throw new InvalidOperationException();
      }

      public int Count
      {
        get
        {
          int index = this.start_index;
          var edges = this.underlying.edges;
          if (index >= edges.Count)
            return 0;
          int blockIndex = edges[index].From.Index;
          
          int count = 0;
          do { ++count; ++index; } while (index < edges.Count && edges[index].From.Index == blockIndex);

          return count;
        }
      }

      public bool IsReadOnly { get { return true; } }
    }

    private static int CompareFirstBlockIndex(Edge<CFGBlock, Tag> edge1, Edge<CFGBlock, Tag> edge2)
    {
      int cmp = edge1.From.Index - edge2.From.Index;
      if (cmp == 0)
        cmp = edge1.To.Index - edge2.To.Index;

      return cmp;
    }

    private int FindStartIndex(CFGBlock from)
    {
      //binary search
      int l = 0;
      int r = this.edges.Count;
      while (l < r) {
        int median = (l + r) / 2;
        int medianBlockIndex = this.edges[median].From.Index;

        if (medianBlockIndex == from.Index) {
          while (median > 0 && this.edges[median - 1].From.Index == medianBlockIndex)
            --median;
          return median;
        }

        if (medianBlockIndex < from.Index)
          l = median + 1;
        else
          r = median;
      }

      return this.edges.Count;
    }

    public void Filter(Predicate<Edge<CFGBlock, Tag>> keep)
    {
      var notKeepEdges = new List<int> ();
      for (int i = 0; i < this.edges.Count; i++) {
        if (!keep(this.edges[i]))
          notKeepEdges.Add (i);
      }

      foreach (var i in notKeepEdges)
        this.edges.RemoveAt (i);
    }

    public void Resort()
    {
      this.edges.Sort(CompareFirstBlockIndex);
    }
  }
}