using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_.NET_schronisko.Models
{
    public class Animal
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Bio { get; set; }
        public string Status { get; set; }
    }
}
