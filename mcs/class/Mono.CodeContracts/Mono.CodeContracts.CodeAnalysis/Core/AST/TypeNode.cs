using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class TypeNode : Member, IEquatable<TypeNode>
  {
    private TypeNode base_type;
    private List<Method> methods;
    private List<TypeNode> nestedTypes;
    private List<Property> properties;

    public TypeNode() : base (NodeType.TypeNode)
    {
    }

    public TypeNode(TypeReference typeReference) : this ()
    {
      TypeDefinition = typeReference as TypeDefinition ?? typeReference.Resolve ();
    }

    public TypeDefinition TypeDefinition { get; set; }

    protected IEnumerable<TypeNode> Interfaces
    {
      get { return TypeDefinition.Interfaces.Select (i => new TypeNode (i)); }
    }

    public TypeNode BaseType
    {
      get
      {
        if (this.base_type == null)
          this.base_type = new TypeNode (TypeDefinition.BaseType);
        return this.base_type;
      }
      set { this.base_type = value; }
    }

    public string FullName
    {
      get { return TypeDefinition.FullName; }
    }

    public List<Property> Properties
    {
      get
      {
        if (this.properties == null)
          this.properties = TypeDefinition.Properties.Select (it => new Property (it)).ToList ();
        return this.properties;
      }
      set { this.properties = value; }
    }

    public List<Method> Methods
    {
      get
      {
        if (this.methods == null)
          this.methods = TypeDefinition.Methods.Select (it => new Method (it)).ToList ();
        return this.methods;
      }
      set { this.methods = value; }
    }

    public List<TypeNode> NestedTypes
    {
      get
      {
        if (this.nestedTypes == null)
          this.nestedTypes = TypeDefinition.NestedTypes.Select (it => new TypeNode (it)).ToList ();
        return this.nestedTypes;
      }
      set { this.nestedTypes = value; }
    }

    public string Name
    {
      get { return TypeDefinition.Name; }
    }

    public static TypeNode GetInheritorTypeNode(TypeReference typeReference)
    {
      TypeDefinition typeDefinition = typeReference.Resolve ();
      if (typeDefinition.IsClass)
        return new Class (typeDefinition);

      return new TypeNode (typeDefinition);
    }

    public bool IsAssignableTo(TypeNode targetType)
    {
      if (this == CoreSystemTypes.Instance.TypeVoid)
        return false;
      if (targetType == this)
        return true;
      if (this == CoreSystemTypes.Instance.TypeObject)
        return false;
      if (targetType == CoreSystemTypes.Instance.TypeObject || BaseType.IsAssignableTo (targetType))
        return true;
      IEnumerable<TypeNode> interfaces = Interfaces;
      if (interfaces == null || !interfaces.Any ())
        return false;
      foreach (TypeNode iface in interfaces) {
        if (iface != null && iface.IsAssignableTo (targetType))
          return true;
      }
      return false;
    }

    public TypeNode GetReferenceType()
    {
      return null;
    }

    public override string ToString()
    {
      return string.Format ("Type({0})", FullName);
    }

    #region Implementation of IEquatable<TypeNode>
    public bool Equals(TypeNode other)
    {
      return TypeDefinition == other.TypeDefinition;
    }

    public override int GetHashCode()
    {
      return TypeDefinition.GetHashCode ();
    }

    public override bool Equals(object obj)
    {
      return Equals (obj as TypeNode);
    }
    #endregion

    #region Overrides of Member
    public override bool IsStatic
    {
      get { return false; }
    }

    public bool IsValueType
    {
      get { return this.TypeDefinition.IsValueType; }
    }
    #endregion
  }
}