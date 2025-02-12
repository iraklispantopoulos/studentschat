using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Repository.Entities;

namespace Repository.EntityConfiguration
{
    public class ChatHistoryConfiguration : IEntityTypeConfiguration<ChatHistory>
    {
        public void Configure(EntityTypeBuilder<ChatHistory> builder)
        {
            builder.HasIndex(x => x.Date)
                   .IncludeProperties(x => new { x.PromptType, x.Text, x.UserId })
                   .HasDatabaseName("IX_ChatHistory_Date");
        }
    }
}
