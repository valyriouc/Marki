using Marki.Cli.Exceptions;

namespace Marki.Cli;

internal enum Parameter
{
    Inline = 0,
    InputFile = 1,
    OutputDir = 2
}

internal static class Program
{
    public static Dictionary<Parameter, string> Parameters { get; } 
        = new Dictionary<Parameter, string>();
    
    public static void Main(string[] args)
    {
        try
        {
            ParseArgs(args);
            ValidateParameters();


        }
        catch(ParameterException)
        {
            return;
        }
    }

    private static void ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                    Parameters.Add(Parameter.Inline, args[i + 1]);
                    i += 1;
                    break;
                case "-f":
                    Parameters.Add(Parameter.InputFile, args[i + 1]);
                    i += 1;
                    break;
                case "-o":
                    Parameters.Add(Parameter.OutputDir, args[i + 1]);
                    i += 1;
                    break;
                default:
                    Help();
                    throw new ParameterException(
                        "Parameter does not exists!");
            }
        }
    }

    private static void ValidateParameters()
    {
        bool inline = Parameters.ContainsKey(Parameter.Inline);

        if (inline)
        {
            return;
        }

        if (!Parameters.ContainsKey(Parameter.InputFile) &&
                !Parameters.ContainsKey(Parameter.OutputDir))
        {
            Help();
            throw new ParameterException(
                "Required parameter is missing!");
        }
    }

    private static void Help()
    {
        Console.WriteLine("<<<<< Marki >>>>>");
        Console.WriteLine("Welcome to marki. A markdown to html generator written in C#");
        Console.WriteLine();
        Console.WriteLine("Parameters:");
        Console.WriteLine("===========");
        Console.WriteLine("-i       Specifies inline markdown");
        Console.WriteLine("-f       The markdown input file");
        Console.WriteLine("-o       Output directory where the html files are stored");
    }
}