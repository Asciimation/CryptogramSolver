using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptogramSolver
{
    class Cryptogram
    {
        // Printable line size.
        const int LineSize = 55;

        public string heading = "";
        public string cryptogram = "";
        public bool isPatristocrat = false;
        public string cleanedCryptogram = "";
        // Lines split for printing.
        public ArrayList printableLines = new ArrayList();
        // Contacts for each letter of the alphabet.
        public ContactsForLetter[] contacts = new ContactsForLetter[26];
        public string solution = "";
        public string plainAlphabet = "abcdefghijklmnopqrstuvwxyz";
        public string cipherAlphabet = "";
        public int numberOfUnknowns = 0;

        public Cryptogram(string heading, string cryptogram, bool isPatristocrat)
        {
            this.heading = heading;
            this.cryptogram = cryptogram;
            this.isPatristocrat = isPatristocrat;

            for (int i = 0; i < 26; i++)
            {
                ContactsForLetter contactList = new ContactsForLetter();
                contacts[i] = contactList;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Initialise cryptogram.
        // Generate a contact list from the cryptogram.
        // Generate lines we can print out.
        ////////////////////////////////////////////////////////////////////////////
        public void Initialise()
        {
            this.CleanInputCryptogram();
            this.GenerateContactList();
            this.GeneratePrintableLines();
        }

        ////////////////////////////////////////////////////////////////////////////
        // Clean the input cryptogram.
        // Remove all non necessary characters from the input cryptogram.
        ////////////////////////////////////////////////////////////////////////////
        void CleanInputCryptogram()
        {
            Console.WriteLine("Input cryptogram: {0}", this.cryptogram);
            string cleanedCryptogram = "";
            cleanedCryptogram = this.cryptogram.Replace("'", "");
            cleanedCryptogram = cleanedCryptogram.Replace(",", "");
            cleanedCryptogram = cleanedCryptogram.Replace("�", "");            
            cleanedCryptogram = cleanedCryptogram.Replace(":", "");
            cleanedCryptogram = cleanedCryptogram.Replace(";", "");
            cleanedCryptogram = cleanedCryptogram.Replace(".", "");
            cleanedCryptogram = cleanedCryptogram.Replace("=", " ");
            cleanedCryptogram = cleanedCryptogram.Replace("!", "");
            cleanedCryptogram = cleanedCryptogram.Replace("?", "");
            cleanedCryptogram = cleanedCryptogram.Replace("\"", "");
            cleanedCryptogram = cleanedCryptogram.Replace("“", "");
            cleanedCryptogram = cleanedCryptogram.Replace("”", "");
            cleanedCryptogram = cleanedCryptogram.Replace("`", "");
            cleanedCryptogram = cleanedCryptogram.Replace("(", "");
            cleanedCryptogram = cleanedCryptogram.Replace(")", "");
            cleanedCryptogram = cleanedCryptogram.Replace("/", " ");
            cleanedCryptogram = cleanedCryptogram.Replace("#", " ");
            cleanedCryptogram = cleanedCryptogram.Replace("’", "");
            cleanedCryptogram = cleanedCryptogram.Replace("[", " ");
            cleanedCryptogram = cleanedCryptogram.Replace("]", " ");
            cleanedCryptogram = Regex.Replace(cleanedCryptogram, @"\d", "");
            while (cleanedCryptogram.Contains("  "))
            {
                cleanedCryptogram = cleanedCryptogram.Replace("  ", " ");
            }

            // Is this a patristocrat?
            if (this.isPatristocrat == true)
            {
                Console.WriteLine("Cryptogram is a patristocrat - removing spaces.");
                cleanedCryptogram = cleanedCryptogram.Replace(" ", "");
            }

            this.cleanedCryptogram = cleanedCryptogram;
            Console.WriteLine("Cleaned cryptogram: " + this.cleanedCryptogram);
        }


        ////////////////////////////////////////////////////////////////////////////
        // Generate printable lines.
        // Turn the cryptogram string into something we can print.
        ////////////////////////////////////////////////////////////////////////////
        void GeneratePrintableLines()
        {
            string[] splitCryptogram = this.cryptogram.Split();
            int lineIndex = 0;
            if (splitCryptogram.Length == 1)
            {
                string s = "";
                foreach (String letter in splitCryptogram)
                {
                    s = s + letter;
                    if (s.Length > LineSize)
                    {
                        printableLines[lineIndex] = s;
                        lineIndex++;
                        s = "";
                    }
                    printableLines[lineIndex] = s;
                }
            }
            else
            {
                string line = "";
                foreach (String word in splitCryptogram)
                {
                    line = line + ' ' + word;
                    if (line.Length > LineSize)
                    {
                        printableLines.Add(line);
                        line = "";
                    }
                }
                printableLines.Add(line);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Generate contacts list.
        // To generate the contact list we need just the letters and the spaces in
        // the cryptogram.
        ////////////////////////////////////////////////////////////////////////////
        void GenerateContactList()
        {
            // Initialise the contacts array.
            for (int i = 0; i > contacts.Length; i++)
            {
                contacts[i] = new ContactsForLetter();
            }

            // Replace spaces with '-'.
            string unspacedCryptogram = this.cleanedCryptogram.Replace(" ", "-");
            // Remove all * characters.
            unspacedCryptogram = unspacedCryptogram.Replace("*", "");
            Console.WriteLine("Input cryptogram unspaced: {0}", unspacedCryptogram);
            Console.WriteLine();
            // For each letter in the cryptogram.
            int index = 0;
            foreach (char letter in unspacedCryptogram)
            {
                // Convert the letter to an index. 
                byte[] asciiBytes = Encoding.ASCII.GetBytes(letter.ToString());
                int letterArrayIndex = asciiBytes[0] - 65;

                // Check it is not a - character.
                if (letter != '-')
                {
                    // Handle the initial letter.
                    if (index == 0)
                    {
                        contacts[letterArrayIndex].Add('-', unspacedCryptogram[index + 1]);
                    }
                    // Handle last letter. 
                    else if (index == (unspacedCryptogram.Length - 1))
                    {
                        contacts[letterArrayIndex].Add(unspacedCryptogram[index - 1], '-');
                        // Handle the middle letters.
                    }
                    else
                    {
                        contacts[letterArrayIndex].Add(unspacedCryptogram[index - 1], unspacedCryptogram[index + 1]);
                    }
                }
                index++;
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        // Create printable HTML file.
        ////////////////////////////////////////////////////////////////////////////
        public void CreateHTMLFile(string HTMLFileToPrint)
        {
            using (StreamWriter file = new StreamWriter(HTMLFileToPrint, false))
            {

                StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(
@"<html> 
   <head>
    <style type='text/css'>
                       body {
                           font-family: Courier New;
                       }
                       h1 {
                           font-size: 1.7em;
                       }
                       h2 {
                           font-size: 1.0em;
                       }
                       p {
                           font-size: 0.6em;
                           line-height: .75;
                           white-space: pre;
                       }
                       p2 {
                           font-size: 1.0em;
                           line-height: .75;
                           white-space: pre;
                       }</style>
 </head>
 <body>
");

                // Header.
                sb.AppendLine("<p>" + heading + "</p>");

                // Printable lines.
                foreach (string line in printableLines)
                {
                    sb.AppendLine("<h1>" + line + "</h1><br>");
                }
                sb.AppendLine();

                // Contacts table.
                // Iterate each alphabet letter.
                int index = 0;
                int letterCount = 0;
                foreach (ContactsForLetter letterContacts in contacts)
                {
                    // Convert to a character based on alphabet collection index.
                    char c = (char)('A' + index);

                    // For each alphabet letter add the contact data into a set so we can get
                    // a count of the unique items. Also build up a nice printable string for output.
                    ArrayList contactLetters = new ArrayList();
                    string output = "<p>" + c + ": ";

                    foreach (Tuple<char, char> contactList in letterContacts.tupleListofContacts)
                    {
                        if (((contactList.Item1) != '-') && (!contactLetters.Contains(contactList.Item1)))
                        {
                            contactLetters.Add(contactList.Item1);
                        }
                        if (((contactList.Item2) != '-') && (!contactLetters.Contains(contactList.Item2)))
                        {
                            contactLetters.Add(contactList.Item2);
                        }
                        output = output + contactList.Item1 + contactList.Item2 + " ";
                    }

                    output = output + "  :  " + contacts[index].tupleListofContacts.Count + "/" + contactLetters.Count + "</p>";
                    letterCount = letterCount + contacts[index].tupleListofContacts.Count;
                    sb.AppendLine(output);

                    index++;
                }
                sb.AppendLine("<p>" + letterCount + "</p><br>");

                sb.AppendLine(
@"<p2>K1:</p><br>
<p2>   A B C D E F G H I J K L M N O P Q R S T U V W X Y Z</p><br>
<p2>   a b c d e f g h i j k l m n o p q r s t u v w x y z</p><br>
<p2>K2:</p><br>");

                sb.AppendLine(@"    </body>");
                sb.AppendLine(@"</html>");

                // Output the file.
                file.Write(sb.ToString());
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Output contacts to the console.
        ////////////////////////////////////////////////////////////////////////////
        public void OutputContacts()
        {
            // Contacts table.
            // Iterate each alphabet letter.
            int index = 0;
            int letterCount = 0;
            foreach (ContactsForLetter letterContacts in contacts)
            {
                // Convert to a character based on alphabet collection index.
                char c = (char)('A' + index);

                // For each alphabet letter add the contact data into a set so we can get
                // a count of the unique items. Also build up a nice printable string for output.
                ArrayList contactLetters = new ArrayList();
                string output = "";
                foreach (Tuple<char, char> contactList in letterContacts.tupleListofContacts)
                {
                    if (((contactList.Item1) != '-') && (!contactLetters.Contains(contactList.Item1)))
                    {
                        contactLetters.Add(contactList.Item1);
                    }
                    if (((contactList.Item2) != '-') && (!contactLetters.Contains(contactList.Item2)))
                    {
                        contactLetters.Add(contactList.Item2);
                    }
                    output = output + contactList.Item1 + contactList.Item2 + " ";
                }

                output = c + "  " + contacts[index].tupleListofContacts.Count.ToString("D2") + "/" + contactLetters.Count.ToString("D2") + "  :  " + output;
                letterCount = letterCount + contacts[index].tupleListofContacts.Count;
                Console.WriteLine(output);

                index++;
            }
            Console.WriteLine(letterCount);
        }
    }
}

