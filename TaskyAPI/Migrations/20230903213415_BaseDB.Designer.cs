﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskyAPI.Data;

#nullable disable

namespace TaskyAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230903213415_BaseDB")]
    partial class BaseDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("TaskyAPI.Models.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<int>("Ordering")
                        .HasColumnType("int");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TaskListId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId")
                        .IsUnique();

                    b.HasIndex("TaskListId");

                    b.ToTable("Task");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("TaskList");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskListMeta", b =>
                {
                    b.Property<int?>("TaskListId")
                        .HasColumnType("int");

                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int?>("UserAccountId")
                        .HasColumnType("int");

                    b.HasKey("TaskListId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("TaskListMeta");
                });

            modelBuilder.Entity("TaskyAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .HasColumnType("longtext");

                    b.Property<string>("Locale")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserAccount");
                });

            modelBuilder.Entity("TaskyAPI.Models.Task", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Creator")
                        .WithOne()
                        .HasForeignKey("TaskyAPI.Models.Task", "CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskyAPI.Models.TaskList", "TaskList")
                        .WithMany("Tasks")
                        .HasForeignKey("TaskListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");

                    b.Navigation("TaskList");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskListMeta", b =>
                {
                    b.HasOne("TaskyAPI.Models.TaskList", "TaskList")
                        .WithMany("TaskListMetas")
                        .HasForeignKey("TaskListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskyAPI.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId");

                    b.Navigation("TaskList");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserAccount", b =>
                {
                    b.HasOne("TaskyAPI.Models.User", "User")
                        .WithMany("Accounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.Navigation("TaskListMetas");

                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("TaskyAPI.Models.User", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}