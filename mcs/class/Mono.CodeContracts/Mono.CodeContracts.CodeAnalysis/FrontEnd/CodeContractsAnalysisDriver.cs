using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public class CodeContractsAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, MethodResult>
    : AnalysisDriverBase<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue, MethodResult>
    where Type : IEquatable<Type>
  {
    public CodeContractsAnalysisDriver(IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> basicDriver) : base (basicDriver)
    {
    }

    #region Overrides of AnalysisDriverBase<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,ExternalExpression<APC,SymbolicValue>,SymbolicValue,MethodResult>
    public override IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue> CreateMethodDriver(Method method)
    {
      return new MethodDriver (method, this);
    }
    #endregion

    #region Nested type: MethodDriver
    private class MethodDriver : BasicMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>,
                                 IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue>
    {
      private new AnalysisDriverBase<Local, Parameter, Method, Field, Property , Event,Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue, MethodResult> AnalysisDriver
      {
        get { return (AnalysisDriverBase<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue, MethodResult>) base.AnalysisDriver; }
      }

      public MethodDriver(Method method, 
                          CodeContractsAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, MethodResult> parent) 
        : base (method, parent)
      {}

      #region Implementation of IMethodDriver<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,ExternalExpression<APC,SymbolicValue>,SymbolicValue>
      
      public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, SymbolicValue, SymbolicValue,
        IValueContext<Local, Parameter, Method, Field, Type, SymbolicValue>,
        IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> ValueLayer { get; private set; }

      public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, ExternalExpression<APC, SymbolicValue>, SymbolicValue,
                        IExpressionContext<Local, Parameter, Method, Field, Type, ExternalExpression<APC, SymbolicValue>, SymbolicValue>,
                        IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> ExpressionLayer { get; private set; }

      public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, SymbolicValue, SymbolicValue,
                        IValueContext<Local, Parameter, Method, Field, Type, SymbolicValue>,
                        IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> HybridLayer { get; private set; }

      public IExpressionContext<Local, Parameter, Method, Field, Type, ExternalExpression<APC, SymbolicValue>, SymbolicValue> Context
      {
        get { return this.ExpressionLayer.ILDecoder.Context; }
      }

      public IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
      {
        get { return this.RawLayer.MetadataDecoder; }
      }

      private HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> heap_analysis;

      public void RunHeapAndExpressionAnalyses()
      {
//        if (this.)
      }
      #endregion
    }
    #endregion
  }
}