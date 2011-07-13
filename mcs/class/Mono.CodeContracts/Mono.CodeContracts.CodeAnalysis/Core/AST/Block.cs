using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Block : Statement
  {
    public int ILOffset { get; set; }
    public List<Statement> Statements { get; set; }
    
    public Block(List<Statement> statements) : base (NodeType.Block)
    {
      Statements = statements;
    }
    public Block() : base(NodeType.Block)
    {
    }

    public override string ToString()
    {
      return string.Format ("Block(Off:{0}, Stmts:{1})", ILOffset, Statements.Count);
    }
  }
}