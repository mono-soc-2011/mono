namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Branch : Statement
  {
    public Expression Condition;
    public Block Target;
    private bool is_short_offset;
    private bool unsigned;
    public readonly bool LeavesExceptionBlock;

    public Branch(Expression condition, Block target, bool isShortOffset, bool unsigned, bool leavesExceptionBlock) : base (NodeType.Branch)
    {
      this.Condition = condition;
      this.Target = target;
      this.is_short_offset = isShortOffset;
      this.unsigned = unsigned;
      this.LeavesExceptionBlock = leavesExceptionBlock;
    }

    public override string ToString()
    {
      return string.Format ("Branch({0}, {1})", this.Condition == null ? "<no cond>" : Condition.ToString (), this.Target == null ? "<no target>" : this.Target.ToString ());
    }
  }

  public enum BranchOperator
  {
    Beq,
    Bge,
    Bge_Un,
    Bgt,
    Bgt_Un,
    Ble,
    Ble_Un,
    Blt,
    Blt_Un,
    Bne_un
  }
}