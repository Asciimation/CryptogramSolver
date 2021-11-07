using System;
using System.Collections.Generic;
using System.Text;

namespace CryptogramSolver
{
    public class ContactsForLetter
    {
        public List<Tuple<char, char>> tupleListofContacts = new List<Tuple<char, char>>();

        public void Add (char leftContact, char rightContact)
        {
            tupleListofContacts.Add(Tuple.Create(leftContact, rightContact));
        }

        public void Sort()
        {
            tupleListofContacts.Sort((x, y) => y.Item2.CompareTo(x.Item2));
        }
    }
}
