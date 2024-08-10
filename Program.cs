using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

public class PgnFilter
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Start");

        //TODO ADD YOUR FILES
        string inputFile = @"D:\Borko\PGN_Filter\Unfiltered\07_2014.pgn";
        string outputFile = @"D:\Borko\PGN_Filter\Filtered\1600-1800\filtered_07_2014.pgn";

        string[] tags = {"WhiteElo","BlackElo","WhiteElo","BlackElo"};
        string[] op = {">",">","<","<"};
        string[] value ={"1600","1600","1800","1800"};
        try 
        {
            List<Game> games = ParsePgnFile(inputFile);

            List<Game> filteredGames = games;
            for(int i = 0; i < tags.Length; i++) {
                filteredGames = FilterGames(filteredGames,tags[i],op[i],value[i]); 
            }
            

            WritePgnFile(outputFile, filteredGames);

            Console.WriteLine($"Filtered {filteredGames.Count} games from {inputFile} and saved to {outputFile}");
        } 
        catch (Exception ex) 
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    private static List<Game> ParsePgnFile(string filePath)
    {
        List<Game> games = new List<Game>();
        string currentGame = "";

        using (StreamReader reader = new StreamReader(filePath))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!string.IsNullOrEmpty(currentGame))
                    {
                        games.Add(ParseGame(currentGame));
                    }
                    currentGame = "";
                }
                else
                {
                    currentGame += line + "\n";
                }
            }

            if (!string.IsNullOrEmpty(currentGame))
            {
                games.Add(ParseGame(currentGame));
            }
        }

        return games;
    }

    private static Game ParseGame(string pgnString)
    {
        Game game = new Game();

        // Use Regex to extract tags and moves from PGN string
        MatchCollection matches = Regex.Matches(pgnString, @"\[(.+?)\s""(.+?)""\]");
        foreach (Match match in matches)
        {
            game.Tags[match.Groups[1].Value] = match.Groups[2].Value;
        }

        game.Moves = pgnString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).SkipWhile(line => line.StartsWith("[")).ToList();

        return game;
    }

private static List<Game> FilterGames(List<Game> games, string tag, string op, string value)
{
    List<Game> filteredGames = games;

    filteredGames = filteredGames.Where(game => FilterGameByTag(game, tag, op, value)).ToList();       

    return filteredGames;
}
private static bool FilterGameByTag(Game game, string tag, string op, string value)
{
    if (!game.Tags.ContainsKey(tag))
    {
        return false; // Tag not found, exclude game
    }

    string gameValue = game.Tags[tag];

    // Handle numeric comparisons
    if (int.TryParse(gameValue, out int intValue) && int.TryParse(value, out int intVal))
    {
        switch (op)
        {
            case ">":
                return intValue > intVal;
            case "<":
                return intValue < intVal;
            case "=":
                return intValue == intVal;
            case ">=":
                return intValue >= intVal;
            case "<=":
                return intValue <= intVal;
            default:
                return false;
        }
    }
    // Handle string comparisons (e.g., Event)
    else
    {
        switch (op)
        {
            case "=":
                return gameValue == value;
            default:
                return false; // Only equality comparison supported for strings
        }
    }
}   

    private static void WritePgnFile(string filePath, List<Game> games)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (Game game in games)
            {
            // Write only the moves using string.Join
            writer.WriteLine(string.Join("\n", game.Moves));
            }
        }
    }
}

public class Game
{
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    public List<string> Moves { get; set; } = new List<string>();

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, string> tag in Tags)
        {
            sb.AppendLine($"[{tag.Key} \"{tag.Value}\"]");
        }
        sb.AppendLine();
        sb.AppendLine(string.Join("\n", Moves));
        return sb.ToString();
    }
}