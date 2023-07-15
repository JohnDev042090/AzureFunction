using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionText07152023.Model
{
    public class Customer
    {
        public string id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long BirthdayInEpoch { get; set; }
        public string Email { get; set; }
    }
}
