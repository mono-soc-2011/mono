using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.StackAnalysis;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public class BasicMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
    where Type : IEquatable<Type>
  {
    private readonly IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> parent;
    private ICFG contractFreeCFG;

    public Method CurrentMethod
    {
      get { return this.method; }
    }

    private readonly Method method;

    public IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> AnalysisDriver
    {
      get { return this.parent; }
    }

    private ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Dummy, Dummy, IMethodContext<Field, Method>, Dummy> contract_free_raw_layer;
    private ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, int, int, IStackContext<Field, Method>, Dummy> contract_free_stack_layer;

    public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Dummy, Dummy, IMethodContext<Field, Method>, Dummy> RawLayer { get; private set; }
    public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, int, int, IStackContext<Field, Method>, Dummy> StackLayer { get; private set; }

    public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Dummy, Dummy, IMethodContext<Field, Method>, Dummy> ContractFreeRawLayer
    {
      get
      {
        if (this.contract_free_raw_layer == null) {
          this.contract_free_raw_layer = CodeLayerFactory.Create (this.ContractFreeCFG.GetDecoder (this.parent.MetadataDecoder),
                                                                  RawLayer.MetadataDecoder,
                                                                  RawLayer.ContractDecoder);
        }
        return this.contract_free_raw_layer;
      }
    }

    public ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, int, int, IStackContext<Field, Method>, Dummy> ContractFreeStackLayer
    {
      get
      {
        if (this.contract_free_stack_layer == null) {
          
          this.contract_free_stack_layer = CodeLayerFactory.Create(StackDepthFactory.Create(ContractFreeRawLayer.ILDecoder, contract_free_raw_layer.MetadataDecoder),
                                                                   ContractFreeRawLayer.MetadataDecoder,
                                                                   ContractFreeRawLayer.ContractDecoder);
        }
        return this.contract_free_stack_layer;
      }
    }

    public ICFG ContractFreeCFG
    {
      get
      {
        if (this.contractFreeCFG == null)
          this.contractFreeCFG = new ContractFilteredCFG (this.RawLayer.ILDecoder.Context.MethodContext.CFG);
        return this.contractFreeCFG;
      }
    }

    public ICFG CFG
    {
      get { return this.StackLayer.ILDecoder.Context.MethodContext.CFG; }
    }

    public BasicMethodDriver(Method method, IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> parent )
    {
      this.method = method;
      this.parent = parent;

      this.RawLayer = CodeLayerFactory.Create (
        this.parent.MethodRepository.GetControlFlowGraph (method).GetDecoder (parent.MetadataDecoder), 
        parent.MetadataDecoder, 
        parent.ContractDecoder);
      Console.WriteLine ("-----APC based CFG-----");

      this.StackLayer = CodeLayerFactory.Create (
        StackDepthFactory.Create (this.RawLayer.ILDecoder, this.RawLayer.MetadataDecoder), 
        this.RawLayer.MetadataDecoder, 
        this.RawLayer.ContractDecoder);

      Console.WriteLine ("-----Stack based CFG-----");
    }
  }
}