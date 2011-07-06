//
// LIB.cs: Task for C++ library tool
// 
// For an overview of the LIB task options, you can check:
//  http://msdn.microsoft.com/en-us/library/ee862484.aspx
//
// For an overview of the LIB options, you can check:
//  http://msdn.microsoft.com/en-us/library/0xb6w1f8.aspx
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
	public class LIB : ToolTaskExtension
	{
		public LIB()
		{
		}

		[MonoTODO]
		protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
		{
			if (Bag["SuppressStartupBanner"] != null)
				commandLine.AppendSwitch("/NOLOGO");

			if (Bag["AdditionalDependencies"] != null)
			{
				foreach (String dep in AdditionalDependencies)
				{
					commandLine.AppendFileNameIfNotNull(dep);
				}
			}

			if (Bag["AdditionalLibraryDirectories"] != null)
			{
				foreach (String add in AdditionalLibraryDirectories)
				{
					commandLine.AppendSwitch("/LIBPATH:" + add);
				}
			}

			if (Bag["AdditionalOptions"] != null)
			{
				commandLine.AppendSwitch(AdditionalOptions);
			}

			if (Bag["DisplayLibrary"] != null)
			{
				commandLine.AppendSwitch("/LIST" + DisplayLibrary);
			}

			if (Bag["ErrorReporting"] != null)
			{
				string err = "";
				if (ErrorReporting == "NoErrorReport") err = "NONE";
				else if (ErrorReporting == "PromptImmediately") err = "PROMPT";
				else if (ErrorReporting == "QueueForNextLogin") err = "QUEUE";
				else if (ErrorReporting == "SendErrorReport") err = "SEND";
				if(err.Length != 0)
					commandLine.AppendSwitch("/ERRORREPORT:" + err);
			}

			if (Bag["ExportNamedFunctions"] != null)
			{
				foreach (string exp in ExportNamedFunctions)
					commandLine.AppendSwitch("/EXPORT:" + exp);
			}

			if (Bag["ForceSymbolReferences"] != null)
			{
				commandLine.AppendSwitch("/INCLUDE" + ForceSymbolReferences);
			}

			if (Bag["IgnoreAllDefaultLibraries"] != null)
			{
				if(IgnoreAllDefaultLibraries)
					commandLine.AppendSwitch("/NODEFAULTLIB");
			}

			if (Bag["IgnoreSpecificDefaultLibraries"] != null)
			{
				foreach (string lib in IgnoreSpecificDefaultLibraries)
					commandLine.AppendSwitch("/NODEFAULTLIB:" + lib);
			}

			if (Bag["LinkLibraryDependencies"] != null)
			{
				if(LinkLibraryDependencies)
					throw new NotImplementedException();
			}

			if (Bag["LinkTimeCodeGeneration"] != null)
			{
				if (LinkTimeCodeGeneration)
					commandLine.AppendSwitch("/LTCG");
			}

			if (Bag["MinimumRequiredVersion"] != null)
			{
				throw new NotImplementedException();
			}

			if (Bag["ModuleDefinitionFile"] != null)
			{
				commandLine.AppendSwitch("/DEF:" + ModuleDefinitionFile);
			}

			if (Bag["Name"] != null)
			{
				commandLine.AppendSwitch("/NAME:" + Name);
			}

			if (Bag["OutputFile"] != null)
			{
				commandLine.AppendSwitch("/OUT:" + OutputFile);
			}

			if (Bag["RemoveObjects"] != null)
			{
				foreach (string obj in RemoveObjects)
					commandLine.AppendSwitch("/REMOVE:" + obj);
			}

			if (Bag["Sources"] != null)
			{
				foreach (ITaskItem item in Sources)
					commandLine.AppendFileNameIfNotNull(item.ItemSpec);
			}

			if (Bag["SubSystem"] != null)
			{
				string sys = "";
				if (SubSystem == "Console") sys = "CONSOLE";
				else if (SubSystem == "Windows") sys = "WINDOWS";
				else if (SubSystem == "Native") sys = "NATIVE";
				else if (SubSystem == "EFI Application") sys = "EFI_APPLICATION";
				else if (SubSystem == "EFI Boot Service Driver") sys = "EFI_BOOT_SERVICE_DRIVER";
				else if (SubSystem == "EFI ROM") sys = "EFI_ROM";
				else if (SubSystem == "EFI Runtime") sys = "EFI_RUNTIME_DRIVER";
				else if (SubSystem == "WindowsCE") sys = "WINDOWSCE";
				else if (SubSystem == "POSIX") sys = "POSIX";
				else {
					Log.LogWarning("SubSystem is unknown");
					sys = SubSystem;
				}

				if (sys.Length != 0)
					commandLine.AppendSwitch("/SUBSYSTEM:" + sys);
			}

			if (Bag["TargetMachine"] != null)
			{
				string machine = "";
				if (TargetMachine == "MachineARM") machine = "ARM";
				else if (TargetMachine == "MachineEBC") machine = "EBC";
				else if (TargetMachine == "MachineIA64") machine = "IA64";
				else if (TargetMachine == "MachineMIPS") machine = "MIPS";
				else if (TargetMachine == "MachineMIPS16") machine = "MIPS16";
				else if (TargetMachine == "MachineMIPSFPU") machine = "MIPSFPU";
				else if (TargetMachine == "MachineMIPSFPU16") machine = "MIPSFPU16";
				else if (TargetMachine == "MachineSH4") machine = "SH4";
				else if (TargetMachine == "MachineTHUMB") machine = "THUMB";
				else if (TargetMachine == "MachineX64") machine = "X64";
				else if (TargetMachine == "MachineX86") machine = "X86";
				else
				{
					Log.LogWarning("TargetMachine is unknown");
					machine = TargetMachine;
				}

				if (machine.Length != 0)
					commandLine.AppendSwitch("/MACHINE:" + machine);
			}

			if (Bag["TrackerLogDirectory"] != null)
			{
				throw new NotImplementedException();
			}

			if (Bag["TreatLibWarningAsErrors"] != null)
			{
				if (TreatLibWarningAsErrors)
					commandLine.AppendSwitch("/WX");
			}

			if (Bag["UseUnicodeResponseFiles"] != null)
			{
				if (UseUnicodeResponseFiles)
					throw new NotImplementedException();
			}

			if (Bag["Verbose"] != null)
			{
				if (Verbose)
					commandLine.AppendSwitch("/VERBOSE");
			}
		}

		[MonoTODO]
		protected override bool HandleTaskExecutionErrors()
		{
			if (!Log.HasLoggedErrors && ExitCode != 0)
				Log.LogError("Library tool crashed with code: {0}.", ExitCode);

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

		public string[] AdditionalDependencies
		{
			get { return (string[])Bag["AdditionalDependencies"]; }
			set { Bag["AdditionalDependencies"] = value; }
		}

		public string[] AdditionalLibraryDirectories
		{
			get { return (string[])Bag["AdditionalLibraryDirectories"]; }
			set { Bag["AdditionalLibraryDirectories"] = value; }
		}

		public string AdditionalOptions
		{
			get { return (string)Bag["AdditionalOptions"]; }
			set { Bag["AdditionalOptions"] = value; }
		}

		public string DisplayLibrary
		{
			get { return (string)Bag["DisplayLibrary"]; }
			set { Bag["DisplayLibrary"] = value; }
		}

		public string ErrorReporting
		{
			get { return (string)Bag["ErrorReporting"]; }
			set { Bag["ErrorReporting"] = value; }
		}

		public string[] ExportNamedFunctions
		{
			get { return (string[])Bag["ExportNamedFunctions"]; }
			set { Bag["ExportNamedFunctions"] = value; }
		}

		public string ForceSymbolReferences
		{
			get { return (string)Bag["ForceSymbolReferences"]; }
			set { Bag["ForceSymbolReferences"] = value; }
		}

		public bool IgnoreAllDefaultLibraries
		{
			get { return (bool)Bag["IgnoreAllDefaultLibraries"]; }
			set { Bag["IgnoreAllDefaultLibraries"] = value; }
		}

		public string[] IgnoreSpecificDefaultLibraries
		{
			get { return (string[])Bag["IgnoreSpecificDefaultLibraries"]; }
			set { Bag["IgnoreSpecificDefaultLibraries"] = value; }
		}

		public bool LinkLibraryDependencies
		{
			get { return (bool)Bag["LinkLibraryDependencies"]; }
			set { Bag["LinkLibraryDependencies"] = value; }
		}

		public bool LinkTimeCodeGeneration
		{
			get { return (bool)Bag["LinkTimeCodeGeneration"]; }
			set { Bag["LinkTimeCodeGeneration"] = value; }
		}

		public string MinimumRequiredVersion
		{
			get { return (string)Bag["MinimumRequiredVersion"]; }
			set { Bag["MinimumRequiredVersion"] = value; }
		}

		public string ModuleDefinitionFile
		{
			get { return (string)Bag["ModuleDefinitionFile"]; }
			set { Bag["ModuleDefinitionFile"] = value; }
		}

		public string Name
		{
			get { return (string)Bag["Name"]; }
			set { Bag["Name"] = value; }
		}

		public string OutputFile
		{
			get { return (string)Bag["OutputFile"]; }
			set { Bag["OutputFile"] = value; }
		}

		public string[] RemoveObjects
		{
			get { return (string[])Bag["RemoveObjects"]; }
			set { Bag["RemoveObjects"] = value; }
		}

		[Required]
		public ITaskItem[] Sources
		{
			get { return (ITaskItem[])Bag["Sources"]; }
			set
			{
				Bag["Sources"] = value;
				if (Bag["OutputFile"] == null && value != null && value.Length >= 1)
					Bag["OutputFile"] = new TaskItem(String.Format("{0}.lib", value[0].ItemSpec));
			}
		}

		public string SubSystem
		{
			get { return (string)Bag["SubSystem"]; }
			set { Bag["SubSystem"] = value; }
		}

		public bool SuppressStartupBanner
		{
			get { return (bool)Bag["SuppressStartupBanner"]; }
			set { Bag["SuppressStartupBanner"] = value; }
		}

		public string TargetMachine
		{
			get { return (string)Bag["TargetMachine"]; }
			set { Bag["TargetMachine"] = value; }
		}

		public string TrackerLogDirectory
		{
			get { return (string)Bag["TrackerLogDirectory"]; }
			set { Bag["TrackerLogDirectory"] = value; }
		}

		public bool TreatLibWarningAsErrors
		{
			get { return (bool)Bag["TreatLibWarningAsErrors"]; }
			set { Bag["TreatLibWarningAsErrors"] = value; }
		}

		public bool UseUnicodeResponseFiles
		{
			get { return (bool)Bag["UseUnicodeResponseFiles"]; }
			set { Bag["UseUnicodeResponseFiles"] = value; }
		}

		public bool Verbose
		{
			get { return (bool)Bag["Verbose"]; }
			set { Bag["Verbose"] = value; }
		}

		protected override string GenerateFullPathToTool()
		{
			return ToolName;
		}

		protected override string ToolName
		{
			get { return "lib.exe"; }
		}
	}
}

#endif
