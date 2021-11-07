using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CryptogramSolver
{
    class AristocratWord
    {
        public int wordNumber;
        public string word;
        public bool isNoun;
        public string regex;
        public ArrayList wordList;
        public bool isDone;

        public AristocratWord(int wordNumber, String word, bool isNoun, string regex, ArrayList wordList)
        {
            this.wordNumber = wordNumber;
            this.word = word;
            this.isNoun = isNoun;
            this.regex = regex;
            this.wordList = new ArrayList(wordList);

            // If there are no words in the list this word is done.
            if (this.wordList.Count == 0)
            {
                this.isDone = true;
            }
            else
            {
                this.isDone = false;
            }
        }
    }
}
