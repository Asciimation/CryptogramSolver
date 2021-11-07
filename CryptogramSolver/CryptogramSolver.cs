using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CryptogramSolver
{
    class CryptogramSolver
    {
        // Printable line size.
        const int LineSize = 55;

        // File path of dictionary files.
        public string dictionaryFilePath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "dictionaries";
        public string dictionaryFullPath;

        // FileName for our compiled dictionary.
        string compiledDictionaryFileName = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "dictionaries" + Path.DirectorySeparatorChar + "dictionaryFromSols.txt";

        // File names for output files.
        string solsFile = Path.DirectorySeparatorChar + "Sols.txt";

        // Collection of cryptograms.
        public ArrayList cryptograms = new ArrayList();

        // Single cryptogram to solve.
        public int cryptogramNumberToSolve = 0;

        // Guessed letters.
        public string inputLetterGuess = "";

        //  Cryptogram split into words.
        ArrayList words = new ArrayList();

        //  Cryptogram split into aristocrat words.
        ArrayList aristocratWords = new ArrayList();

        // Alphabets.
        string plain = "abcdefghijklmnopqrstuvwxyz";
        string cipher = "??????????????????????????";

        // Compares.
        ulong compares = 0;

        ////////////////////////////////////////////////////////////////////////////
        // Constructor.
        ////////////////////////////////////////////////////////////////////////////
        public CryptogramSolver(string dictionaryToUse)
        {
            dictionaryFullPath = dictionaryFilePath + Path.DirectorySeparatorChar + dictionaryToUse;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print out the alphabets.
        ////////////////////////////////////////////////////////////////////////////
        void PrintAlphabets(string plainAlphabet, string cipherAlphabet)
        {
            // Loop through 26 times.
            Console.WriteLine();
            Console.Write("plain:  ");
            for (int i = 0; i < 26; i++)
            {
                int index = cipherAlphabet.IndexOf((char)('A' + i));
                if (index == -1)
                {
                    Console.Write("?");
                }
                else
                {
                    Console.Write("{0}", plainAlphabet[index]);
                }
            }
            Console.WriteLine("");
            Console.WriteLine("cipher: ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Console.WriteLine("plain:  {0}", plainAlphabet);
            Console.WriteLine("cipher: {0}", cipherAlphabet);
        }

        ////////////////////////////////////////////////////////////////////////////
        // Given an encrypted word and a plain text word update
        // our alphabets.
        ////////////////////////////////////////////////////////////////////////////
        public void UpdateAlphabets(string cipherText, string plainText)
        {
            if (cipherText.Length == plainText.Length)
            {
                Console.WriteLine("UpdateAlphabets - plain:         abcdefghijklmnopqrstuvwxyz");
                Console.WriteLine("UpdateAlphabets - cipher before: " + cipher);

                // For each letter in the encrypted word.
                int index = 0;

                foreach (char cipherLetter in cipherText)
                {
                    // If we haven't already added this letter.
                    if (!(cipher.Contains(cipherLetter)))
                    {
                        // Get the matching plain text letter.
                        char plainLetter = plainText[index];
                        // Find it in the plain alphabet.
                        int plainIndex = plain.IndexOf(plainLetter);
                        // And insert the encrypted letter in the cipher alphabet.
                        cipher = cipher.Substring(0, plainIndex) + cipherLetter + cipher.Substring(plainIndex + 1);
                    }
                    index++;
                }
                Console.WriteLine("UpdateAlphabets - cipher after:  " + cipher);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Given an encrypted letter get the known plain text letter
        // and return it. If unknown return a '.' character.
        ////////////////////////////////////////////////////////////////////////////
        char GetDecryptedLetter(char letter)
        {
            // Get the index.
            int index = cipher.IndexOf(Char.ToUpper(letter));
            if (index == -1)
            {
                // Not found so return '.'
                return '.';
            }
            else
            {
                // It was found so return the matching unencrypted letter.
                return plain[index];
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        // Given an input string convert it to a regular expression
        // with the appropriate back references. Compare with the
        // cipher alphabet to insert any known letters.
        ////////////////////////////////////////////////////////////////////////////
        string ConvertToRegex(string input, bool substitutions)
        {

            ArrayList regex = new ArrayList(input.Length);
            // Break the input word into string arrays.            
            foreach (char letter in input)
            {
                regex.Add(letter.ToString());
            }

            // Back references start at 1.
            int backRef = 1;

            // First pass for each letter in the word.          
            for (int index1 = 0; index1 < input.Length; index1++)
            {
                // If this is a single letter.
                if ((regex[index1].ToString().Length == 1) && (Char.IsLetter(char.Parse(regex[index1].ToString()))))
                {
                    // If the word contains other instances of this letter.               
                    int count = input.Split(char.Parse(regex[index1].ToString())).Length - 1;
                    if (count > 1)
                    {
                        char letter = regex[index1].ToString()[0];
                        // Change the letter at index to a pattern.
                        if (substitutions)
                        {
                            char decryptedLetter = GetDecryptedLetter(letter);
                            regex[index1] = "(" + decryptedLetter + ")";
                        }
                        else
                        {
                            regex[index1] = "(.)";
                        }

                        // Replace all other instances with the current back reference.
                        int indexBackRefLetter = 0;
                        for (int index2 = 0; index2 < input.Length; index2++)
                        {
                            if (regex[index2].ToString() == letter.ToString())
                            {
                                regex[indexBackRefLetter] = @"\" + backRef.ToString();
                            }
                            indexBackRefLetter++;
                        }

                        // Increment the back reference.
                        backRef = backRef + 1;
                    }
                }
            }

            // Second pass we convert any remaining single characters to a dot.
            for (int index3 = 0; index3 < input.Length; index3++)
            {
                // If this is a single letter.
                if ((regex[index3].ToString().Length == 1) && (Char.IsLetter(char.Parse(regex[index3].ToString()))))
                {
                    // Change the letter at index to a pattern.
                    if (substitutions)
                    {
                        char letter = regex[index3].ToString()[0];
                        char decryptedLetter = GetDecryptedLetter(letter);
                        regex[index3] = decryptedLetter.ToString();
                    }
                    else
                    {
                        regex[index3] = ".";
                    }
                }
            }

            // Add the anchors to the returned string.
            string regexString = "";
            foreach (string s in regex)
            {
                regexString = regexString + s;
            }
            regexString = '^' + regexString + '$';
            return regexString;
        }


        ////////////////////////////////////////////////////////////////////////////
        // Remove words with mismatching letters from the word list.
        // For each word check the known letter substitutions. We can remove any
        // words from the list where the letters in the words don't match the
        // already known letters.
        ////////////////////////////////////////////////////////////////////////////
        ArrayList RemoveWordsWithLetterMismatches(ArrayList wordList, string cipherWord)
        {
            ArrayList cleanedList = new ArrayList();

            foreach (String word in wordList)
            {
                //Console.WriteLine("word: " + word);

                // Check that any pairings match our known matches.
                bool knownLetterMismatch = false;

                int i = 0;
                foreach (char letter in word)
                {
                    if (Char.IsLetter(letter))
                    {
                        // Get the given cipher letter.
                        char givenCipher = cipherWord[i];
                        // See if there is a matching known cipher letter.
                        char knownCipher = (char)cipher[letter - 97];

                        // If it is known and it doesn't match the given letter reject this word.
                        if ((knownCipher != '?') && (knownCipher != givenCipher))
                        {
                            knownLetterMismatch = true;
                            break;
                        }
                    }
                    i++;
                }

                // If the word is valid add it to the cleaned word list.
                if (!knownLetterMismatch)
                {
                    if (!cleanedList.Contains(word))
                    {
                        cleanedList.Add(word);
                    }
                }
            }

            // Return the cleaned list.
            return cleanedList;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Get a list of words from a file given an input cipher word.
        // - Given an cipher word generate a regex with known letter substitutions.
        // - For each line in the file check the line against the regex with
        // substitutions.
        // - If a line matches generate a regex for the line and the cipher word
        // with no substitutions and compare them.
        // - If they match compare each letter in the line with each letter in the
        // cipher word.
        // - In none match (i.e. no letter encodes to itself) add the word to our
        // list of possible matches.
        ////////////////////////////////////////////////////////////////////////////
        ArrayList GetWordListFromFileUsingInputWord(String cipherWord)
        {
            ArrayList list = new ArrayList();
            // Convert the cipher word into regexes with and without substitutions.
            string wordRegexSubs = ConvertToRegex(cipherWord, true);
            string wordRegexNoSubs = ConvertToRegex(cipherWord, false);
            Regex rgx = new Regex(wordRegexSubs);

            foreach (string line in File.ReadLines(dictionaryFullPath))
            {
                string lineLowerCase = line.ToLower();
                // Does the regex match the line?
                bool regexMatch = Regex.Match(lineLowerCase, wordRegexSubs).Success;
                if (regexMatch)
                {
                    // Convert each dictionary word into a regex with no substitutions.                
                    string lineRegex = ConvertToRegex(lineLowerCase, false);

                    // If the regex of this dictionary word matches our original regex.
                    if (lineRegex.Equals(wordRegexNoSubs, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Compare the letters of the cipher word with the plain word
                        // and reject any word with a letter that encodes to itself.
                        bool encodeToSelf = false;

                        int i = 0;
                        foreach (char cipherLetter in cipherWord)
                        {
                            if (Char.ToUpperInvariant(cipherLetter) == Char.ToUpperInvariant(lineLowerCase[i]))
                            {
                                encodeToSelf = true;
                            }
                            i++;
                        }

                        // If the word isn't valid we don't add it.
                        if (!encodeToSelf)
                        {
                            list.Add(lineLowerCase);
                        }
                    }
                }
            }
            // Return the new list.
            return list;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Build a list of words from a given list where the words match the regex.
        ////////////////////////////////////////////////////////////////////////////
        static ArrayList GetWordListFromListUsingRegex(ArrayList wordList, string regex)
        {
            ArrayList list = new ArrayList();
            foreach (string word in wordList)
            {
                // If the regex of this dictionary word matches our original regex.
                bool regexMatch = Regex.Match(word, regex).Success;
                if (regexMatch)
                {
                    list.Add(word);
                }
            }
            return list;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Given two words build a mapping of the position of
        // identical letters in both.
        ////////////////////////////////////////////////////////////////////////////
        static List<Tuple<int, int>> GetMatchingLetters(string word1, string word2)
        {
            List<Tuple<int, int>> tupleListofPairs = new List<Tuple<int, int>>();

            int indexWord1 = 0;
            foreach (char letter1 in word1)
            {
                int indexWord2 = 0;
                foreach (char letter2 in word2)
                {
                    if (letter1 == letter2)
                    {
                        tupleListofPairs.Add(Tuple.Create(indexWord1, indexWord2));
                    }
                    indexWord2++;
                }
                indexWord1++;
            }
            return tupleListofPairs;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Input is two lists.
        // A list of words we compare against and a list of words we wish to compare.
        // The list were generated from a dictionary search based on a regex so all
        // the words in the list match a particular pattern.
        // We return a third list which is the list of words we have compared with
        // any words that didn't have any possible matches to the first list
        // removed.
        // The comparison is based on checking letter matches between the two words.
        // A match is a pair of letters common to both words. If all the pairs of
        // letters in the positions match up we have potentially matching words.
        // If there are no possible matches at all that word can't be correct so
        // it is removed from the list.
        // We return a list of all words with the non matches removed.

        ////////////////////////////////////////////////////////////////////////////
        ArrayList ReduceList(List<Tuple<int, int>> matches, ArrayList listOfWordsToCompare, ArrayList listToCompareAgainst)
        {

            // Our return list starts as a copy of the list of words to compare.
            ArrayList reducedList = new ArrayList(listOfWordsToCompare);

            // For each word in the list of words to compare.
            foreach (String modifyListWord in listOfWordsToCompare)
            {
                // We assume we have no possible matching words.
                bool wordMatch = false;

                foreach (String compareListWord in listToCompareAgainst)
                {
                    // Check if all the letters match.
                    // Assume they do and set the flag to false if there are any mismatches.
                    bool letterMatch = true;

                    foreach (Tuple<int, int> pair in matches)
                    {
                        int firstOfPair = pair.Item1;
                        int secondOfPair = pair.Item2;
                        string letter1 = modifyListWord[firstOfPair].ToString();
                        string letter2 = compareListWord[secondOfPair].ToString();
                        //Console.Write("modifyListWord: " + modifyListWord + " compareListWord: " + compareListWord + " Pair: " + pair.Item1 + "," + pair.Item2 + " ");
                        compares++;

                        if (!(letter1.Equals(letter2, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            // The letters don't match.                            
                            //Console.WriteLine("Letters don't match.");
                            letterMatch = false;
                            break;
                        }
                        else
                        {
                            //Console.WriteLine("Letters match.");
                        }
                    }

                    // If all the letters matched we might have a matching word.
                    if (letterMatch)
                    {
                        //Console.WriteLine("Possible matching words.");
                        wordMatch = true;
                    }
                }

                // We remove the word if there were no possible matches at all.
                if (!wordMatch)
                {
                    //Console.WriteLine("Removing word: " + modifyListWord);
                    reducedList.Remove(modifyListWord);
                }
            }
            return reducedList;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Compare and reduce lists.
        // Only compare lists with words in them.
        ////////////////////////////////////////////////////////////////////////////
        void CompareAndReduceLists(int list1Index, int list2Index)
        {

            List<Tuple<int, int>> matches;

            AristocratWord aristocratWord1 = (AristocratWord)aristocratWords[list1Index];
            AristocratWord aristocratWord2 = (AristocratWord)aristocratWords[list2Index];

            if ((aristocratWord1.wordList.Count > 1) && (aristocratWord2.wordList.Count > 1))
            {
                matches = GetMatchingLetters(aristocratWord1.word, aristocratWord2.word);
                // Only compare if we have matching letters between words.
                if (matches.Count > 0)
                {
                    ArrayList reducedList = new ArrayList();
                    //Console.WriteLine("Comparing cryptogram word: " + aristocratWord1.word + " to: " + aristocratWord2.word);
                    reducedList = ReduceList(matches, aristocratWord1.wordList, aristocratWord2.wordList);
                    aristocratWord1.wordList = reducedList;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Compare lists.
        // Then reduce the list based on the matches.
        ////////////////////////////////////////////////////////////////////////////
        void CompareLists()
        {
            Console.WriteLine("Comparing lists.");

            // Compare each word against all other words if different.
            int index = 0;
            foreach (AristocratWord aristocratWordToCompare in aristocratWords)
            {
                // If the list needs further reducing.
                if (aristocratWordToCompare.isDone == false)
                {
                    // Compare against all other words in turn.
                    for (int j = 0; j < aristocratWords.Count; j++)
                    {
                        // If we're on the last word in the list compare to the first.
                        if (j == (aristocratWords.Count - 1))
                        {
                            // If the words are different.
                            if (!(aristocratWordToCompare.word).ToString().Equals(((AristocratWord)aristocratWords[0]).word, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // And neither is a noun.
                                if ((aristocratWordToCompare.isNoun == false) && (((AristocratWord)aristocratWords[0]).isNoun == false))
                                {
                                    CompareAndReduceLists(index, 0);
                                }
                            }
                        }
                        else
                        {
                            // If the words are different.
                            if (!(aristocratWordToCompare.word).ToString().Equals(((AristocratWord)aristocratWords[j]).word, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // And neither is a noun.
                                if ((aristocratWordToCompare.isNoun == false) && (((AristocratWord)aristocratWords[j]).isNoun == false))
                                {
                                    CompareAndReduceLists(index, j);
                                }
                            }
                        }

                        // If the list we just worked on now only has one word we update our alphabets and regenerate
                        // all the regexes before doing more comparisons.
                        if (aristocratWordToCompare.wordList.Count == 1)
                        {
                            UpdateAndRegenerateAllLists(aristocratWordToCompare);                           
                        }
                    }
                }
                index++;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Update and regenerate regexes.
        // When a word list gets to one word we can update our alphabets and
        // regenerate all regexes and reduce the lists.
        ////////////////////////////////////////////////////////////////////////////
        void UpdateAndRegenerateAllLists(AristocratWord aristocratWord)
        {
            // If the list we just worked on now only has one word we update our alphabets and regenerate
            // all the regexes before doing more comparisons.

            string plainText = aristocratWord.wordList[0].ToString();
            UpdateAlphabets(aristocratWord.word.ToUpper(), plainText);

            // Update that we have done this word.
            aristocratWord.isDone = true;
            Console.WriteLine("Single word found: " + aristocratWord.wordList[0]);

            // Update all the regexes for ALL words and remove non matching words.
            foreach (AristocratWord aristocratWordToUpdate in aristocratWords)
            {
                if ((aristocratWordToUpdate.isDone == false) && (aristocratWordToUpdate.isNoun == false))
                {
                    int count = 0;
                    ArrayList originalList;

                    // Generate the regex.
                    aristocratWordToUpdate.regex = ConvertToRegex(aristocratWordToUpdate.word, true);
                    Console.WriteLine("Aristocrat word regex: " + aristocratWordToUpdate.regex);

                    // Reduce the list based on the new regex.                   
                    count = aristocratWordToUpdate.wordList.Count;
                    originalList = aristocratWordToUpdate.wordList;
                    aristocratWordToUpdate.wordList = GetWordListFromListUsingRegex(aristocratWordToUpdate.wordList, aristocratWordToUpdate.regex);
                    if (aristocratWordToUpdate.wordList.Count != count)
                    {
                        Console.WriteLine("Word list has reduced.");
                        Console.WriteLine("Before GetWordListFromListUsingRegex");
                        PrintList(originalList);
                        Console.WriteLine("After GetWordListFromListUsingRegex");
                        PrintList(aristocratWordToUpdate.wordList);
                    }

                    // Remove any words that don't match the the already known solved letters.
                    count = aristocratWordToUpdate.wordList.Count;
                    originalList = aristocratWordToUpdate.wordList;
                    aristocratWordToUpdate.wordList = RemoveWordsWithLetterMismatches(aristocratWordToUpdate.wordList, aristocratWordToUpdate.word);
                    if (aristocratWordToUpdate.wordList.Count != count)
                    {
                        Console.WriteLine("Word list has reduced.");
                        Console.WriteLine("Before RemoveWordsWithLetterMismatches");
                        PrintList(originalList);
                        Console.WriteLine("After RemoveWordsWithLetterMismatches");
                        PrintList(aristocratWordToUpdate.wordList);
                    }

                    // Now for any words not already done we recursively keep reducing if any other
                    // lists now only have one words in them.
                    if (aristocratWordToUpdate.wordList.Count == 1)
                    {                        
                        Console.WriteLine("Recursion!");
                        UpdateAndRegenerateAllLists(aristocratWordToUpdate);
                    }
                }             
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Get the word count.
        ////////////////////////////////////////////////////////////////////////////
        int GetWordCount()
        {
            int wordCount = 0;
            // Sum all the word lists.

            foreach (AristocratWord aristocratWordToCount in aristocratWords)
            {
                wordCount = wordCount + aristocratWordToCount.wordList.Count;
            }
            return wordCount;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print out the word lists and count.
        ////////////////////////////////////////////////////////////////////////////
        void PrintWordLists()
        {
            // Print all the word lists.
            Console.WriteLine("");
            foreach (AristocratWord aristocratWordToPrint in aristocratWords)
            {
                Console.Write(aristocratWordToPrint.wordNumber + " (" + aristocratWordToPrint.regex + ") : " + aristocratWordToPrint.word + ":(" + aristocratWordToPrint.wordList.Count + "):");
                // Only print the first X number of words to save time!
                int wordsPrinted = 0;
                const int wordsToPrint = 25;
                foreach (string word in aristocratWordToPrint.wordList)
                {
                    if (wordsPrinted < wordsToPrint)
                    {
                        Console.Write(word + " ");
                    }
                    else
                    {
                        Console.Write("... plus " + (aristocratWordToPrint.wordList.Count - wordsToPrint) + " more words");
                        break;
                    }
                    wordsPrinted++;
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
            Console.WriteLine("Word count: " + GetWordCount());
            Console.WriteLine("");
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print a list
        ////////////////////////////////////////////////////////////////////////////
        void PrintList(ArrayList list)
        {
            Console.Write("Count: " + list.Count);
            foreach (string word in list)
            {

                Console.Write(" " + word);

            }
            Console.WriteLine("");
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print out the substitutions.
        ////////////////////////////////////////////////////////////////////////////
        void PrintSubstitutions()
        {

            // Print out letter counts.
            int index = 0;
            foreach (string word in words)
            {
                int wordSize = word.ToString().Length;
                Console.Write(index + 1);
                int spacesToPrint = wordSize - (index + 1).ToString().Length + 2;

                for (int i = 0; i < spacesToPrint; i++)
                {
                    // Print the correct number of padding spaces.
                    Console.Write(" ");
                }
                index++;
            }
            Console.WriteLine("");
            // Print out all the cipher words.
            foreach (string word in words)
            {
                Console.Write(word + "  ");
            }
            Console.WriteLine("");

            // For each word we have.
            foreach (string currentWord in words)
            {
                // Is it a noun?
                if (currentWord.Contains('*'))
                {
                    Console.Write(" ");
                }

                // For each letter.
                foreach (char letter in currentWord)
                {
                    if (Char.IsLetter(letter))
                    {
                        // Look at each letter in the cipher alphabet.
                        int matchingIndex = -1;

                        int letterIndex = 0;
                        foreach (char cipherLetter in cipher)
                        {
                            // Does it match?
                            if ((char.ToUpperInvariant(letter)) == (char.ToUpperInvariant(cipherLetter)))
                            {
                                // If it matched remember the index.
                                matchingIndex = letterIndex;
                            }
                            letterIndex++;
                        }

                        // If there was a match print it.
                        if (matchingIndex > -1)
                        {
                            Console.Write(plain[matchingIndex]);
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }
                }
                Console.Write("  ");
            }
            Console.WriteLine("");
        }

        ////////////////////////////////////////////////////////////////////////////
        // Update the solution for an Cryptogram.
        ////////////////////////////////////////////////////////////////////////////
        void UpdateCryptogramSolution(Cryptogram a)
        {
            // Rebuild the solution.
            a.solution = "";
            a.numberOfUnknowns = 0;

            foreach (char ch in a.cryptogram)
            {
                if (Char.IsLetter(ch))
                {
                    // Look at each letter in the cipher alphabet.
                    int matchingIndex = -1;

                    int letterIndex = 0;
                    foreach (char cipherLetter in cipher)
                    {
                        // Does it match?
                        if ((char.ToUpperInvariant(ch)) == (char.ToUpperInvariant(cipherLetter)))
                        {
                            // If it matched remember the index.
                            matchingIndex = letterIndex;
                            break;
                        }
                        letterIndex++;
                    }
                    // If there was a match print it.
                    if (matchingIndex > -1)
                    {
                        a.solution = a.solution + (plain[matchingIndex]);
                    }
                    else
                    {
                        a.solution = a.solution + '_';
                        // Count how many are unknown still.
                        a.numberOfUnknowns++;
                    }
                }
                else
                {
                    a.solution = a.solution + ch;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print out a single solution to the sols file.
        ////////////////////////////////////////////////////////////////////////////
        public void PrintSolToFile(string inputFile, int sol)
        {
            // Use the directory of the digital cons file to be the directory of the sols file.
            string outputFile = Path.GetDirectoryName(inputFile);
            outputFile = outputFile + solsFile;
            using (StreamWriter file = new StreamWriter(outputFile, true))
            {

                // For the given cryptogram.
                Cryptogram a = (Cryptogram)cryptograms[sol - 1];

                // Output to screen.
                Console.WriteLine(a.heading);
                Console.WriteLine(a.cryptogram);
                Console.WriteLine(a.solution);
                Console.WriteLine();

                // Output to file.
                file.WriteLine();
                file.WriteLine(a.heading);
                file.WriteLine(a.cryptogram);
                file.WriteLine(a.solution);

                // Loop through 26 times.
                file.WriteLine();
                file.Write("plain:  ");
                for (int i = 0; i < 26; i++)
                {
                    int index = a.cipherAlphabet.IndexOf((char)('A' + i));
                    if (index == -1)
                    {
                        file.Write("?");
                    }
                    else
                    {
                        file.Write("{0}", a.plainAlphabet[index]);
                    }
                }
                file.WriteLine("");
                file.WriteLine("cipher: ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                file.WriteLine("plain:  {0}", a.plainAlphabet);
                file.WriteLine("cipher: {0}", a.cipherAlphabet);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Print just the solutiosn at the end of the sols file.
        ////////////////////////////////////////////////////////////////////////////
        public void PrintJustSolsToFile(string inputFile)
        {
            // Use the directory of the digital cons file to be the directory of the sols file.
            string outputFile = Path.GetDirectoryName(inputFile);
            outputFile = outputFile + solsFile;
            using (StreamWriter file = new StreamWriter(outputFile, true))
            {
                file.WriteLine();
                file.WriteLine("Used dictionary file: " + dictionaryFullPath);
                file.WriteLine("Word compares done: " + compares);
                file.WriteLine();
                // For each cryptogram.
                foreach (Cryptogram a in cryptograms)
                {
                    if (!a.isPatristocrat)
                    {
                        if (a.solution != string.Empty)
                        {
                            file.Write(a.heading);
                            file.Write("   ");
                            file.WriteLine(a.solution);
                        }
                    }
                }
                file.WriteLine();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Add new words to dictionary.
        ////////////////////////////////////////////////////////////////////////////
        public void AddToDictionary()
        {
            Console.WriteLine("Adding new words to dictionary.");
            ArrayList dictionaryWords = new ArrayList();
            int initialSize = 0;

            // Read in existing dictionary into a list.
            foreach (string line in File.ReadLines(compiledDictionaryFileName))
            {
                string lineLowerCase = line.ToLower();
                lineLowerCase = lineLowerCase.Replace(@"'", "");
                lineLowerCase = lineLowerCase.Replace(@"*", "");
                if (!dictionaryWords.Contains(lineLowerCase))
                {
                    dictionaryWords.Add(lineLowerCase);
                }
            }
            initialSize = dictionaryWords.Count;

            // For each cryptogram add the know words to the list.
            foreach (Cryptogram a in cryptograms)
            {
                // Generate the words to work on.
                ArrayList solutionWords = new ArrayList(a.solution.Split());

                // For each word in the cryptogram.
                foreach (string word in solutionWords)
                {
                    if ((!word.Contains("_")) && (!word.StartsWith("*")))
                    {
                        // Remove anything not a letter.
                        Regex rgx = new Regex("[^a-z]");
                        string cleanedWord = rgx.Replace(word, "");
                        cleanedWord = cleanedWord.Trim();
                        if (!dictionaryWords.Contains(cleanedWord))
                        {
                            dictionaryWords.Add(cleanedWord);
                            Console.WriteLine(cleanedWord);
                        }
                    }
                }
            }

            // Output the new list.
            Console.WriteLine(dictionaryWords.Count - initialSize + " new words added. Start size: " + initialSize + " Final size: " + dictionaryWords.Count);
            using (StreamWriter file = new StreamWriter(compiledDictionaryFileName, false))
            {
                // Output the new dictionary.
                foreach (string word in dictionaryWords)
                {
                    file.WriteLine(word);
                }
            }

        }

        ////////////////////////////////////////////////////////////////////////////
        // Print out all solutions to the sols file.
        ////////////////////////////////////////////////////////////////////////////
        public void PrintAllSolsToFile(string inputFile)
        {
            // For each cryptogram.
            int index = 1;
            foreach (Cryptogram a in cryptograms)
            {
                PrintSolToFile(inputFile, index);
                index++;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Generate printable output.
        ////////////////////////////////////////////////////////////////////////////
        public void GeneratePrintableOutput(string inputFile, Cryptogram cryptogram)
        {
            // Use the directory of the digital cons file to be the directory of the sols file.
            string htmlFile = Path.GetDirectoryName(inputFile);
            htmlFile = htmlFile + Path.DirectorySeparatorChar + cryptogram.heading + ".html";
            cryptogram.CreateHTMLFile(htmlFile);
        }

        ////////////////////////////////////////////////////////////////////////////
        // Output contacts table.
        ////////////////////////////////////////////////////////////////////////////
        public void OutputContacts(Cryptogram cryptogram)
        {
            cryptogram.OutputContacts();
        }

        ////////////////////////////////////////////////////////////////////////////
        // Reset.
        ////////////////////////////////////////////////////////////////////////////
        public void Reset()
        {
            //  Cryptogram split into words.
            words.Clear();
            //  Cryptogram split into aristocrat words.
            aristocratWords.Clear();
            // Alphabets.
            cipher = "??????????????????????????";
            // Number of compares.
            compares = 0;
        }

        ////////////////////////////////////////////////////////////////////////////
        // SolveCryptogram.
        ////////////////////////////////////////////////////////////////////////////
        public void SolveCryptogram(Cryptogram cryptogram)
        {
            // Convert to upper case.
            cryptogram.cryptogram = cryptogram.cryptogram.ToUpper();
            Console.WriteLine();
            Console.WriteLine(cryptogram.heading);
            Console.WriteLine(cryptogram.cryptogram);

            // Generate the words to work on.
            words = new ArrayList(cryptogram.cleanedCryptogram.Split());

            // For each word build the aristocrat word list.
            int index = 0;
            foreach (string currentWord in words)
            {
                if ((currentWord.Contains("*")))
                {
                    string modifiedWord = currentWord.Replace("\\*", "");
                    string regex = "";
                    ArrayList wordList = new ArrayList();
                    aristocratWords.Add(new AristocratWord(index + 1, modifiedWord, true, regex, wordList));
                }
                else
                {
                    string regex = ConvertToRegex(currentWord as String, true);
                    ArrayList wordList = new ArrayList(GetWordListFromFileUsingInputWord(currentWord as String));
                    aristocratWords.Add(new AristocratWord(index + 1, currentWord as String, false, regex, wordList));
                }
                index++;
            }

            // Update all the regexes.
            foreach (AristocratWord aristocratWord in aristocratWords)
            {
                if (aristocratWord.isNoun == false)
                {
                    aristocratWord.regex = ConvertToRegex(aristocratWord.word, true);
                }
            }

            int wordCount = GetWordCount();
            int previousWordCount = 0;
            int iterations = 0;
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            PrintWordLists();

            // While the count keeps going down...
            while (wordCount != previousWordCount)
            {

                // Count words and iterations.
                previousWordCount = wordCount;
                iterations = iterations + 1;

                // Compare all the lists with each other.
                CompareLists();

                // Print the results of this iteration.
                PrintWordLists();
                PrintSubstitutions();
                wordCount = GetWordCount();
            }

            // Store the cipher alphabet.
            cryptogram.cipherAlphabet = cipher;
            PrintAlphabets(plain, cipher);

            Console.WriteLine();

            long endTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("Time taken (S): {0}", (endTime - startTime) / 1000);
            Console.WriteLine("Iterations: {0}", iterations - 1);
            Console.WriteLine("Number of comparisons: {0}", compares);

            Console.WriteLine();
            Console.WriteLine("Search complete.");
            Console.WriteLine();
            PrintSubstitutions();
            UpdateCryptogramSolution(cryptogram);
            Console.WriteLine();
        }
    }
}
