using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public class BasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
    : IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
    where Type : IEquatable<Type>
  {
    private readonly IContractProvider<Local, Parameter, Method, Field, Type> contract_decoder;
    private readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;
    private readonly MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> method_repository;

    #region Implementation of IBasicAnalysisDriver<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>
    public MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodRepository
    {
      get { return this.method_repository; }
    }

    public IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
    {
      get { return this.metadata_decoder; }
    }

    public IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder
    {
      get { return this.contract_decoder; }
    }
    #endregion

    public BasicAnalysisDriver(IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
                               IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder)
    {
      this.method_repository = new MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> (metadataDecoder, contractDecoder);
      this.metadata_decoder = metadataDecoder;
      this.contract_decoder = contractDecoder;
    }

    public BasicMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> CreateMethodDriver(Method method)
    {
      return new BasicMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> (method, this);
    }
  }
}