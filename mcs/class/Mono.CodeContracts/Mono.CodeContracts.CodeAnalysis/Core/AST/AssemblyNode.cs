using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class AssemblyNode : Node
  {
    private readonly AssemblyDefinition definition;

    public AssemblyNode(AssemblyDefinition definition) : base (NodeType.Assembly)
    {
      this.definition = definition;
    }

    private IEnumerable<Module> modules;
    public string FullName
    {
      get { return this.definition.FullName; }
    }

    public IEnumerable<Module> Modules
    {
      get
      {
        if (modules == null)
          modules = this.definition.Modules.Select (it => new Module (it)).ToList ();
        return modules;
      }
    }

    public TypeNode GetType(string ns, string className)
    {
      foreach (var module in this.Modules) {
        var type = module.GetType (ns, className);
        if (type != null)
          return type;
      }
      var enumerable = this.definition.Modules.SelectMany(m=>m.Types);
      var firstOrDefault = enumerable.FirstOrDefault(t=>t.Namespace == ns && t.Name == className);
      if (firstOrDefault == null)
        return null;

      return new Class (firstOrDefault);
    }

    public static AssemblyNode ReadAssembly(string filename)
    {
      ReaderParameters readerParameters = new ReaderParameters();
      AssemblyDefinition definition = AssemblyDefinition.ReadAssembly (filename, readerParameters);

      return new AssemblyNode (definition);
    }
  }
}