using System;
using System.Collections.Generic;
using Xunit;
using SourcemapToolkit.SourcemapParser;
using NSubstitute;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class StackFrameDeminifierUnitTests
	{ 
		private IStackFrameDeminifier GetStackFrameDeminifierWithMockDependencies(ISourceMapStore sourceMapStore = null, IFunctionMapStore functionMapStore = null, IFunctionMapConsumer functionMapConsumer = null, bool useSimpleStackFrameDeminier = false)
		{
			if (sourceMapStore == null)
			{
				sourceMapStore = Substitute.For<ISourceMapStore>();
			}

			if (functionMapStore == null)
			{
				functionMapStore = Substitute.For<IFunctionMapStore>();
			}

			if (functionMapConsumer == null)
			{
				functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			}

			if (useSimpleStackFrameDeminier)
			{
				return new SimpleStackFrameDeminifier(functionMapStore, functionMapConsumer);
			}
			else
			{
				return new StackFrameDeminifier(sourceMapStore, functionMapStore, functionMapConsumer);
			}
		}

		[Fact]
		public void DeminifyStackFrame_NullInputStackFrame_ThrowsException()
		{
			// Arrange
			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();
			StackFrame stackFrame = null;

			// Act
			Assert.Throws<ArgumentNullException>( ()=> stackFrameDeminifier.DeminifyStackFrame(stackFrame));
		}

		[Fact]
		public void DeminifyStackFrame_StackFrameNullProperties_DoesNotThrowException()
		{
			// Arrange
			StackFrame stackFrame = new StackFrame();
			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Fact]
		public void SimpleStackFrameDeminierDeminifyStackFrame_FunctionMapReturnsNull_NoFunctionMapDeminificationError()
		{
			// Arrange
			string filePath = "foo";
			StackFrame stackFrame = new StackFrame {FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath).Returns( (List<FunctionMapEntry>) null);

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, useSimpleStackFrameDeminier:true);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.NoSourceCodeProvided, stackFrameDeminification.DeminificationError);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Fact]
		public void SimpleStackFrameDeminierDeminifyStackFrame_GetWRappingFunctionForSourceLocationReturnsNull_NoWrapingFunctionDeminificationError()
		{
			// Arrange
			string filePath = "foo";
			StackFrame stackFrame = new StackFrame { FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath)
				.Returns(new List<FunctionMapEntry>());
			IFunctionMapConsumer functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			functionMapConsumer.GetWrappingFunctionForSourceLocation(Arg.Any<SourcePosition>(), Arg.Any<List<FunctionMapEntry>>())
				.Returns((FunctionMapEntry) null);

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminier: true);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.NoWrapingFunctionFound, stackFrameDeminification.DeminificationError);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Fact]
		public void SimpleStackFrameDeminierDeminifyStackFrame_WrapingFunctionFound_NoDeminificationError()
		{
			// Arrange
			string filePath = "foo";
			FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry {DeminfifiedMethodName = "DeminifiedFoo"};
			StackFrame stackFrame = new StackFrame { FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath)
				.Returns(new List<FunctionMapEntry>());
			IFunctionMapConsumer functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			functionMapConsumer.GetWrappingFunctionForSourceLocation(Arg.Any<SourcePosition>(), Arg.Any<List<FunctionMapEntry>>())
				.Returns(wrapingFunctionMapEntry);

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminier: true);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.None, stackFrameDeminification.DeminificationError);
			Assert.Equal(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}


		[Fact]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapProviderReturnsNull_NoSourcemapProvidedError()
		{
			// Arrange
			string filePath = "foo";
			FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
			StackFrame stackFrame = new StackFrame { FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath)
				.Returns(new List<FunctionMapEntry>());
			IFunctionMapConsumer functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			functionMapConsumer.GetWrappingFunctionForSourceLocation(Arg.Any<SourcePosition>(), Arg.Any<List<FunctionMapEntry>>())
				.Returns(wrapingFunctionMapEntry);

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.NoSourceMap, stackFrameDeminification.DeminificationError);
			Assert.Equal(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Fact]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapParsingNull_SourceMapFailedToParseError()
		{
			// Arrange
			string filePath = "foo";
			FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
			StackFrame stackFrame = new StackFrame { FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath)
				.Returns(new List<FunctionMapEntry>());
			IFunctionMapConsumer functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			functionMapConsumer.GetWrappingFunctionForSourceLocation(Arg.Any<SourcePosition>(), Arg.Any<List<FunctionMapEntry>>())
				.Returns(wrapingFunctionMapEntry);
			ISourceMapStore sourceMapStore = Substitute.For<ISourceMapStore>();
			sourceMapStore.GetSourceMapForUrl(Arg.Any<string>()).Returns(new SourceMap());

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore,functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.SourceMapFailedToParse, stackFrameDeminification.DeminificationError);
			Assert.Equal(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

		[Fact]
		public void StackFrameDeminierDeminifyStackFrame_SourceMapGeneratedMappingEntryNull_NoMatchingMapingInSourceMapError()
		{
			// Arrange
			string filePath = "foo";
			FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
			StackFrame stackFrame = new StackFrame { FilePath = filePath };
			IFunctionMapStore functionMapStore = Substitute.For<IFunctionMapStore>();
			functionMapStore.GetFunctionMapForSourceCode(filePath)
				.Returns(new List<FunctionMapEntry>());
			ISourceMapStore sourceMapStore = Substitute.For<ISourceMapStore>();
			SourceMap sourceMap = new SourceMap() {ParsedMappings = new List<MappingEntry>()};

			sourceMapStore.GetSourceMapForUrl(Arg.Any<string>()).Returns(sourceMap);
			IFunctionMapConsumer functionMapConsumer = Substitute.For<IFunctionMapConsumer>();
			functionMapConsumer.GetWrappingFunctionForSourceLocation(Arg.Any<SourcePosition>(), Arg.Any<List<FunctionMapEntry>>())
				.Returns(wrapingFunctionMapEntry);

			IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore, functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

			// Act
			StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

			// Assert
			Assert.Equal(DeminificationError.NoMatchingMapingInSourceMap, stackFrameDeminification.DeminificationError);
			Assert.Equal(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
			Assert.Null(stackFrameDeminification.DeminifiedStackFrame.FilePath);
		}

	}
}
