using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rush
{
    namespace trait
    {
        public static class Person_Trait
        {
            public static void ShowAge(this ref Person person)
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace traitImpl1
    {
        public static class Person_Impl1
        {
            public static void ShowAge(this ref Person person)
            {
                Console.Write(person.Age + " from Person_Impl1");
            }
        }
    }

    namespace traitImpl2
    {
        public static class Person_Impl2
        {
            public static void ShowAge(this ref Person person)
            {
                Console.Write(person.Age + " from Person_Impl2");
            }
        }
    }
}
