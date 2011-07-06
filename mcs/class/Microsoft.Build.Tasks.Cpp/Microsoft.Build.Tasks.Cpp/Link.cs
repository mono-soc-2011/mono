//
// Link.cs: Task for C++ linker
// 
// For an overview of the Link task options, you can check:
//  http://msdn.microsoft.com/en-us/library/ee862471.aspx
//
// For an overview of the LINK tool options, you can check:
//  http://msdn.microsoft.com/en-us/library/y0zzbyt4.aspx
//
// Author:
//   João Matos (triton@vapor3d.org)
// 
// (C) 2011 João Matos
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks.Cpp
{
	public class Link : ToolTaskExtension
	{
		public Link()
		{
		}

		[MonoTODO]
		protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
		{
			if (Bag["SuppressStartupBanner"] != null)
				commandLine.AppendSwitch("/nologo");

			if (Bag["AdditionalDependencies"] != null)
			{
				foreach (String dep in AdditionalDependencies)
				{
					commandLine.AppendSwitch(dep);
				}
			}

			if (Bag["AdditionalLibraryDirectories"] != null)
			{
				foreach (String add in AdditionalLibraryDirectories)
				{
					commandLine.AppendSwitch("/LIBPATH:" + add);
				}
			}

			if (Bag["AdditionalManifestDependencies"] != null)
			{
				foreach (String add in AdditionalManifestDependencies)
				{
					commandLine.AppendSwitch("/MANIFESTDEPENDENCY:" + add);
				}
			}

			if (Bag["AdditionalOptions"] != null)
			{
				commandLine.AppendSwitch(AdditionalOptions);
			}

			if (Bag["AddModuleNamesToAssembly"] != null)
			{
				foreach (String add in AddModuleNamesToAssembly)
				{
					commandLine.AppendSwitch("/ASSEMBLYMODULE:" + add);
				}
			}

			if (Bag["AllowIsolation"] != null)
			{
				if (AllowIsolation)
					commandLine.AppendSwitch("/ALLOWISOLATION");
			}

			if (Bag["AssemblyDebug"] != null)
			{
				if (AllowIsolation)
					commandLine.AppendSwitch("/ASSEMBLYDEBUG");
			}

			if (Bag["AssemblyLinkResource"] != null)
			{
				foreach (String add in AssemblyLinkResource)
				{
					commandLine.AppendSwitch("/ASSEMBLYLINKRESOURCE:" + add);
				}
			}

			if (Bag["BaseAddress"] != null)
			{
				commandLine.AppendSwitch("/BASE:" + BaseAddress);
			}

			if (Bag["DataExecutionPrevention"] != null)
			{
				if (DataExecutionPrevention)
					commandLine.AppendSwitch("/NXCOMPAT");
			}

			if (Bag["DelayLoadDLLs"] != null)
			{
				foreach (String add in DelayLoadDLLs)
				{
					commandLine.AppendSwitch("/DELAYSIGN:" + add);
				}
			}

			if (Bag["DelaySign"] != null)
			{
				if (DelaySign)
					commandLine.AppendSwitch("/DELAYSIGN");
			}

			if (Bag["EmbedManagedResourceFile"] != null)
			{
				foreach (String add in EmbedManagedResourceFile)
				{
					commandLine.AppendSwitch("/ASSEMBLYRESOURCE:" + add);
				}
			}

			if (Bag["EnableUAC"] != null)
			{
				if (EnableUAC)
					commandLine.AppendSwitch("/MANIFESTUAC");
			}

			if (Bag["EntryPointSymbol"] != null)
			{
				commandLine.AppendSwitch("/ENTRY" + EntryPointSymbol);
			}

			if (Bag["FixedBaseAddress"] != null)
			{
				if (FixedBaseAddress)
					commandLine.AppendSwitch("/FIXED");
			}

			if (Bag["ForceSymbolReferences"] != null)
			{
				foreach (String add in ForceSymbolReferences)
				{
					commandLine.AppendSwitch("/INCLUDE:" + add);
				}
			}

			if (Bag["FunctionOrder"] != null)
			{
				commandLine.AppendSwitch("/ORDER" + EntryPointSymbol);
			}

			if (Bag["GenerateDebugInformation"] != null)
			{
				if (GenerateDebugInformation)
					commandLine.AppendSwitch("/DEBUG");
			}

			if (Bag["GenerateManifest"] != null)
			{
				if (GenerateManifest)
					commandLine.AppendSwitch("/MANIFEST");
			}

			if (Bag["GenerateMapFile"] != null)
			{
				if (GenerateMapFile)
					commandLine.AppendSwitch("/MAP");
			}

			if ((Bag["HeapCommitSize"] != null) || (Bag["HeapReserveSize"] != null))
			{
				String heap = "";
				if (Bag["HeapReserveSize"] != null) heap += HeapReserveSize;
				if (Bag["HeapCommitSize"] != null) heap += "," + HeapCommitSize;
				commandLine.AppendSwitch("/HEAP" + heap);
			}

			if (Bag["IgnoreAllDefaultLibraries"] != null)
			{
				if (IgnoreAllDefaultLibraries)
					commandLine.AppendSwitch("/NODEFAULTLIB");
			}

			if (Bag["IgnoreEmbeddedIDL"] != null)
			{
				if (IgnoreEmbeddedIDL)
					commandLine.AppendSwitch("/IGNOREIDL");
			}

			if (Bag["IgnoreSpecificDefaultLibraries"] != null)
			{
				String libs = "";
				bool first = true;
				foreach (String add in IgnoreSpecificDefaultLibraries)
				{
					if (!first) libs += ";";
					libs += add;
					first = false;
				}
				commandLine.AppendSwitch("/NODEFAULTLIB:" + libs);
			}

			if (Bag["ImageHasSafeExceptionHandlers"] != null)
			{
				if (ImageHasSafeExceptionHandlers)
					commandLine.AppendSwitch("/SAFESEH");
			}

			if (Bag["ImportLibrary"] != null)
			{
				commandLine.AppendSwitch("/IMPLIB" + ImportLibrary);
			}

			if (Bag["KeyContainer"] != null)
			{
				commandLine.AppendSwitch("/KEYCONTAINER" + KeyContainer);
			}

			if (Bag["KeyFile"] != null)
			{
				commandLine.AppendSwitch("/KEYFILE" + KeyFile);
			}

			if (Bag["LargeAddressAware"] != null)
			{
				if (LargeAddressAware)
					commandLine.AppendSwitch("/LARGEADDRESSAWARE");
			}

			if (Bag["LinkDLL"] != null)
			{
				if (LinkDLL)
					commandLine.AppendSwitch("/DLL");
			}

			if (Bag["LinkIncremental"] != null)
			{
				if (LinkIncremental)
					commandLine.AppendSwitch("/INCREMENTAL");
			}

			if (Bag["LinkStatus"] != null)
			{
				if (LinkStatus)
					commandLine.AppendSwitch("/LTCG" + ":STATUS");
			}

			if (Bag["ManifestFile"] != null)
			{
				commandLine.AppendSwitch("/MANIFESTFILE" + ManifestFile);
			}

			if (Bag["MapExports"] != null)
			{
				if (MapExports)
					commandLine.AppendSwitch("/MAPINFO");
			}

			if (Bag["MapFileName"] != null)
			{
			}

			if (Bag["MergedIDLBaseFileName"] != null)
			{
				commandLine.AppendSwitch("/IDLOUT" + MergedIDLBaseFileName);
			}

			if (Bag["MergeSections"] != null)
			{
				commandLine.AppendSwitch("/MERGE" + MergeSections);
			}

			if (Bag["MinimumRequiredVersion"] != null)
			{
				throw new NotImplementedException();
				commandLine.AppendSwitch(MinimumRequiredVersion);
			}

			if (Bag["ModuleDefinitionFile"] != null)
			{
				commandLine.AppendSwitch("/DEF" + ModuleDefinitionFile);
			}

			if (Bag["MSDOSStubFileName"] != null)
			{
				commandLine.AppendSwitch("/STUB" + MSDOSStubFileName);
			}

			if (Bag["NoEntryPoint"] != null)
			{
				if (NoEntryPoint)
					commandLine.AppendSwitch("/NOENTRY");
			}
		}

		[MonoTODO]
		protected override bool HandleTaskExecutionErrors()
		{
			if (!Log.HasLoggedErrors && ExitCode != 0)
				Log.LogError("Compiler crashed with code: {0}.", ExitCode);

			return ExitCode == 0 && !Log.HasLoggedErrors;
		}

		[MonoTODO]
		protected bool ListHasNoDuplicateItems(ITaskItem[] itemList,
							string parameterName)
		{
			Dictionary<string, object> items = new Dictionary<string, object>();

			foreach (ITaskItem item in itemList)
			{
				if (!items.ContainsKey(item.ItemSpec))
					items.Add(item.ItemSpec, null);
				else
					return false;
			}

			return true;
		}

		[MonoTODO]
		protected override bool ValidateParameters()
		{
			return true;
		}

		public String[] AdditionalDependencies
		{
			get { return (String[])Bag["AdditionalDependencies"]; }
			set { Bag["AdditionalDependencies"] = value; }
		}

		public String[] AdditionalLibraryDirectories
		{
			get { return (String[])Bag["AdditionalLibraryDirectories"]; }
			set { Bag["AdditionalLibraryDirectories"] = value; }
		}

		public String[] AdditionalManifestDependencies
		{
			get { return (String[])Bag["AdditionalManifestDependencies"]; }
			set { Bag["AdditionalManifestDependencies"] = value; }
		}

		public String AdditionalOptions
		{
			get { return (String)Bag["AdditionalOptions"]; }
			set { Bag["AdditionalOptions"] = value; }
		}

		public String[] AddModuleNamesToAssembly
		{
			get { return (String[])Bag["AddModuleNamesToAssembly"]; }
			set { Bag["AddModuleNamesToAssembly"] = value; }
		}

		public bool AllowIsolation
		{
			get { return (bool)Bag["AllowIsolation"]; }
			set { Bag["AllowIsolation"] = value; }
		}

		public bool AssemblyDebug
		{
			get { return (bool)Bag["AssemblyDebug"]; }
			set { Bag["AssemblyDebug"] = value; }
		}

		public String[] AssemblyLinkResource
		{
			get { return (String[])Bag["AssemblyLinkResource"]; }
			set { Bag["AssemblyLinkResource"] = value; }
		}

		public bool AttributeFileTracking
		{
			// Enables deeper file tracking to capture link incremental's behavior. Always returns true.
			//get { return (bool)Bag["AttributeFileTracking"]; }
			get { return true; }
			set { Bag["AttributeFileTracking"] = value; }
		}

		public String BaseAddress
		{
			get { return (String)Bag["BaseAddress"]; }
			set { Bag["BaseAddress"] = value; }
		}

		public bool BuildingInIDE
		{
			get { return (bool)Bag["BuildingInIDE"]; }
			set { Bag["BuildingInIDE"] = value; }
		}

		public String CLRImageType
		{
			get { return (String)Bag["CLRImageType"]; }
			set { Bag["CLRImageType"] = value; }
		}

		public String CLRSupportLastError
		{
			get { return (String)Bag["CLRSupportLastError"]; }
			set { Bag["CLRSupportLastError"] = value; }
		}

		public String CLRThreadAttribute
		{
			get { return (String)Bag["CLRThreadAttribute"]; }
			set { Bag["CLRThreadAttribute"] = value; }
		}

		public bool CLRUnmanagedCodeCheck
		{
			get { return (bool)Bag["CLRUnmanagedCodeCheck"]; }
			set { Bag["CLRUnmanagedCodeCheck"] = value; }
		}

		public String CreateHotPatchableImage
		{
			get { return (String)Bag["CreateHotPatchableImage"]; }
			set { Bag["CreateHotPatchableImage"] = value; }
		}

		public bool DataExecutionPrevention
		{
			get { return (bool)Bag["DataExecutionPrevention"]; }
			set { Bag["DataExecutionPrevention"] = value; }
		}

		public String[] DelayLoadDLLs
		{
			get { return (String[])Bag["DelayLoadDLLs"]; }
			set { Bag["DelayLoadDLLs"] = value; }
		}

		public bool DelaySign
		{
			get { return (bool)Bag["DelaySign"]; }
			set { Bag["DelaySign"] = value; }
		}

		public String Driver
		{
			get { return (String)Bag["Driver"]; }
			set { Bag["Driver"] = value; }
		}

		public String[] EmbedManagedResourceFile
		{
			get { return (String[])Bag["EmbedManagedResourceFile"]; }
			set { Bag["EmbedManagedResourceFile"] = value; }
		}

		public bool EnableCOMDATFolding
		{
			get { return (bool)Bag["EnableCOMDATFolding"]; }
			set { Bag["EnableCOMDATFolding"] = value; }
		}

		public bool EnableUAC
		{
			get { return (bool)Bag["EnableUAC"]; }
			set { Bag["EnableUAC"] = value; }
		}

		public String EntryPointSymbol
		{
			get { return (String)Bag["EntryPointSymbol"]; }
			set { Bag["EntryPointSymbol"] = value; }
		}

		public bool FixedBaseAddress
		{
			get { return (bool)Bag["FixedBaseAddress"]; }
			set { Bag["FixedBaseAddress"] = value; }
		}

		public String ForceFileOutput
		{
			get { return (String)Bag["ForceFileOutput"]; }
			set { Bag["ForceFileOutput"] = value; }
		}

		public String[] ForceSymbolReferences
		{
			get { return (String[])Bag["ForceSymbolReferences"]; }
			set { Bag["ForceSymbolReferences"] = value; }
		}

		public String FunctionOrder
		{
			get { return (String)Bag["FunctionOrder"]; }
			set { Bag["FunctionOrder"] = value; }
		}

		public bool GenerateDebugInformation
		{
			get { return (bool)Bag["GenerateDebugInformation"]; }
			set { Bag["GenerateDebugInformation"] = value; }
		}

		public bool GenerateManifest
		{
			get { return (bool)Bag["GenerateManifest"]; }
			set { Bag["GenerateManifest"] = value; }
		}

		public bool GenerateMapFile
		{
			get { return (bool)Bag["GenerateMapFile"]; }
			set { Bag["GenerateMapFile"] = value; }
		}

		public String HeapCommitSize
		{
			get { return (String)Bag["HeapCommitSize"]; }
			set { Bag["HeapCommitSize"] = value; }
		}

		public String HeapReserveSize
		{
			get { return (String)Bag["HeapReserveSize"]; }
			set { Bag["HeapReserveSize"] = value; }
		}

		public bool IgnoreAllDefaultLibraries
		{
			get { return (bool)Bag["IgnoreAllDefaultLibraries"]; }
			set { Bag["IgnoreAllDefaultLibraries"] = value; }
		}

		public bool IgnoreEmbeddedIDL
		{
			get { return (bool)Bag["IgnoreEmbeddedIDL"]; }
			set { Bag["IgnoreEmbeddedIDL"] = value; }
		}

		public bool IgnoreImportLibrary
		{
			get { return (bool)Bag["IgnoreImportLibrary"]; }
			set { Bag["IgnoreImportLibrary"] = value; }
		}

		public String[] IgnoreSpecificDefaultLibraries
		{
			get { return (String[])Bag["IgnoreSpecificDefaultLibraries"]; }
			set { Bag["IgnoreSpecificDefaultLibraries"] = value; }
		}

		public bool ImageHasSafeExceptionHandlers
		{
			get { return (bool)Bag["ImageHasSafeExceptionHandlers"]; }
			set { Bag["ImageHasSafeExceptionHandlers"] = value; }
		}

		public String ImportLibrary
		{
			get { return (String)Bag["ImportLibrary"]; }
			set { Bag["ImportLibrary"] = value; }
		}

		public String KeyContainer
		{
			get { return (String)Bag["KeyContainer"]; }
			set { Bag["KeyContainer"] = value; }
		}

		public String KeyFile
		{
			get { return (String)Bag["KeyFile"]; }
			set { Bag["KeyFile"] = value; }
		}

		public bool LargeAddressAware
		{
			get { return (bool)Bag["LargeAddressAware"]; }
			set { Bag["LargeAddressAware"] = value; }
		}

		public bool LinkDLL
		{
			get { return (bool)Bag["LinkDLL"]; }
			set { Bag["LinkDLL"] = value; }
		}

		public String LinkErrorReporting
		{
			get { return (String)Bag["LinkErrorReporting"]; }
			set { Bag["LinkErrorReporting"] = value; }
		}

		public bool LinkIncremental
		{
			get { return (bool)Bag["LinkIncremental"]; }
			set { Bag["LinkIncremental"] = value; }
		}

		public bool LinkLibraryDependencies
		{
			get { return (bool)Bag["LinkLibraryDependencies"]; }
			set { Bag["LinkLibraryDependencies"] = value; }
		}

		public bool LinkStatus
		{
			get { return (bool)Bag["LinkStatus"]; }
			set { Bag["LinkStatus"] = value; }
		}

		public String LinkTimeCodeGeneration
		{
			get { return (String)Bag["LinkTimeCodeGeneration"]; }
			set { Bag["LinkTimeCodeGeneration"] = value; }
		}

		public String ManifestFile
		{
			get { return (String)Bag["ManifestFile"]; }
			set { Bag["ManifestFile"] = value; }
		}

		public bool MapExports
		{
			get { return (bool)Bag["MapExports"]; }
			set { Bag["MapExports"] = value; }
		}

		public String MapFileName
		{
			get { return (String)Bag["MapFileName"]; }
			set { Bag["MapFileName"] = value; }
		}

		public String MergedIDLBaseFileName
		{
			get { return (String)Bag["MergedIDLBaseFileName"]; }
			set { Bag["MergedIDLBaseFileName"] = value; }
		}

		public String MergeSections
		{
			get { return (String)Bag["MergeSections"]; }
			set { Bag["MergeSections"] = value; }
		}

		public String MidlCommandFile
		{
			get { return (String)Bag["MidlCommandFile"]; }
			set { Bag["MidlCommandFile"] = value; }
		}

		public String MinimumRequiredVersion
		{
			get { return (String)Bag["MinimumRequiredVersion"]; }
			set { Bag["MinimumRequiredVersion"] = value; }
		}

		public String ModuleDefinitionFile
		{
			get { return (String)Bag["ModuleDefinitionFile"]; }
			set { Bag["ModuleDefinitionFile"] = value; }
		}

		public String MSDOSStubFileName
		{
			get { return (String)Bag["MSDOSStubFileName"]; }
			set { Bag["MSDOSStubFileName"] = value; }
		}

		public bool NoEntryPoint
		{
			get { return (bool)Bag["NoEntryPoint"]; }
			set { Bag["NoEntryPoint"] = value; }
		}

		public String[] ObjectFiles
		{
			get { return (String[])Bag["ObjectFiles"]; }
			set { Bag["ObjectFiles"] = value; }
		}

		public bool OptimizeReferences
		{
			get { return (bool)Bag["OptimizeReferences"]; }
			set { Bag["OptimizeReferences"] = value; }
		}

		public String OutputFile
		{
			get { return (String)Bag["OutputFile"]; }
			set { Bag["OutputFile"] = value; }
		}

		public bool PerUserRedirection
		{
			get { return (bool)Bag["PerUserRedirection"]; }
			set { Bag["PerUserRedirection"] = value; }
		}

		public ITaskItem[] PreprocessOutput
		{
			get { return (ITaskItem[])Bag["PreprocessOutput"]; }
			set { Bag["PreprocessOutput"] = value; }
		}

		public bool PreventDllBinding
		{
			get { return (bool)Bag["PreventDllBinding"]; }
			set { Bag["PreventDllBinding"] = value; }
		}

		public bool Profile
		{
			get { return (bool)Bag["Profile"]; }
			set { Bag["Profile"] = value; }
		}

		public String ProfileGuidedDatabase
		{
			get { return (String)Bag["ProfileGuidedDatabase"]; }
			set { Bag["ProfileGuidedDatabase"] = value; }
		}

		public String ProgramDatabaseFile
		{
			get { return (String)Bag["ProgramDatabaseFile"]; }
			set { Bag["ProgramDatabaseFile"] = value; }
		}

		public bool RandomizedBaseAddress
		{
			get { return (bool)Bag["RandomizedBaseAddress"]; }
			set { Bag["RandomizedBaseAddress"] = value; }
		}

		public bool RegisterOutput
		{
			get { return (bool)Bag["RegisterOutput"]; }
			set { Bag["RegisterOutput"] = value; }
		}

		public int SectionAlignment
		{
			get { return (int)Bag["SectionAlignment"]; }
			set { Bag["SectionAlignment"] = value; }
		}

		public bool SetChecksum
		{
			get { return (bool)Bag["SetChecksum"]; }
			set { Bag["SetChecksum"] = value; }
		}

		public String ShowProgress
		{
			get { return (String)Bag["ShowProgress"]; }
			set { Bag["ShowProgress"] = value; }
		}

		public ITaskItem[] Sources
		{
			get { return (ITaskItem[])Bag["Sources"]; }
			set { Bag["Sources"] = value; }
		}

		public String SpecifySectionAttributes
		{
			get { return (String)Bag["SpecifySectionAttributes"]; }
			set { Bag["SpecifySectionAttributes"] = value; }
		}

		public String StackCommitSize
		{
			get { return (String)Bag["StackCommitSize"]; }
			set { Bag["StackCommitSize"] = value; }
		}

		public String StackReserveSize
		{
			get { return (String)Bag["StackReserveSize"]; }
			set { Bag["StackReserveSize"] = value; }
		}

		public String StripPrivateSymbols
		{
			get { return (String)Bag["StripPrivateSymbols"]; }
			set { Bag["StripPrivateSymbols"] = value; }
		}

		public String SubSystem
		{
			get { return (String)Bag["SubSystem"]; }
			set { Bag["SubSystem"] = value; }
		}

		public bool SupportNobindOfDelayLoadedDLL
		{
			get { return (bool)Bag["SupportNobindOfDelayLoadedDLL"]; }
			set { Bag["SupportNobindOfDelayLoadedDLL"] = value; }
		}

		public bool SupportUnloadOfDelayLoadedDLL
		{
			get { return (bool)Bag["SupportUnloadOfDelayLoadedDLL"]; }
			set { Bag["SupportUnloadOfDelayLoadedDLL"] = value; }
		}

		public bool SuppressStartupBanner
		{
			get { return (bool)Bag["SuppressStartupBanner"]; }
			set { Bag["SuppressStartupBanner"] = value; }
		}

		public bool SwapRunFromCD
		{
			get { return (bool)Bag["SwapRunFromCD"]; }
			set { Bag["SwapRunFromCD"] = value; }
		}

		public bool SwapRunFromNET
		{
			get { return (bool)Bag["SwapRunFromNET"]; }
			set { Bag["SwapRunFromNET"] = value; }
		}

		public String TargetMachine
		{
			get { return (String)Bag["TargetMachine"]; }
			set { Bag["TargetMachine"] = value; }
		}

		public bool TerminalServerAware
		{
			get { return (bool)Bag["TerminalServerAware"]; }
			set { Bag["TerminalServerAware"] = value; }
		}

		public String TrackerLogDirectory
		{
			get { return (String)Bag["TrackerLogDirectory"]; }
			set { Bag["TrackerLogDirectory"] = value; }
		}

		public bool TreatLinkerWarningAsErrors
		{
			get { return (bool)Bag["TreatLinkerWarningAsErrors"]; }
			set { Bag["TreatLinkerWarningAsErrors"] = value; }
		}

		public bool TurnOffAssemblyGeneration
		{
			get { return (bool)Bag["TurnOffAssemblyGeneration"]; }
			set { Bag["TurnOffAssemblyGeneration"] = value; }
		}

		public String TypeLibraryFile
		{
			get { return (String)Bag["TypeLibraryFile"]; }
			set { Bag["TypeLibraryFile"] = value; }
		}

		public int TypeLibraryResourceID
		{
			get { return (int)Bag["TypeLibraryResourceID"]; }
			set { Bag["TypeLibraryResourceID"] = value; }
		}

		public String UACExecutionLevel
		{
			get { return (String)Bag["UACExecutionLevel"]; }
			set { Bag["UACExecutionLevel"] = value; }
		}

		public String UACUIAccess
		{
			get { return (String)Bag["UACUIAccess"]; }
			set { Bag["UACUIAccess"] = value; }
		}

		public bool UseLibraryDependencyInputs
		{
			get { return (bool)Bag["UseLibraryDependencyInputs"]; }
			set { Bag["UseLibraryDependencyInputs"] = value; }
		}

		public String Version
		{
			get { return (String)Bag["Version"]; }
			set { Bag["Version"] = value; }
		}

		protected override string GenerateFullPathToTool()
		{
			throw new NotImplementedException();
		}

		protected override string ToolName
		{
			get { return "link.exe"; }
		}
	}
}

#endif
