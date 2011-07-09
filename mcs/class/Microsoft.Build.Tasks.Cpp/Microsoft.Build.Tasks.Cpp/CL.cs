//
// CL.cs: Task for C++ compilers
//
// For an overview of the CL options, you can check:
//  http://msdn.microsoft.com/en-us/library/19z1t1wy.aspx
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

namespace Microsoft.Build.Tasks.Cpp {
	public class CL : ToolTaskExtension {
		public CL()
		{
		}

		[MonoTODO]
		protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
		{
			if (Bag["SuppressStartupBanner"] != null)
				commandLine.AppendSwitch("/NoLogo");

			if (Bag["AdditionalIncludeDirectories"] != null) {
				foreach (string inc in AdditionalIncludeDirectories) {
					commandLine.AppendSwitch("/I " + inc);
				}
			}

			if (Bag["AdditionalOptions"] != null) {
				commandLine.AppendSwitch(AdditionalOptions);
			}

			if (Bag["CallingConvention"] != null) {
				if (CallingConvention == "Cdecl") commandLine.AppendSwitch("/Gd");
				if (CallingConvention == "FastCall") commandLine.AppendSwitch("/Gr");
				if (CallingConvention == "StdCall") commandLine.AppendSwitch("/Gz");
			}

			if (Bag["CompileAs"] != null) {
				//if (CompileAs == "Default") ;
				if (CompileAs == "CompileAsC") commandLine.AppendSwitch("/TC");
				if (CompileAs == "CompileAsCpp") commandLine.AppendSwitch("/TP");
			}

			if (Bag["DebugInformationFormat"] != null) {
				if (DebugInformationFormat == "OldStyle") commandLine.AppendSwitch("/Z7");
				if (DebugInformationFormat == "ProgramDatabase") commandLine.AppendSwitch("/Zi");
				if (DebugInformationFormat == "EditAndContinue") commandLine.AppendSwitch("/ZI");
			}

			if (Bag["DisableLanguageExtensions"] != null) {
				commandLine.AppendSwitch(DisableLanguageExtensions ? "/Za" : "/Ze");
			}

			if (Bag["DisableSpecificWarnings"] != null) {
				foreach (String warning in DisableSpecificWarnings) {
					commandLine.AppendSwitch("/Wd" + warning);
				}
			}

			if (Bag["ExceptionHandling"] != null) {
				if (ExceptionHandling == "Async") commandLine.AppendSwitch("/EHa");
				if (ExceptionHandling == "Sync") commandLine.AppendSwitch("/EHsc");
				if (ExceptionHandling == "SyncCThrow") commandLine.AppendSwitch("/EHs");
			}

			if (Bag["FavorSizeOrSpeed"] != null) {
				if (FavorSizeOrSpeed == "Size") commandLine.AppendSwitch("/Os");
				if (FavorSizeOrSpeed == "Speed") commandLine.AppendSwitch("/Ot");
			}

			if (Bag["FloatingPointModel"] != null) {
				if (FloatingPointModel == "Precise") commandLine.AppendSwitch("/fp:precise");
				if (FloatingPointModel == "Strict") commandLine.AppendSwitch("/fp:strict");
				if (FloatingPointModel == "Fast") commandLine.AppendSwitch("/fp:fast");
			}

			if (Bag["Optimization"] != null) {
				if (Optimization == "Disabled") commandLine.AppendSwitch("/Od");
				if (Optimization == "MinSpace") commandLine.AppendSwitch("/O1");
				if (Optimization == "MaxSpeed") commandLine.AppendSwitch("/O2");
				if (Optimization == "Full") commandLine.AppendSwitch("/Ox");
			}

			if (Bag["Sources"] != null) {
				foreach (ITaskItem source in Sources) {
					commandLine.AppendFileNameIfNotNull(source.ItemSpec);
				}
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

			foreach (ITaskItem item in itemList) {
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

		public string[] AdditionalIncludeDirectories {
			get { return (string[])Bag["AdditionalIncludeDirectories"]; }
			set { Bag["AdditionalIncludeDirectories"] = value; }
		}

		public string AdditionalOptions {
			get { return (string)Bag["AdditionalOptions"]; }
			set { Bag["AdditionalOptions"] = value; }
		}

		public string[] AdditionalUsingDirectories {
			get { return (string[])Bag["AdditionalUsingDirectories"]; }
			set { Bag["AdditionalUsingDirectories"] = value; }
		}

		public string AlwaysAppend {
			get { return (string)Bag["AlwaysAppend"]; }
			set { Bag["AlwaysAppend"] = value; }
		}

		public string AssemblerListingLocation {
			get { return (string)Bag["AssemblerListingLocation"]; }
			set { Bag["AssemblerListingLocation"] = value; }
		}

		public string AssemblerOutput {
			get { return (string)Bag["AssemblerOutput"]; }
			set { Bag["AssemblerOutput"] = value; }
		}

		public string BasicRuntimeChecks {
			get { return (string)Bag["BasicRuntimeChecks"]; }
			set { Bag["BasicRuntimeChecks"] = value; }
		}

		public bool BrowseInformation {
			get { return (bool)Bag["BrowseInformation"]; }
			set { Bag["BrowseInformation"] = value; }
		}

		public string BrowseInformationFile {
			get { return (string)Bag["BrowseInformationFile"]; }
			set { Bag["BrowseInformationFile"] = value; }
		}

		public bool BufferSecurityCheck {
			get { return (bool)Bag["BufferSecurityCheck"]; }
			set { Bag["BufferSecurityCheck"] = value; }
		}

		public bool BuildingInIDE {
			get { return (bool)Bag["BuildingInIDE"]; }
			set { Bag["BuildingInIDE"] = value; }
		}

		public string CallingConvention {
			get { return (string)Bag["CallingConvention"]; }
			set { Bag["CallingConvention"] = value; }
		}

		public string CompileAs {
			get { return (string)Bag["CompileAs"]; }
			set { Bag["CompileAs"] = value; }
		}

		public string CompileAsManaged {
			get { return (string)Bag["CompileAsManaged"]; }
			set { Bag["CompileAsManaged"] = value; }
		}

		public bool CreateHotpatchableImage {
			get { return (bool)Bag["CreateHotpatchableImage"]; }
			set { Bag["CreateHotpatchableImage"] = value; }
		}

		public string DebugInformationFormat {
			get { return (string)Bag["DebugInformationFormat"]; }
			set { Bag["DebugInformationFormat"] = value; }
		}

		public bool DisableLanguageExtensions {
			get { return (bool)Bag["DisableLanguageExtensions"]; }
			set { Bag["DisableLanguageExtensions"] = value; }
		}

		public string[] DisableSpecificWarnings {
			get { return (string[])Bag["DisableSpecificWarnings"]; }
			set { Bag["DisableSpecificWarnings"] = value; }
		}

		public string EnableEnhancedInstructionSet {
			get { return (string)Bag["EnableEnhancedInstructionSet"]; }
			set { Bag["EnableEnhancedInstructionSet"] = value; }
		}

		public bool EnableFiberSafeOptimizations {
			get { return (bool)Bag["EnableFiberSafeOptimizations"]; }
			set { Bag["EnableFiberSafeOptimizations"] = value; }
		}

		public bool EnablePREfast {
			get { return (bool)Bag["EnablePREfast"]; }
			set { Bag["EnablePREfast"] = value; }
		}

		public string ErrorReporting {
			get { return (string)Bag["ErrorReporting"]; }
			set { Bag["ErrorReporting"] = value; }
		}

		public string ExceptionHandling {
			get { return (string)Bag["ExceptionHandling"]; }
			set { Bag["ExceptionHandling"] = value; }
		}

		public string[] ExcludedInputPaths {
			get { return (string[])Bag["ExcludedInputPaths"]; }
			set { Bag["ExcludedInputPaths"] = value; }
		}

		public bool ExpandAttributedSource {
			get { return (bool)Bag["ExpandAttributedSource"]; }
			set { Bag["ExpandAttributedSource"] = value; }
		}

		public string FavorSizeOrSpeed {
			get { return (string)Bag["FavorSizeOrSpeed"]; }
			set { Bag["FavorSizeOrSpeed"] = value; }
		}

		public bool FloatingPointExceptions {
			get { return (bool)Bag["FloatingPointExceptions"]; }
			set { Bag["FloatingPointExceptions"] = value; }
		}

		public string FloatingPointModel {
			get { return (string)Bag["FloatingPointModel"]; }
			set { Bag["FloatingPointModel"] = value; }
		}

		public bool ForceConformanceInForLoopScope {
			get { return (bool)Bag["ForceConformanceInForLoopScope"]; }
			set { Bag["ForceConformanceInForLoopScope"] = value; }
		}

		public string[] ForcedIncludeFiles {
			get { return (string[])Bag["ForcedIncludeFiles"]; }
			set { Bag["ForcedIncludeFiles"] = value; }
		}

		public string[] ForcedUsingFiles {
			get { return (string[])Bag["ForcedUsingFiles"]; }
			set { Bag["ForcedUsingFiles"] = value; }
		}

		public bool FunctionLevelLinking {
			get { return (bool)Bag["FunctionLevelLinking"]; }
			set { Bag["FunctionLevelLinking"] = value; }
		}

		public bool GenerateXMLDocumentationFiles {
			get { return (bool)Bag["GenerateXMLDocumentationFiles"]; }
			set { Bag["GenerateXMLDocumentationFiles"] = value; }
		}

		public bool IgnoreStandardIncludePath {
			get { return (bool)Bag["IgnoreStandardIncludePath"]; }
			set { Bag["IgnoreStandardIncludePath"] = value; }
		}

		public string InlineFunctionExpansion {
			get { return (string)Bag["InlineFunctionExpansion"]; }
			set { Bag["InlineFunctionExpansion"] = value; }
		}

		public bool IntrinsicFunctions {
			get { return (bool)Bag["IntrinsicFunctions"]; }
			set { Bag["IntrinsicFunctions"] = value; }
		}

		public bool MinimalRebuild {
			get { return (bool)Bag["MinimalRebuild"]; }
			set { Bag["MinimalRebuild"] = value; }
		}

		public bool MultiProcessorCompilation {
			get { return (bool)Bag["MultiProcessorCompilation"]; }
			set { Bag["MultiProcessorCompilation"] = value; }
		}

		public string ObjectFileName {
			get { return (string)Bag["ObjectFileName"]; }
			set { Bag["ObjectFileName"] = value; }
		}

		public string[] ObjectFiles {
			get { return (string[])Bag["ObjectFiles"]; }
			set { Bag["ObjectFiles"] = value; }
		}

		public bool OmitDefaultLibName {
			get { return (bool)Bag["OmitDefaultLibName"]; }
			set { Bag["OmitDefaultLibName"] = value; }
		}

		public bool OmitFramePointers {
			get { return (bool)Bag["OmitFramePointers"]; }
			set { Bag["OmitFramePointers"] = value; }
		}

		public bool OpenMPSupport {
			get { return (bool)Bag["OpenMPSupport"]; }
			set { Bag["OpenMPSupport"] = value; }
		}

		public string Optimization {
			get { return (string)Bag["Optimization"]; }
			set { Bag["Optimization"] = value; }
		}

		public string PrecompiledHeader {
			get { return (string)Bag["PrecompiledHeader"]; }
			set { Bag["PrecompiledHeader"] = value; }
		}

		public string PrecompiledHeaderFile {
			get { return (string)Bag["PrecompiledHeaderFile"]; }
			set { Bag["PrecompiledHeaderFile"] = value; }
		}

		public string PrecompiledHeaderOutputFile {
			get { return (string)Bag["PrecompiledHeaderOutputFile"]; }
			set { Bag["PrecompiledHeaderOutputFile"] = value; }
		}

		public bool PreprocessKeepComments {
			get { return (bool)Bag["PreprocessKeepComments"]; }
			set { Bag["PreprocessKeepComments"] = value; }
		}

		public string[] PreprocessorDefinitions {
			get { return (string[])Bag["PreprocessorDefinitions"]; }
			set { Bag["PreprocessorDefinitions"] = value; }
		}

		public ITaskItem[] PreprocessOutput {
			get { return (ITaskItem[])Bag["PreprocessOutput"]; }
			set { Bag["PreprocessOutput"] = value; }
		}

		public string PreprocessOutputPath {
			get { return (string)Bag["PreprocessOutputPath"]; }
			set { Bag["PreprocessOutputPath"] = value; }
		}

		public bool PreprocessSuppressLineNumbers {
			get { return (bool)Bag["PreprocessSuppressLineNumbers"]; }
			set { Bag["PreprocessSuppressLineNumbers"] = value; }
		}

		public bool PreprocessToFile {
			get { return (bool)Bag["PreprocessToFile"]; }
			set { Bag["PreprocessToFile"] = value; }
		}

		public int ProcessorNumber {
			get { return (int)Bag["ProcessorNumber"]; }
			set { Bag["ProcessorNumber"] = value; }
		}

		public string ProgramDataBaseFileName {
			get { return (string)Bag["ProgramDataBaseFileName"]; }
			set { Bag["ProgramDataBaseFileName"] = value; }
		}

		public string RuntimeLibrary {
			get { return (string)Bag["RuntimeLibrary"]; }
			set { Bag["RuntimeLibrary"] = value; }
		}

		public bool RuntimeTypeInfo {
			get { return (bool)Bag["RuntimeTypeInfo"]; }
			set { Bag["RuntimeTypeInfo"] = value; }
		}

		public bool ShowIncludes {
			get { return (bool)Bag["ShowIncludes"]; }
			set { Bag["ShowIncludes"] = value; }
		}

		public bool SmallerTypeCheck {
			get { return (bool)Bag["SmallerTypeCheck"]; }
			set { Bag["SmallerTypeCheck"] = value; }
		}

		[Required]
		public ITaskItem[] Sources {
			get { return (ITaskItem[])Bag["Sources"]; }
			set { Bag["Sources"] = value; }
		}

		public bool stringPooling {
			get { return (bool)Bag["stringPooling"]; }
			set { Bag["stringPooling"] = value; }
		}

		public string StructMemberAlignment {
			get { return (string)Bag["StructMemberAlignment"]; }
			set { Bag["StructMemberAlignment"] = value; }
		}

		public bool SuppressStartupBanner {
			get { return (bool)Bag["SuppressStartupBanner"]; }
			set { Bag["SuppressStartupBanner"] = value; }
		}

		public string ToolArchitecture {
			get { return (string)Bag["ToolArchitecture"]; }
			set { Bag["ToolArchitecture"] = value; }
		}

		public string[] TreatSpecificWarningsAsErrors {
			get { return (string[])Bag["TreatSpecificWarningsAsErrors"]; }
			set { Bag["TreatSpecificWarningsAsErrors"] = value; }
		}

		public bool TreatWarningAsError {
			get { return (bool)Bag["TreatWarningAsError"]; }
			set { Bag["TreatWarningAsError"] = value; }
		}

		public bool TreatWChar_tAsBuiltInType {
			get { return (bool)Bag["TreatWChar_tAsBuiltInType"]; }
			set { Bag["TreatWChar_tAsBuiltInType"] = value; }
		}

		public bool UndefineAllPreprocessorDefinitions {
			get { return (bool)Bag["UndefineAllPreprocessorDefinitions"]; }
			set { Bag["UndefineAllPreprocessorDefinitions"] = value; }
		}

		public string[] UndefinePreprocessorDefinitions {
			get { return (string[])Bag["UndefinePreprocessorDefinitions"]; }
			set { Bag["UndefinePreprocessorDefinitions"] = value; }
		}

		public bool UseFullPaths {
			get { return (bool)Bag["UseFullPaths"]; }
			set { Bag["UseFullPaths"] = value; }
		}

		public bool UseUnicodeForAssemblerListing {
			get { return (bool)Bag["UseUnicodeForAssemblerListing"]; }
			set { Bag["UseUnicodeForAssemblerListing"] = value; }
		}

		public string WarningLevel {
			get { return (string)Bag["WarningLevel"]; }
			set { Bag["WarningLevel"] = value; }
		}

		public bool WholeProgramOptimization {
			get { return (bool)Bag["WholeProgramOptimization"]; }
			set { Bag["WholeProgramOptimization"] = value; }
		}

		public string XMLDocumentationFileName {
			get { return (string)Bag["XMLDocumentationFileName"]; }
			set { Bag["XMLDocumentationFileName"] = value; }
		}

		public bool MinimalRebuildFromTracking {
			get { return (bool)Bag["MinimalRebuildFromTracking"]; }
			set { Bag["MinimalRebuildFromTracking"] = value; }
		}

		public ITaskItem[] TLogReadFiles {
			get { return (ITaskItem[])Bag["TLogReadFiles"]; }
			set { Bag["TLogReadFiles"] = value; }
		}

		public ITaskItem[] TLogWriteFiles {
			get { return (ITaskItem[])Bag["TLogWriteFiles"]; }
			set { Bag["TLogWriteFiles"] = value; }
		}

		public bool TrackFileAccess {
			get { return (bool)Bag["TrackFileAccess"]; }
			set { Bag["TrackFileAccess"] = value; }
		}

		protected override string GenerateFullPathToTool() {
			return ToolName;
		}

		protected override string ToolName {
			get { return "cl.exe"; }
		}
	}
}

#endif
