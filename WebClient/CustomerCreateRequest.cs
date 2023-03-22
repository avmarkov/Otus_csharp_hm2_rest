using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClient
{
    public class CustomerCreateRequest
    {
        public CustomerCreateRequest()
        {
        }

        public CustomerCreateRequest(
            string firstName,
            string lastName)
        {
            Firstname = firstName;
            Lastname = lastName;
        }

        public string Firstname { get; init; }

        public string Lastname { get; init; }
    }
}
