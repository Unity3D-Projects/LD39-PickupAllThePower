using System.IO;
using System.Text;

public static class IOExtensions
{
    public static int ReadNextInt(this StreamReader reader)
    {
        var s = string.Empty;
        var result = 0;
        while (!string.IsNullOrEmpty(s = ReadNextString(reader)))
        {
            if (int.TryParse(s, out result))
                return result;
        }
        return 0;
    }

    public static string ReadNextString(this StreamReader reader)
    {
        SkipWhiteSpace(reader);

        var builder = new StringBuilder();

        while (!reader.EndOfStream && !char.IsWhiteSpace((char)reader.Peek()))
            builder.Append((char)reader.Read());

        return builder.ToString();
    }

    public static void SkipWhiteSpace(this StreamReader reader)
    {
        while (!reader.EndOfStream && char.IsWhiteSpace((char)reader.Peek()))
            reader.Read();
    }
}
