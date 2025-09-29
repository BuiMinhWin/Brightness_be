using System;

namespace SocialAPI.Domain.Entities
{
    public class PostTag
    {
        public Guid PostId { get; set; }
        public Guid TagId { get; set; }
    }
}
