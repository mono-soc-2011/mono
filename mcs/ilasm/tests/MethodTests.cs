// 
// MethodTests.cs
//  
// Author:
//       Alex Rønne Petersen <xtzgzorex@gmail.com>
// 
// Copyright (c) 2011 Alex Rønne Petersen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using NUnit.Framework;

namespace Mono.ILAsm.Tests {
	[TestFixture]
	public sealed class MethodTests : AssemblerTester {
		[Test]
		public void TestSimpleMethod ()
		{
			ILAsm ()
				.Input ("method/method-001.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetModuleType ().Methods.ContainsOne (
					y => y.Name == "test001",
					y => y.ReturnType.Name == "Void"));
		}
		
		[Test]
		public void TestEmptyMethod ()
		{
			ILAsm ()
				.Input ("method/method-002.il")
				.ExpectWarning (Warning.DefaultReturnEmitted)
				.Run ()
				.Expect (ExitCode.Success);
		}
		
		[Test]
		public void TestParametrizedMethod ()
		{
			ILAsm ()
				.Input ("method/method-003.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetModuleType ().Methods.ContainsOne (
					y => y.Parameters.ContainsMany (
						z => z.Name == "val1" && z.ParameterType.Name == "UInt32",
						z => z.Name == "val2" && z.ParameterType.Name == "Single")));
		}
		
		[Test]
		public void TestGenericMethod ()
		{
			ILAsm ()
				.Input ("method-generic/method-generic-001.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetModuleType ().Methods.ContainsOne (
					y => y.GenericParameters.ContainsMany (
						z => z.Name == "T1",
						z => z.Name == "T2")));
		}
		
		[Test]
		public void TestGenericMethodWithGenericReturnType ()
		{
			ILAsm ()
				.Input ("method-generic/method-generic-002.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetModuleType ().Methods.ContainsOne (
					y => y.ReturnType.Name == "T"));
		}
		
		[Test]
		public void TestGenericMethodWithGenericParameterType ()
		{
			ILAsm ()
				.Input ("method-generic/method-generic-003.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetModuleType ().Methods.ContainsOne (
					y => y.Parameters.ContainsOne (
						z => z.ParameterType.Name == "T")));
		}
	}
}
