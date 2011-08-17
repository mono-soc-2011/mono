// 
// ClassTests.cs
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
using Mono.Cecil;
using NUnit.Framework;

namespace Mono.ILAsm.Tests {
	[TestFixture]
	public sealed class ClassTests : AssemblerTester {
		[Test]
		public void TestSimpleClassDirective ()
		{
			ILAsm ()
				.Input ("class-001.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.Types.Contains (
					y => y.Name == "test001",
					y => y.BaseType.FullName == "System.Object"));
		}
		
		[Test]
		public void TestGenericClassDirective ()
		{
			ILAsm ()
				.Input ("class-002.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetTypes ().Contains (
					y => y.GenericParameters.ContainsMany (
						z => z.Name == "T1",
						z => z.Name == "T2")));
		}
		
		[Test]
		public void TestValueTypeClassDirective ()
		{
			ILAsm ()
				.Input ("class-003.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test003").IsValueType);
		}
		
		[Test]
		public void TestEnumClassDirective ()
		{
			ILAsm ()
				.Input ("class-004.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test004").IsEnum);
		}
		
		[Test]
		public void TestInterfaceImplementation ()
		{
			ILAsm ()
				.Input ("class-005.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test005").Interfaces.Contains (
					y => y.FullName == "System.ICloneable"));
		}
		
		[Test]
		public void TestMultipleInterfaceImplementations ()
		{
			ILAsm ()
				.Input ("class-006.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test006").Interfaces.ContainsMany (
					y => y.FullName == "System.ICloneable",
					y => y.FullName == "System.IDisposable"));
		}
		
		[Test]
		public void TestSimpleClassInheritance ()
		{
			ILAsm ()
				.Input ("class-007.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test007").BaseType.Name == "test007_base");
		}
		
		[Test]
		public void TestGenericValueTypeConstraint ()
		{
			ILAsm ()
				.Input ("class-008.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetTypes ().Contains (
					y => y.GenericParameters.Contains (
						z => z.Attributes.HasBitFlag (GenericParameterAttributes.NotNullableValueTypeConstraint))));
		}
		
		[Test]
		public void TestGenericReferenceTypeConstraint ()
		{
			ILAsm ()
				.Input ("class-009.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetTypes ().Contains (
					y => y.GenericParameters.Contains (
						z => z.Attributes.HasBitFlag (GenericParameterAttributes.ReferenceTypeConstraint))));
		}
		
		[Test]
		public void TestGenericConstructorConstraint ()
		{
			ILAsm ()
				.Input ("class-010.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetTypes ().Contains (
					y => y.GenericParameters.Contains (
						z => z.Attributes.HasBitFlag (GenericParameterAttributes.DefaultConstructorConstraint))));
		}
		
		[Test]
		public void TestInterfaceClassDirective ()
		{
			ILAsm ()
				.Input ("class-011.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test011").IsInterface);
		}
		
		[Test]
		public void TestSimpleInterfaceImplementation ()
		{
			ILAsm ()
				.Input ("class-012.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test012").Interfaces.Contains (
					y => y.Name == "test012_if"));
		}
		
		[Test]
		public void TestGenericInterfaceImplementation ()
		{
			ILAsm ()
				.Input ("class-013.il")
				.Run ()
				.Expect (ExitCode.Success)
				.GetModule ()
				.Expect (x => x.GetType ("test013").Interfaces.Contains (
					y => y.FullName == "System.IEquatable`1<test013_dummy>"));
		}
	}
}
