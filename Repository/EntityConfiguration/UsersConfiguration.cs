using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Repository.Entities;
using Shared;

namespace Repository.EntityConfiguration
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Attributes)
            .HasConversion(
                attributes => JsonSerializer.Serialize(attributes, (JsonSerializerOptions)null), // Convert to JSON for storage
                json => JsonSerializer.Deserialize<List<UserAttribute>>(json, (JsonSerializerOptions)null)) // Convert back to List<UserAttribute>
            .HasColumnType("nvarchar(max)");
        }
    }
}
