using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class PackageDestinations
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PackageId { get; set; }
        public Guid DestinationId { get; set; }
        public int SequenceNo { get; set; }

        // Navigation
        public Packages? Package { get; set; }
        public Destinations? Destination { get; set; }
    }
}
