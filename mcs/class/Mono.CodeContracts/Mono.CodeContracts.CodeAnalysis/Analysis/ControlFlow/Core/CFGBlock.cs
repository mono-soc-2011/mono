using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public abstract class CFGBlock
  {
    public int Index;
    public abstract int Count { get; }
    public Subroutine Subroutine { get; private set; }
    public int ReversePostOrderIndex { get; set; }

    public APC First
    {
      get { return APC.ForStart(this, null); }
    }

    public APC Last
    {
      get { return APC.ForEnd (this, null); }
    }

    protected CFGBlock(Subroutine subroutine, ref int idGen)
    {
      Index = idGen++;
      Subroutine = subroutine;
    }

    public virtual bool IsMethodCallBlock<TMethod>(out TMethod calledMethod, out bool isNewObj, out bool isVirtual)
    {
      calledMethod = default(TMethod);
      isNewObj = false;
      isVirtual = false;

      return false;
    }

    public void Renumber(ref int idGen)
    {
      this.Index = idGen++;
    }

    public abstract int GetILOffset(APC pc);

    public IEnumerable<APC> APCs()
    {
      return this.APCs(null);
    }

    private IEnumerable<APC> APCs(LispList<Edge<CFGBlock, EdgeTag>> context)
    {
      for (int i = 0; i < this.Count; i++) {
        yield return new APC (this, i, context);
      }
    }
  }
}