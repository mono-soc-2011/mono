using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Class : TypeNode
  {
    public Class(TypeDefinition firstOrDefault) : base(firstOrDefault)
    {
      this.NodeType = NodeType.Class;
    }

    public IEnumerable<Method> GetMethods(string name, params TypeNode[] args)
    {
      var enumerable = this.Methods.Where (m => m.Name == name);
      foreach (var method in enumerable) {
        var parameters = method.Parameters;
        bool ok = true;
        if (args.Length != parameters.Count) 
          continue;
        
        for (int i = 0; i < args.Length; i++) {
          if (!parameters[i].Type.Equals (args[i])) {
            ok = false;
            break;
          }
        }

        if (ok)
          yield return method;
      }
    }

    public Method GetMethod(string name, params TypeNode[] args)
    {
      return GetMethods (name, args).FirstOrDefault ();
    }
  }
}