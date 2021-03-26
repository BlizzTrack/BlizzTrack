using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class GameChildren
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public string Code { get; set; }
        
        [Required]
        public string ParentCode { get; set; }
        
        public GameParents Parent { get; set; }
        
        public GameConfig GameConfig { get; set; }
    }
}