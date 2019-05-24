using System.Collections.Generic;
using Xunit;
using NSubstitute;
namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class StackTraceDeminifierUnitTests
	{
		[Fact]
		public void DeminifyStackTrace_UnableToParseStackTraceString_ReturnsEmptyList()
		{
			// Arrange
			IStackTraceParser stackTraceParser = Substitute.For<IStackTraceParser>();
			string stackTraceString = "foobar";
			stackTraceParser.ParseStackTrace(stackTraceString).Returns(new List<StackFrame>());

			IStackFrameDeminifier stackFrameDeminifier = Substitute.For<IStackFrameDeminifier>();

			StackTraceDeminifier stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);

			// Act
			DeminifyStackTraceResult result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

			// Assert
			Assert.Empty(result.DeminifiedStackFrameResults);
		}

		[Fact]
		public void DeminifyStackTrace_UnableToDeminifyStackTrace_ResultContainsNullDeminifiedFrame()
		{
			// Arrange
			IStackTraceParser stackTraceParser = Substitute.For<IStackTraceParser>();
			List<StackFrame> minifiedStackFrames = new List<StackFrame> { new StackFrame() };
			string stackTraceString = "foobar";
			stackTraceParser.ParseStackTrace(stackTraceString).Returns(minifiedStackFrames);

			IStackFrameDeminifier stackFrameDeminifier = Substitute.For<IStackFrameDeminifier>();
			stackFrameDeminifier.DeminifyStackFrame(minifiedStackFrames[0]).Returns((StackFrameDeminificationResult)null);
			
			StackTraceDeminifier stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);

			// Act
			DeminifyStackTraceResult result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

			// Assert
			Assert.Single(result.DeminifiedStackFrameResults);
			Assert.Equal(minifiedStackFrames[0], result.MinifiedStackFrames[0]);
			Assert.Null(result.DeminifiedStackFrameResults[0]);
		}

		[Fact]
		public void DeminifyStackTrace_AbleToDeminifyStackTrace_ResultContainsDeminifiedFrame()
		{
			// Arrange
			IStackTraceParser stackTraceParser = Substitute.For<IStackTraceParser>();
			List<StackFrame> minifiedStackFrames = new List<StackFrame> { new StackFrame() };
			string stackTraceString = "foobar";
			stackTraceParser.ParseStackTrace(stackTraceString).Returns(minifiedStackFrames);

			IStackFrameDeminifier stackFrameDeminifier = Substitute.For<IStackFrameDeminifier>();
			StackFrameDeminificationResult stackFrameDeminification = new StackFrameDeminificationResult();
			stackFrameDeminifier.DeminifyStackFrame(minifiedStackFrames[0]).Returns(stackFrameDeminification);

			StackTraceDeminifier stackTraceDeminifier = new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);

			// Act
			DeminifyStackTraceResult result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

			// Assert
			Assert.Single(result.DeminifiedStackFrameResults);
			Assert.Equal(minifiedStackFrames[0], result.MinifiedStackFrames[0]);
			Assert.Equal(stackFrameDeminification, result.DeminifiedStackFrameResults[0]);
		}
	}
}