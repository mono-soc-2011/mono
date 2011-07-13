using Mono.Cecil.Cil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class BasicStatement : Statement
  {
    private OpCode opCode;

    public BasicStatement(OpCode opCode)
      : base (NodeType.Instruction)
    {
      this.opCode = opCode;
    }

    public BasicStatement() : base (NodeType.Instruction)
    {
    }
  }
}