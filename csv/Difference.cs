﻿using BusterWood.Data;
using BusterWood.Data.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Csv
{
    class Difference
    {
        public static void Run(List<string> args)
        {
            try
            {
                if (args.Remove("--help")) Help();
                var all = args.Remove("--all");
                var reverse = args.Remove("--rev");

                DataSequence input = Args.GetDataSequence(args);

                var others = args
                    .Select(file => new { file, reader = new StreamReader(file) })
                    .Select(r => r.reader.ToCsvDataSequence(r.file))
                    .ToList();

                input.CheckSchemaCompatibility(others);

                var unionOp = all ? (Func<DataSequence, DataSequence, DataSequence>)Data.Extensions.DifferenceAll : Data.Extensions.Difference;
                DataSequence result = reverse
                    ? others.Aggregate(input, (acc, o) => unionOp(o, acc)) // reverse diff
                    : others.Aggregate(input, (acc, o) => unionOp(acc, o));

                Console.WriteLine(result.Schema.ToCsv());
                foreach (var row in result)
                    Console.WriteLine(row.ToCsv());
            }
            catch (Exception ex)
            {
                StdErr.Warning(ex.Message);
                Help();
            }
        }

        static void Help()
        {
            Console.Error.WriteLine($"csv diff[erence] [--all] [--in file] [--rev] [file ...]");
            Console.Error.WriteLine($"Outputs the rows in the input CSV that do not appear in any of the additional files");
            Console.Error.WriteLine($"\t--all    do NOT remove duplicates from the result");
            Console.Error.WriteLine($"\t--in     read the input from a file path (rather than standard input)");
            Console.Error.WriteLine($"\t--rev    reverse the difference");
            Programs.Exit(1);
        }
    }
}
