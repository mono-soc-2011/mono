using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.AST.Visitors;

namespace Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction
{
  public class GatherLocals : NodeInspector
  {
    private Local exemptResultLocal;
    public HashSet<Local> Locals;

    public override void VisitLocal(Local node)
    {
      if (!this.IsLocalExempt (node) && !this.Locals.Contains (node))
        this.Locals.Add (node);
      base.VisitLocal (node);
    }

    public override void VisitAssignmentStatement(AssignmentStatement node)
    {
      if (node.Target is Local && this.IsResultExpression (node.Source))
        this.exemptResultLocal = (Local) node.Target;
      base.VisitAssignmentStatement (node);
    }

    private bool IsResultExpression(Expression expression)
    {
      var methodCall = expression as MethodCall;
      if (methodCall == null)
        return false;
      
      var memberBinding = methodCall.Callee as MemberBinding;
      if (memberBinding == null)
        return false;
      
      var method = memberBinding.BoundMember as Method;
      if (method == null)
        return false;

      return method.HasGenericParameters && method.Name == "Result" && method.DeclaringType != null && method.DeclaringType.Name == "Contract";
    }

    private bool IsLocalExempt(Local local)
    {
      if (local == this.exemptResultLocal)
        return true;
      bool result = false;
      if (local.Name != null && !local.Name.StartsWith ("local"))
        result = true;
      var type = local.Type;
      if (type == null || HelperMethods.IsCompilerGenerated(type) || local.Name == "_preconditionHolds")
        return true;
      
      if (result)
        return LocalNameIsExempt (local.Name);

      return true;
    }

    private bool LocalNameIsExempt(string name)
    {
      return name.StartsWith ("CS$") || name.StartsWith ("VB$");
    }
  }
}