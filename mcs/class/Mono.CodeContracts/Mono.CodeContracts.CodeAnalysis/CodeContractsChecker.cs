using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;
using Mono.CodeContracts.CodeAnalysis.FrontEnd;
using Mono.CodeContracts.CodeAnalysis.FrontEnd.Analyses;

namespace Mono.CodeContracts.Static.ControlFlow
{
  public class CodeContractsChecker
  {
    public static int CheckMain<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
      (string[] args,
       IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
       IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder,
       List<string> assemblies)
      where Type : IEquatable<Type>
    {
      using (var bindings = new Bind<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> (args, metadataDecoder, contractDecoder, assemblies)) 
        return bindings.Analyze ();
    }

    #region Nested type: Bind
    private class Bind<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
      : IDisposable
      where Type : IEquatable<Type>
    {
      private Dictionary<string, IMethodAnalysis> analyzers;
      private BasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> analysisDriver;
      private List<string> assemblies;

      public Bind(string[] args, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder, IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder, List<string> assemblies)
      {
        analyzers = new Dictionary<string, IMethodAnalysis> () {{"non-null", new NonNullAnalysis ()}};
        analysisDriver = new BasicAnalysisDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> (metadataDecoder, contractDecoder);
        this.assemblies = assemblies;
      }

      #region Implementation of IDisposable
      public void Dispose()
      {
        throw new NotImplementedException ();
      }
      #endregion

      public int Analyze()
      {
        try {
          return this.AnalyzeInternal ();
        } catch (Exception ex) {
          Console.WriteLine ("Internal error: {0}", ex.Message);
          return -1;
        }
      }

      private int AnalyzeInternal()
      {
        foreach (string assemblyPath in assemblies) {
          this.AnalyzeAssembly (assemblyPath);
        }
        return 0;
      }

      private void AnalyzeAssembly(string assemblyPath)
      {
        var metadataDecoder = this.analysisDriver.MetadataDecoder;
        Assembly assembly;
        if (!metadataDecoder.TryLoadAssembly (assemblyPath, out assembly)) {
          Console.WriteLine ("Cannot load assembly '{0}'", assemblyPath);
          return;
        }

        foreach (Method method in metadataDecoder.GetMethods(assembly)) {
          this.AnalyzeMethod (method);
        }
      }

      private void AnalyzeMethod(Method method)
      {
        var metadataDecoder = this.analysisDriver.MetadataDecoder;
        if (!metadataDecoder.HasBody(method))
          return;
        if (!metadataDecoder.FullName(method).Contains("Method"))
          return;

        this.AnalyzeMethodInternal (method);
      }

      private void AnalyzeMethodInternal(Method method)
      {
        var methodDriver = this.analysisDriver.CreateMethodDriver (method);
        Console.WriteLine (methodDriver);
      }
    }
    #endregion
  }
}