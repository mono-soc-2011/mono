using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public interface IBasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
  {
    MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodRepository { get; }
    IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder { get; }
    IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder { get; }
  }
}