using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public abstract class AnalysisDriverBase<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, MethodResult>
    : IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
    where Type : IEquatable<Type>
  {
    private readonly IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> basic_driver;

    protected AnalysisDriverBase(IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> basicDriver)
    {
      this.basic_driver = basicDriver;
    }

    #region IBasicAnalysisDriver<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly> Members
    public MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodRepository
    {
      get { return this.basic_driver.MethodRepository; }
    }

    public IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
    {
      get { return this.basic_driver.MetadataDecoder; }
    }

    public IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder
    {
      get { return this.basic_driver.ContractDecoder; }
    }
    #endregion

    public abstract IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> CreateMethodDriver(Method method);
  }
}