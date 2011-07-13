using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class MethodContract : Node
  {
    public Method DeclaringMethod;

    public MethodContract(Method method) : base (NodeType.MethodContract)
    {
      this.DeclaringMethod = method;
    }

    public List<Requires> Requires { get; set; }

    public List<Ensures> Ensures { get; set; }

    public int RequiresCount
    {
      get
      {
        List<Requires> list = Requires;
        if (list == null)
          return 0;
        return list.Count;
      }
    }

    public int EnsuresCount
    {
      get
      {
        List<Ensures> list = Ensures;
        if (list == null)
          return 0;
        return list.Count;
      }
    }
  }
}