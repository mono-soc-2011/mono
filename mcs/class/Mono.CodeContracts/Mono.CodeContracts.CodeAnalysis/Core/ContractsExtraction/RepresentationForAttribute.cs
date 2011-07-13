using System;

namespace Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction
{
  [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  internal sealed class RepresentationForAttribute : Attribute
  {
    public string RuntimeName { get; private set; }
    public bool IsRequired { get; private set; }

    public RepresentationForAttribute(string runtimeName)
      : this(runtimeName, true)
    {
    }

    private RepresentationForAttribute(string runtimeName, bool isRequired)
    {
      this.RuntimeName = runtimeName;
      this.IsRequired = isRequired;
    }
  }
}