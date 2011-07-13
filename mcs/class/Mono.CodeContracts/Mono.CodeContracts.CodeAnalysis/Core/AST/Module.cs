using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Module : Node
  {
    private readonly ModuleDefinition definition;
    public Module(ModuleDefinition module ) : base (NodeType.Module)
    {
      definition = module;
    }

    public ModuleDefinition Definition
    {
      get { return this.definition; }
    }

    private List<TypeNode> types;
    

    public List<TypeNode> Types
    {
      get
      {
        if (types == null)
          types = definition.Types.Select (it => new TypeNode (it)).ToList ();

        return types;
      }
    }

    public TypeNode GetType(string ns, string className)
    {
      TypeReference firstOrDefault = this.definition.Types.FirstOrDefault(t => t.Namespace == ns && t.Name == className);
      if (firstOrDefault == null)
        return null;

      return TypeNode.GetInheritorTypeNode(firstOrDefault);
    }
  }
}