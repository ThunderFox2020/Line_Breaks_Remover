using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Line_Breaks_Remover
{
    public class Program
    {
        public static void Main()
        {
            Console.Write("Введите путь к директории со скриптами: ");

            string path = Console.ReadLine()!;

            Directory.CreateDirectory(path + @"\Output");
            Directory.CreateDirectory(path + @"\Logs");

            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                RemoveLineBreaks(directory, file);
            }

            Console.WriteLine("Выполнено");
            Console.ReadLine();
        }
        
        private static void RemoveLineBreaks(DirectoryInfo directory, FileInfo file)
        {
            string script = GetScriptFromFile(file);
            GetCorrectAndIncorrectStringsFromScript(script, out List<string> correctStrings, out List<string> incorrectStrings);
            ScriptCorrection(ref script, directory, file, correctStrings, incorrectStrings);
            OverwriteScriptToFile(script, directory, file);
        }
        private static string GetScriptFromFile(FileInfo file)
        {
            string script = "";

            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    script = streamReader.ReadToEnd();
                    script = script.Replace("\\\"", "↑");
                }
            }

            return script;
        }
        private static void GetCorrectAndIncorrectStringsFromScript(string script, out List<string> correctStrings, out List<string> incorrectStrings)
        {
            Regex allStringsRegex = new Regex("\"[^\"]*((\\\\\")?.*(\\\\\"))?[^\"]*\"");
            MatchCollection allStringsMatches = allStringsRegex.Matches(script);

            incorrectStrings = (from match in allStringsMatches select match.Value).ToList();
            correctStrings = new List<string>();

            for (int i = 0; i < incorrectStrings.Count; i++)
            {
                string phrase = incorrectStrings[i];
                phrase = phrase.Replace(Environment.NewLine, " ");
                phrase = RemovingMultipleSpaces(phrase);
                correctStrings.Add(phrase);
            }
        }
        private static void ScriptCorrection(ref string script, DirectoryInfo directory, FileInfo file, List<string> correctStrings, List<string> incorrectStrings)
        {
            using (FileStream fileStream = new FileStream(directory.FullName + $"\\Logs\\{file.Name}.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    for (int i = 0; i < incorrectStrings.Count; i++)
                    {
                        if (!incorrectStrings[i].Equals(correctStrings[i]))
                        {
                            script = script.Replace(incorrectStrings[i], correctStrings[i]);
                            streamWriter.WriteLine($"{incorrectStrings[i]} => {correctStrings[i]}\n");
                        }
                    }
                }
            }
        }
        private static void OverwriteScriptToFile(string script, DirectoryInfo directory, FileInfo file)
        {
            using (FileStream fileStream = new FileStream(directory.FullName + $"\\Output\\{file.Name}", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    script = script.Replace("↑", "\\\"");
                    streamWriter.WriteLine(script);
                }
            }
        }
        private static string RemovingMultipleSpaces(string str)
        {
            string s1;
            string s2 = str;

            do
            {
                s1 = s2;
                s2 = s1.Replace("  ", " ");
            }
            while (s1 != s2);

            return s2;
        }
    }
}