using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public interface IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
    where Type : IEquatable<Type>
  {
    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable, Variable, 
      IValueContext<Local, Parameter, Method, Field, Type, Variable>,
      IImmutableMap<Variable, LispList<Variable>>> ValueLayer { get; }

    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable,
      IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable>,
      IImmutableMap<Variable, LispList<Variable>>> ExpressionLayer { get; }

    ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable, Variable, 
      IValueContext<Local, Parameter, Method, Field, Type, Variable>,
      IImmutableMap<Variable, LispList<Variable>>> HybridLayer { get; }

    IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable> Context { get; }
    IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder { get; }
    ICFG CFG { get; }
    Method CurrentMethod { get; }

    void RunHeapAndExpressionAnalyses();
  }
}