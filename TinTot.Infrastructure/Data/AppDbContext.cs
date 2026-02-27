using Microsoft.EntityFrameworkCore;
using TinTot.Domain.Entities;
namespace TinTot.Infrastructure.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Listing> Listings => Set<Listing>();
        public DbSet<Image> Images => base.Set<Image>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<Follow> Follows => Set<Follow>();  
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<Banner> Banners => Set<Banner>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            ConfigureUsers(modelBuilder);
            ConfigureCategories(modelBuilder);
            ConfigureListings(modelBuilder);
            ConfigureImages(modelBuilder);
            ConfigureFavorites(modelBuilder);
            ConfigureFollows(modelBuilder);
            ConfigureMessages(modelBuilder);
            ConfigureNotifications(modelBuilder);
            ConfigureRatings(modelBuilder);
            ConfigureBanners(modelBuilder);
        }

        private void ConfigureUsers(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.LoginName).IsUnique();
                entity.Property(x => x.FullName).HasMaxLength(255);
                entity.Property(x => x.Email).HasMaxLength(255);
                entity.Property(x => x.Phone).HasMaxLength(50);
                entity.Property(x => x.LoginName).HasMaxLength(100);
                entity.Property(x => x.Password).HasMaxLength(255);
                entity.Property(x => x.Avatar).HasMaxLength(255);

                entity.Property(x => x.CreatedAt)
                      .HasDefaultValueSql("NOW()");
            });
        }

        private void ConfigureCategories(ModelBuilder builder)
        {
            builder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(255);
                entity.Property(x => x.Image).HasMaxLength(255);

                entity.HasOne(x => x.Parent)
                      .WithMany(x => x.Children)
                      .HasForeignKey(x => x.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureListings(ModelBuilder builder)
        {
            builder.Entity<Listing>(entity =>
            {
                entity.ToTable("listings");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Price)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Location)
                      .HasMaxLength(255);

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.CategoryId);

                entity.HasOne(x => x.User)
                      .WithMany(x => x.Listings)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Category)
                      .WithMany(x => x.Listings)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureImages(ModelBuilder builder)
        {
            builder.Entity<Image>(entity =>
            {
                entity.ToTable("images");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ImageUrl)
                      .HasMaxLength(500);

                entity.HasOne(x => x.Listing)
                      .WithMany(x => x.Images)
                      .HasForeignKey(x => x.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureFavorites(ModelBuilder builder)
        {
            builder.Entity<Favorite>(entity =>
            {
                entity.ToTable("favorites");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new { x.UserId, x.ListingId })
                      .IsUnique();

                entity.HasOne(x => x.User)
                      .WithMany(x => x.Favorites)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Listing)
                      .WithMany(x => x.Favorites)
                      .HasForeignKey(x => x.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureFollows(ModelBuilder builder)
        {
            builder.Entity<Follow>(entity =>
            {
                entity.ToTable("follows");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new { x.FollowerId, x.SellerId })
                      .IsUnique();

                entity.HasOne(x => x.Follower)
                      .WithMany(x => x.Following)
                      .HasForeignKey(x => x.FollowerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Seller)
                      .WithMany(x => x.Followers)
                      .HasForeignKey(x => x.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureMessages(ModelBuilder builder)
        {
            builder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");

                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Sender)
                      .WithMany()
                      .HasForeignKey(x => x.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Receiver)
                      .WithMany()
                      .HasForeignKey(x => x.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Listing)
                      .WithMany()
                      .HasForeignKey(x => x.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureNotifications(ModelBuilder builder)
        {
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.RelatedUser)
                      .WithMany()
                      .HasForeignKey(x => x.RelatedUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Listing)
                      .WithMany()
                      .HasForeignKey(x => x.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureRatings(ModelBuilder builder)
        {
            builder.Entity<Rating>(entity =>
            {
                entity.ToTable("ratings");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Score)
                      .HasPrecision(3, 2);

                entity.HasOne(x => x.User)
                      .WithMany(x => x.ReceivedRatings)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Reviewer)
                      .WithMany(x => x.GivenRatings)
                      .HasForeignKey(x => x.ReviewerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureBanners(ModelBuilder builder)
        {
            builder.Entity<Banner>(entity =>
            {
                entity.ToTable("banners");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Link).HasMaxLength(500);
                entity.Property(x => x.Image).HasMaxLength(500);

                entity.HasOne(x => x.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
