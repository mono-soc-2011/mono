using Mono.Cecil.Cil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Local : Variable
  {
    public VariableDefinition Definition { get; private set; }

    public string Name
    {
      get { return Definition.Name; }
    }

    public Local(VariableDefinition definition)
      : base (NodeType.Local)
    {
      this.Definition = definition;
    }

    public override string ToString()
    {
      return string.Format ("Local({0})", Definition);
    }
  }
}