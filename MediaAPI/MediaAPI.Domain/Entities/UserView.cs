
using System;

namespace MediaAPI.Domain.Entities
{
    public class UserView
    {
        public Guid UserId { get; set; }
        public Guid MediaItemId { get; set; }
        public MediaItem MediaItem { get; set; } = null!;
    }
}
