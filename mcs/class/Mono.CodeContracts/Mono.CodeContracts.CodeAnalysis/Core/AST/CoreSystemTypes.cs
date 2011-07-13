using System;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public sealed class CoreSystemTypes
  {
    private static CoreSystemTypes _instance;
    private static CoreSystemTypes GetOrCreateInstance(ModuleDefinition module)
    {
      if (_instance == null)
        _instance = new CoreSystemTypes (module);

      return _instance;
    }

    public static ModuleDefinition ModuleDefinition { get; set; }
    public static CoreSystemTypes Instance { get { return GetOrCreateInstance (ModuleDefinition); } }

    private readonly ModuleDefinition Module;
    private Lazy<TypeNode> typeByte;
    private Lazy<TypeNode> typeDouble;
    private Lazy<TypeNode> typeInt16;
    private Lazy<TypeNode> typeInt32;
    private Lazy<TypeNode> typeInt64;
    private Lazy<TypeNode> typeSByte;
    private Lazy<TypeNode> typeSingle;
    private Lazy<TypeNode> typeUInt32;
    private Lazy<TypeNode> typeUInt64;

    private Lazy<TypeNode> typeVoid;
    private Lazy<TypeNode> typeBoolean;
    private Lazy<TypeNode> typeObject;

    public TypeNode TypeObject
    {
      get { return this.typeObject.Value; }
    }

    private Lazy<TypeNode> typeString;

    public TypeNode TypeString
    {
      get { return this.typeString.Value; }
    }

    public CoreSystemTypes(ModuleDefinition module)
    {
      this.Module = module;

      InitializeLazyTypes ();
    }

    public TypeNode TypeBoolean
    {
      get { return this.typeBoolean.Value; }
    }
    public TypeNode TypeVoid
    {
      get { return this.typeVoid.Value; }
    }
    public TypeNode TypeSByte
    {
      get { return this.typeSByte.Value; }
    }
    public TypeNode TypeByte
    {
      get { return this.typeByte.Value; }
    }
    public TypeNode TypeInt16
    {
      get { return this.typeInt16.Value; }
    }
    public TypeNode TypeInt32
    {
      get { return this.typeInt32.Value; }
    }
    public TypeNode TypeInt64
    {
      get { return this.typeInt64.Value; }
    }
    public TypeNode TypeSingle
    {
      get { return this.typeSingle.Value; }
    }
    public TypeNode TypeDouble
    {
      get { return this.typeDouble.Value; }
    }
    public TypeNode TypeUInt32
    {
      get { return this.typeUInt32.Value; }
    }
    public TypeNode TypeUInt64
    {
      get { return this.typeUInt64.Value; }
    }

    private void InitializeLazyTypes()
    {
      this.typeVoid =    new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (void))));
      this.typeSByte =   new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (SByte))));
      this.typeByte =    new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Byte))));
      this.typeInt16 =   new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Int16))));
      this.typeInt32 =   new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Int32))));
      this.typeInt64 =   new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Int64))));
      this.typeUInt32 =  new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (UInt32))));
      this.typeUInt64 =  new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (UInt64))));
      this.typeSingle =  new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Single))));
      this.typeDouble =  new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Double))));
      this.typeBoolean = new Lazy<TypeNode> (() => new TypeNode(this.Module.Import (typeof (Boolean))));
      this.typeObject = new Lazy<TypeNode> (() => new TypeNode (this.Module.Import (typeof (object))));
      this.typeString = new Lazy<TypeNode> (() => new TypeNode (this.Module.Import (typeof (string))));
    }
  }
}