using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA_Data_Structures_and_LINQ_Dushkevych
{
    class Todo
    {
        public int Id { get; set; }
        public string CreatedAt { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
        public int UserId { get; set; }

        public override string ToString()
        {
            return "Todo item: " + Name;
        }
    }
}
