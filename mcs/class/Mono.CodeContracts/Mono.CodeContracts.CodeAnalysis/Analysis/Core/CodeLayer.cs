using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public class CodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeData> : ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeData> where Type : IEquatable<Type> where ContextData : IMethodContext<Field, Method>
  {
    #region Implementation of ICodeLayer<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable,ContextData,EdgeConversionData>
    public IILDecoder<APC, Local, Parameter, Method, Field, Type, Expression, Variable, ContextData, EdgeData> ILDecoder { get; private set; }
    public IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder { get; private set; }
    public IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder { get; private set; }

    public CodeLayer(IILDecoder<APC, Local, Parameter, Method, Field, Type, Expression, Variable, ContextData, EdgeData> ilDecoder,
                     IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
                     IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder)
    {
      this.ILDecoder = ilDecoder;
      this.MetadataDecoder = metadataDecoder;
      this.ContractDecoder = contractDecoder;
    }

    public Func<AnalysisState, IFixPointInfo<APC, AnalysisState>> CreateForward<AnalysisState>(IAnalysis<APC, AnalysisState, IILVisitor<APC, Local, Parameter, Method, Field, Type, Expression, Variable, AnalysisState, AnalysisState>, EdgeData> analysis)
    {
      ForwardAnalysis<AnalysisState, Type, EdgeData> solver = ForwardAnalysis<AnalysisState, Type, EdgeData>.Make (this.ILDecoder, analysis);
      return (initialState) => {
               solver.Run (initialState);
               return solver;
             };
    }
    #endregion
  }
}