namespace HugeTextProcessing.Abstractions.IO;
public interface ILinesWriter
{
    /// <summary>
    /// Writes the collection of <see cref="Line"/> to stream as text (e.g. file)
    /// </summary>
    /// <param name="stream">The stream to write</param>
    /// <param name="lines">Collection of lines</param>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>Number of bytes written to the stream</returns>
    long WriteAsText(Stream stream, IEnumerable<Line> lines);

    /// <summary>
    /// Writes the <see cref="Line"/> to stream as text (e.g. file)
    /// </summary>
    /// <param name="stream">The stream to write</param>
    /// <param name="line">Line</param>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>Number of bytes written to the stream</returns>
    long WriteAsText(Stream stream, Line line);
}
