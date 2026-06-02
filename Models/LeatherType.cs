using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class LeatherType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; } // Common, Specialty, Exotic

        [Required]
        public string Description { get; set; }

        public static List<LeatherType> GetDefaultTypes()
        {
            return new List<LeatherType>
            {
                // Common Leather Types
                new LeatherType { Id = -1, Name = "Full-Grain Leather", Category = "Common", Description = "Highest quality, natural grain surface" },
                new LeatherType { Id = -2, Name = "Top-Grain Leather", Category = "Common", Description = "Sanded and finished, more uniform than full-grain" },
                new LeatherType { Id = -3, Name = "Corrected-Grain Leather", Category = "Common", Description = "Treated to remove imperfections, embossed texture" },
                new LeatherType { Id = -4, Name = "Split Leather", Category = "Common", Description = "Lower layers of hide, often used for suede or coated leather" },

                // Specialty Leathers
                new LeatherType { Id = -5, Name = "Suede Leather", Category = "Specialty", Description = "Made from the underside of the hide, soft and napped finish" },
                new LeatherType { Id = -6, Name = "Nubuck Leather", Category = "Specialty", Description = "Sanded on the grain side, velvety texture" },
                new LeatherType { Id = -7, Name = "Patent Leather", Category = "Specialty", Description = "High-gloss finish, coated with lacquer or plastic" },
                new LeatherType { Id = -8, Name = "Pull-Up Leather", Category = "Specialty", Description = "Treated with waxes and oils for a distressed look" },
                new LeatherType { Id = -9, Name = "Vegetable-Tanned Leather", Category = "Specialty", Description = "Naturally tanned using plant-based tannins" },
                new LeatherType { Id = -10, Name = "Chrome-Tanned Leather", Category = "Specialty", Description = "Tanned with chromium salts, softer and more pliable" },

                // Exotic Leathers
                new LeatherType { Id = -11, Name = "Ostrich Leather", Category = "Exotic", Description = "Characteristic quill pattern, luxurious" },
                new LeatherType { Id = -12, Name = "Crocodile/Alligator Leather", Category = "Exotic", Description = "Distinctive scales, high-end" },
                new LeatherType { Id = -13, Name = "Snake (Python) Leather", Category = "Exotic", Description = "Unique patterns, used in fashion accessories" },
                new LeatherType { Id = -14, Name = "Lizard Leather", Category = "Exotic", Description = "Smaller scales, often used for wallets and belts" },
                new LeatherType { Id = -15, Name = "Stingray Leather", Category = "Exotic", Description = "Durable, unique grain pattern with a pearly finish" }
            };
        }
    }
} 