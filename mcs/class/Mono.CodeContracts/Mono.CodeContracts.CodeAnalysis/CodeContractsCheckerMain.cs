using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.Providers.Implementation;

namespace Mono.CodeContracts.Static.ControlFlow
{
  public class CodeContractsCheckerMain
  {
    public static int Main(string[] args)
    {
      return CodeContractsChecker.CheckMain(args, MetaDataProvider.Instance, CodeContractDecoder.Instance, new List<string> { "SampleAssembly.dll" });
    }
  }
}