using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDrop.Models
{
    //
    public class Basket
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
