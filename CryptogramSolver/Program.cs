using System;
using System.IO;

namespace CryptogramSolver
{
    class Program
    {

        static int Main(string[] args)
        {           

            // Cryptogram variables.
            string heading = "";
            string cryptogram = "";
            bool readingCryptogram = false;
            bool isPatristocrat = false;
            string cryptogramToSolve = "";
            string substitutions = "";

            // Cryptogram to be solved.
            Cryptogram cryptogramToBeSolved = null;
            string numberToSolve = "";

            // Input, output and program control.
            string inputFile = "";
            string generateHtmlInput = "";
            bool generateHtml = false;
            string solveAllInput = "";
            bool solveAll = false;
            string continueInput = "";
            bool continueRunning = true;
            string showContactsInput = "";
            string dictionaryFile = "words.txt"; // Use as default dictionary.

            // Input file comes from command line.
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter the full file path to the input file.");
                return 1;
            }
            inputFile = args[0];
            System.Console.WriteLine("Using input file: {0}", inputFile);

            // Dictionary may be specified as second paramater.
            if (args.Length == 2)
            {
                dictionaryFile = args[1];
                System.Console.WriteLine("Using dictionary file: {0}", dictionaryFile);
            }

            // Handle the file separators.
            inputFile = inputFile.Replace('\\', Path.DirectorySeparatorChar);
            inputFile = inputFile.Replace('/', Path.DirectorySeparatorChar);

            Console.Write("Generate HTML (y or n): ");
            generateHtmlInput = Console.ReadLine();
            if (generateHtmlInput.ToLower() == "y")
            {
                generateHtml = true;
            }
            else
            {
                generateHtml = false;
            }

            // Main solver.
            CryptogramSolver h = new CryptogramSolver(dictionaryFile);

            // For each line in the input file...
            string[] lines = System.IO.File.ReadAllLines(inputFile);
            foreach (string line in lines)
            {
                // Is this the start of an aristocrat?
                if ((line.StartsWith("A-")) && (char.IsNumber(line[2])))
                {
                    readingCryptogram = true;
                    heading = line;
                    // Remove the . from the heading.
                    heading = heading.Replace(".", "");
                }
                else if ((line.StartsWith("P-")) && (char.IsNumber(line[2])))
                {
                    // If this is a pastriocrat we will remove the spaces before printing.
                    readingCryptogram = true;
                    heading = line;
                    // Remove the . from the heading.
                    heading = heading.Replace(".", "");
                    isPatristocrat = true;
                }
                else
                {
                    // Not an aristocrat or patristocrat so ignore.
                }

                // Read in the lines of the cryptogram.
                if ((readingCryptogram == true) && !((line.StartsWith("A-")) || (line.StartsWith("P-"))))
                {
                    cryptogram = cryptogram + line + " ";
                }

                // Stop reading when we have a blank line.
                if ((readingCryptogram == true) && (line == ""))
                {
                    readingCryptogram = false;
                    // Strip trailing space.
                    cryptogram = cryptogram.Trim();
                    // Store this aristocrat.
                    Cryptogram newCryptogram = new Cryptogram(heading, cryptogram, isPatristocrat);
                    newCryptogram.Initialise();
                    h.cryptograms.Add(newCryptogram);
                    cryptogram = "";
                }
            }

            if (generateHtml == true)
            {
                // Generate the HTML.
                Console.WriteLine("Generating html files for all aristorcats and patristocrats.");
                foreach (Cryptogram crypto in h.cryptograms)
                {
                    h.GeneratePrintableOutput(inputFile, crypto);
                }
            }

            Console.Write("Solve all Aristocrats (y or n): ");
            solveAllInput = Console.ReadLine();
            if (solveAllInput.ToLower() == "y")
            {
                solveAll = true;
            }
            else
            {
                solveAll = false;
            }

