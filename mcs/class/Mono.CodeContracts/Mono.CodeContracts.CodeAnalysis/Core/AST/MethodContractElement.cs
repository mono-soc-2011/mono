namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public abstract class MethodContractElement : Node
  {
    public Expression UserMessage;

    protected MethodContractElement(NodeType nodeType) : base (nodeType)
    {
    }

    public Expression Assertion { get; set; }
  }

  public class Requires : MethodContractElement
  {
    public Requires()
      : base(NodeType.Requires)
    {
    }

    public Requires(NodeType nodeType)
      : base(nodeType)
    {
    }

    public Requires(Expression condition)
      : this()
    {
      Assertion = condition;
    }
  }

  public class Ensures : MethodContractElement
  {
    public Ensures()
      : base(NodeType.Ensures)
    {
    }

    public Ensures(NodeType nodeType)
      : base(nodeType)
    {
    }

    public Ensures(Expression condition)
      : this()
    {
      Assertion = condition;
    }
  }
}