            // Main loop.
            while (continueRunning == true)
            {
                // If solving them all.
                if (solveAll == true)
                {
                    // We are solving all.
                    Console.WriteLine("Solving all cryptograms.");
                    Console.WriteLine("Using dictionary: " + h.dictionaryFilePath + "//" + dictionaryFile);

                    foreach (Cryptogram cryptoToSolve in h.cryptograms)
                    {
                        if (!cryptoToSolve.isPatristocrat)
                        {
                            // Solve the aristocrat.
                            h.SolveCryptogram(cryptoToSolve);
                            // Reset after each one.
                            h.Reset();
                        }
                    }
                    // Update the solutions file.
                    h.PrintAllSolsToFile(inputFile);
                    h.PrintJustSolsToFile(inputFile);
                }
                else
                {
                    string solveSameInput = "n";
                    if (cryptogramToSolve.Length != 0)
                    {
                        Console.WriteLine("Last cryptogram: " + cryptogramToSolve);
                        Console.Write("Solve same cryptogram (y or n): ");
                        solveSameInput = Console.ReadLine();
                    }

                    // Are we solving a new cryptogram?
                    if (solveSameInput.ToLower() == "n")
                    {
                        Console.Write("Cryptogram to solve (A-?? or P-??): ");
                        cryptogramToSolve = Console.ReadLine();
                        bool isPatristocratToSolve = false;
                        string type = cryptogramToSolve.Substring(0, 1).ToUpper();
                        if ( type == "P")
                        {
                            isPatristocratToSolve = true;
                        }

                        numberToSolve = cryptogramToSolve.Substring(2, cryptogramToSolve.Length - 2);
                        h.cryptogramNumberToSolve = Int32.Parse(numberToSolve);

                        // Find the one matching the one to solve.
                        bool foundCryptogramToSolve = false;

                        cryptogramToBeSolved = null;
                        foreach (Cryptogram cryptoToSolve in h.cryptograms)
                        {
                            string numToSolve = cryptoToSolve.heading.Substring(2, cryptoToSolve.heading.Length - 2);
                            if (Int32.Parse(numToSolve) == h.cryptogramNumberToSolve)
                            {
                                // Check if a patristrocrat or not.
                                if ( ((cryptoToSolve.isPatristocrat == true)  && (isPatristocratToSolve == true)) ||
                                    ((cryptoToSolve.isPatristocrat == false) && (isPatristocratToSolve == false)))
                                {
                                    // We found it.
                                    cryptogramToBeSolved = cryptoToSolve;
                                    foundCryptogramToSolve = true;
                                    break;
                                }
                            }
                        }

                        // Did we find it?
                        if (foundCryptogramToSolve == false)
                        {
                            Console.WriteLine("No cryptogram " + cryptogramToSolve + " found to solve.");
                            return 0;
                        }

                        // Clear previous substitutions.
                        substitutions = String.Empty;
                    }
                    else
                    {
                        // Solving same cryptogram.
                    }

                    // Output to screen.
                    Console.WriteLine(cryptogramToBeSolved.heading);
                    Console.WriteLine(cryptogramToBeSolved.cryptogram);

                    // Show contact information.
                    Console.Write("Show contacts (y or n): ");
                    showContactsInput = Console.ReadLine();
                    if (showContactsInput.ToLower() == "y")
                    {
                        h.OutputContacts(cryptogramToBeSolved);
                    }

                    Console.WriteLine("Last substitutions: " + substitutions);
                    Console.Write("Known substitutions (AaBbCc....): ");
                    substitutions = Console.ReadLine();
                    substitutions = substitutions.Trim();

                    // Reset before running.
                    h.Reset();

                    // Do we have any substitutions?
                    if (substitutions.Length % 2 == 0)
                    {
                        substitutions = substitutions.Replace(",", "");
                        substitutions = substitutions.Replace(" ", "");
                        h.inputLetterGuess = substitutions;
                        int index = 0;
                        foreach (char letter in h.inputLetterGuess)
                        {
                            // Is it the first of the pair?
                            if ((index % 2) == 0)
                            {
                                h.UpdateAlphabets(h.inputLetterGuess[index].ToString().ToUpper(), h.inputLetterGuess[index + 1].ToString().ToLower());
                            }
                            index++;
                        }
                    }
                    else
                    {
                        Console.Write("Substitutions must be in paired form AaBbCc.... ");
                    }

                    // Solving a single cryptogram.
                    Console.WriteLine("Solving single cryptogram: " + h.cryptogramNumberToSolve);
                    Console.WriteLine("Using dictionary: " + dictionaryFile);
                    h.SolveCryptogram(cryptogramToBeSolved);
                    if (cryptogramToBeSolved.isPatristocrat)
                    {
                        h.PrintSolToFile(inputFile, h.cryptogramNumberToSolve + 25);
                    }
                    else
                    {
                        h.PrintSolToFile(inputFile, h.cryptogramNumberToSolve);
                    }

                    // Check if all words in this aristocrat are solved in which case we can add them to the dictionary.
                    if ( !(cryptogramToBeSolved.isPatristocrat) && (cryptogramToBeSolved.numberOfUnknowns == 0) )
                    {
                        h.AddToDictionary();
                    }
                }

                solveAll = false; // Only solve all of them once.
                Console.Write("Solve again (y or n): ");
                continueInput = Console.ReadLine();
                if (continueInput.ToLower() == "y")
                {
                    continueRunning = true;
                }
                else
                {
                    continueRunning = false;
                }
            }
            Console.Write("Output all sols (y or n): ");
            string outputSols = Console.ReadLine();
            if (outputSols.ToLower() == "y")
            {
                h.PrintAllSolsToFile(inputFile);
            }

            return 0;
        }
    }
}


